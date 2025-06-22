using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class E2ServerPropAuthority : ksServerEntityScript
{
    // Called after all other scripts on all entities are attached.
    public override void Initialize()
    {
        Room.OnUpdate[0] += Update;
        Entity.Properties[Consts.Prop.HP] = 100;
    }
    
    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
    }
    
    // Called during the update cycle
    private void Update()
    {
        
    }
}