
using MalbersAnimations;
using UnityEngine;

namespace E2MultiPlayer
{
    public class PlayerAgent:Actor
    {
        private E2ClientAuthority m_ClientAuthority;
        private PlayerCharacterController m_PlayerController;
        private E2ClientAnimSync m_ClientAnimSync;
        private E2ClientEntityScript m_EntityScript;
        private ProxyMove m_ProxyMove = null;
        private Transform m_PlayerControl = null;
        public ProxyMove ProxyMove => m_ProxyMove;
        private PlayerMovementAdvanced m_AdvancedMove;
        public override void Bind(GameObject controller,GameObject syncTarget)
        {
            m_InputData = new PlayerInputData();
            //m_Controller.Initialize();
            m_SyncTarget = syncTarget;
            m_bIsCreated = true;
            //m_PlayerController = m_Controller as PlayerCharacterController;

            // m_PlayerPuppet = controller.GetComponent<E2Puppet>();
            // if (null == m_PlayerPuppet)
            // {
            //     Log.Error($"PlayerAgent.Bind Invalid Puppet :{controller}");
            // }
            
            m_PlayerControl = controller.transform;
            var rg = m_PlayerControl.gameObject.GetComponent<Rigidbody>();
           
            if (null != syncTarget)
            {
                var clientEntity = syncTarget.GetComponent<E2ClientEntityScript>();
                m_PlayerControl.GetComponent<CapsuleCollider>().enabled = clientEntity.IsOwner;
                if (!clientEntity.IsOwner)
                {
                    rg.isKinematic = true;
                    rg.useGravity = false;
                    rg.Sleep();
                   
                }
                else
                {
                    rg.isKinematic = false;
                }
                m_ClientAuthority = syncTarget.GetComponent<E2ClientAuthority>();
                m_ClientAnimSync = syncTarget.GetComponent<E2ClientAnimSync>();
                var animator = controller.GetComponent<Animator>();
                m_ClientAnimSync.InitAnimSync(animator);
                m_EntityScript = clientEntity;
                
                var input = m_PlayerControl.GetComponent<MalbersInput>();
                if (null != input)
                {
                    input.enabled = clientEntity.IsOwner; 
                }
            }
            
            m_AdvancedMove = controller.GetComponent<PlayerMovementAdvanced>();
            if (null != m_AdvancedMove)
            {
                m_AdvancedMove.enabled = m_EntityScript.IsOwner;
            }
            
            m_ProxyMove = new  ProxyMove();
            m_ProxyMove.Initialize(m_PlayerControl.transform,m_EntityScript);
        }

        public override void Update(float dtTime)
        {
            if (!m_bIsCreated)
            {
                return;
            }

            
            m_InputData.m_MoveDirection = new Vector3
            {
                x = Input.GetAxisRaw("Horizontal"),
                y = 0.0f,
                z = Input.GetAxisRaw("Vertical")
            };
            
            m_InputData.m_bJump = Input.GetButton("Jump");

            m_InputData.m_bCrouch = Input.GetKey(KeyCode.C);
            
            //m_PlayerController.InputData.Clone(m_InputData);
            
            //m_Controller.OnUpdate(dtTime);

            if (null != m_ClientAuthority)
            {
                m_ClientAuthority.RefreshProperty(Consts.Prop.MOVEDIRECTION, m_InputData.m_MoveDirection);    
            }
            
            if (Common.s_IsAlone)
            {
                base.Update(dtTime);    
            }
            else
            {
                base.Update(dtTime);
               
            }
            
        }

        private Vector3 m_SmoothSpeed = Vector3.zero;
        public override void FixedUpdate(float dtTime)
        {
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
                        m_SyncTarget.transform.position = m_PlayerControl.transform.position;
                        m_SyncTarget.transform.rotation = m_PlayerControl.transform.rotation;   
                        
                        m_ProxyMove.SmoothSyncUpdate(dtTime,true);
                    }
                    else
                    {
                        //Log.Info($"PlayerAgent.FixedUpdate {m_Avatar_Help.gameObject} {m_SyncTarget.transform.position}");
                        var curPos = m_PlayerControl.transform.position;
                        var tarPos = m_SyncTarget.transform.position;
                        //m_PlayerControl.transform.position = Vector3.SmoothDamp(curPos,tarPos,ref m_SmoothSpeed,dtTime) ;
                        //m_PlayerControl.transform.rotation = m_SyncTarget.transform.rotation;
                        
                        m_ProxyMove.SmoothSyncUpdate(dtTime,false);
                        
                        //m_ProxyMove.ApplyInterpolationOrExtrapolation();
                    }
                }
            }
        }
    }
}