using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;
using System.Runtime.InteropServices;

public class ServerPlayerHandler : ksServerRoomScript
{
    ServerAvatarAuthority _avatarClientAuthority;
    ServerCarAuthority _carClientAuthority;
    ServerCydroidAuthority _cydroidClientAuthority;
    List<ksIServerEntity> etherEntities;
    List<ksIServerEntity> monsterEntities;
    List<ServerAvatarAuthority> serverClientAuthoritys = new List<ServerAvatarAuthority>();
    int etherCnt = 30;

    public override void Initialize()
    {
        //Room.UpdateTimer.LogInterval = 5000;
        //Room.EncodeTimer.LogInterval = 5000;
        Room.OnPlayerLeave += PlayerLeave;
    }

    public override void Detached()
    {
        Room.OnPlayerLeave += PlayerLeave;
    }

    void PlayerLeave(ksIServerPlayer player)
    {
        for (int i = 0; i < serverClientAuthoritys.Count; i++)
        {
            if (serverClientAuthoritys[i].Owner == player)
            {
                serverClientAuthoritys.RemoveAt(i);
            }
        }
    }

    [ksRPC(Consts.RPC.AVATAR_SPAWN_REQUEST)]
    private void Spawn_Player_Avatar(ksIServerPlayer player, string avatar_prefab_name, ksVector3 spawnPos, int index)
    {
        ksLog.Info("request character spawning");
        _avatarClientAuthority = Room.SpawnEntity(avatar_prefab_name, spawnPos).Scripts.Get<ServerAvatarAuthority>();
        _avatarClientAuthority.avatar_index = index;
        _avatarClientAuthority.SetOwner(player);
        serverClientAuthoritys.Add(_avatarClientAuthority);
    }

    [ksRPC(Consts.RPC.CAR_SPAWN_REQUEST)]
    private void Spawn_Player_Car(ksIServerPlayer player, string car_prefab_name, ksVector3 spawnPos)
    {
        _carClientAuthority = Room.SpawnEntity(car_prefab_name, spawnPos).Scripts.Get<ServerCarAuthority>();
        _carClientAuthority.SetOwner(player);
    }

    [ksRPC(Consts.RPC.CYDROID_SPAWN_REQUEST)]
    private void Spawn_Player_Cydroid(ksIServerPlayer player, string cydroid_prefab_name, ksVector3 spawnPos)
    {
        _cydroidClientAuthority = Room.SpawnEntity(cydroid_prefab_name, spawnPos).Scripts.Get<ServerCydroidAuthority>();
        _cydroidClientAuthority.SetOwner(player);
    }

    [ksRPC(Consts.RPC.MENTAR_SPAWN_REQUEST)]
    private void Spawn_Mentar(ksIServerPlayer player, string mentarName, ksVector3 spawnPos)
    {
        ServerMentarAuthority serverMentarAuthority = Room.SpawnEntity(mentarName, spawnPos).Scripts.Get<ServerMentarAuthority>();
        serverMentarAuthority.SetOwner(player);
    }

    [ksRPC(Consts.RPC.REMOVE_ENTITY_REQUEST)]
    private void Remove_Player_Car(ksIServerPlayer player, ksMultiType[] args)
    {
        uint id = args[0];
        Room.GetEntity(id).Destroy();

        if (serverClientAuthoritys != null)
        for (int i = 0; i < serverClientAuthoritys.Count; i++)
        {
            var serverClientAuthority = serverClientAuthoritys[i];
            if (id == serverClientAuthority.Owner.Id)
            {
                serverClientAuthoritys.RemoveAt(i);
                ksLog.Info($"remove player avatar");
                return;
            }
        }

        if(monsterEntities != null)
        for (int i = 0; i < monsterEntities.Count; i++)
        {
            if (monsterEntities[i].Id == id)
            {
                monsterEntities.RemoveAt(i);
                ksLog.Info("remove monster");
                return;
            }
        }

        if(etherEntities != null)
        for (int i = 0; i < etherEntities.Count; i++)
        {
            if (etherEntities[i].Id == id)
            {
                etherEntities.RemoveAt(i);
                ksLog.Info("remove ether");
                return;
            }
        }
    }

    [ksRPC(Consts.RPC.ETHER_SPAWN_REQUEST)]
    private void Spawn_Ether(ksIServerPlayer player, ksMultiType[] args)
    {
        if(etherEntities == null) etherEntities = new List<ksIServerEntity>();

        string etherPrefab = args[0];
        ksVector3 spawnPos = args[1];

        var serverEtherAuthority = Room.SpawnEntity(etherPrefab, spawnPos);
        etherEntities.Add(serverEtherAuthority);
    }

    [ksRPC(Consts.RPC.ETHER_REMOVE_REQUEST)]
    private void Remove_Ether(ksIServerPlayer player, ksMultiType[] args)
    {
        for (int i = 0; i < etherEntities.Count; i++)
        {
            if (etherEntities[i] == null) continue;

            uint entityId = etherEntities[i].Id;
            Room.GetEntity(entityId).Destroy();
        }
        etherEntities.Clear();
    }

    [ksRPC(Consts.RPC.MONSTER_SPAWN_REQUEST)]
    private void Spawn_Monster(ksIServerPlayer player, ksMultiType[] args)
    {
        string monsterPrefab = args[0];
        ksVector3 spawnCenterPos = args[1];

        if (monsterEntities == null) monsterEntities = new List<ksIServerEntity>();

        ksLog.Info($"{monsterPrefab} is spawned");
        var serverMonsterAuthority = Room.SpawnEntity(monsterPrefab, spawnCenterPos);
        serverMonsterAuthority.Scripts.Get<ServerMonsterAuthority>().SetOwner(player);
        monsterEntities.Add(serverMonsterAuthority);
    }

    [ksRPC(Consts.RPC.MONSTER_HEALTH_CHANGE)]
    private void ChangeMonsterHealth(ksIServerPlayer player, ksMultiType[] args)
    {
        uint monsterId = args[0];
        float monsterHealth = args[1];

        Room.GetEntity(monsterId).Properties[Consts.Prop.MONSTER_HEALTH] = monsterHealth;
        ksLog.Info($"set {monsterId} monster health {monsterHealth}");
        /*
        for(int j = 0; j <  monsterEntities.Count; j++)
        {
            if (monsterEntities[j].Id == monsterId)
            {
                monsterEntities[j].Properties[Consts.Prop.MONSTER_HEALTH] = monsterHealth;
                ksLog.Info($"set {monsterId} monster health {monsterHealth}");
            }
        }
        */
    }

    //[ksRPC(Consts.RPC.MONSTER_RECEIVE_DAMAGE)]
    //private void OnReceiveDamage(ksIServerPlayer player, ksMultiType[] args)
    //{
    //    uint monsterId = args[0];
    //    float damageValue = args[1];

    //    Room.GetEntity(monsterId).Scripts.Get<ServerMonsterAuthority>().DamageReceived(damageValue);
    //    /*
    //    for (int j = 0; j < monsterEntities.Count; j++)
    //    {
    //        if (monsterEntities[j].Id == monsterId)
    //        {
    //            monsterEntities[j].Scripts.Get<ServerMonsterAuthority>().DamageReceived(damageValue);
    //            ksLog.Info($"{monsterId} monster receive Damage {damageValue}");
    //        }
    //    }
    //    */
    //}

    [ksRPC(Consts.RPC.MONSTER_ALL_REMOVE)]
    private void RemoveAllMonster(ksIServerPlayer player)
    {
        for (int j = 0; j < monsterEntities.Count; j++)
        {
            if (monsterEntities[j] == null) continue;

            uint entityId = monsterEntities[j].Id;
            Room.GetEntity(entityId).Destroy();
        }
        monsterEntities.Clear();
    }

    #region Monster mini-game events
    [ksRPC(Consts.RPC.MONSTER_MINIGAME_START)]
    private void OnMonsterMinigameStart(ksIServerPlayer player, bool show)
    {
        ksLog.Info($"game start {show}");
        for (int i = 0; i < Room.Entities.Count; i++)
        {
            ServerAvatarAuthority serverAvatarAuthority = Room.Entities[i].Scripts.Get<ServerAvatarAuthority>();
            if (serverAvatarAuthority == null) continue;
            serverAvatarAuthority.StartMonsterMinigame(show);
        }
    }

    [ksRPC(Consts.RPC.MONSTER_MINIGAME_END)]
    private void OnMonsterMinigameEnd(ksIServerPlayer player, bool result)
    {
        ksLog.Info($"game end {result}");
        for (int i = 0; i < Room.Entities.Count; i++)
        {
            ServerAvatarAuthority serverAvatarAuthority = Room.Entities[i].Scripts.Get<ServerAvatarAuthority>();
            if (serverAvatarAuthority == null) continue;
            serverAvatarAuthority.EndMonsterMinigame(result);
        }
    }

    [ksRPC(Consts.RPC.MONSTER_MINIGAME_WAVE)]
    private void OnMonsterMinigameWaveStart(ksIServerPlayer player, int waveIndex)
    {
        ksLog.Info($"game wave {waveIndex}");
        for (int i = 0; i < Room.Entities.Count; i++)
        {
            ServerAvatarAuthority serverAvatarAuthority = Room.Entities[i].Scripts.Get<ServerAvatarAuthority>();
            if (serverAvatarAuthority == null) continue;
            serverAvatarAuthority.NotifyMonsterMinigameWave(waveIndex);
        }
    }
    #endregion

    #region Mentar
    [ksRPC(Consts.RPC.MENTAR_HEALTH_CHANGE)]
    private void ChangeMentarHealth(ksIServerPlayer player, ksMultiType[] args)
    {
        uint mentarId = args[0];
        float mentarHealth = args[1];

        Room.GetEntity(mentarId).Properties[Consts.Prop.MONSTER_HEALTH] = mentarHealth;
    }

    [ksRPC(Consts.RPC.MENTAR_RECEIVE_DAMAGE)]
    private void OnMentarReceiveDamage(ksIServerPlayer player, uint mentarId, float damageValue)
    {
        ksLog.Info($"{mentarId} mentar receive Damage {damageValue}");
        Room.GetEntity(mentarId).Scripts.Get<ServerMentarAuthority>().DamageReceived(damageValue);
    }
    #endregion

    #region Ragdoll
    [ksRPC(Consts.RPC.MONSTER_RAGDOLL_START)]
    private void OnMonsterRagdollStart(ksIServerPlayer player)
    {
        for (int i = 0; i < Room.Entities.Count; i++)
        {
            ServerMonsterAuthority serverMonsterAuthority = Room.Entities[i].Scripts.Get<ServerMonsterAuthority>();
            if (serverMonsterAuthority == null || serverMonsterAuthority.Owner != player) continue;
            serverMonsterAuthority.RagdollStart();
        }
    }
    #endregion
}