using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using KS.Reactor.Client.Unity;
using KS.Reactor.Client;
using KS.Reactor;
using E2MultiPlayer;

// KS RoomInfo
public class E2ClientRoomScript : ksRoomScript
{
    // Called when the script is attached, before other scripts/entities are attached/spawned.
    public override void Attached()
    {
            
    }
        
    // Called after all other scripts/entities are attached/spawned.
    public override void Initialize()
    {
        Room.OnPlayerJoin += PlayerJoin;
        Room.OnPlayerLeave += PlayerLeave;
            
    }
        
    // Called when the script is detached.
    public override void Detached()
    {
        Room.OnPlayerJoin -= PlayerJoin;
        Room.OnPlayerLeave -= PlayerLeave;
    }
        
    // Called every frame.
    private void Update()
    {
            
    }
        
    // Called when a player connects.
    // use room.localid to identify controlled player
    private void PlayerJoin(ksPlayer player)
    {
        Log.Info($"E2ClientRoomScript.PlayerJoin {player.Id} {Room.LocalPlayerId}");
        ActorManager.Instance.OnPlayerJoin(player);
        if (player.Id == Room.LocalPlayerId)
        {
            ActorManager.Instance.RefreshMainPlayer(player.Id);
        }
    }
        
    // Called when a player disconnects.
    private void PlayerLeave(ksPlayer player)
    {
        Log.Info($"E2ClientRoomScript.PlayerLeave {player.Id}");
        ActorManager.Instance.OnPlayerLeave(player);
    }
    
    [ksRPC(Consts.RPC.TRANSFORM_RESPONSE)]
    private void ResponseTransform(uint ownerID, ksVector3 position, ksQuaternion rotation,float timeStamp)
    {
        //Log.Info($"E2ClientRoomScript.ResponseTransform {ownerID} {position} {timeStamp}");
        var player = ActorManager.Instance.GetActor(1000000);
        if ( null != player)
        {
            var plAgent = player as PlayerAgent;
            if (null != plAgent)
            {
                var state = new StateNetcode();
                state.m_Position = position;
                state.m_Rotation = rotation;
                state.m_Scale = new Vector3(1f, 1f, 1f);
                state.m_OwnerTimestamp = timeStamp;
                state.m_ReceivedTimestamp = plAgent.ProxyMove.LocalTime;

                if (plAgent.ProxyMove.m_ReceivedStatesCounter < plAgent.ProxyMove.m_SendRate)
                {
                    plAgent.ProxyMove.m_ReceivedStatesCounter++;
                }
                
                plAgent.ProxyMove.AddState(state);
            }
            
            
        }
    }

    // collect player input 
    [ksRPC(Consts.RPC.PlayerInput)]
    public void PlayerInput(ksPlayer player,PlayerInputData data)
    {
        if (null == player)
        {
            return;
        }
            
        // locate id
        ActorManager.Instance.OnPlayerInput(player, data);
    }
}