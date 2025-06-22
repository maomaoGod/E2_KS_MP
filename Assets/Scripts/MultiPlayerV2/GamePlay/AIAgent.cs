
using UnityEngine;
using UnityEngine.AI;

namespace E2MultiPlayer
{
    public class AIAgent:Actor
    {
        private Transform m_EmeraldMovement;
        private NavMeshAgent m_NavMeshAgent;
        private Transform m_Target;
        private float m_fBulletTime = 5.0f;
        private GameObject m_BulletGo = null;
        private GameObject m_FlyBullet = null;
        
        private E2ClientAuthority m_ClientAuthority;
        private Transform m_SyncTransform;
        public override void Bind(GameObject controller,GameObject plInst)
        {
            m_SyncTarget = plInst;
            m_EmeraldMovement = controller?.GetComponentInChildren<Transform>();

            var nvMesh = m_EmeraldMovement.gameObject.GetComponent<NavMeshAgent>();
            if (null == nvMesh)
            {
                nvMesh = m_EmeraldMovement.gameObject.AddComponent<NavMeshAgent>();
            }

            m_NavMeshAgent = nvMesh;
            if (!nvMesh.isOnNavMesh)
            {
                nvMesh.enabled = false;
                
                nvMesh.enabled = true;
            }
            
            m_InputData = new PlayerInputData();
            
            if (null != plInst)
            {
                m_SyncTransform = plInst.transform;
                m_ClientAuthority = plInst.GetComponent<E2ClientAuthority>();
                var animator = controller.GetComponentInChildren<Animator>();
                if (null == animator)
                {
                    Log.Error($"E2ClientAnimSync::Bind {controller} animator component not found");
                }
                var animSync = plInst.GetComponent<E2ClientAnimSync>();
                animSync.InitAnimSync(animator);
            }
            
            if(Common.s_IsAlone)
                m_BulletGo = Resources.Load<GameObject>("Fire_Bullet");
            
            m_bIsCreated = true;
        }
        
        
        public override void Update(float dtTime)
        {
            if(!m_bIsCreated)
                return;
            
            // if (null != m_Target)
            // {
            //     m_InputData.m_MoveDirection = (m_Target.transform.position - m_Controller.transform.position).normalized;
            // }
            // else
            // {
            //     var cachedActors = ActorManager.Instance.CachedPlayers;
            //     foreach (var actor in cachedActors)
            //     {
            //         if (actor.Value != this)
            //         {
            //             m_Target = actor.Value.Controller.transform;
            //             break;
            //         }
            //     }
            // }
            //
            // m_NPCController.InputData.Clone(m_InputData);
            //
            // m_Controller.OnUpdate(dtTime);


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
                    if (m_ClientAuthority.IsOwner)
                    {
                        base.FixedUpdate(dtTime);
                        m_SyncTarget.transform.position = m_EmeraldMovement.transform.position;
                        m_SyncTarget.transform.rotation = m_EmeraldMovement.transform.rotation;   
                    }
                    else
                    {
                        m_EmeraldMovement.transform.position = m_SyncTarget.transform.position;
                        m_EmeraldMovement.transform.rotation = m_SyncTarget.transform.rotation;
                    }
                }
            }
        }
    }
}