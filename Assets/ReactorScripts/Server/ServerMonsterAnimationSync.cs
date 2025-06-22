using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class ServerMonsterAnimationSync : ksServerEntityScript
{
    private ServerMonsterAuthority _clientAuthority;

    public override void Initialize()
    {
        _clientAuthority = Entity.Scripts.Get<ServerMonsterAuthority>();
    }

    [ksRPC(Consts.RPC.ANIMATION_STATE)]
    private void SetAnimationState(ksIServerPlayer player, uint index, ksMultiType value)
    {
        if (player == _clientAuthority.Owner)
        {
            Properties[Consts.Prop.ANIMATION_STATES + index] = value;
        }
    }

    [ksRPC(Consts.RPC.ANIMATION_PARAM)]
    private void SetAnimationParameter(ksIServerPlayer player, uint[] changedFlags, ksMultiType[] values)
    {
        if (player != _clientAuthority.Owner)
        {
            return;
        }
        int index = 0;
        for (int i = 0; i < changedFlags.Length; i++)
        {
            uint flags = changedFlags[i];
            for (int j = 0; j < 32; j++)
            {
                if ((flags & 1u << j) != 0)
                {
                    uint propertyNum = (uint)(i * 32 + j);
                    Properties[Consts.Prop.ANIMATION_PARAMS + propertyNum] = values[index++];
                    if (index == values.Length)
                    {
                        return;
                    }
                }
            }
        }
    }

    [ksRPC(Consts.RPC.ANIMATION_TRIGGER)]
    private void SetAnimationTrigger(ksIServerPlayer player, int index)
    {
        if (player == _clientAuthority.Owner)
        {
            Entity.CallRPC(Consts.RPC.ANIMATION_TRIGGER, index);
        }
    }
}