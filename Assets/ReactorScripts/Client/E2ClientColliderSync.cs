using System;
using System.Collections.Generic;
using System.Collections;
using E2MultiPlayer;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;

public class E2ClientColliderSync : ksEntityScript
{

    public override void Attached()
    {
        
    }

     public bool IsOwner
    {
        get { return m_isOwner; }
    }
    private bool m_isOwner;

    private float m_timer = 0f;
    private uint m_OwnerID = 0;
    public uint OwnerID => m_OwnerID;
    private Consts.EntityType m_EntityType;

    
    public override void Initialize()
    {
        m_OwnerID = Properties[Consts.Prop.OWNER];
        m_EntityType = (Consts.EntityType)(Properties[Consts.Prop.ENTITYTYPE].Int);
        m_isOwner = m_OwnerID  == Room.LocalPlayerId;
        gameObject.name += $"_{m_OwnerID}";
        Log.Info($"E2ClientColliderSync.Initialized OwnerID:{m_OwnerID} LocalPlayerID:{Room.LocalPlayerId} {m_EntityType}");
        if (m_isOwner)
        {
            // Stop Reactor from updating the game object transform so it can be controlled by the client.
            Entity.ApplyTransformUpdates = false;
            Entity.PredictionEnabled = false;
            enabled = true;

            var playerActor = ActorManager.Instance.GetActor(m_OwnerID);
            if (null != playerActor)
            {
                var plAgent = playerActor as PlayerAgent;
                if (null != plAgent)
                {
                    plAgent.BindColliderSync(this);
                }
            }
        }
        else
        {
            Entity.ApplyTransformUpdates = true;
            Entity.PredictionEnabled = true;
            enabled = false;
            
        }
    }
    
    // Called when the script is detached.
    public override void Detached()
    {
        
    }
    
    // Called every frame.
    private void Update()
    {
        
    }
}