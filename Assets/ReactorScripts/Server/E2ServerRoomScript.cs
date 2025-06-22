using System;
using System.Collections.Generic;
using System.Collections;
using KS.Reactor.Server;
using KS.Reactor;

public class E2ServerRoomScript : ksServerRoomScript
{
    private Dictionary<ksIServerPlayer,ksIServerEntity> m_CachedPlayers = new Dictionary<ksIServerPlayer, ksIServerEntity>();
    private List<ksIServerPlayer> m_ListPlayers = new List<ksIServerPlayer>();
    private Dictionary<ksIServerPlayer,ksIServerEntity> m_Player2Follower = new  Dictionary<ksIServerPlayer,ksIServerEntity>();

    public Dictionary<ksIServerPlayer, ksIServerEntity> Player2Follower
    {
        get
        {
            return m_Player2Follower;
        }
    }
    public Dictionary<ksIServerPlayer, ksIServerEntity> CachedPlayers
    {
        get
        {
            return m_CachedPlayers;
        }
    }

    public List<ksIServerPlayer> ListPlayers
    {
        get
        {
            return m_ListPlayers;
        }
    }
    
    private static E2ServerRoomScript m_Instance;

    public static E2ServerRoomScript Instance
    {
        get
        {
            return m_Instance;
        }
    }


    private uint m_RoomPlayerId = 1000000;

    public override void Attached()
    {
        base.Attached();
        m_Instance = this;
    }

    // Called after all other scripts on all entities are attached.
    public override void Initialize()
    {
        Room.OnUpdate[0] += Update;
        Room.OnPlayerJoin += PlayerJoin;
        Room.OnPlayerLeave += PlayerLeave;
        m_RoomPlayerId = 1000000;
    }
    
    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnUpdate[0] -= Update;
        Room.OnPlayerJoin -= PlayerJoin;
        Room.OnPlayerLeave -= PlayerLeave;
        m_Instance = null;
    }

    private uint AllocNPCID()
    {
        var res = m_RoomPlayerId;
        m_RoomPlayerId++;
        return res;
    }
    
    // Authenticates a player. Return non-zero to fail authentication.
    public override uint Authenticate(ksIServerPlayer player, ksMultiType[] args)
    {
        return 0;
    }
    
    // Called during the update cycle
    private void Update()
    {
        
    }

    private void InitPlayerCollider(string colliderName,ksIServerPlayer serverPlayer)
    {
        var playerincCol = Room.SpawnEntity(colliderName);
        E2ServerColliderAuthority serverCollider = playerincCol.Scripts.Get<E2ServerColliderAuthority>();
        if (serverCollider == null)
        {
            serverCollider = new E2ServerColliderAuthority();
            playerincCol.Scripts.Attach(serverCollider);
        }
        serverCollider.Owner = serverPlayer.Id;
    }
    
    // Called when a player connects.
    private void PlayerJoin(ksIServerPlayer player)
    {
        ksLog.Info($"E2ServerRoomScript.PlayerJoin {player.Id} {player.IsVirtual}");
        //Generate player entity
        ksIServerEntity playerEntiry = Room.SpawnEntity("PlayerSpawner");
        //Generate npc entity
        //ksIServerEntity npcEntity = Room.SpawnEntity("");
            
        m_CachedPlayers.Add(player, playerEntiry);
        // owner is remote client
        E2ServerPlayerAuthority serverAuthority = playerEntiry.Scripts.Get<E2ServerPlayerAuthority>();
        if (serverAuthority == null)
        {
            serverAuthority = new E2ServerPlayerAuthority();
            playerEntiry.Scripts.Attach(serverAuthority);
        }
        serverAuthority.Owner = player.Id;
        
        InitPlayerCollider("Player_Interactions_Collider",player);
        InitPlayerCollider("Player_LHand_Collider",player);
        InitPlayerCollider("Player_RHand_Collider",player);
        
        
         ksIServerEntity playerFollow = Room.SpawnEntity("PlayerSpawner");
         // owner is remote client
         var followAuthority = playerFollow.Scripts.Get<E2ServerPlayerAuthority>();
         if (followAuthority == null)
         {
             followAuthority = new E2ServerPlayerAuthority();
             playerFollow.Scripts.Attach(followAuthority);
         }
        
         followAuthority.Owner = 1000000; AllocNPCID();
         followAuthority.Properties[Consts.Prop.ENTITYTYPE] = (int)Consts.EntityType.E_Entity_FollowPlayer;
        
        m_Player2Follower[player] =  playerFollow;
        
        // owner is server
        // ksIServerEntity followNPC = Room.SpawnEntity("NPCSpawner");
        // E2ServerNPCAuthority npcAuthority = followNPC.Scripts.Get<E2ServerNPCAuthority>();
        // if (npcAuthority == null)
        // {
        //     npcAuthority = new E2ServerNPCAuthority();
        //     followNPC.Scripts.Attach(npcAuthority);
        // }
        // npcAuthority.Owner = AllocNPCID();
        // npcAuthority.Properties[Consts.Prop.BUDDYPLAYER] =  player.Id;
        
        
        m_CachedPlayers[player] = playerEntiry;
        m_ListPlayers.Add(player);
    }
    
    // Called when a player disconnects.
    private void PlayerLeave(ksIServerPlayer player)
    {
        ksLog.Info($"E2ServerRoomScript.PlayerLeave {player.Id} {player.IsVirtual}");
        if (m_CachedPlayers.TryGetValue(player, out var playerEntity))
        {
            playerEntity.Destroy();
            m_CachedPlayers.Remove(player);
        }

        if (m_Player2Follower.TryGetValue(player, out var followEntity))
        {
            followEntity.Destroy();
            m_Player2Follower.Remove(player);
        }
        
        m_ListPlayers.Remove(player);
    }
}