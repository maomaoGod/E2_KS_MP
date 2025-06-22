using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using KS.Reactor.Server;
using KS.Reactor;

public class E2ServerColliderAuthority : ksServerEntityScript
{
    public uint Owner
    {
        get { return m_OwnerId; }
        set
        {
            m_OwnerId = value;
            Properties[Consts.Prop.OWNER] = value;
            Properties[Consts.Prop.ENTITYTYPE] = (int)Consts.EntityType.E_Entity_Collider;
        }
    }
    
    private uint m_OwnerId;
    private ksCollider m_Collider;
    
    // Called when the script is attached, before other scripts on all entities are attached.
    public override void Attached()
    {
        m_Collider = Entity.Scripts.GetAll<ksCollider>()[0];
    }
    
    // Called after all other scripts on all entities are attached.
    public override void Initialize()
    {
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
        
    }
    
    [ksRPC(Consts.RPC.TRANSFORM_COLLIDER)]
    private void SetTransformCollider(ksIServerPlayer player, uint entityId,  uint ownerID, ksVector3 position, ksQuaternion rotation, ksVector3 scale)
    {
        ksLog.Info($"E2ServerPlayerAuthority.SetTransformCollider {player.Id} {entityId} {ownerID} {position}");
        if (entityId == Entity.Id && player.Id == m_OwnerId)
        {
            m_Collider.Transform.Position  = position;
            m_Collider.Transform.Rotation  = rotation;
            m_Collider.Transform.Scale = scale;
        }
    }

}