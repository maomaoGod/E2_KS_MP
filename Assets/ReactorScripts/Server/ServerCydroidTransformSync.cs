using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class ServerCydroidTransformSync : ksServerEntityScript
{
    private ServerCydroidAuthority _clientAuthority;

    public override void Initialize()
    {
        _clientAuthority = Entity.Scripts.Get<ServerCydroidAuthority>();
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