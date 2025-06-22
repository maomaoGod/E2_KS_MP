
using System.Collections.Generic;
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
        private WeaponProxy m_WeaponProxy;
        public WeaponProxy WeaponProxy => m_WeaponProxy;

        private string m_InteractionColliderTag = "InteractionCollider";
        private string m_LeftHandColliderTag = "LeftHandCollider";
        private string m_RightHandColliderTag = "RightHandCollider";
        
        private Dictionary<string,Transform> m_CachedTransforms = new Dictionary<string, Transform>();
        private Dictionary<Transform,E2ClientColliderSync> m_Sourc2Target = new  Dictionary<Transform,E2ClientColliderSync>();
        public override void Bind(GameObject controller,GameObject syncTarget)
        {
            m_CachedTransforms.Clear();
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

            var colliders = m_PlayerControl.gameObject.GetComponentsInChildren<Collider>(true);
            foreach (var collider in colliders)
            {
                if (collider.CompareTag(m_LeftHandColliderTag))
                {
                    m_CachedTransforms[m_LeftHandColliderTag] = collider.transform;
                }else if (collider.CompareTag(m_RightHandColliderTag))
                {
                    m_CachedTransforms[m_RightHandColliderTag] = collider.transform;
                }else if (collider.CompareTag(m_InteractionColliderTag))
                {
                    m_CachedTransforms[m_InteractionColliderTag] = collider.transform;
                }
            }

            m_WeaponProxy = new WeaponProxy();
            m_WeaponProxy.Initialize(m_PlayerControl, m_EntityScript);
        }

        public void BindColliderSync(E2ClientColliderSync sync)
        {
            if (null == sync)
            {
                Log.Error("PlayerAgent.BindColliderSync called with a null syncTarget");
                return;
            }
            
            var syncTag = sync.gameObject.tag;
            if (m_CachedTransforms.TryGetValue(syncTag, out Transform transform))
            {
                if (m_Sourc2Target.ContainsKey(transform))
                {
                    Log.Error($"PlayerAgent.BindColliderSync called with a duplicate syncTarget {syncTag}");
                }
                m_Sourc2Target[transform] = sync;
            }
            else
            {
                Log.Error($"PlayerAgent.BindColliderSync called with a non-existing transform {syncTag}");
            }
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

                        foreach (var kv in m_Sourc2Target)
                        {
                            var entityId = kv.Value.Entity.Id;
                            kv.Value.transform.position = kv.Key.position;
                            kv.Value.transform.rotation = kv.Key.rotation;
                            kv.Value.transform.localScale = kv.Key.lossyScale;
                            
                            kv.Value.Entity.CallRPC(Consts.RPC.TRANSFORM_COLLIDER, entityId, m_EntityScript.OwnerId ,  kv.Key.position , kv.Key.rotation,
                                    kv.Key.lossyScale);
                        }
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