using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class ServerCarTransformSync : ksServerEntityScript
{
    private ServerCarAuthority _clientAuthority;

    public override void Initialize()
    {
        _clientAuthority = Entity.Scripts.Get<ServerCarAuthority>();
    }


    [ksRPC(Consts.RPC.CAR_TRANSFORM)]
    private void SetTransform(ksIServerPlayer player, ksVector3 position, ksQuaternion rotation)
    {
        if (player == _clientAuthority.Owner)
        {
            Transform.Position = position;
            Transform.Rotation = rotation;
        }
    }

    [ksRPC(Consts.RPC.CAR_FRONT_WHEEL_TRANSFORM)]
    private void SetFrontWheelRotation(ksIServerPlayer player, ksQuaternion rotation)
    {
        if (player == _clientAuthority.Owner)
        {
            Properties[Consts.Prop.FRONT_WHEEL] = rotation;
        }
    }

    [ksRPC(Consts.RPC.CAR_FRONT_WHEELY_TRANSFORM)]
    private void SetFrontWheelRotationY(ksIServerPlayer player, ksQuaternion rotation)
    {
        if (player == _clientAuthority.Owner)
        {
            Properties[Consts.Prop.FRONT_WHEELY] = rotation;
        }
    }

    [ksRPC(Consts.RPC.CAR_BACK_WHEEL_TRANSFORM)]
    private void SetBackWheelRotation(ksIServerPlayer player, ksQuaternion rotation)
    {
        if (player == _clientAuthority.Owner)
        {
            Properties[Consts.Prop.BACK_WHEEL] = rotation;
        }
    }
}