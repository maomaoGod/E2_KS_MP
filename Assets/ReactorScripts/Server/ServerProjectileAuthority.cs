using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class ServerProjectileAuthority : ksServerEntityScript
{
    private ServerProjectileAuthority _clientAuthority;
    
    float time;
    public ksIServerPlayer Owner
    {
        get { return m_owner; }
        private set
        {
            m_owner = value;
        }
    }
    private ksIServerPlayer m_owner;

    bool isFired;

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


    // Called after all other scripts on all entities are attached.
    public override void Initialize()
    {
        _clientAuthority = Entity.Scripts.Get<ServerProjectileAuthority>();

        Room.OnUpdate[0] += Update;
        time = 0;
        isFired = false;
    }

    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
    }

    public void FireProjectile()
    {
        isFired = true;
    }

    // Called during the update cycle
    private void Update()
    {
        if (isFired)
        {
            time += Time.Delta;

            if (time > 5) Entity.Destroy();
        }
    }
    
    [ksRPC(Consts.RPC.REMOVE_ENTITY_REQUEST)]
    private void EntityDestroy()
    {
        Entity.Destroy();
    }
    
    [ksRPC(Consts.RPC.CYDROID_TRANSFORM)]
    private void SetTransform(ksIServerPlayer player, ksVector3 position, ksQuaternion rotation)
    {
        if (player == _clientAuthority.Owner)
        {
            Transform.Position = position;
            Transform.Rotation = rotation;
        }
    }
}