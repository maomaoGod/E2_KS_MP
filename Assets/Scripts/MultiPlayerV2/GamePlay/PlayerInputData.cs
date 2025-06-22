using UnityEngine;

namespace E2MultiPlayer
{
    public class PlayerInputData
    {
        public bool m_bHasData = false;
        public bool m_bMove;
        public Vector3 m_MoveDirection;
        public float m_MoveSpeed;
        public bool m_bJump;
        public bool m_bCrouch;
        public bool m_bFire;

        public void Clone(PlayerInputData data)
        {
            m_bHasData = data.m_bHasData;
            m_bMove = data.m_bMove;
            m_MoveDirection = data.m_MoveDirection;
            m_MoveSpeed = data.m_MoveSpeed;
            m_bJump = data.m_bJump;
            m_bCrouch = data.m_bCrouch;
            m_bFire = data.m_bFire;
        }
    }
}