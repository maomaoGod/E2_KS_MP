
using UnityEngine;

namespace E2MultiPlayer
{
    public class BulletAgent:Actor
    {
        private float m_fBulletTime = 5.0f;
        private E2ClientAuthority m_ClientAuthority;
        private Transform m_SyncTransform;
        private Transform m_BulletController;
        public override void Bind(GameObject controller,GameObject plInst)
        {
            m_SyncTarget = plInst;
            m_BulletController = controller.transform;
            if (null != plInst)
            {
                m_SyncTransform = plInst.transform;
                m_ClientAuthority = plInst.GetComponent<E2ClientAuthority>();
            }
            m_bIsCreated = true;
        }

        public override void Update(float dtTime)
        {
            if(!m_bIsCreated)
                return;
            
            //do the ray cast
        }
        
        public override void FixedUpdate(float dtTime)
        {
            if(!m_bIsCreated)
                return;
            
            if (Common.s_IsAlone)
            {
                base.FixedUpdate(dtTime);    
            }
            else
            {
                if (null != m_ClientAuthority)
                {
                    m_BulletController.transform.position = m_SyncTarget.transform.position;
                    m_BulletController.transform.rotation = m_SyncTarget.transform.rotation;
                }
            }
        }
    }
}