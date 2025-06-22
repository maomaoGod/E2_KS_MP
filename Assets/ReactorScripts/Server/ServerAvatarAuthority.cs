using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class ServerAvatarAuthority : ksServerEntityScript
{
    public ksIServerPlayer Owner
    {
        get { return m_owner; }
        private set
        {
            m_owner = value;
        }
    }
    private ksIServerPlayer m_owner;
    public int avatar_index;
    ServerProjectileAuthority seProjectileAuthority;

    public void SetOwner(ksIServerPlayer owner)
    {
        m_owner = owner;
        Properties[Consts.Prop.OWNER] = owner == null ? uint.MaxValue : owner.Id;
        Properties[Consts.Prop.AVATAR_INDEX] = avatar_index;

        if (owner != null)
        {
            // Destroy the entity when the owner disconnects.
            owner.OnLeave += Entity.Destroy;
        }

        InitializeVariables();
    }

    void InitializeVariables()
    {
        Properties[Consts.Prop.HOLSTER_WEAPON1] = -1;
        Properties[Consts.Prop.HOLSTER_WEAPON2] = -1;
        Properties[Consts.Prop.HOLSTER_WEAPON3] = -1;
        Properties[Consts.Prop.HOLSTER_WEAPON4] = -1;
        Properties[Consts.Prop.EQUIP_WEAPON] = -1;
    }

    [ksRPC(Consts.RPC.SAVE_AVATAR_INFO)]
    public void SetAvatarInfo(ksIServerPlayer player, int index)
    {
        //Properties[Consts.Prop.AVATAR_ID] = index;
        ksLog.Info("Saving Avatar Info" + index);
    }

    #region Equip/Unequip Weapons
    // param holsterIndex
    // 0: left holster
    // 1: right holster
    // 2: back holster 1
    // 3: back holster 2
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

        Entity.CallRPC(Consts.RPC.SPAWN_WEAPON, weaponIndex, holsterIndex);
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

        Entity.CallRPC(Consts.RPC.EQUIP_WEAPON, previousHolsterIndex, weaponIndex);
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

        Entity.CallRPC(Consts.RPC.UNEQUIP_WEAPON, previousHolsterIndex, weaponIndex);
    }
    #endregion

    #region Monster mini-game events
    /// <summary>
    /// Announce that the monster mini game is started.
    /// </summary>
    /// <param name="show">
    /// If result is true, then the game modal is showed.
    /// If reuslt is false, then the game modal is hided.
    /// </param>
    public void StartMonsterMinigame(bool show)
    {
        Entity.CallRPC(Consts.RPC.MONSTER_MINIGAME_START, show);
    }

    /// <summary>
    /// Announce that the monster mini game is ended.
    /// </summary>
    /// <param name="result">
    /// If reuslt is true, then the game is win.
    /// If result is false, then the game is lose.
    /// </param>
    public void EndMonsterMinigame(bool result)
    {
        Entity.CallRPC(Consts.RPC.MONSTER_MINIGAME_END, result);
    }

    public void NotifyMonsterMinigameWave(int waveIndex)
    {
        Entity.CallRPC(Consts.RPC.MONSTER_MINIGAME_WAVE, waveIndex);
    }
    #endregion

    #region Player Magical Attack VFX
    [ksRPC(Consts.RPC.SKY_FALL)]
    private void SkyFall()
    {
        Entity.CallRPC(Consts.RPC.SKY_FALL);
    }

    [ksRPC(Consts.RPC.SKY_FALL_CHARGE)]
    private void SkyFallCharge()
    {
        Entity.CallRPC(Consts.RPC.SKY_FALL_CHARGE);
    }

    [ksRPC(Consts.RPC.SKY_FALL_CHARGE_END)]
    private void SkyFallChargeEnd()
    {
        Entity.CallRPC(Consts.RPC.SKY_FALL_CHARGE_END);
    }

    [ksRPC(Consts.RPC.DASH_STRICK_HOLD)]
    private void DashStrickHold()
    {
        Entity.CallRPC(Consts.RPC.DASH_STRICK_HOLD);
    }

    [ksRPC(Consts.RPC.DASH_CHARGE)]
    private void DashCharge()
    {
        Entity.CallRPC(Consts.RPC.DASH_CHARGE);
    }

    [ksRPC(Consts.RPC.DASH_CHARGE_END)]
    private void DashChargeEnd()
    {
        Entity.CallRPC(Consts.RPC.DASH_CHARGE_END);
    }

    [ksRPC(Consts.RPC.DASH_ATTACK)]
    private void DashAttack()
    {
        Entity.CallRPC(Consts.RPC.DASH_ATTACK);
    }

    [ksRPC(Consts.RPC.DASH_ATTACK_END)]
    private void DashAttackEnd()
    {
        Entity.CallRPC(Consts.RPC.DASH_ATTACK_END);
    }

    [ksRPC(Consts.RPC.CHARGE_STAMINA)]
    private void ChargeStamina()
    {
        ksLog.Info("charge stamina in server side");
        Entity.CallRPC(Consts.RPC.CHARGE_STAMINA);
    }

    [ksRPC(Consts.RPC.CHARGE_STAMINA_END)]
    private void ChargeStaminaEnd()
    {
        Entity.CallRPC(Consts.RPC.CHARGE_STAMINA_END);
    }

    [ksRPC(Consts.RPC.COMBO_STRICK)]
    private void ComboStrick(int index)
    {
        Entity.CallRPC(Consts.RPC.COMBO_STRICK, index);
    }

    [ksRPC(Consts.RPC.BLIZZARD_START)]
    private void BlizzardStart(ksVector3 vfxPos)
    {
        Entity.CallRPC(Consts.RPC.BLIZZARD_START, vfxPos);
    }

    [ksRPC(Consts.RPC.BLIZZARD_END)]
    private void BlizzardEnd()
    {
        Entity.CallRPC(Consts.RPC.BLIZZARD_END);
    }

    [ksRPC(Consts.RPC.PYROBLAST_CHARGE_START)]
    private void PyroblastChargeStart()
    {
        Entity.CallRPC(Consts.RPC.PYROBLAST_CHARGE_START);
    }

    [ksRPC(Consts.RPC.PYROBLAST_CHARGE_END)]
    private void PyroblastChargeEnd()
    {
        Entity.CallRPC(Consts.RPC.PYROBLAST_CHARGE_END);
    }

    [ksRPC(Consts.RPC.LIGHTNING_START)]
    private void LightningStart(ksVector3 vfxPos)
    {
        Entity.CallRPC(Consts.RPC.LIGHTNING_START, vfxPos);
    }

    [ksRPC(Consts.RPC.LIGHTNING_END)]
    private void LightningEnd()
    {
        Entity.CallRPC(Consts.RPC.LIGHTNING_END);
    }

    //[ksRPC(Consts.RPC.COMBO)]
    //public void Combo()
    //{
    //    Entity.CallRPC(Consts.RPC.COMBO);
    //}

    #endregion

    #region Magic projectile
    [ksRPC(Consts.RPC.MAGIC_PROJECTILE_SPAWN)]
    private void MagicProjectileSpawn(ksIServerPlayer player, string entityName, ksVector3 entityPos)
    {
        seProjectileAuthority = Room.SpawnEntity(entityName, entityPos).Scripts.Get<ServerProjectileAuthority>();
        seProjectileAuthority.SetOwner(player);
    }

    [ksRPC(Consts.RPC.MAGIC_PROJECTILE_FIRE)]
    private void MagicProjectileFire()
    {
        seProjectileAuthority.FireProjectile();
    }
    #endregion

    #region Sword Magical Attack VFX
    [ksRPC(Consts.RPC.DASH_STRICK)]
    private void DashStrick()
    {
        Entity.CallRPC(Consts.RPC.DASH_STRICK);
    }

    [ksRPC(Consts.RPC.DASH_STRICK_END)]
    private void DashStrickEnd()
    {
        Entity.CallRPC(Consts.RPC.DASH_STRICK_END);
    }

    private void ComboStrickEnd()
    {
        Entity.CallRPC(Consts.RPC.COMBO_STRICK_END);
    }

    private void SkyfallStrick()
    {
        Entity.CallRPC(Consts.RPC.SKYFALL_STRICK);
    }

    private void SkyfallStrickEnd()
    {
        Entity.CallRPC(Consts.RPC.SKYFALL_STRICK_END);
    }
    #endregion

    [ksRPC(Consts.RPC.HEALTH_CHANGE)]
    private void ChangeHealth(float health)
    {
        Properties[Consts.Prop.HEALTH] = health;
        Entity.CallRPC(Consts.RPC.HEALTH_CHANGE, health);
        ksLog.Info($"change health {Entity.Id} - {Properties[Consts.Prop.HEALTH]}");
    }

    [ksRPC(Consts.RPC.WEAPON_SOUND)]
    void OnPlaySound(ksIServerPlayer player, int soundID)
    {
        Entity.CallRPC(Consts.RPC.WEAPON_SOUND, soundID);
    }
}