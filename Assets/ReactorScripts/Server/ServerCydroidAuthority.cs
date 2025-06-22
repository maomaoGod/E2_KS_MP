using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class ServerCydroidAuthority : ksServerEntityScript
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

    [ksRPC(Consts.RPC.CYDROID_INDEX_SAVE_REQUEST)]
    public void SetCydroidInfo(ksIServerPlayer player, int index)
    {
        Properties[Consts.Prop.CYDROID_INDEX] = index;
        ksLog.Info("Saving CYDROID Info" + index);
    }

    [ksRPC(Consts.RPC.CYDROID_INDEX_CHANGED)]
    public void ChangeCydroidInfo(ksIServerPlayer player, int index)
    {
        Properties[Consts.Prop.CYDROID_INDEX] = index;
        ksLog.Info("Changing CYDROID Info" + index);
    }
}