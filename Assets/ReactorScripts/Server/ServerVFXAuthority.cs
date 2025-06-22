using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class ServerVFXAuthority : ksServerEntityScript
{
    float localTime;
    // Called after all other scripts on all entities are attached.
    public override void Initialize()
    {
        localTime = 0;
        Room.OnUpdate[0] += Update;
    }
    
    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
    }
    
    // Called during the update cycle
    private void Update()
    {
        localTime += Time.RealDelta;

        if (localTime > 10) Entity.Destroy();
    }
}