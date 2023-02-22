using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror.Discovery;
using UnityEngine.Events;
using System.Net;
using Mirror;
using System;

namespace JetNetwork
{

    public struct RoomInfo
    {
        public int roomMaxPlayers;
        public int roomPlayersCount;
        public string roomName;
        public string modelName;
        public bool gamePlaying;
    }

    public struct ServerRoomInfoResponse: NetworkMessage
    {
        // 服务器回应消息
        public IPEndPoint EndPoint { get; set; }

        public Uri uri;

        // 当可以通过多个NIC上的LAN进行连接时，防止服务器出现重复
        public long serverId;

        //房间内信息
        public RoomInfo roomInfo;
    }

    [Serializable]
    public class ServerFoundUnityEvent : UnityEvent<ServerRoomInfoResponse> { };

    public class NetworkRoomInfoDiscovery : NetworkDiscoveryBase<ServerRequest/*客户端请求服务器回应消息*/, ServerRoomInfoResponse/*带有房间信息的服务器应答*/>
    {
        #region Server
        public long ServerId { get; private set; }

        [Tooltip("Transport to be advertised during discovery")]
        public Transport transport;

        [Tooltip("Invoked when a server is found")]
        public ServerFoundUnityEvent OnServerFound;

        public override void Start()
        {
            ServerId = RandomLong();

            // active transport gets initialized in awake
            // so make sure we set it here in Start()  (after awakes)
            // Or just let the user assign it in the inspector
            if (transport == null)
                transport = Transport.activeTransport;

            base.Start();
        }

        protected override ServerRoomInfoResponse ProcessRequest(ServerRequest request, IPEndPoint endpoint)
        {
            try
            {
                NetworkPlayingRoomManager roomManager= NetworkPlayingRoomManager.singleton.GetComponent<NetworkPlayingRoomManager>();
                RoomInfo roomInfoTemp = new RoomInfo(); 
                roomInfoTemp.roomMaxPlayers = roomManager.maxPlayers;
                roomInfoTemp.roomPlayersCount = roomManager.roomSlots.Count;
                roomInfoTemp.gamePlaying = roomManager.roomGamePlaying;
                roomInfoTemp.roomName = roomManager.RoomName;
                roomInfoTemp.modelName = NetworkPlayingRoomGameModel.singleton.GameModelName; ;
                // this is an example reply message,  return your own
                // to include whatever is relevant for your game
                return new ServerRoomInfoResponse
                {
                    serverId = ServerId,
                    uri = transport.ServerUri(),
                    //补充房间信息
                    roomInfo = roomInfoTemp
                };
            }
            catch (NotImplementedException)
            {
                Debug.LogError($"Transport {transport} does not support network discovery");
                throw;
            }
        }
        #endregion

        #region Client

        protected override ServerRequest GetRequest() => new ServerRequest();

        protected override void ProcessResponse(ServerRoomInfoResponse response, IPEndPoint endpoint)
        {
            response.EndPoint = endpoint;
            UriBuilder realUri = new UriBuilder(response.uri)
            {
                Host = response.EndPoint.Address.ToString()
            };
            response.uri = realUri.Uri;

            OnServerFound.Invoke(response);
        }

        #endregion
    }
}