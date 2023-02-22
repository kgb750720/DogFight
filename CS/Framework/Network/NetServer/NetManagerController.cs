using JetNetwork;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetManagerController : MonoBehaviour
{
    public NetworkRoomInfoDiscovery standAloneDiscovery;
    public NetworkPlayingRoomManager roomManager;
    public string IP = "127.0.0.1";
    public int port = 8888;
    public ServerFoundUnityEvent OnServerFound = new ServerFoundUnityEvent();
    // Start is called before the first frame update
    void Start()
    {
        NetManager.Connect(IP, port);
        NetManager.AddMsgListener("MsgKick", OnMsgKick);
        NetManager.AddMsgListener("MsgGetRoomList", OnMsgGetRoomList);
        NetManager.AddMsgListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.AddMsgListener("MsgLeaveRoom", OnMsgLeaveRoom);
        if (!roomManager)
            roomManager = NetworkPlayingRoomManager.singleton as NetworkPlayingRoomManager;
    }

    private void OnMsgLeaveRoom(MsgBase msgBase)
    {
        print("在服务器端注销广播");
    }

    private void OnMsgCreateRoom(MsgBase msgBase)
    {
        MsgCreateRoom msg = msgBase as MsgCreateRoom;
        if (msg.result == 0)
            print("创建房间成功");
        else
            print("创建房间失败");
    }

    private void OnMsgGetRoomList(MsgBase msgBase)
    {
        MsgGetRoomList msg = (MsgGetRoomList)msgBase;
        foreach (var item in msg.rooms)
        {
            ServerRoomInfoResponse response=JsonUtility.FromJson<ServerRoomInfoResponse>(item.RoomInfoJson);
            OnServerFound?.Invoke(response);
        }
    }

    private void OnMsgKick(MsgBase msgBase)
    {
        GameManager.isOnline = false;
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Update();
    }

    public void StartDiscovery()
    {
        if (!GameManager.isOnline)
            standAloneDiscovery.StartDiscovery();
        else
        {
            MsgGetRoomList msg = new MsgGetRoomList();
            NetManager.Send(msg);
        }    
    }

    public void StopDiscovery()
    {
        standAloneDiscovery.StopDiscovery();
    }

    public void StartBroadcastRoomInfo()
    {
        StartCoroutine(_broadcastRoomInfoLoop());
    }

    public void StopBroadcastRoomInfo()
    {
        StopCoroutine(_broadcastRoomInfoLoop());
        NetManager.Send(new MsgLeaveRoom());
    }

    WaitForSeconds _forSeconds = new WaitForSeconds(5);
    IEnumerator _broadcastRoomInfoLoop()
    {
        while (true)
        { 
            MsgUpdateRoomInfoJson msg = new MsgUpdateRoomInfoJson();
            ServerRoomInfoResponse response = new ServerRoomInfoResponse();
            response.roomInfo.roomMaxPlayers = roomManager.maxPlayers;
            response.roomInfo.roomPlayersCount = roomManager.roomSlots.Count;
            response.roomInfo.gamePlaying = roomManager.roomGamePlaying;
            response.roomInfo.roomName = roomManager.RoomName;
            msg.RoomInfoJson = JsonUtility.ToJson(response);
            NetManager.Send(msg);
            yield return _forSeconds;
        }
    }

}
