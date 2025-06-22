using System;
using System.Collections.Generic;
using KS.Reactor.Client.Unity;
using UnityEngine;

namespace E2MultiPlayer
{
    public class ActorManager:TSingleton<ActorManager>
    {
        private Dictionary<ulong, Actor> m_CachedActors = new();
        private Dictionary<ulong,Actor> m_CachedPlayers = new();
        public Dictionary<ulong,Actor> AllActors => m_CachedActors;
        public Dictionary<ulong,Actor> CachedPlayers => m_CachedPlayers;
        
        private uint m_LocalPlayerId = 0;
        public uint LocalPlayerId => m_LocalPlayerId;
        public Actor GetActor(ulong id)
        {
            Actor result = null;
            m_CachedActors.TryGetValue(id, out result);
            return result;
        }

        public void AddActor(ulong id, Actor ac)
        {
            if (!m_CachedActors.ContainsKey(id))
            {
                m_CachedActors.Add(id, ac);
            }
            else
            {
                
            }
        }
        
        
        public void RemoveActor(ulong id)
        {
            m_CachedActors.Remove(id);
        }

        public void RefreshMainPlayer(uint mainPlayerId)
        {
            Log.Info($"ActorManager.RefreshMainPlayer {mainPlayerId} ");
            m_LocalPlayerId = mainPlayerId;
            if (m_CachedActors.TryGetValue(mainPlayerId, out Actor result))
            {
                // if (null != result.Controller)
                // {
                //     CameraManager.Instance.TargetTransform = result.Controller.transform;    
                // }
            }
        }
        
        public  void Initialize()
        {
            m_LocalPlayerId = 0;
        }

        public void UnInitialize()
        {
            
        }

        public void OnUse()
        {
            
        }

        public void Update(float dtTime)
        {
            foreach (var actor in m_CachedActors.Values)
            {
                actor.Update(dtTime);
            }
        }

        public void FixedUpdate(float dtTime)
        {
            foreach (var actor in m_CachedActors.Values)
            {
                actor.FixedUpdate(dtTime);
            }
        }

        public void LateUpdate(float dtTime)
        {
            foreach (var actor in m_CachedActors.Values)
            {
                actor.LateUpdate(dtTime);
            }
        }

        public void OnPlayerCreated(uint playerId, GameObject player,GameObject syncTransform)
        {
            Log.Info($"ActorManager::OnPlayerCreated {playerId} {player}");
            if (m_CachedActors.TryGetValue(playerId, out Actor actor))
            {
                actor.Bind(player,syncTransform);
            }
            else
            {
                actor = new PlayerAgent();
                m_CachedActors.Add(playerId, actor);
                actor.Bind(player,syncTransform);
            }
            
            m_CachedPlayers[playerId] =  actor;
        }

        public void OnPlayerDestroyed(uint playerId, GameObject player)
        {
            Log.Info($"ActorManager::OnPlayerDestroyed {player}");
            if (m_CachedActors.TryGetValue(playerId, out Actor actor))
            {
                m_CachedActors.Remove(playerId);
            }

            if (m_CachedPlayers.TryGetValue(playerId, out Actor result))
            {
                m_CachedPlayers.Remove(playerId);
            }
        }

        
        public void OnNPCCreated(uint npcId, GameObject npc, GameObject syncTransform)
        {
            Log.Info($"ActorManager::OnNPCCreated {npcId} {npc}");
            if (m_CachedActors.TryGetValue(npcId, out Actor actor))
            {
                actor.Bind(npc,syncTransform);
            }
            else
            {
                actor = new AIAgent();
                m_CachedActors.Add(npcId, actor);
                actor.Bind(npc,syncTransform);
            }
        }

        private void OnNPCDestroyed(uint npcId)
        {
            
        }

        public void OnBulletCreated(uint bulletId, GameObject bullet, GameObject syncTransform)
        {
            Log.Info($"ActorManager::OnBulletCreated {bullet} {bullet}");
            if (m_CachedActors.TryGetValue(bulletId, out Actor actor))
            {
                actor.Bind(bullet,syncTransform);
            }
            else
            {
                actor = new BulletAgent();
                m_CachedActors.Add(bulletId, actor);
                actor.Bind(bullet,syncTransform);
            }
        }

        public void OnBulletDestroyed(uint bulletId, GameObject bullet, GameObject syncTransform)
        {
            
        }
        
       
        public void OnPlayerJoin(ksPlayer player)
        {
            if (null == player)
            {
                return;
            }
            
            var id = player.Id;
            if (!m_CachedActors.ContainsKey(id))
            {
                var actor = new PlayerAgent();
                m_CachedActors.Add(id, actor);
            }
        }

        public void OnPlayerLeave(ksPlayer player)
        {
            if (null == player)
            {
                return;
            }
            
            var id = player.Id;

            if (m_CachedActors.ContainsKey(id))
            {
                m_CachedActors.Remove(id);
            }
        }

        public void OnPlayerInput(ksPlayer player, PlayerInputData data)
        {
            if (null == player ||
                null == data)
            {
                return;
            }
            
            var id = player.Id;
            if (m_CachedActors.TryGetValue(id, out var actor))
            {
                actor.SyncPlayerInput(data);
            }
            
        }
    }
}