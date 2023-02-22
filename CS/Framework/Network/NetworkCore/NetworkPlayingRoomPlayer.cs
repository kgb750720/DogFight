using Mirror;
using System;
using UnityEngine;
using UnityEngine.Events;
using static NetworkPlayingRoomManager;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-room-player
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkRoomPlayer.html
*/

[SerializeField]
public class StateChangedEvent : UnityEvent<NetworkPlayingRoomPlayer> { }

/// <summary>
/// This component works in conjunction with the NetworkRoomManager to make up the multiplayer room system.
/// The RoomPrefab object of the NetworkRoomManager must have this component on it.
/// This component holds basic room player data required for the room to function.
/// Game specific data for room players can be put in other components on the RoomPrefab or in scripts derived from NetworkRoomPlayer.
/// </summary>

public class NetworkPlayingRoomPlayer : NetworkBehaviour
{
    /// <summary>
    /// This flag controls whether the default UI is shown for the room player.
    /// <para>As this UI is rendered using the old GUI system, it is only recommended for testing purposes.</para>
    /// </summary>
    [Tooltip("This flag controls whether the default UI is shown for the room player")]
    public bool showRoomGUI = true;

    public NetworkPlayingRoomPlayerUICtrl uiCtrl;
    [SerializeField]
    public enum RoomReadyState
    {
        Ready,
        NotReady,
        Playing
    }
    [Header("Diagnostics")]
    /// <summary>
    /// Diagnostic flag indicating whether this player is ready for the game to begin.
    /// <para>Invoke CmdChangeReadyState method on the client to set this flag.</para>
    /// <para>When all players are ready to begin, the game will start. This should not be set directly, CmdChangeReadyState should be called on the client to set it on the server.</para>
    /// </summary>
    [Tooltip("Diagnostic flag indicating whether this player is ready for the game to begin")]
    [SyncVar(hook = nameof(ReadyStateChanged))]
    public RoomReadyState readyState;

    /// <summary>
    /// Diagnostic index of the player, e.g. Player1, Player2, etc.
    /// </summary>
    [Tooltip("Diagnostic index of the player, e.g. Player1, Player2, etc.")]
    [SyncVar(hook = nameof(IndexChanged))]
    public int index;

    /// <summary>
    /// 游戏对局中的玩家物体
    /// </summary>
    [SyncVar(hook =nameof(OnGamePlayerChanged))]
    public GameObject GamePlayer;
    private void OnGamePlayerChanged(GameObject oldGamePlayer, GameObject newGamePlayer)
    {
        GamePlayer = newGamePlayer;
    }
    //[Command]
    //public void CmdSetGamePlayer(GameObject gamePlayer)
    //{
    //    SCallbackSetGamePlayer(gamePlayer);
    //}
    //[ServerCallback]
    //public void SCallbackSetGamePlayer(GameObject gamePlayer)
    //{
    //    GamePlayer = gamePlayer;
    //}
    //[ClientRpc]
    //public void CRpcSetGamePlayer(GameObject gamePlayer)
    //{
    //    GamePlayer = gamePlayer;
    //}


    #region Unity Callbacks


    /// <summary>
    /// Do not use Start - Override OnStartHost / OnStartClient instead!
    /// </summary>
    public void Start()
    {
        if (!uiCtrl)
            uiCtrl = GetComponent<NetworkPlayingRoomPlayerUICtrl>();

        if (isLocalPlayer)
        {
            uiCtrl.InstanceUI();
        }

        if (NetworkManager.singleton is NetworkPlayingRoomManager room)
        {
            // NetworkRoomPlayer object must be set to DontDestroyOnLoad along with NetworkRoomManager
            // in server and all clients, otherwise it will be respawned in the game scene which would
            // have undesirable effects.
            if (room.dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);

            room.roomSlots.Add(this);
            room.roomSlotsConnetSet.Add(netIdentity.connectionToClient);

            if (NetworkServer.active)
            {
                room.RecalculateRoomPlayerIndices();
                var conn = netIdentity.connectionToClient;
                room.pendingPlayers.Add(new NetworkPlayingRoomManager.PendingPlayer { roomPlayer = conn.identity.gameObject,conn=conn });
            }

            if (NetworkClient.active)
                room.CallOnClientEnterRoom();
        }

        else Debug.LogError("RoomPlayer could not find a NetworkRoomManager. The RoomPlayer requires a NetworkRoomManager object to function. Make sure that there is one in the scene.");
    }

    public virtual void OnDisable()
    {
        if (NetworkClient.active && NetworkManager.singleton is NetworkPlayingRoomManager room)
        {
            // only need to call this on client as server removes it before object is destroyed
            room.roomSlots.Remove(this);
            room.roomSlotsConnetSet.Remove(netIdentity.connectionToClient);
            PendingPlayer removePending = new PendingPlayer();
            foreach (var pendingPlayer in room.pendingPlayers)
            {
                if (pendingPlayer.conn == netIdentity.connectionToClient)
                {
                    removePending = pendingPlayer;
                    break;
                }
            }
            if (removePending.conn != null)
                room.pendingPlayers.Remove(removePending);
            room.CallOnClientExitRoom();
        }
        
    }

    #endregion

    #region Commands

    [Command]
    public void CmdChangeReadyState(RoomReadyState readyState)
    {
        this.readyState = readyState;
        NetworkPlayingRoomManager room = NetworkManager.singleton as NetworkPlayingRoomManager;
        if (room != null)
        {
            room.ReadyStatusChanged();
        }
    }

    [Command]
    public void CmdClientReturnToRoom()
    {
        NetworkPlayingRoomManager room = NetworkManager.singleton as NetworkPlayingRoomManager;
        if (room)
            room.RoomPlayerGamePlayingStop(this);
    }

    #endregion

    #region SyncVar Hooks

    /// <summary>
    /// This is a hook that is invoked on clients when the index changes.
    /// </summary>
    /// <param name="oldIndex">The old index value</param>
    /// <param name="newIndex">The new index value</param>
    public virtual void IndexChanged(int oldIndex, int newIndex) { }

    /// <summary>
    /// This is a hook that is invoked on clients when a RoomPlayer switches between ready or not ready.
    /// <para>This function is called when the a client player calls CmdChangeReadyState.</para>
    /// </summary>
    /// <param name="newReadyState">New Ready State</param>
    public virtual void ReadyStateChanged(RoomReadyState oldReadyState, RoomReadyState newReadyState)
    {
        oldReadyState = newReadyState;
        OnReadyStateChanged?.Invoke(this);
    }

    public StateChangedEvent OnReadyStateChanged = new StateChangedEvent();

    #endregion

    #region Room Client Virtuals

    /// <summary>
    /// This is a hook that is invoked on clients for all room player objects when entering the room.
    /// <para>Note: isLocalPlayer is not guaranteed to be set until OnStartLocalPlayer is called.</para>
    /// </summary>
    public virtual void OnClientEnterRoom() { }

    /// <summary>
    /// This is a hook that is invoked on clients for all room player objects when exiting the room.
    /// </summary>
    public virtual void OnClientExitRoom() { }

    #endregion

    #region Optional UI

    /// <summary>
    /// Render a UI for the room. Override to provide your own UI
    /// </summary>
    public virtual void OnGUI()
    {
        if (!showRoomGUI)
            return;

        NetworkRoomManager room = NetworkManager.singleton as NetworkRoomManager;
        if (room)
        {
            if (!room.showRoomGUI)
                return;

            if (!NetworkManager.IsSceneActive(room.RoomScene))
                return;

            DrawPlayerReadyState();
            DrawPlayerReadyButton();
        }
    }

    void DrawPlayerReadyState()
    {
        GUILayout.BeginArea(new Rect(20f + (index * 100), 200f, 90f, 130f));

        GUILayout.Label($"Player [{index + 1}]");

        if (readyState==RoomReadyState.Ready)
            GUILayout.Label("Ready");
        else
            GUILayout.Label("Not Ready");

        if (((isServer && index > 0) || isServerOnly) && GUILayout.Button("REMOVE"))
        {
            // This button only shows on the Host for all players other than the Host
            // Host and Players can't remove themselves (stop the client instead)
            // Host can kick a Player this way.
            GetComponent<NetworkIdentity>().connectionToClient.Disconnect();
        }

        GUILayout.EndArea();
    }

    void DrawPlayerReadyButton()
    {
        if (NetworkClient.active && isLocalPlayer)
        {
            GUILayout.BeginArea(new Rect(20f, 300f, 120f, 20f));

            //if (readyToBegin)
            //{
            //    if (GUILayout.Button("Cancel"))
            //        CmdChangeReadyState(false);
            //}
            //else
            //{
            //    if (GUILayout.Button("Ready"))
            //        CmdChangeReadyState(true);
            //}
            CmdChangeReadyState(readyState);

            GUILayout.EndArea();
        }
    }

    #endregion

    [ServerCallback]
    public void SCallbackDestroyRoomUI()
    {
        CRpcDestroyRoomUI();
    }

    [ClientRpc]
    private void CRpcDestroyRoomUI()
    {
        if(isLocalPlayer)
            uiCtrl.DestroyUI();
    }

    public void SCallbackInstanceRoomUI()
    {
        CRpcInstanceRoomUI();
    }

    [ClientRpc]
    private void CRpcInstanceRoomUI()
    {
        if (isLocalPlayer)
            uiCtrl.InstanceUI();
    }
}
