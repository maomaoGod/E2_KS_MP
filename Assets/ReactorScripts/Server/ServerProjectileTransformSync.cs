using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class ServerProjectileTransformSync : ksServerEntityScript
{
    private ServerProjectileAuthority _clientAuthority;

    public override void Initialize()
    {
        _clientAuthority = Entity.Scripts.Get<ServerProjectileAuthority>();
    }

    [ksRPC(Consts.RPC.TRANSFORM)]
    private void SetTransform(ksIServerPlayer player, ksVector3 position, ksQuaternion rotation)
    {
        if (player == _clientAuthority.Owner)
        {
            Transform.Position = position;
            Transform.Rotation = rotation;
        }
    }
}