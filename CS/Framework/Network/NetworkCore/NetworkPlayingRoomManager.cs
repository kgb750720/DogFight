using UnityEngine;
using Mirror;
using System;
using UnityEngine.Serialization;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine.Events;
using System.Collections;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-room-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkRoomManager.html

	See Also: NetworkManager
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

//请求取得GamePlaySecene信息
public struct RequireGamePlaySeceneMessage : NetworkMessage { }

//改变GamePlaySecene
public struct ChangeGamePlaySeceneMessage : NetworkMessage 
{
    public string newGamePlaySecene;
}

//准备中且地图加载完毕
public struct ReadyAndSeceneLoadedMessage : NetworkMessage{}

//预加载附加地图
public struct PreLoadSceneMessage : NetworkMessage 
{
    public string PreLoadScene;
}


/// <summary>
/// This is a specialized NetworkManager that includes a networked room.
/// The room has slots that track the joined players, and a maximum player count that is enforced.
/// It requires that the NetworkPlayingRoomPlayer component be on the room player objects.
/// NetworkRoomManager is derived from NetworkManager, and so it implements many of the virtual functions provided by the NetworkManager class.
/// </summary>
public class NetworkPlayingRoomManager : NetworkManager
{
    [Serializable]
    public struct PendingPlayer
    {
        public NetworkConnection conn;
        public GameObject roomPlayer;
    }

    [SerializeField]
    [Tooltip("房间名称")]
    private string roomName = "";
    public string RoomName { 
        get => roomName;
        set 
        {
            roomName = value;
            OnRoomNameChangeEvent?.Invoke(roomName);
        } 
    }
    public UnityEvent<string> OnRoomNameChangeEvent = new UnityEvent<string>();

    [FormerlySerializedAs("m_MinPlayers")]
    [SerializeField]
    [Tooltip("Minimum number of players to auto-start the game")]
    public int minPlayers = 1;

    [FormerlySerializedAs("m_MaxPlayers")]
    [SerializeField]
    [Tooltip("Max number of players to auto-start the game")]
    public int maxPlayers = 8;


    [FormerlySerializedAs("m_RoomPlayerPrefab")]
    [SerializeField]
    [Tooltip("Prefab to use for the Room Player")]
    public NetworkPlayingRoomPlayer roomPlayerPrefab;

    /// <summary>
    /// The scene to use for the room. This is similar to the offlineScene of the NetworkManager.
    /// </summary>
    [Scene]
    public string RoomScene;

    /// <summary>
    /// The scene to use for the playing the game from the room. This is similar to the onlineScene of the NetworkManager.
    /// </summary>
    [Scene]
    public string GameplayScene;

    [Scene]
    private List<string> AdditiveScenes = new List<string>();

    /// <summary>
    /// List of players that are in the Room (仅仅只有服务器端的有作用)
    /// </summary>
    [FormerlySerializedAs("m_PendingPlayers")]
    public List<PendingPlayer> pendingPlayers = new List<PendingPlayer>();

    [Header("Diagnostics")]

    /// <summary>
    /// True when all players have submitted a Ready message
    /// </summary>
    [Tooltip("Diagnostic flag indicating all players are ready to play")]
    [FormerlySerializedAs("allPlayersReady")]
    [SerializeField] bool _allPlayersReady;

    /// <summary>
    /// These slots track players that enter the room.游戏内包括房间页面和游戏中所有的玩家
    /// <para>The slotId on players is global to the game - across all players.</para>
    /// </summary>
    [Tooltip("List of Room Player objects")]
    public List<NetworkPlayingRoomPlayer> roomSlots = new List<NetworkPlayingRoomPlayer>();
    public HashSet<NetworkConnectionToClient> roomSlotsConnetSet = new HashSet<NetworkConnectionToClient>();
    [Tooltip("调用onSceneLoadedForPlayer回调时采取的策略")]
    public NetworkPlayingRoomOnLoadedGamePlayerBase onLoadedGamePlayer;

    [SerializeField]
    public UnityEvent OnRoomStateChanged = new UnityEvent();

    public bool allPlayersReady
    {
        get => _allPlayersReady;
        set
        {
            bool wasReady = _allPlayersReady;
            bool nowReady = value;

            if (wasReady != nowReady)
            {
                _allPlayersReady = value;

                if (nowReady)
                {
                    OnRoomServerPlayersReady();
                }
                else
                {
                    OnRoomServerPlayersNotReady();
                }
            }
        }
    }

    private bool _roomGamePlaying = false;
    public bool roomGamePlaying 
    {
        get => _roomGamePlaying;
        private set
        {
            if(_roomGamePlaying != value)
            {
                _roomGamePlaying = value;
                if (_roomGamePlaying)
                    EventsManager.Invoke(GameModelCtrl.GameBeginEvent);
            }
        }
    }

    public UnityEvent OnStartServerEvent = new UnityEvent();
    public UnityEvent OnStopServerEvent = new UnityEvent();
    public UnityEvent OnStartHostEvent = new UnityEvent();
    public UnityEvent OnStopHostEvent = new UnityEvent();
    public UnityEvent OnStartClientEvent = new UnityEvent();
    public UnityEvent OnStopClientEvent = new UnityEvent();

    public override void OnValidate()
    {
        // always >= 0
        maxConnections = Mathf.Max(maxConnections, 0);

        // always <= maxConnections
        minPlayers = Mathf.Min(minPlayers, maxConnections);
        maxPlayers = Mathf.Min(maxPlayers, maxConnections);

        // always >= 0
        minPlayers = Mathf.Max(minPlayers, 0);
        maxPlayers = Mathf.Max(maxPlayers, 0);

        if (roomPlayerPrefab != null)
        {
            NetworkIdentity identity = roomPlayerPrefab.GetComponent<NetworkIdentity>();
            if (identity == null)
            {
                roomPlayerPrefab = null;
                Debug.LogError("RoomPlayer prefab must have a NetworkIdentity component.");
            }
        }

        base.OnValidate();
    }

    public void ReadyStatusChanged()
    {
        int CurrentPlayers = 0;
        int ReadyPlayers = 0;
        if (!roomGamePlaying)
        {
            foreach (NetworkPlayingRoomPlayer item in roomSlots)
            {
                if (item != null)
                {
                    CurrentPlayers++;
                    if (item.readyState == NetworkPlayingRoomPlayer.RoomReadyState.Ready)
                        ReadyPlayers++;
                }
            }

            if (CurrentPlayers == ReadyPlayers)
                CheckReadyToBegin();
            else
                allPlayersReady = false;
        }
        else
        {
            LoadGamePlayerForReadyPendingPlayers();
        }
    }

    

    /// <summary>
    /// Called on the server when a client is ready.    当服务器收到客户端协议接收准备完毕时的回调
    /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerReady(NetworkConnection conn)
    {
        Debug.Log("NetworkRoomManager OnServerReady");
        base.OnServerReady(conn);
    }

    /// <summary>
    /// 在场景刚刚完成加载时的事件里调用，根据玩家是进入GamePlayer场景或者是Room场景产生对应的玩家
    /// </summary>
    /// <param name="conn"></param>
    /// <param name="roomPlayer"></param>
    private void SceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer)
    {
        Debug.Log($"NetworkRoom SceneLoadedForPlayer scene: {SceneManager.GetActiveScene().path} {conn}");

        //游戏场景转房间场景时进入代码块
        if (IsSceneActive(RoomScene))
        {
            // cant be ready in room, add to ready list
            PendingPlayer pending;
            pending.conn = conn;
            pending.roomPlayer = roomPlayer;
            pendingPlayers.Add(pending);
            return;
        }

        //房间场景转游戏场景进入代码块
        GameObject gamePlayer = OnRoomServerCreateGamePlayer(conn, roomPlayer);
        if (gamePlayer == null)
        {
            // get start position from base class
            Transform startPos = GetStartPosition();
            gamePlayer = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        }

        if (!OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer))
            return;

        // replace room player with game player
        NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, true);
        OnLoadedGamePlayer(gamePlayer,roomPlayer.GetComponent<NetworkPlayingRoomPlayer>());
    }

    /// <summary>
    /// 在加载完GamePlayer后的回调
    /// </summary>
    /// <param name="gamePlayer"></param>
    /// <param name="roomPlayer"></param>
    protected virtual void OnLoadedGamePlayer(GameObject gamePlayer ,NetworkPlayingRoomPlayer roomPlayer)
    {
        if(onLoadedGamePlayer)
        {
            onLoadedGamePlayer.OnLoadedGamePlayer(gamePlayer, roomPlayer);
        }
    }

    /// <summary>
    /// CheckReadyToBegin checks all of the players in the room to see if their readyToBegin flag is set.
    /// <para>If all of the players are ready, then the server switches from the RoomScene to the PlayScene, essentially starting the game. This is called automatically in response to NetworkPlayingRoomPlayer.CmdChangeReadyState.</para>
    /// </summary>
    public void CheckReadyToBegin()
    {
        if (!IsSceneActive(RoomScene))
        {
            return;
        }

        int numberOfReadyPlayers = NetworkServer.connections.Count(conn => conn.Value != null &&
            conn.Value.identity.gameObject.GetComponent<NetworkPlayingRoomPlayer>().readyState==NetworkPlayingRoomPlayer.RoomReadyState.Ready);
        bool enoughReadyPlayers = minPlayers <= 0 || numberOfReadyPlayers >= minPlayers;
        if (enoughReadyPlayers)
        {
            //pendingPlayers.Clear();
            allPlayersReady = true;
        }
        else
        {
            allPlayersReady = false;
        }
    }

    internal void CallOnClientEnterRoom()
    {
        OnRoomClientEnter();
        foreach (NetworkPlayingRoomPlayer player in roomSlots)
            if (player != null)
            {
                player.OnClientEnterRoom();
            }
    }

    internal void CallOnClientExitRoom()
    {
        OnRoomClientExit();
        foreach (NetworkPlayingRoomPlayer player in roomSlots)
            if (player != null)
            {
                player.OnClientExitRoom();
            }
    }

    #region server handlers

    /// <summary>
    /// Called on the server when a new client connects.
    /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerConnect(NetworkConnection conn)
    {
        if (numPlayers >= maxPlayers)
        {
            conn.Disconnect();
            return;
        }


        base.OnServerConnect(conn);
        OnRoomServerConnect(conn);
    }

    /// <summary>
    /// Called on the server when a client disconnects.
    /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
    /// </summary>
    /// <param name="conn">Connection from client.</param>
    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            NetworkPlayingRoomPlayer roomPlayer = conn.identity.GetComponent<NetworkPlayingRoomPlayer>();

            if (roomPlayer != null)
                roomSlots.Remove(roomPlayer);

            foreach (NetworkIdentity clientOwnedObject in conn.clientOwnedObjects)
            {
                roomPlayer = clientOwnedObject.GetComponent<NetworkPlayingRoomPlayer>();
                if (roomPlayer != null)
                    roomSlots.Remove(roomPlayer);
            }
        }

        allPlayersReady = false;


        if (IsSceneActive(RoomScene))
            RecalculateRoomPlayerIndices();

        OnRoomServerDisconnect(conn);
        base.OnServerDisconnect(conn);

#if UNITY_SERVER
            if (numPlayers < 1)
                StopServer();
#endif
    }

    // Sequential index used in round-robin deployment of players into instances and score positioning
    //public int clientIndex;

    /// <summary>
    /// Called on the server when a client adds a new player with NetworkClient.AddPlayer. 玩家连接进入房间和玩家加载完地图时都会在服务器端调用
    /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
    /// </summary> 
    /// <param name="conn">Connection from client.</param>
    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        OnRoomServerAddPlayer(conn);
    }

    [Server]
    public void RecalculateRoomPlayerIndices()
    {
        if (roomSlots.Count > 0)
        {
            for (int i = 0; i < roomSlots.Count; i++)
            {
                roomSlots[i].index = i;
            }
        }
    }

    /// <summary>
    /// This causes the server to switch scenes and sets the networkSceneName.
    /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
    /// </summary>
    /// <param name="newSceneName"></param>
    public override void ServerChangeScene(string newSceneName)
    {
        
        //回到房间或进入房间时
        if (newSceneName == RoomScene)
        {
            allPlayersReady = false;
        }
        base.ServerChangeScene(newSceneName);

    }

    /// <summary>
    /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
    /// </summary>
    /// <param name="sceneName">The name of the new scene.</param>
    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName != RoomScene) //加载GamePlayer
        {
            // call SceneLoadedForPlayer on any players that become ready while we were loading the scene.
            LoadGamePlayerForReadyPendingPlayers();
        }
        else
        {
            //加载RoomPlayer
            LoadPendingPlayersForInRoomStolsPlayer();
        }
        EventsManager<string>.Invoke(GameModelCtrl.GameRoomSceneChanged, sceneName);
        OnRoomStateChanged?.Invoke();
        OnRoomServerSceneChanged(sceneName);
    }

    /// <summary>
    /// 将roomSlots中Playing状态和NotReady的roomPlayer重新添加到PendingPlayers中
    /// </summary>
    protected void LoadPendingPlayersForInRoomStolsPlayer()
    {
        foreach (var roomPlayer in roomSlots)
        {
            if(roomPlayer.readyState!= NetworkPlayingRoomPlayer.RoomReadyState.NotReady)
            {
                //SceneLoadedForPlayer(roomPlayer.connectionToClient,roomPlayer.gameObject);
                PendingPlayer pending;
                pending.conn = roomPlayer.connectionToClient;
                pending.roomPlayer = roomPlayer.gameObject;
                pendingPlayers.Add(pending);
                roomPlayer.readyState = NetworkPlayingRoomPlayer.RoomReadyState.NotReady;
                NetworkServer.Destroy(roomPlayer.gameObject);
                roomPlayer.SCallbackInstanceRoomUI();
            }
        }
    }

    protected void LoadPendingPlayersForPlayingRoomPlayer(NetworkPlayingRoomPlayer roomPlayer)
    {
        if (roomPlayer.readyState == NetworkPlayingRoomPlayer.RoomReadyState.Playing)
        {
            //SceneLoadedForPlayer(roomPlayer.connectionToClient,roomPlayer.gameObject);
            if (roomPlayer.GamePlayer)
                Destroy(roomPlayer.GamePlayer);
            PendingPlayer pending;
            pending.conn = roomPlayer.connectionToClient;
            pending.roomPlayer = roomPlayer.gameObject;
            pendingPlayers.Add(pending);
            roomPlayer.readyState = NetworkPlayingRoomPlayer.RoomReadyState.NotReady;
            roomPlayer.SCallbackInstanceRoomUI();
        }
    }

    /// <summary>
    /// 为Ready状态的PendingPlayers加载GamePlayer
    /// </summary>
    protected void LoadGamePlayerForReadyPendingPlayers()
    {
        List<PendingPlayer>  notReadyPendingPlayers = new List<PendingPlayer>(pendingPlayers.Count);
        foreach (PendingPlayer pending in pendingPlayers)
        {
            if (pending.roomPlayer.GetComponent<NetworkPlayingRoomPlayer>().readyState == NetworkPlayingRoomPlayer.RoomReadyState.Ready)
            {
                //SceneLoadedForPlayer(pending.conn, pending.roomPlayer);
                GameObject gamePlayer = OnRoomServerCreateGamePlayer(pending.conn, pending.roomPlayer);
                if (gamePlayer == null)
                {
                    // get start position from base class
                    Transform startPos = GetStartPosition();
                    gamePlayer = startPos != null
                        ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                        : Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
                }

                if (!OnRoomServerSceneLoadedForPlayer(pending.conn, pending.roomPlayer, gamePlayer))
                    return;

                //确保产生的玩家在GameplayScene
                //SceneManager.MoveGameObjectToScene(gamePlayer, SceneManager.GetSceneByPath(GameplayScene));

                NetworkPlayingRoomPlayer roomPlayer = pending.roomPlayer.GetComponent<NetworkPlayingRoomPlayer>();
                NetworkServer.SetClientReady(roomPlayer.netIdentity.connectionToClient);
                roomPlayer.readyState = NetworkPlayingRoomPlayer.RoomReadyState.Playing;
                roomPlayer.SCallbackDestroyRoomUI();
                NetworkServer.Spawn(gamePlayer, pending.roomPlayer);
                roomPlayer.GetComponent<NetworkPlayingRoomPlayer>().GamePlayer = gamePlayer;
                //NetworkServer.ReplacePlayerForConnection(pending.conn, gamePlayer, true);

                OnLoadedGamePlayer(gamePlayer, roomPlayer);
            }
            else if(pending.roomPlayer.GetComponent<NetworkPlayingRoomPlayer>().readyState==NetworkPlayingRoomPlayer.RoomReadyState.NotReady)
            {
                 notReadyPendingPlayers.Add(pending);
            }
        }
        pendingPlayers = notReadyPendingPlayers;
    }

    /// <summary>
    /// This is invoked when a server is started - including when a host is started.
    /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartServer()
    {
        OnStartServerEvent.Invoke();
        if (string.IsNullOrWhiteSpace(RoomScene))
        {
            Debug.LogError("NetworkRoomManager RoomScene is empty. Set the RoomScene in the inspector for the NetworkRoomManager");
            return;
        }

        if (string.IsNullOrWhiteSpace(GameplayScene))
        {
            Debug.LogError("NetworkRoomManager PlayScene is empty. Set the PlayScene in the inspector for the NetworkRoomManager");
            return;
        }


        OnRoomStartServer();
    }



    /// <summary>
    /// This is invoked when a host is started.
    /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
    /// </summary>
    public override void OnStartHost()
    {
        OnStartHostEvent.Invoke();
        OnRoomStartHost();
    }

    /// <summary>
    /// This is called when a server is stopped - including when a host is stopped.
    /// </summary>
    public override void OnStopServer()
    {
        OnStopServerEvent.Invoke();
        roomSlots.Clear();
        OnRoomStopServer();
        //StartCoroutine(UnloadAdditiveScenes());
    }

    /// <summary>
    /// This is called when a host is stopped.
    /// </summary>
    public override void OnStopHost()
    {
        OnStopHostEvent.Invoke();
        OnRoomStopHost();
    }

    #endregion

    #region client handlers

    /// <summary>
    /// This is invoked when the client is started.
    /// </summary>
    public override void OnStartClient()
    {
        OnStartClientEvent.Invoke();
        if (roomPlayerPrefab == null || roomPlayerPrefab.gameObject == null)
            Debug.LogError("NetworkRoomManager no RoomPlayer prefab is registered. Please add a RoomPlayer prefab.");
        else
            NetworkClient.RegisterPrefab(roomPlayerPrefab.gameObject);

        if (playerPrefab == null)
            Debug.LogError("NetworkRoomManager no GamePlayer prefab is registered. Please add a GamePlayer prefab.");

        OnRoomStartClient();
    }

    /// <summary>
    /// Called on the client when connected to a server.
    /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
    /// </summary>
    public override void OnClientConnect()
    {
#pragma warning disable 618
        // obsolete method calls new method
        OnRoomClientConnect(NetworkClient.connection);
#pragma warning restore 618
        base.OnClientConnect();
    }


    /// <summary>
    /// Called on clients when disconnected from a server.
    /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
    /// </summary>
    public override void OnClientDisconnect()
    {
#pragma warning disable 618
        OnRoomClientDisconnect(NetworkClient.connection);
#pragma warning restore 618
        base.OnClientDisconnect();
    }

    /// <summary>
    /// This is called when a client is stopped.
    /// </summary>
    public override void OnStopClient()
    {
        OnStartClientEvent.Invoke();
        OnRoomStopClient();
        CallOnClientExitRoom();
        //StartCoroutine(UnloadAdditiveScenes());
        roomSlots.Clear();
    }
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);
    }


    /// <summary>
    /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.在客户端调用
    /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
    /// </summary>
    public override void OnClientSceneChanged()
    {

        base.OnClientSceneChanged();    //设为将客户端设为Ready并通知服务器加入玩家
        OnRoomStateChanged?.Invoke();
        if (IsSceneActive(RoomScene) && NetworkClient.isConnected)
        {
            CallOnClientEnterRoom();
        }
        else if (NetworkClient.isConnected)  //进入游戏场景
        {

            foreach (var roomPlayer in roomSlots)
            {
                if (roomPlayer.isLocalPlayer && roomPlayer.readyState == NetworkPlayingRoomPlayer.RoomReadyState.Ready)
                {
                    NetworkClient.Send(new ReadyAndSeceneLoadedMessage());
                    break;
                }
            }
        }
        else
        {
            //退出游戏房间
            CallOnClientExitRoom();
        }
#pragma warning disable 618
        // obsolete method calls new method
        OnRoomClientSceneChanged(NetworkClient.connection);
#pragma warning restore 618
    }

    #endregion

    #region room server virtuals

    /// <summary>
    /// This is called on the host when a host is started.
    /// </summary>
    public virtual void OnRoomStartHost() { }

    /// <summary>
    /// This is called on the host when the host is stopped.
    /// </summary>
    public virtual void OnRoomStopHost() { }

    /// <summary>
    /// This is called on the server when the server is started - including when a host is started.
    /// </summary>
    public virtual void OnRoomStartServer() 
    {
        RegisterPlayingRoomServerMessages();
    }

    /// <summary>
    /// This is called on the server when the server is started - including when a host is stopped.
    /// </summary>
    public virtual void OnRoomStopServer() { }

    /// <summary>
    /// This is called on the server when a new client connects to the server.
    /// </summary>
    /// <param name="conn">The new connection.</param>
    public virtual void OnRoomServerConnect(NetworkConnection conn) { }

    /// <summary>
    /// This is called on the server when a client disconnects.
    /// </summary>
    /// <param name="conn">The connection that disconnected.</param>
    public virtual void OnRoomServerDisconnect(NetworkConnection conn) { }

    /// <summary>
    /// This is called on the server when a networked scene finishes loading.
    /// </summary>
    /// <param name="sceneName">Name of the new scene.</param>
    public virtual void OnRoomServerSceneChanged(string sceneName) { }

    /// <summary>
    /// This allows customization of the creation of the room-player object on the server.
    /// <para>By default the roomPlayerPrefab is used to create the room-player, but this function allows that behaviour to be customized.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    /// <returns>The new room-player object.</returns>
    public virtual GameObject OnRoomServerCreateRoomPlayer(NetworkConnection conn)
    {
        return null;
    }

    /// <summary>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// <para>By default the gamePlayerPrefab is used to create the game-player, but this function allows that behaviour to be customized. The object returned from the function will be used to replace the room-player on the connection.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    /// <param name="roomPlayer">The room player object for this connection.</param>
    /// <returns>A new GamePlayer object.</returns>
    public virtual GameObject OnRoomServerCreateGamePlayer(NetworkConnection conn, GameObject roomPlayer)
    {
        return null;
    }

    /// <summary>
    /// This allows customization of the creation of the GamePlayer object on the server.
    /// <para>This is only called for subsequent GamePlay scenes after the first one.</para>
    /// <para>See <see cref="OnRoomServerCreateGamePlayer(NetworkConnection, GameObject)">OnRoomServerCreateGamePlayer(NetworkConnection, GameObject)</see> to customize the player object for the initial GamePlay scene.</para>
    /// </summary>
    /// <param name="conn">The connection the player object is for.</param>
    public virtual void OnRoomServerAddPlayer(NetworkConnection conn)
    {
        if (roomSlots.Count == maxPlayers)
            return;

        allPlayersReady = false;
        GameObject newRoomGameObject = OnRoomServerCreateRoomPlayer(conn);
        if (newRoomGameObject == null)
            newRoomGameObject = Instantiate(roomPlayerPrefab.gameObject, Vector3.zero, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, newRoomGameObject);
    }

    // for users to apply settings from their room player object to their in-game player object
    /// <summary>
    /// This is called on the server when it is told that a client has finished switching from the room scene to a game player scene.
    /// <para>When switching from the room, the room-player is replaced with a game-player object. This callback function gives an opportunity to apply state from the room-player to the game-player object.</para>
    /// </summary>
    /// <param name="conn">The connection of the player</param>
    /// <param name="roomPlayer">The room player object.</param>
    /// <param name="gamePlayer">The game player object.</param>
    /// <returns>False to not allow this player to replace the room player.</returns>
    public virtual bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        return true;
    }

    /// <summary>
    /// This is called on the server when all the players in the room are ready.
    /// <para>The default implementation of this function uses ServerChangeScene() to switch to the game player scene. By implementing this callback you can customize what happens when all the players in the room are ready, such as adding a countdown or a confirmation for a group leader.</para>
    /// </summary>
    public virtual void OnRoomServerPlayersReady()
    {
        // all players are readyToBegin, start the game
        SeverRoomGamePalyingRun();
    }

    private void SeverRoomGamePalyingRun()
    {
        roomGamePlaying = true;
        
        ServerChangeScene(GameplayScene);
    }

    /// <summary>
    /// This is called on the server when CheckReadyToBegin finds that players are not ready
    /// <para>May be called multiple times while not ready players are joining</para>
    /// </summary>
    public virtual void OnRoomServerPlayersNotReady() { }

    ///// <summary>
    ///// 为RoomPlayingManager添加服务器响应消息
    ///// </summary>
    protected virtual void RegisterPlayingRoomServerMessages()
    {
        //NetworkServer.RegisterHandler<RequireGamePlaySeceneMessage>(OnRequireGamePlaySeceneMessage);
        NetworkServer.RegisterHandler<ReadyAndSeceneLoadedMessage>(OnReadyAndSeceneLoadedMessage);
    }

    protected virtual void OnReadyAndSeceneLoadedMessage(NetworkConnection conn, ReadyAndSeceneLoadedMessage msg)
    {
        LoadGamePlayerForReadyPendingPlayers();
    }

    //protected virtual void OnRequireGamePlaySeceneMessage(NetworkConnection conn, RequireGamePlaySeceneMessage msg)
    //{
    //    ChangeGamePlaySeceneMessage sendMsg = new ChangeGamePlaySeceneMessage();
    //    sendMsg.newGamePlaySecene = GameplayScene;
    //    conn.Send(sendMsg);
    //}


    #endregion

    #region room client virtuals

    /// <summary>
    /// This is a hook to allow custom behaviour when the game client enters the room.
    /// </summary>
    public virtual void OnRoomClientEnter() 
    {
        if(NetworkClient.active)
        {
            //NetworkClient.Send(new RequireGamePlaySeceneMessage());
        }
    }

    /// <summary>
    /// This is a hook to allow custom behaviour when the game client exits the room.
    /// </summary>
    public virtual void OnRoomClientExit() { }

    /// <summary>
    /// This is called on the client when it connects to server.
    /// </summary>
    public virtual void OnRoomClientConnect() { }

    // Deprecated 2021-10-30
    [Obsolete("Remove NetworkConnection from your override and use NetworkClient.connection instead.")]
    public virtual void OnRoomClientConnect(NetworkConnection conn)=>OnRoomClientConnect();

    /// <summary>
    /// This is called on the client when disconnected from a server.
    /// </summary>
    public virtual void OnRoomClientDisconnect() { }

    // Deprecated 2021-10-30
    [Obsolete("Remove NetworkConnection from your override and use NetworkClient.connection instead.")]
    public virtual void OnRoomClientDisconnect(NetworkConnection conn) => OnRoomClientDisconnect();

    /// <summary>
    /// This is called on the client when a client is started.
    /// </summary>
    public virtual void OnRoomStartClient() 
    {
        RegisterChangeGamePlayMessages();
    }

    /// <summary>
    /// This is called on the client when the client stops.
    /// </summary>
    public virtual void OnRoomStopClient() { }

    /// <summary>
    /// This is called on the client when the client is finished loading a new networked scene.
    /// </summary>
    public virtual void OnRoomClientSceneChanged() { }

    // Deprecated 2021-10-30
    [Obsolete("Remove NetworkConnection from your override and use NetworkClient.connection instead.")]
    public virtual void OnRoomClientSceneChanged(NetworkConnection conn) => OnRoomClientSceneChanged();

    /// <summary>
    /// Called on the client when adding a player to the room fails.
    /// <para>This could be because the room is full, or the connection is not allowed to have more players.</para>
    /// </summary>
    public virtual void OnRoomClientAddPlayerFailed() { }

    public virtual void RegisterChangeGamePlayMessages()
    {
        NetworkClient.RegisterHandler<ChangeGamePlaySeceneMessage>(OnChangeGamePlaySecene);
        NetworkClient.RegisterHandler<PreLoadSceneMessage>(OnPreLoadScene);
    }

    protected virtual void OnChangeGamePlaySecene(ChangeGamePlaySeceneMessage msg)
    {
        if(GameplayScene!=msg.newGamePlaySecene)
        {
            GameplayScene = msg.newGamePlaySecene;
        }
    }

    protected virtual void OnPreLoadScene(PreLoadSceneMessage msg)
    {
        StartCoroutine(PreLoadScenes(msg.PreLoadScene));
    }


    #endregion


    public void OnGUI()
    {
        if (NetworkServer.active &&mode==NetworkManagerMode.ServerOnly && IsSceneActive(GameplayScene))
        {
            GUILayout.BeginArea(new Rect(Screen.width - 150f, 10f, 140f, 30f));
            if (GUILayout.Button("Return to Room"))
                ServerChangeScene(RoomScene);
            GUILayout.EndArea();
        }
    }



    public void ServerRoomGamePlayingStop()
    {
        roomGamePlaying = false;
        ServerChangeScene(RoomScene);
    }

    public void RoomPlayerGamePlayingStop(NetworkPlayingRoomPlayer roomPlayer)
    {
        if (NetworkServer.active&&roomPlayer.readyState==NetworkPlayingRoomPlayer.RoomReadyState.Playing)
        {
            LoadPendingPlayersForPlayingRoomPlayer(roomPlayer);
        }
    }

    IEnumerator PreLoadScenes(string newScene)
    {
        if (!SceneManager.GetSceneByName(newScene).IsValid() && !SceneManager.GetSceneByPath(newScene).IsValid())
        {
            AsyncOperation asyncload =SceneManager.LoadSceneAsync(newScene);
            asyncload.allowSceneActivation = false;
            yield return asyncload;
        }
    }


    IEnumerator UnloadAdditiveScenes()
    {
        foreach (string sceneName in AdditiveScenes)
            if (SceneManager.GetSceneByName(sceneName).IsValid() || SceneManager.GetSceneByPath(sceneName).IsValid())
            {
                yield return SceneManager.UnloadSceneAsync(sceneName);
                // Debug.Log($"Unloaded {sceneName}");
            }
        AdditiveScenes.Clear();
        yield return Resources.UnloadUnusedAssets();
    }
}
