
using UnityEngine;

namespace E2MultiPlayer
{
    public class Actor
    {
        protected PlayerInputData m_InputData;
        protected bool m_bIsCreated = false;
        protected GameObject m_SyncTarget = null;
        protected GameObject m_ArtTarget = null;
        public GameObject ArtTarget => m_ArtTarget;

        private int m_Hp = 100;
        public virtual void SetHp(int hp)
        {
            m_Hp = hp;
        }

        public virtual int GetHp()
        {
            return m_Hp;
        }
        
        public virtual void Bind(GameObject controller,GameObject syncTarget)
        {
            // var controllerCom = controller?.GetComponentInChildren<Avatar_Help>();
            // m_Controller = controllerCom;
            // m_InputData = new PlayerInputData();
            // m_Controller.Initialize();
            // m_SyncTarget = syncTarget;
            // m_bIsCreated = true;
        }


        public virtual void SyncPlayerInput(PlayerInputData inputData)
        {
            if (null == inputData)
            {
                return;
            }
            m_InputData.Clone(inputData);
        }
        
        public virtual void FixedUpdate(float dtTime)
        {
            if(!m_bIsCreated)
                return;
            //m_Controller.OnFixedUpdate(dtTime);
        }
        
        public virtual void Update(float dtTime)
        {
            
            
            
        }

        public virtual void LateUpdate(float dtTime)
        {
            
        }

        
    }
}