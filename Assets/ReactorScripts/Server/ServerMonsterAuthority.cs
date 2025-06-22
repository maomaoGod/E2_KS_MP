using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class ServerMonsterAuthority : ksServerEntityScript
{
    public ksIServerPlayer Owner
    {
        get { return m_owner; }
        private set
        {
            m_owner = value;
        }
    }
    private ksIServerPlayer m_owner;

    public void SetOwner(ksIServerPlayer owner)
    {
        m_owner = owner;
        Properties[Consts.Prop.OWNER] = owner == null ? uint.MaxValue : owner.Id;

        if (owner != null)
        {
            // Destroy the entity when the owner disconnects.
            owner.OnLeave += Entity.Destroy;
        }
    }
    
    [ksRPC(Consts.RPC.MONSTER_HEALTH_CHANGE)]
    public void OnHealthChanged(float currentHealth)
    {
        ksLog.Info($"{Entity.Id} monster: {currentHealth}");
        Properties[Consts.Prop.MONSTER_HEALTH] = currentHealth;
        Entity.CallRPC(Consts.RPC.MONSTER_HEALTH_CHANGE, currentHealth);
    }
    
    // [ksRPC(Consts.RPC.REMOVE_ENTITY_REQUEST)]
    // public void OnMonsterDead()
    // {
    //     ksLog.Info($"{Entity.Id} monster is dead");
    //     Entity.Destroy();
    // }

    public void RagdollStart()
    {
        Entity.CallRPC(Consts.RPC.MONSTER_RAGDOLL_START);
    }
}