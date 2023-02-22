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
        // ��������Ӧ��Ϣ
        public IPEndPoint EndPoint { get; set; }

        public Uri uri;

        // ������ͨ�����NIC�ϵ�LAN��������ʱ����ֹ�����������ظ�
        public long serverId;

        //��������Ϣ
        public RoomInfo roomInfo;
    }

    [Serializable]
    public class ServerFoundUnityEvent : UnityEvent<ServerRoomInfoResponse> { };

    public class NetworkRoomInfoDiscovery : NetworkDiscoveryBase<ServerRequest/*�ͻ��������������Ӧ��Ϣ*/, ServerRoomInfoResponse/*���з�����Ϣ�ķ�����Ӧ��*/>
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
                    //���䷿����Ϣ
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