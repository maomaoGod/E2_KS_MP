using System;
using System.Collections.Generic;
using System.Collections;
using E2MultiPlayer;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

public class E2ClientAuthority : ksEntityScript
{
    // The number of updates sent to the server per second. 0 or less to send every frame.
    public float UpdatesPerSecond = 60f;

    // Called when it's time to send an update to the server.
    public event Action OnSendUpdate;

    // Does the local player control this entity?
    public bool IsOwner
    {
        get { return m_isOwner; }
    }
    private bool m_isOwner;

    private float m_timer = 0f;
    private uint m_OwnerID = 0;
    public uint OwnerID => m_OwnerID;
    private Consts.EntityType m_EntityType;
    private uint m_BuddyPlayerID = 0;
    private WeaponProxy m_ProxyWeapon;

    // Called after properties are initialized.
    public override void Attached()
    {
            
    }
    
    // 1P player move by itself
    // 3P player by sync
    public override void Initialize()
    {
        m_OwnerID = Properties[Consts.Prop.OWNER];
        m_BuddyPlayerID = Properties[Consts.Prop.BUDDYPLAYER];
        m_EntityType = (Consts.EntityType)Properties[Consts.Prop.ENTITYTYPE].Int;
        m_isOwner = m_OwnerID  == Room.LocalPlayerId;
        Log.Info($"E2ClientAuthority.Initialized OwnerID:{m_OwnerID} LocalPlayerID:{Room.LocalPlayerId} {m_EntityType}");
        if (m_isOwner)
        {
            // Stop Reactor from updating the game object transform so it can be controlled by the client.
            Entity.ApplyTransformUpdates = false;
            Entity.PredictionEnabled = false;
            enabled = true;
        }
        else
        {
            if (m_EntityType == Consts.EntityType.E_Entity_NPC)
            {
                if (m_BuddyPlayerID == Room.LocalPlayerId)
                {
                    Entity.ApplyTransformUpdates = false;
                    Entity.PredictionEnabled = false;
                    m_isOwner = true;
                }
                else
                {
                    Entity.ApplyTransformUpdates = true;
                    Entity.PredictionEnabled = true;
                    enabled = false;
                }
            }else if (m_EntityType == Consts.EntityType.E_Entity_Bullet)
            {
                Entity.ApplyTransformUpdates = true;
                Entity.PredictionEnabled = true;
                enabled = false;
            }else if (m_EntityType == Consts.EntityType.E_Entity_FollowPlayer)
            {
                Entity.ApplyTransformUpdates = true;
                Entity.PredictionEnabled = true;
                enabled = false;
            }
            
        }
        
    }
        
    // Called when the script is detached.
    public override void Detached()
    {
            
    }

    private void InitWeaponProxy()
    {
        var actor = ActorManager.Instance.GetActor(m_OwnerID);
        if (null != actor)
        {
            var plAgent = actor as PlayerAgent;
            if (null != plAgent)
            {
                m_ProxyWeapon = plAgent.WeaponProxy;
            }
        }
    }
        

        
    /// <summary>
    /// Sync Position and Rotation to Server
    /// </summary>
    private void LateUpdate()
    {
        m_timer -= Time.RealDelta;
        if (m_timer > 0f)
        {
            return;
        }
        if (UpdatesPerSecond > 0)
        {
            m_timer += 1f / UpdatesPerSecond;
        }
        m_timer = Math.Max(0f, m_timer);
        
        //Entity.CallRPC(Consts.RPC.TRANSFORM, m_OwnerID,  transform.position, transform.rotation);
        if (OnSendUpdate != null)
        {
            OnSendUpdate();
        }
    }

    public void RefreshProperty(uint propKey, ksMultiType val)
    {
        //Properties[propKey] = val;
    }
}