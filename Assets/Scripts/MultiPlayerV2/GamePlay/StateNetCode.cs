using UnityEngine;

namespace E2MultiPlayer
{
    public class StateNetcode
    {
        public float m_OwnerTimestamp;
        public Vector3 m_Position;
        public Quaternion m_Rotation;
        public Vector3 m_Scale;
        public Vector3 m_Velocity;
        public Vector3 m_AngularVelocity;
        public bool m_Teleport;
        public bool m_AtPositionalRest;
        public bool m_AtRotationalRest;
        public float m_ReceivedOnServerTimestamp;
        public float m_ReceivedTimestamp;
        public int m_LocalTimeResetIndicator;
        
        public StateNetcode() { }

        public void CopyFromProxy(ProxyMove smoothSyncScript)
        {
            m_OwnerTimestamp = smoothSyncScript.LocalTime;
            m_Position = smoothSyncScript.GetPosition();
            m_Rotation = smoothSyncScript.GetRotation();
            m_Scale = smoothSyncScript.GetScale();
            m_LocalTimeResetIndicator = smoothSyncScript.m_LocalTimeResetIndicator;
        }
        
        public StateNetcode CopyFromState(StateNetcode state)
        {
            m_OwnerTimestamp = state.m_OwnerTimestamp;
            m_Position = state.m_Position;
            m_Rotation = state.m_Rotation;
            m_Scale = state.m_Scale;
            m_Velocity = state.m_Velocity;
            m_AngularVelocity = state.m_AngularVelocity;
            m_ReceivedTimestamp = state.m_ReceivedTimestamp;
            m_LocalTimeResetIndicator = state.m_LocalTimeResetIndicator;
            return this;
        }

        /// <summary>Returns a Lerped StateNetcode that is between two StateNetcodes in time.</summary>
        /// <param name="start">Start StateNetcode</param>
        /// <param name="end">End StateNetcode</param>
        /// <param name="t">Time</param>
        /// <returns></returns>
        public static StateNetcode Lerp(StateNetcode targetTempStateNetcode, StateNetcode start, StateNetcode end, float t)
        {
            targetTempStateNetcode.m_Position = Vector3.Lerp(start.m_Position, end.m_Position, t);
            targetTempStateNetcode.m_Rotation = Quaternion.Lerp(start.m_Rotation, end.m_Rotation, t);
            targetTempStateNetcode.m_Scale = Vector3.Lerp(start.m_Scale, end.m_Scale, t);
            targetTempStateNetcode.m_Velocity = Vector3.Lerp(start.m_Velocity, end.m_Velocity, t);
            targetTempStateNetcode.m_AngularVelocity = Vector3.Lerp(start.m_AngularVelocity, end.m_AngularVelocity, t);

            targetTempStateNetcode.m_OwnerTimestamp = Mathf.Lerp(start.m_OwnerTimestamp, end.m_OwnerTimestamp, t);

            return targetTempStateNetcode;
        }

        /// <summary>Reset everything so this state can be re-used</summary>
        public void ResetTheVariables()
        {
            m_OwnerTimestamp = 0;
            m_Position = Vector3.zero;
            m_Rotation = Quaternion.identity;
            m_Scale = Vector3.zero;
            m_Velocity = Vector3.zero;
            m_AngularVelocity = Vector3.zero;
            m_AtPositionalRest = false;
            m_AtRotationalRest = false;
            m_Teleport = false;
            m_ReceivedTimestamp = 0;
            m_LocalTimeResetIndicator = 0;
        }
        
        const byte positionMask = 1;        // 0000_0001
        const byte rotationMask = 2;        // 0000_0010
        const byte scaleMask = 4;        // 0000_0100
        const byte velocityMask = 8;        // 0000_1000
        const byte angularVelocityMask = 16; // 0001_0000
        const byte atPositionalRestMask = 64; // 0100_0000
        const byte atRotationalRestMask = 128; // 1000_0000
        /// <summary>Encode sync info based on what we want to send.</summary>
        static byte EncodeSyncInformation(bool sendPosition, bool sendRotation, bool sendScale, bool sendVelocity, bool sendAngularVelocity, bool atPositionalRest, bool atRotationalRest)
        {
            byte encoded = 0;

            if (sendPosition)
            {
                encoded = (byte)(encoded | positionMask);
            }
            if (sendRotation)
            {
                encoded = (byte)(encoded | rotationMask);
            }
            if (sendScale)
            {
                encoded = (byte)(encoded | scaleMask);
            }
            if (sendVelocity)
            {
                encoded = (byte)(encoded | velocityMask);
            }
            if (sendAngularVelocity)
            {
                encoded = (byte)(encoded | angularVelocityMask);
            }
            if (atPositionalRest)
            {
                encoded = (byte)(encoded | atPositionalRestMask);
            }
            if (atRotationalRest)
            {
                encoded = (byte)(encoded | atRotationalRestMask);
            }
            return encoded;
        }
        /// <summary>Decode sync info to see if we want to sync position.</summary>
        static bool ShouldSyncPosition(byte syncInformation)
        {
            if ((syncInformation & positionMask) == positionMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>Decode sync info to see if we want to sync rotation.</summary>
        static bool ShouldSyncRotation(byte syncInformation)
        {
            if ((syncInformation & rotationMask) == rotationMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>Decode sync info to see if we want to sync scale.</summary>
        static bool ShouldSyncScale(byte syncInformation)
        {
            if ((syncInformation & scaleMask) == scaleMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>Decode sync info to see if we want to sync velocity.</summary>
        static bool ShouldSyncVelocity(byte syncInformation)
        {
            if ((syncInformation & velocityMask) == velocityMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>Decode sync info to see if we want to sync angular velocity.</summary>
        static bool ShouldSyncAngularVelocity(byte syncInformation)
        {
            if ((syncInformation & angularVelocityMask) == angularVelocityMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>Decode sync info to see if we should be at positional rest. (Stop extrapolating)</summary>
        static bool ShouldBeAtPositionalRest(byte syncInformation)
        {
            if ((syncInformation & atPositionalRestMask) == atPositionalRestMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>Decode sync info to see if we should be at rotational rest. (Stop extrapolating)</summary>
        static bool ShouldBeAtRotationalRest(byte syncInformation)
        {
            if ((syncInformation & atRotationalRestMask) == atRotationalRestMask)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}