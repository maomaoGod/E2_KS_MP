using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class E2ServerPlayerAuthority : ksServerEntityScript
{
    // Should the entity be destroyed when the owner disconnects?
    [ksEditable]
    public bool DestroyOnOwnerDisconnect = true;

    // The player who controls this entity.
    public uint Owner
    {
        get { return m_OwnerId; }
        set
        {
            m_OwnerId = value;
            Properties[Consts.Prop.OWNER] = value;
            Properties[Consts.Prop.ENTITYTYPE] = (int)Consts.EntityType.E_Entity_Player;
        }
    }
    
    private uint m_OwnerId;

    #region Weapon

     [ksRPC(Consts.RPC.SPAWN_WEAPON)]
    private void SpawnWeapon(int weaponIndex, int holsterIndex)
    {
        switch (holsterIndex)
        {
            case 0:
                Properties[Consts.Prop.HOLSTER_WEAPON1] = weaponIndex;
                break;
            case 1:
                Properties[Consts.Prop.HOLSTER_WEAPON2] = weaponIndex;
                break;
            case 2:
                Properties[Consts.Prop.HOLSTER_WEAPON3] = weaponIndex;
                break;
            case 3:
                Properties[Consts.Prop.HOLSTER_WEAPON4] = weaponIndex;
                break;
        }

        Entity.Room.CallRPC(Consts.RPC.SPAWN_WEAPON, m_OwnerId, weaponIndex, holsterIndex);
    }

    [ksRPC(Consts.RPC.EQUIP_WEAPON)]
    private void EquipWeapon(int previousHolsterIndex, int weaponIndex)
    {
        switch (previousHolsterIndex)
        {
            case 0:
                Properties[Consts.Prop.HOLSTER_WEAPON1] = -1;
                break;
            case 1:
                Properties[Consts.Prop.HOLSTER_WEAPON2] = -1;
                break;
            case 2:
                Properties[Consts.Prop.HOLSTER_WEAPON3] = -1;
                break;
            case 3:
                Properties[Consts.Prop.HOLSTER_WEAPON4] = -1;
                break;
        }
        Properties[Consts.Prop.EQUIP_WEAPON] = weaponIndex;

        Entity.Room.CallRPC(Consts.RPC.EQUIP_WEAPON, m_OwnerId, previousHolsterIndex, weaponIndex);
    }

    [ksRPC(Consts.RPC.UNEQUIP_WEAPON)]
    private void UnequipWeapon(int previousHolsterIndex, int weaponIndex)
    {
        switch (previousHolsterIndex)
        {
            case 0:
                Properties[Consts.Prop.HOLSTER_WEAPON1] = weaponIndex;
                break;
            case 1:
                Properties[Consts.Prop.HOLSTER_WEAPON2] = weaponIndex;
                break;
            case 2:
                Properties[Consts.Prop.HOLSTER_WEAPON3] = weaponIndex;
                break;
            case 3:
                Properties[Consts.Prop.HOLSTER_WEAPON4] = weaponIndex;
                break;
        }
        Properties[Consts.Prop.EQUIP_WEAPON] = -1;

        Entity.Room.CallRPC(Consts.RPC.UNEQUIP_WEAPON, m_OwnerId, previousHolsterIndex, weaponIndex);
    }

    #endregion
    
    [ksRPC(Consts.RPC.HEALTH_CHANGE)]
    private void SetHp(ksIServerPlayer player, uint ownerID, float hp)
    {
        if (player.Id == m_OwnerId)
        {
            Properties[Consts.Prop.HP] = hp;
        }
    }
    
    [ksRPC(Consts.RPC.TRANSFORM)]
    private void SetTransform(ksIServerPlayer player, uint ownerID, ksVector3 position, ksQuaternion rotation,float timeStamp)
    {
        //ksLog.Info($"E2ServerPlayerAuthority.SetTransfrom {player.Id} {position}");
        if (E2ServerRoomScript.Instance.Player2Follower.TryGetValue(player, out var et))
        {
            if (null != et)
            {
                et.Transform.Position = position + new ksVector3(0.0F, 2.2F, 0.0F);
                et.Transform.Rotation = rotation;
                var svr = et.Scripts.Get<E2ServerPlayerAuthority>();
                //ksLog.Info($"E2ServerPlayerAuthority.SetTransfrom {svr.Owner} {et.Transform.Position}");
                Entity.Room.CallRPC(Consts.RPC.TRANSFORM_RESPONSE, ownerID,et.Transform.Position, rotation, timeStamp);
            }
        }
        
        if (player.Id == m_OwnerId)
        {
            Transform.Position = position;
            Transform.Rotation = rotation;
        }
    }

   
    
    [ksRPC(Consts.RPC.ANIMATION_STATE)]
    private void SetAnimationState(ksIServerPlayer player, uint ownerID, uint index, ksMultiType value)
    {
        if (E2ServerRoomScript.Instance.Player2Follower.TryGetValue(player, out var et))
        {
            if (null != et)
            {
                et.Properties[Consts.Prop.ANIMATION_STATES + index] = value;
                
                //ksLog.Info($"E2ServerPlayerAuthority.SetTransfrom {et.Owner} {et.Transform.Position}");
            }
        }
        
        if (player.Id== m_OwnerId)
        {
            Properties[Consts.Prop.ANIMATION_STATES + index] = value;
        }
    }

    [ksRPC(Consts.RPC.ANIMATION_PARAM)]
    private void SetAnimationParameter(ksIServerPlayer player, ksMultiType[] values)
    {
        if (player.Id != m_OwnerId)
        {
            return;
        }
     
        if (E2ServerRoomScript.Instance.Player2Follower.TryGetValue(player, out var et))
        {
            if (null != et)
            {
                for (uint i = 0; i < values.Length; i++)
                {
                    ksMultiType multiType = values[i];
                    et.Properties[Consts.Prop.ANIMATION_PARAMS + i] = multiType;
                }
                
                //ksLog.Info($"E2ServerPlayerAuthority.SetTransfrom {et.Owner} {et.Transform.Position}");
            }
        }
        
        for (uint i = 0; i < values.Length; i++)
        {
            ksMultiType multiType = values[i];
            Properties[Consts.Prop.ANIMATION_PARAMS + i] = multiType;
        }
    }

    [ksRPC(Consts.RPC.ANIMATION_TRIGGER)]
    private void SetAnimationTrigger(ksIServerPlayer player, uint ownerID, int index)
    {
        if (player.Id == m_OwnerId)
        {
            Entity.CallRPC(Consts.RPC.ANIMATION_TRIGGER, ownerID, index);
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