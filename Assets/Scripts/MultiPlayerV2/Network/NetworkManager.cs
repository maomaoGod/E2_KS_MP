
using KS.Reactor;
using KS.Reactor.Client;
using KS.Reactor.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace E2MultiPlayer
{
    //Step1: Connect Done
    //Step2: CreatePlayer Done
    //Step3: SetupResource Done
    //Step4: ProcessInput Done
    //Step5: CreateNPC
    //Step6: NPCExecute Stratigity
    //Step7: Test 3p player
    //Step8: Animation Sync
    //Step9: Build and Publish
    //Step10: Test Published 
    public class NetworkManager:TSingleton<NetworkManager>
    {
        private ksBaseRoom.ConnectStatus m_ConnectResult = ksBaseRoom.ConnectStatus.UNKNOWN_ERROR;
        public ksBaseRoom.ConnectStatus ConnectResult => m_ConnectResult;
        private ksRoomType m_RoomType;
        private ksConnect m_Connect = null;
        private ksRoom m_ConnectRoom = null;
        public ksRoom ConnectRoom => m_ConnectRoom;
        public void BeginConnect()
        {
            if (null == m_Connect)
            {
                m_Connect = GameObject.FindObjectOfType<ksConnect>();
            }

            if (null == m_Connect)
            {
                Log.Error($"NetworkManager.BeginConnect Can't find {nameof(ksConnect)}");
                return;
            }

            if (null == m_RoomType)
            {
                m_RoomType = GameObject.FindObjectOfType<ksRoomType>();
            }

            if (null == m_RoomType)
            {
                Log.Error($"NetworkManager.BeginConnect Can't find {nameof(ksRoomType)}");
                return;
            }
            
            
            Log.Info($"NetworkManager.BeginConnect {m_RoomType}");
            //正式的online 模式
            if (m_Connect.ConnectMode == ksConnect.ConnectModes.LOCAL)
            {
                ksRoomInfo roomInfo =  m_RoomType.GetRoomInfo();
                roomInfo.Host = "localhost";
                roomInfo.Port = 8000;
                HandleGetRooms(new List<ksRoomInfo> { roomInfo }, null);
            }
            else
            {
                ksReactor.GetServers(HandleGetRooms);    
            }
            
        }
        
        private void HandleGetRooms(List<ksRoomInfo> rooms, string error)
        {
            Log.Info($"NetworkManager.HandleGetRooms {rooms.Count} {error}");
            
            Connect(rooms[0]);
        }
        
        private void Connect(ksRoomInfo roomInfo, params ksMultiType[] connectParams)
        {
            Debug.LogWarning($"NetworkManager.Connect {roomInfo} ");
            if (m_ConnectRoom != null)
            {
                return;
            }

            m_ConnectRoom = new ksRoom(roomInfo);
            m_ConnectRoom.OnConnect += HandleConnect;
            m_ConnectRoom.OnDisconnect += HandleDisconnect;

            switch (m_Connect.ConnectProtocol)
            {
                case ksConnect.ConnectProtocols.RUDP:
                    m_ConnectRoom.Protocol =  ksConnectionProtocols.RUDP;
                    break;
                case ksConnect.ConnectProtocols.TCP:
                    m_ConnectRoom.Protocol = ksConnectionProtocols.TCP;
                    break;
            }

            var kt = m_ConnectRoom.GetTimeAdjuster<ksTimeKeeper>();
            kt.TargetLatency = 1 / 60.0f;
            
            m_ConnectRoom.Connect(connectParams);
        }

        /// <summary> Disconnect from a connected room.</summary>
        public void Disconnect(bool immediate)
        {
            if (m_ConnectRoom != null)
            {
                m_ConnectRoom.Disconnect(immediate);
            }
        }
        
        private void HandleConnect(ksBaseRoom.ConnectStatus status, ksAuthenticationResult customStatus)
        {
            m_ConnectResult = status;
            if (status == ksBaseRoom.ConnectStatus.SUCCESS)
            {
                
                Log.Info($"NetworkManager.HandleConnect Connected to room {m_ConnectRoom.Info.Host}:{m_ConnectRoom.Info.Port}.");
            }
            else
            {
                Log.Warning($"NetworkManager.HandleConnect  Unable to connect to room {m_ConnectRoom.Info.Host}:{m_ConnectRoom.Info.Port}. Status = {status} ({customStatus})");
                m_ConnectRoom.CleanUp();
                m_ConnectRoom = null;
            }
        }

        /// <summary>Handle room disconnect events.</summary>
        /// <param name=""status"">Disconnect reason.</param>
        private void HandleDisconnect(ksBaseRoom.ConnectStatus status)
        {
            ksLog.Info(this, $"Disconnected from room {m_ConnectRoom.Info.Host}:{m_ConnectRoom.Info.Port}. Status = {status}");
            m_ConnectRoom.CleanUp();
            m_ConnectRoom = null;
        }

    }

}

