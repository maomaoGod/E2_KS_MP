using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class ServerAvatarTransformSync : ksServerEntityScript
{
    [ksEditable]
    public float CorrectionDuration = 1f / 3f;

    private ServerAvatarAuthority _clientAuthority;

    private ksVector3 previousPosition, latestPosition;
    private ksQuaternion previousRotation, latestRotation;
    bool updateSinceSync = false;
    float clientDelta = 0;

    int extrapolated = 0;
    private ksVector3 eStartPosition;

    private ksVector3 latestPositionCD;
    private ksVector3 latestVelocityCD;
    //private ksVector3 latestPositionSD;

    private ksVector3 m_positionCorrection;
    private ksVector3 m_rotationCorrection;
    private float m_correctionFactor;

    public override void Initialize()
    {
        Room.OnUpdate[0] += Update;
        InitializeVariables();
    }

    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
    }

    private void Update()
    {
        if (Time.FramesUntilSync == 0 && !updateSinceSync)
        {
            extrapolated++;
            //ksLog.Debug($"Extrapolated for {extrapolated} updates");

            /* METRICS 
            if (extrapolated == 0)
            {
                eStartPosition = latestPosition;
            }
            /**/

            if (clientDelta != 0 && Time.Delta != 0)
            {
                latestVelocityCD = (extrapolated == 0) ? (latestPosition - previousPosition) / clientDelta : latestVelocityCD;

                //var latestVelocitySD = (latestPosition - previousPosition) / updateTimeDelta;

                previousPosition = Transform.Position;

                latestPositionCD = latestPosition + latestVelocityCD * Time.Delta;
                //latestPositionSD = latestPosition + latestVelocitySD * ((float)((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) - lastUpdateTime) / 1000);
                Transform.Position = latestPositionCD;

                var angularVelocity = latestRotation * previousRotation.Inverse();
                latestRotation.ToAxisAngle(out var axis, out var angle);
                Transform.Rotation = latestRotation * ksQuaternion.FromAxisAngle(axis, angle * Time.Delta);
            }
            else if (m_correctionFactor > 0)
            {
                Transform.Position = latestPosition;
                Transform.Rotation = latestRotation;
            }
        }
        else
        {
            Transform.Position = latestPosition;
            Transform.Rotation = latestRotation;
        }

        if (m_correctionFactor > 0)
        {
            Transform.Position += m_positionCorrection * m_correctionFactor;
            Transform.RotateRadians(m_rotationCorrection * m_correctionFactor);
            m_correctionFactor -= Time.Delta / CorrectionDuration;
        }

        if (Time.FramesUntilSync == 0)
        {
            updateSinceSync = false;
        }

    }

    void InitializeVariables()
    {
        _clientAuthority = Entity.Scripts.Get<ServerAvatarAuthority>();

        previousPosition = latestPosition = Transform.Position;
        previousRotation = latestRotation = Transform.Rotation;

        latestPositionCD = Transform.Position;

        eStartPosition = latestPositionCD = Transform.Position;
    }

    [ksRPC(Consts.RPC.TRANSFORM)]
    private void SetTransform(ksIServerPlayer player, ksVector3 position, ksQuaternion rotation, ksQuaternion aimRotation, float deltaTime)
    {
        if (player == _clientAuthority.Owner)
        {
            if (extrapolated > 0 && CorrectionDuration > 0)
            {
                m_positionCorrection = Transform.Position - position;
                m_rotationCorrection = ksQuaternion.AngularDisplacementRadians(rotation, Transform.Rotation);
                if (m_positionCorrection.MagnitudeSquared() <= .001f && m_rotationCorrection.MagnitudeSquared() <= .001f)
                {
                    m_correctionFactor = 0;
                }
                else
                {
                    m_correctionFactor = 1 - Time.Delta / CorrectionDuration;
                }
            }

            extrapolated = 0;
            clientDelta = deltaTime;

            previousPosition = latestPosition;
            previousRotation = latestRotation;

            latestPosition = position;
            latestRotation = rotation;

            updateSinceSync = true;
        }
        Entity.CallRPC(Consts.RPC.TRANSFORM, aimRotation);
    }

    [ksRPC(Consts.RPC.RAGDOLL_CHANGE)]
    private void SetRagdollState(ksIServerPlayer player, bool active)
    {
        Properties[Consts.Prop.RAGDOLL_STATE] = active;
    }
}