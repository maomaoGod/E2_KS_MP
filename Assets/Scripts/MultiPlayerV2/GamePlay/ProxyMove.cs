using UnityEngine;

namespace E2MultiPlayer
{
    public class ProxyMove
    {
        public StateNetcode[] m_StateBuffer;
        public int m_StateCount;
        public Transform m_Transform;
        public E2ClientEntityScript m_EntityScript;
        public bool m_HasRigidbody = false;
        private Rigidbody m_RigidBody = null;
        StateNetcode m_TargetTempState;
        StateNetcode m_SendingTempState;
        bool m_TriedToExtrapolateTooFar = false;
        float m_TimeSpentExtrapolating = 0;
        bool m_ExtrapolatedLastFrame = false;
        bool m_SetVelocityInsteadOfPositionOnNonOwners = false;
        bool m_IsSmoothingAuthorityChanges = false;
        Vector3 m_PositionLastFrame;
        bool m_ChangedPositionLastFrame;
        Quaternion m_RotationLastFrame;
        bool m_ChangedRotationLastFrame;
        int m_AtRestThresholdCount = 3;
        bool m_DontEasePosition = false;
        bool m_DontEaseRotation = false;
        bool m_DontEaseScale = false;
        StateNetcode m_LatestEndStateUsed = null;
        public bool m_UseExtrapolationTimeLimit = true;
        public float m_ExtrapolationTimeLimit = 5.0f;
        public bool m_UseExtrapolationDistanceLimit = false;
        public float m_ExtrapolationDistanceLimit = 20.0f;
        public enum ExtrapolationMode
        {
            None, Limited, Unlimited
        }
        public ExtrapolationMode m_ExtrapolationMode = ExtrapolationMode.Limited;

        public int m_SendRate = 30;
        public float m_InterpolationBackTime = .1f;
        public int m_LocalTimeResetIndicator = 0;
        float m_OwnerTime;
        public float LocalTime { get; private set; }
        
        const int MaxTimePower = 12;
        readonly float MaxLocalTime = Mathf.Pow(2, MaxTimePower);
        readonly float MinTimePrecision = Mathf.Pow(2, MaxTimePower - 24);
        
        float m_LastTimeOwnerTimeWasSet;
        float m_LatestAuthorityChangeZeroTime;
        public float ApproximateNetworkTimeOnOwner {
            get {
                return m_OwnerTime + (LocalTime - m_LastTimeOwnerTimeWasSet);
            }
            set {
                m_OwnerTime = value;
                //Log.Info($"ApproximateNetworkTimeOnOwner: {value} {LocalTime}");
                m_LastTimeOwnerTimeWasSet = LocalTime;
            }
        }
        public float m_MaxPositionDifferenceForVelocitySyncing = 10;
        
        public float m_ReceivedPositionThreshold = 0.0f;
        public float m_ReceivedRotationThreshold = 0.0f;
        public float m_SnapPositionThreshold = 0;
        public float m_SnapRotationThreshold = 0;
        public float m_SnapScaleThreshold = 0;
        
        public float m_PositionLerpSpeed = .85f;
        public float m_RotationLerpSpeed = .85f;
        public float m_ScaleLerpSpeed = .85f;

        
        public float m_TimeCorrectionSpeed = .1f;
        public float m_SnapTimeThreshold = 3.0f;
        float m_FirstReceivedMessageZeroTime;
        public int m_ReceivedStatesCounter;
        public ProxyMove()
        {
            int calculatedStateBufferSize = ((int)(m_SendRate * m_InterpolationBackTime) + 1) * 2;
            m_StateBuffer = new StateNetcode[Mathf.Max(calculatedStateBufferSize, 30)];
            

            // If we want to extrapolate forever, force variables accordingly. 
            if (m_ExtrapolationMode == ExtrapolationMode.Unlimited)
            {
                m_UseExtrapolationDistanceLimit = false;
                m_UseExtrapolationTimeLimit = false;
            }

            m_TargetTempState = new StateNetcode();
            m_SendingTempState = new StateNetcode();

            LocalTime = 0.0f;
        }

        public void Initialize(Transform trans,E2ClientEntityScript entityScript)
        {
            m_Transform = trans;
            m_EntityScript = entityScript;
        }
        
        public void ResetLocalTime()
        {
            Log.Info("ProxyMove.ResetLocalTime");
            m_LocalTimeResetIndicator++;
            if (m_LocalTimeResetIndicator >= 128) 
                m_LocalTimeResetIndicator = 0;
            m_LastTimeOwnerTimeWasSet -= LocalTime;
            m_LatestAuthorityChangeZeroTime -= LocalTime;
            for (int i = 0; i < m_StateCount; i++)
            {
                m_StateBuffer[i].m_ReceivedTimestamp -= LocalTime;
            }
            LocalTime = 0.0f;
        }
        
        public void AddState(StateNetcode state)
        {
            if (m_StateCount > 1)
            {
                float deltaTime = state.m_OwnerTimestamp - m_StateBuffer[0].m_OwnerTimestamp;
                bool isOutOfOrder = deltaTime <= 0;
                bool isResettingTime = state.m_LocalTimeResetIndicator != m_StateBuffer[0].m_LocalTimeResetIndicator;

                // If State arrived out of order and is not resetting time, do not add the State.
                if (isOutOfOrder && !isResettingTime)
                {
                    return;
                }

                // A way to handle time resetting so we know to change the times of States already in the buffer.
                if (isResettingTime)
                {
                    OnRemoteTimeReset();
                }
            }

            // Shift the buffer, deleting the oldest State.
            for (int i = m_StateBuffer.Length - 1; i >= 1; i--)
            {
                m_StateBuffer[i] = m_StateBuffer[i - 1];
            }

            // Add the new State at the front of the buffer.
            m_StateBuffer[0] = state;

            // Keep track of how many States are in the buffer.
            m_StateCount = Mathf.Min(m_StateCount + 1, m_StateBuffer.Length);
        }
        
        public void OnRemoteTimeReset()
        {
            // Also adjust owner time.
            ApproximateNetworkTimeOnOwner -= m_StateBuffer[0].m_OwnerTimestamp;
            // Don't forget the temp state used for extrapolation
            m_TargetTempState.m_OwnerTimestamp -= m_StateBuffer[0].m_OwnerTimestamp;
            for (int i = m_StateCount - 1; i >= 0; i--)
            {
                m_StateBuffer[i].m_OwnerTimestamp -= m_StateBuffer[0].m_OwnerTimestamp;
            }
        }

        public void SmoothSyncUpdate(float dtTime,bool bOwner)
        {
            LocalTime += Time.deltaTime;
            // If time is high and float imprecision is happening, reset down to more precise float numbers
            // and force a State send so non-owners know to reset time.
            if (LocalTime > MaxLocalTime)
            {
                ResetLocalTime();
            }

            // Set the interpolated / extrapolated Transforms and Rigidbodies of non-owned objects.
            if (!bOwner)
            {
                AdjustOwnerTime();
                ApplyInterpolationOrExtrapolation();
            }
            else
            {
                SendState();
            }
        }
        
        void AdjustOwnerTime()
        {
            // Don't adjust time if at rest or no State received yet.
            if (m_StateBuffer[0] == null || (m_StateBuffer[0].m_AtPositionalRest && m_StateBuffer[0].m_AtRotationalRest)) 
                return;

            float newTime = m_StateBuffer[0].m_OwnerTimestamp + (LocalTime - m_StateBuffer[0].m_ReceivedTimestamp);

            // Time correction can only be as small as the minTimePrecision
            float timeCorrection = Mathf.Max(m_TimeCorrectionSpeed * Time.deltaTime, MinTimePrecision);

            if (m_FirstReceivedMessageZeroTime == 0)
            {
                m_FirstReceivedMessageZeroTime = LocalTime;
            }

            float timeChangeMagnitude = Mathf.Abs(ApproximateNetworkTimeOnOwner - newTime);
            if (m_ReceivedStatesCounter< m_SendRate ||
                timeChangeMagnitude < timeCorrection ||
                timeChangeMagnitude > m_SnapTimeThreshold)
            {
                ApproximateNetworkTimeOnOwner = newTime;
            }
            else
            {
                if (ApproximateNetworkTimeOnOwner < newTime)
                {
                    ApproximateNetworkTimeOnOwner += timeCorrection;
                }
                else
                {
                    ApproximateNetworkTimeOnOwner -= timeCorrection;
                }
            }
        }
        
        public void ApplyInterpolationOrExtrapolation()
        {
            if (m_StateCount == 0) 
                return;

            if (!m_ExtrapolatedLastFrame)
            {
                m_TargetTempState.ResetTheVariables();
            }

            m_TriedToExtrapolateTooFar = false;

            // The target playback time.
            float interpolationTime = ApproximateNetworkTimeOnOwner - m_InterpolationBackTime;

            // If there is only one state just copy it
            if (m_StateCount == 1)
            {
                m_TargetTempState.CopyFromState(m_StateBuffer[0]);
            }
            // Use interpolation if the target playback time is present in the buffer.
            else if (m_StateCount > 1 && m_StateBuffer[0].m_OwnerTimestamp > interpolationTime)
            {
                Interpolate(interpolationTime);
                m_ExtrapolatedLastFrame = false;
            }
            // If we are at rest, continue moving towards the final destination.
            else if (m_StateBuffer[0].m_AtPositionalRest && m_StateBuffer[0].m_AtRotationalRest)
            {
                m_TargetTempState.CopyFromState(m_StateBuffer[0]);
                m_ExtrapolatedLastFrame = false;
                // If using VelocityDrivenSyncing, set it up so that the velocities will be zero'd.
                if (m_SetVelocityInsteadOfPositionOnNonOwners) 
                    m_TriedToExtrapolateTooFar = true;
            }
            // The newest State is too old, we'll have to use extrapolation. 
            // Don't extrapolate if we just changed authority.
            else if ((m_IsSmoothingAuthorityChanges &&
                LocalTime - m_LatestAuthorityChangeZeroTime > m_InterpolationBackTime * 2.0f) ||
                !m_IsSmoothingAuthorityChanges)
            {
                bool success = Extrapolate(interpolationTime);
                m_ExtrapolatedLastFrame = true;
                m_TriedToExtrapolateTooFar = !success;

                // Determine the velocity to set the object to if we are syncing in that manner.
                if (m_SetVelocityInsteadOfPositionOnNonOwners)
                {
                    float timeSinceLatestReceive = interpolationTime - m_StateBuffer[0].m_OwnerTimestamp;
                    m_TargetTempState.m_Velocity = m_StateBuffer[0].m_Velocity;
                    m_TargetTempState.m_Position = m_StateBuffer[0].m_Position + m_TargetTempState.m_Velocity * timeSinceLatestReceive;
                    Vector3 predictedPos = m_Transform.position + m_TargetTempState.m_Velocity * Time.deltaTime;
                    float percent = (m_TargetTempState.m_Position - predictedPos).sqrMagnitude / (m_MaxPositionDifferenceForVelocitySyncing * m_MaxPositionDifferenceForVelocitySyncing);
                    m_TargetTempState.m_Velocity = Vector3.Lerp(m_TargetTempState.m_Velocity, (m_TargetTempState.m_Position - m_Transform.position) / Time.deltaTime, percent);
                }
            }
            else
            {
                return;
            }

            float actualPositionLerpSpeed = m_PositionLerpSpeed;
            float actualRotationLerpSpeed = m_RotationLerpSpeed;
            float actualScaleLerpSpeed = m_ScaleLerpSpeed;

            bool teleportPosition = false;
            bool teleportRotation = false;

            if (m_DontEasePosition)
            {
                actualPositionLerpSpeed = 1;
                teleportPosition = true;
                m_DontEasePosition = false;
            }
            if (m_DontEaseRotation)
            {
                actualRotationLerpSpeed = 1;
                teleportRotation = true;
                m_DontEaseRotation = false;
            }
            if (m_DontEaseScale)
            {
                actualScaleLerpSpeed = 1;
                m_DontEaseScale = false;
            }

            // Set position, rotation, scale, velocity, and angular velocity (as long as we didn't try and extrapolate too far).
            if (!m_TriedToExtrapolateTooFar)
            {
                bool changedPositionEnough = false;
                float distance = 0;
                // If the current position is different from target position
                if (GetPosition() != m_TargetTempState.m_Position)
                {
                    // If we want to use either of these variables, we need to calculate the distance.
                    if (m_ReceivedPositionThreshold != 0)
                    {
                        distance = Vector3.Distance(GetPosition(), m_TargetTempState.m_Position);
                    }
                }
                // If we want to use receivedPositionThreshold, check if the distance has passed the threshold.
                if (m_ReceivedPositionThreshold != 0)
                {
                    if (distance > m_ReceivedPositionThreshold)
                    {
                        changedPositionEnough = true;
                    }
                }
                else // If we don't want to use receivedPositionThreshold, we will always set the new position.
                {
                    changedPositionEnough = true;
                }

                bool changedRotationEnough = false;
                float angleDifference = 0;
                // If the current rotation is different from target rotation
                if (GetRotation() != m_TargetTempState.m_Rotation)
                {
                    // If we want to use either of these variables, we need to calculate the angle difference.
                    if (m_ReceivedRotationThreshold != 0)
                    {
                        angleDifference = Quaternion.Angle(GetRotation(), m_TargetTempState.m_Rotation);
                    }
                }
                // If we want to use receivedRotationThreshold, check if the angle difference has passed the threshold.
                if (m_ReceivedRotationThreshold != 0)
                {
                    if (angleDifference > m_ReceivedRotationThreshold)
                    {
                        changedRotationEnough = true;
                    }
                }
                else // If we don't want to use receivedRotationThreshold, we will always set the new position.
                {
                    changedRotationEnough = true;
                }


                // If current scale is different from target scale
                bool changedScaleEnough = false;
                if (GetScale() != m_TargetTempState.m_Scale)
                {
                    changedScaleEnough = true;
                }
                
                if (changedPositionEnough)
                {
                    Vector3 newPosition = GetPosition();
                    newPosition = m_TargetTempState.m_Position;
                    // Set Velocity or Position of the object.
                    if (m_SetVelocityInsteadOfPositionOnNonOwners && !teleportPosition)
                    {
                        if (m_HasRigidbody) 
                            m_RigidBody.velocity = m_TargetTempState.m_Velocity;
                    }
                    else
                    {
                        SetPosition(Vector3.Lerp(GetPosition(), newPosition, actualPositionLerpSpeed), teleportPosition);
                    }
                }
                if (changedRotationEnough)
                {
                    Vector3 newRotation = GetRotation().eulerAngles;
                    newRotation = m_TargetTempState.m_Rotation.eulerAngles;
                    Quaternion newQuaternion = Quaternion.Euler(newRotation);
                    SetRotation(Quaternion.Lerp(GetRotation(), newQuaternion, actualRotationLerpSpeed), teleportRotation);
                }
                if (changedScaleEnough)
                {
                    Vector3 newScale = GetScale();
                    newScale = m_TargetTempState.m_Scale;
                    SetScale(Vector3.Lerp(GetScale(), newScale, actualScaleLerpSpeed));
                }
            }
            else if (m_TriedToExtrapolateTooFar)
            {
                if (m_HasRigidbody)
                {
                    m_RigidBody.velocity = Vector3.zero;
                    m_RigidBody.angularVelocity = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Interpolate between two States from the m_StateBuffer in order calculate the targetState.
        /// </summary>
        /// <param name="interpolationTime">The target time</param>
        void Interpolate(float interpolationTime)
        {
            //Log.Info($"ProxyMove.Interpolate {interpolationTime} OwnerTime:{m_OwnerTime} LocalTime:{LocalTime} LastResetTime:{m_LastTimeOwnerTimeWasSet} ");
            
            // Go through buffer and find correct State to start at.
            int stateIndex = 0;
            for (; stateIndex < m_StateCount; stateIndex++)
            {
                if (m_StateBuffer[stateIndex].m_OwnerTimestamp <= interpolationTime) 
                    break;
            }

            if (stateIndex == m_StateCount)
            {
                stateIndex--;
            }

            // The State one slot newer than the starting State.
            StateNetcode end = m_StateBuffer[Mathf.Max(stateIndex - 1, 0)];
            // The starting playback State.
            StateNetcode start = m_StateBuffer[stateIndex];

            // Calculate how far between the two States we should be.
            float t = (interpolationTime - start.m_OwnerTimestamp) / (end.m_OwnerTimestamp - start.m_OwnerTimestamp);

            ShouldTeleport(start, ref end, interpolationTime, ref t);

            // Interpolate between the States to get the target State.
            m_TargetTempState = StateNetcode.Lerp(m_TargetTempState, start, end, t);

            // Snap thresholds
            if (m_SnapPositionThreshold != 0)
            {
                float positionDifference = (end.m_Position - start.m_Position).magnitude;
                if (positionDifference > m_SnapPositionThreshold)
                {
                    m_TargetTempState.m_Position = end.m_Position;
                }
                m_DontEasePosition = true;
            }

            if (m_SnapScaleThreshold != 0)
            {
                float scaleDifference = (end.m_Scale - start.m_Scale).magnitude;
                if (scaleDifference > m_SnapScaleThreshold)
                {
                    m_TargetTempState.m_Scale = end.m_Scale;
                }
                m_DontEaseScale = true;
            }

            if (m_SnapRotationThreshold != 0)
            {
                float rotationDifference = Quaternion.Angle(end.m_Rotation, start.m_Rotation);
                if (rotationDifference > m_SnapRotationThreshold)
                {
                    m_TargetTempState.m_Rotation = end.m_Rotation;
                }
                m_DontEaseRotation = true;
            }

            // Determine velocity we'll be setting the object to have if we are sycning in that manner.
            if (m_SetVelocityInsteadOfPositionOnNonOwners)
            {
                Vector3 predictedPos = m_Transform.position + m_TargetTempState.m_Velocity * Time.deltaTime;
                float percent = (m_TargetTempState.m_Position - predictedPos).sqrMagnitude / (m_MaxPositionDifferenceForVelocitySyncing * m_MaxPositionDifferenceForVelocitySyncing);
                m_TargetTempState.m_Velocity = Vector3.Lerp(m_TargetTempState.m_Velocity, (m_TargetTempState.m_Position - m_Transform.position) / Time.deltaTime, percent);
            }
        }
        
        bool Extrapolate(float interpolationTime)
        {
            Log.Info("ProxyMove.Extrapolate");
            
            // Start from the latest State
            if (!m_ExtrapolatedLastFrame || m_TargetTempState.m_OwnerTimestamp < m_StateBuffer[0].m_OwnerTimestamp)
            {
                m_TargetTempState.CopyFromState(m_StateBuffer[0]);
                m_TimeSpentExtrapolating = 0;
            }

            // Determines velocities based on previous State. Used on non-rigidbodies and when not syncing velocity 
            // to save bandwidth. This is less accurate than syncing velocity for rigidbodies. 
            if (m_ExtrapolationMode != ExtrapolationMode.None && m_StateCount >= 2)
            {
                if ( !m_StateBuffer[0].m_AtPositionalRest)
                {
                    m_TargetTempState.m_Velocity = (m_StateBuffer[0].m_Position - m_StateBuffer[1].m_Position) / (m_StateBuffer[0].m_OwnerTimestamp - m_StateBuffer[1].m_OwnerTimestamp);
                }
                if (!m_StateBuffer[0].m_AtRotationalRest)
                {
                    Quaternion deltaRot = m_StateBuffer[0].m_Rotation * Quaternion.Inverse(m_StateBuffer[1].m_Rotation);
                    Vector3 eulerRot = new Vector3(Mathf.DeltaAngle(0, deltaRot.eulerAngles.x), Mathf.DeltaAngle(0, deltaRot.eulerAngles.y), Mathf.DeltaAngle(0, deltaRot.eulerAngles.z));
                    Vector3 angularVelocity = eulerRot / (m_StateBuffer[0].m_OwnerTimestamp - m_StateBuffer[1].m_OwnerTimestamp);
                    m_TargetTempState.m_AngularVelocity = angularVelocity;
                }
            }

            // If we don't want to extrapolate, don't.
            if (m_ExtrapolationMode == ExtrapolationMode.None) return false;

            // Don't extrapolate for more than extrapolationTimeLimit if we are using it.
            if (m_UseExtrapolationTimeLimit &&
                m_TimeSpentExtrapolating > m_ExtrapolationTimeLimit)
            {
                return false;
            }

            // Set up some booleans for if we are moving.
            bool hasVelocity = Mathf.Abs(m_TargetTempState.m_Velocity.x) >= .01f || Mathf.Abs(m_TargetTempState.m_Velocity.y) >= .01f ||
                Mathf.Abs(m_TargetTempState.m_Velocity.z) >= .01f;
            bool hasAngularVelocity = Mathf.Abs(m_TargetTempState.m_AngularVelocity.x) >= .01f || Mathf.Abs(m_TargetTempState.m_AngularVelocity.y) >= .01f ||
                Mathf.Abs(m_TargetTempState.m_AngularVelocity.z) >= .01f;

            // If not moving, don't extrapolate. This is so we don't try to extrapolate while at rest.
            if (!hasVelocity && !hasAngularVelocity)
            {
                return false;
            }

            // Calculate how long to extrapolate from the current target State.
            float timeDif = 0;
            if (m_TimeSpentExtrapolating == 0)
            {
                timeDif = interpolationTime - m_TargetTempState.m_OwnerTimestamp;
            }
            else
            {
                timeDif = Time.deltaTime;
            }
            m_TimeSpentExtrapolating += timeDif;

            // Only extrapolate position if enabled and not at positional rest.
            if (hasVelocity)
            {
                // Velocity.
                m_TargetTempState.m_Position += m_TargetTempState.m_Velocity * timeDif;

                // Gravity. Only if not at rest in the y axis.
                if (Mathf.Abs(m_TargetTempState.m_Velocity.y) >= .01f)
                {
                    if (m_HasRigidbody && m_RigidBody.useGravity)
                    {
                        m_TargetTempState.m_Velocity += Physics.gravity * timeDif;
                    }
                }

                // Drag.
                if (m_HasRigidbody)
                {
                    m_TargetTempState.m_Velocity -= m_TargetTempState.m_Velocity * timeDif * m_RigidBody.drag;
                }
            }

            // Only extrapolate rotation if enabled and not at rotational rest.
            if (hasAngularVelocity)
            {
                // Angular velocity.
                float axisLength = timeDif * m_TargetTempState.m_AngularVelocity.magnitude;
                Quaternion angularRotation = Quaternion.AngleAxis(axisLength, m_TargetTempState.m_AngularVelocity);
                m_TargetTempState.m_Rotation = angularRotation * m_TargetTempState.m_Rotation;

                // Angular drag.
                float angularDrag = 0;
                if (m_HasRigidbody) angularDrag = m_RigidBody.angularDrag;
                if (m_HasRigidbody)
                {
                    if (angularDrag > 0)
                    {
                        m_TargetTempState.m_AngularVelocity -= m_TargetTempState.m_AngularVelocity * timeDif * angularDrag;
                    }
                }
            }

            // Don't extrapolate for more than extrapolationDistanceLimit if we are using it.
            if (m_UseExtrapolationDistanceLimit &&
                Vector3.Distance(m_StateBuffer[0].m_Position, m_TargetTempState.m_Position) >= m_ExtrapolationDistanceLimit)
            {
                return false;
            }

            return true;
        }
        
        void ShouldTeleport(StateNetcode start, ref StateNetcode end, float interpolationTime, ref float t)
        {
            // If the interpolationTime is further back than the start State time and start State is a teleport, then teleport.
            if (start.m_OwnerTimestamp > interpolationTime && start.m_Teleport && m_StateCount == 2)
            {
                // Because we are further back than the Start state, the Start state is our end State.
                end = start;
                t = 1;
                StopEasing();
            }

            // Check if low FPS caused us to skip a teleport State. If yes, teleport.
            for (int i = 0; i < m_StateCount; i++)
            {
                if (m_StateBuffer[i] == m_LatestEndStateUsed && m_LatestEndStateUsed != end && m_LatestEndStateUsed != start)
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (m_StateBuffer[j].m_Teleport == true)
                        {
                            t = 1;
                            StopEasing();
                        }
                        if (m_StateBuffer[j] == start) break;
                    }
                    break;
                }
            }
            m_LatestEndStateUsed = end;

            // If target State is a teleport State, stop lerping and immediately move to it.
            if (end.m_Teleport == true)
            {
                t = 1;
                StopEasing();
            }
        }

        
        void SendState()
        {
            // Get the current state of the object and send it out
            m_SendingTempState.CopyFromProxy(this);
            //m_EntityScript.Entity.CallRPC();
            m_EntityScript.Entity.CallRPC(Consts.RPC.TRANSFORM, m_EntityScript.OwnerId,  m_SendingTempState.m_Position,m_SendingTempState.m_Rotation,
                m_SendingTempState.m_OwnerTimestamp);
        }
        
        public Vector3 GetPosition()
        {
            return m_Transform.position;
        }
        /// <summary>Get rotation of object based on if child or not.</summary>
        public Quaternion GetRotation()
        {
            return m_Transform.rotation;
        }
        /// <summary>Get scale of object.</summary>
        public Vector3 GetScale()
        {
            return m_Transform.localScale;
        }
        /// <summary>Set position of object based on if child or not.</summary>
        public void SetPosition(Vector3 position, bool isTeleporting)
        {
            m_Transform.position = position;
            
        }
        /// <summary>Set rotation of object based on if child or not.</summary>
        public void SetRotation(Quaternion rotation, bool isTeleporting)
        {
            m_Transform.rotation = rotation;
            
        }
        /// <summary>Set scale of object.</summary>
        public void SetScale(Vector3 scale)
        {
            m_Transform.localScale = scale;
        }
        
        public void StopEasing()
        {
            m_DontEasePosition = true;
            m_DontEaseRotation = true;
            m_DontEaseScale = true;
        }
    }
}