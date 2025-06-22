using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class ServerEtherTransformSync : ksServerEntityScript
{
    private ServerEtherAuthority _clientAuthority;

    public override void Initialize()
    {
        _clientAuthority = Entity.Scripts.Get<ServerEtherAuthority>();
    }


    [ksRPC(Consts.RPC.TRANSFORM)]
    private void SetTransform(ksIServerPlayer player, ksVector3 position, ksQuaternion rotation)
    {
        Transform.Position = position;
        Transform.Rotation = rotation;
    }
}