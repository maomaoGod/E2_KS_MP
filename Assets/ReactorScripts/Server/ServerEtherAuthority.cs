using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class ServerEtherAuthority : ksServerEntityScript
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
            owner.OnLeave += Entity.Destroy;
        }
    }
}