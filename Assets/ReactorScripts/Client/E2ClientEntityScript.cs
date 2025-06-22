using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;
using System.Threading.Tasks;
using E2MultiPlayer;
using KS.Reactor.Server;
using UnityEngine.AI;


public class E2ClientEntityScript : ksEntityScript
{
    public GameObject m_LocalPlayer;
    public int UpdatePerSecond { get; private set; } = 60;
    public event Action OnSendUpdate;
    private float m_timer = 0f;

    public bool IsOwner { get { return m_isOwner; } }
    private uint m_OwnerId;
    public uint OwnerId { get { return m_OwnerId; } }
    private bool m_isOwner;
    private GameObject m_LocalPlayerObj = null;
    
    private Consts.EntityType m_EntityType;

    private ProxyMove m_ProxyMove = null;

    // Called after properties are initialized.
    private static int idx = 0;
    public override void Initialize()
    {
        m_OwnerId = Properties[Consts.Prop.OWNER];
        m_EntityType = (Consts.EntityType)(Properties[Consts.Prop.ENTITYTYPE].Int);
        m_isOwner = m_OwnerId == Room.LocalPlayerId;

        var rm = GameObject.FindFirstObjectByType<ksRoomComponent>();
        gameObject.transform.parent = rm.transform;
        
        gameObject.name += $"_{m_OwnerId}";
        
        if (null != m_LocalPlayer)
        {
            Vector3 spawn_vector3 = Vector3.zero;
            if(idx>0)
                spawn_vector3 = new Vector3(2.5f,1,0);// DynamicNavigationMgr.Instance.GenerateBornPoint();
            else
            {
                spawn_vector3 = new Vector3(0.0f,1,0);
            }
            ++idx;
            
            // Vector3 spawn_vector3 = MonsterMiniGame.instance.GetSpawnPosition();
            
            Log.Info($"E2ClientEntityScript::Initialize {Room.LocalPlayerId} {m_OwnerId} EntityType: {m_EntityType}  SpawnPos:{spawn_vector3}");
            
            if (m_EntityType == Consts.EntityType.E_Entity_Player)
            {
                if (IsOwner)
                {
                    m_LocalPlayerObj = GameObject.Instantiate(m_LocalPlayer,spawn_vector3,transform.rotation);
                }
                else
                {
                    m_LocalPlayerObj = GameObject.Instantiate(m_LocalPlayer,spawn_vector3,transform.rotation);
                }
            }
            else if (m_EntityType == Consts.EntityType.E_Entity_FollowPlayer)
            {
                m_LocalPlayerObj = GameObject.Instantiate(m_LocalPlayer,spawn_vector3,transform.rotation);
            }else
            {
                m_LocalPlayerObj = GameObject.Instantiate(m_LocalPlayer,spawn_vector3,transform.rotation);    
            }
            
            m_LocalPlayerObj.name += $"_{m_OwnerId}";
            if (m_EntityType == Consts.EntityType.E_Entity_Player || m_EntityType == Consts.EntityType.E_Entity_FollowPlayer)
            {
                ActorManager.Instance.OnPlayerCreated(m_OwnerId,m_LocalPlayerObj,this.gameObject);    
            }else if (m_EntityType == Consts.EntityType.E_Entity_NPC)
            {
                ActorManager.Instance.OnNPCCreated(m_OwnerId,m_LocalPlayerObj,this.gameObject);

                var npcNv = m_LocalPlayerObj.GetComponentInChildren<NavMeshAgent>();
                if (null != npcNv)
                {
                    if (!npcNv.isOnNavMesh)
                    {
                        npcNv.enabled = false;
                        npcNv.Warp(spawn_vector3);
                        npcNv.enabled = true;
                    }
                }

            }else if (m_EntityType == Consts.EntityType.E_Entity_Bullet)
            {
                ActorManager.Instance.OnBulletCreated(m_OwnerId,m_LocalPlayerObj,this.gameObject);
            }
                
        }

        if (m_isOwner)
        {
            ActorManager.Instance.RefreshMainPlayer(m_OwnerId);
        }
        else
        {
            var player = ActorManager.Instance.GetActor(m_OwnerId);
            if (null != player)
            {
                var plAgent = player as PlayerAgent;
                if (null != plAgent)
                {
                    m_ProxyMove = plAgent.ProxyMove;
                }
            }
        }
    }
    

    public override void Detached()
    {
        var actorId = Properties[Consts.Prop.OWNER];
        Log.Info($"E2ClientEntityScript::Detached {Room.LocalPlayerId} {Properties[Consts.Prop.OWNER]}");
        m_isOwner = actorId == Room.LocalPlayerId;
        GameObject controlledObject = null;

        ActorManager.Instance.OnPlayerDestroyed(actorId,this.gameObject);    
        
        if (m_EntityType == Consts.EntityType.E_Entity_Player || m_EntityType == Consts.EntityType.E_Entity_NPC)
        {
           
        }
        else
        {
            // do nothing
        }
        
        if (null != m_LocalPlayerObj)
        {
            GameObject.Destroy(m_LocalPlayerObj);
        }
    }

    [ksRPC(Consts.RPC.TRANSFORM_RESPONSE)]
    private void ResponseTransform(uint ownerID, ksVector3 position, ksQuaternion rotation,float timeStamp)
    {
        Log.Info($"E2ClientEntityScript.ResponseTransform {ownerID} {position} {timeStamp}");
        // if ( null != m_ProxyMove && !IsOwner)
        // {
        //     var state = new StateNetcode();
        //     state.m_Position = position;
        //     state.m_Rotation = rotation;
        //     state.m_OwnerTimestamp = timeStamp;
        //     state.m_ReceivedTimestamp = m_ProxyMove.LocalTime;
        //     m_ProxyMove.AddState(state);
        // }
    }
    
    
    //Process State in Update
    private void Update()
    {
        if (!m_isOwner)
        {
            
        }
    }
    
    private void LateUpdate()
    {

        if (m_EntityType == Consts.EntityType.E_Entity_Player)
        {
            var actor = ActorManager.Instance.GetActor(m_OwnerId);
            if (null != actor)
            {
                actor.SetHp(Entity.Properties[Consts.Prop.HP].Int);
            }
        }
        
        
        m_timer -= Time.RealDelta;
        if (m_timer > 0f)
        {
            return;
        }
        m_timer += 1f / UpdatePerSecond;
        m_timer = Math.Max(0f, m_timer);

        OnSendUpdate?.Invoke();
    }
}