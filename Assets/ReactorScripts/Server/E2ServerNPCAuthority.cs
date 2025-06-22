using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class E2ServerNPCAuthority : ksServerEntityScript
{
    private static Random s_Random = new Random();
    // Should the entity be destroyed when the owner disconnects?
    [ksEditable]
    public bool DestroyOnOwnerDisconnect = true;

    private static uint m_BulletBaseId = 10000000;
    private Dictionary<uint,E2ServerBulletAuthority> m_ControlledBullets = new Dictionary<uint,E2ServerBulletAuthority>();
    // Called during the update cycle
    private float m_fBulletTime = 5.0f;
    // The player who controls this entity.
    public uint Owner
    {
        get { return m_OwnerId; }
        set
        {
            m_OwnerId = value;
            Properties[Consts.Prop.OWNER] = value;
            Properties[Consts.Prop.ENTITYTYPE] = (int)Consts.EntityType.E_Entity_NPC;
            
        }
    }
    private uint m_OwnerId;
    private uint m_BulletNum = 0;
    private uint AllocBulletID()
    {
        var res = m_BulletBaseId;
        ++m_BulletBaseId;
        return res;
    }
    
    public override void Initialize()
    {
        Room.OnUpdate[0] += Update;
    }
    
    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
    }
    

    private void Update()
    {
        m_fBulletTime -= Room.Time.RealDelta;
        if (m_fBulletTime < 0.0f)
        {
            if (m_fBulletTime > -2.0f)
            {
                
            }
            else
            {
                if (E2ServerRoomScript.Instance.CachedPlayers.Count > 0)
                {
                    ksLog.Info("E2ServerNPCAuthority.Update() SpawnBullet");
                    
                    var et = Room.SpawnEntity("BulletSpawner",Transform.Position);
                    var bulletAuthority = et.Scripts.Get<E2ServerBulletAuthority>();
                    if (bulletAuthority == null)
                    {
                        bulletAuthority = new E2ServerBulletAuthority();
                        et.Scripts.Attach(bulletAuthority);
                    }
                    bulletAuthority.Owner = AllocBulletID();
                    bulletAuthority.Properties[Consts.Prop.BUDDYPLAYER] =  Owner;
                
                    m_ControlledBullets.Add(bulletAuthority.Owner,bulletAuthority);
                    
                    // select target
                    var players = E2ServerRoomScript.Instance.ListPlayers;
                    var rd = s_Random.Next(0, players.Count);
                    var td  = players[rd];
                    bulletAuthority.Target = td;
                    ++m_BulletNum;
                    var tdPos = E2ServerRoomScript.Instance.CachedPlayers[td].Transform.Position;
                    var curPos = Transform.Position;
                    var dist = (tdPos - curPos).Magnitude();
                    if (m_BulletNum %2 == 0)
                    {
                        bulletAuthority.Properties[Consts.Prop.BULLETTRACK] = (int)Consts.BulletTrack.E_Track_Linear;
                        
                        var plEntity = E2ServerRoomScript.Instance.CachedPlayers[td];
                        var plPos = plEntity.Transform.Position;
                        bulletAuthority.Transform.LookAt(plPos);
                        var tarPos = plEntity.Transform.Position;
                        tarPos.Y = 0.2f;
                        var dir = (tarPos - Transform.Position).Normalized();
                        bulletAuthority.LinearDir = dir;
                        
                    }
                    else
                    {
                        bulletAuthority.Properties[Consts.Prop.BULLETTRACK] = (int)Consts.BulletTrack.E_Track_Follow;

                    }

                    bulletAuthority.BulletDead += OnBulletDestroy;
                }
                m_fBulletTime = 5.0f;
            }
        }
    }

    private void OnBulletDestroy(E2ServerBulletAuthority bulletAuthority)
    {

        if (null == bulletAuthority)
        {
            return;
        }
        
        bulletAuthority.BulletDead -= OnBulletDestroy;
        bulletAuthority.Entity.Destroy();
    }
    
    [ksRPC(Consts.RPC.HEALTH_CHANGE)]
    private void SetHp(ksIServerPlayer player, uint ownerID, float hp)
    {
        if (ownerID  == m_OwnerId)
        {
            Properties[Consts.Prop.HP] = hp;
        }
    }
    
    [ksRPC(Consts.RPC.TRANSFORM)]
    private void SetTransform(ksIServerPlayer player, uint ownerID, ksVector3 position, ksQuaternion rotation)
    {
        if (ownerID == m_OwnerId)
        {
            Transform.Position = position;
            Transform.Rotation = rotation;
        }
    }

    [ksRPC(Consts.RPC.ANIMATION_STATE)]
    private void SetAnimationState(ksIServerPlayer player, uint ownerID,  uint index, ksMultiType value)
    {
        if (ownerID== m_OwnerId)
        {
            Properties[Consts.Prop.ANIMATION_STATES + index] = value;
        }
    }

    // [ksRPC(Const.RPC.ANIMATION_PARAMS)]
    // private void SetAnimationParameter(ksIServerPlayer player,uint[] changedFlags, ksMultiType[] values)
    // {
    //     // if (ownerID != m_OwnerId)
    //     // {
    //     //     return;
    //     // }
    //     int index = 0;
    //     for (int i = 0; i < changedFlags.Length; i++)
    //     {
    //         uint flags = changedFlags[i];
    //         for (int j = 0; j < 32; j++)
    //         {
    //             if ((flags & 1u << j) != 0)
    //             {
    //                 uint propertyNum = (uint)(i * 32 + j);
    //                 Properties[Const.Prop.ANIMATION_PARAMS + propertyNum] = values[index++];
    //                 if (index == values.Length)
    //                 {
    //                     return;
    //                 }
    //             }
    //         }
    //     }
    // }

    [ksRPC(Consts.RPC.ANIMATION_TRIGGER)]
    private void SetAnimationTrigger(ksIServerPlayer player, uint ownerID, int index)
    {
        if (ownerID == m_OwnerId)
        {
            Entity.CallRPC(Consts.RPC.ANIMATION_TRIGGER,ownerID,  index);
        }
    }

    private void OwnerDisconnected()
    {
        if (DestroyOnOwnerDisconnect && Entity != null)
        {
            Entity.Destroy();
        }
    }
}