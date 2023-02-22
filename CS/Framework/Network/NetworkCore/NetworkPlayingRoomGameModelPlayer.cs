using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.Events;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class NetworkPlayingRoomGameModelPlayer : NetworkBehaviour
{
    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// Invoked on the server when the object is unspawned
    /// <para>Useful for saving object data in persistent storage</para>
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() { }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() { }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() { }

    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnection parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() { }

    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }

    #endregion

    string lastPath;
    [SyncVar]
    string gameModelPanelPath;
    public string GameModelPanelPath{get => gameModelPanelPath;}

    string lastUpdateMessage;
    [SyncVar]
    string panelUpdateMessage;
    public ModelPanelUpdateBaseMessage GameModelPanelMessage 
    { 
        get 
        {
            ModelPanelUpdateBaseMessage msgBase = JsonUtility.FromJson<ModelPanelUpdateBaseMessage>(panelUpdateMessage);
            return JsonUtility.FromJson(panelUpdateMessage, Type.GetType(msgBase.MessageName)) as ModelPanelUpdateBaseMessage;
        }
    }

    public NetworkPlayingRoomGameModel networkPlayingRoomGameModel;

    public NetworkPlayingRoomPlayer roomPlayer;

    public UnityEvent<NetworkPlayingRoomGameModelPlayer> OnGameModelPanelChanged = new UnityEvent<NetworkPlayingRoomGameModelPlayer>();

    public UnityEvent<ModelPanelUpdateBaseMessage> OnResponsePanelUpdateMessage = new UnityEvent<ModelPanelUpdateBaseMessage>();

    private void Start()
    {
        if (!roomPlayer)
            roomPlayer = GetComponent<NetworkPlayingRoomPlayer>();
        if (!networkPlayingRoomGameModel)
            networkPlayingRoomGameModel = NetworkManager.singleton.GetComponent<NetworkPlayingRoomGameModel>();
        networkPlayingRoomGameModel.AddModelPlayer(this);
        networkPlayingRoomGameModel.CurrentGameModelPanelPrefabPathChanged.AddListener(SCallbackSetGamePanelPath);
        if (isServer)
        {
            gameModelPanelPath = networkPlayingRoomGameModel.GameModelPanel;
            //SetPanelUpdateMessage(networkPlayingRoomGameModel.GetPanelUpdateMessage(this));
            //EventsManager<string>.AddListener(GameModelCtrl.PanelUpdateEvent, SetPanelUpdateMessage);
        }
    }

    private void Update()
    {
        if(gameModelPanelPath!=lastPath)
        {
            OnGameModelPanelChanged?.Invoke(this);
            lastPath = gameModelPanelPath;
        }

        if (panelUpdateMessage != lastUpdateMessage)
        {
            ModelPanelUpdateBaseMessage baseMessage = JsonUtility.FromJson<ModelPanelUpdateBaseMessage>(panelUpdateMessage);
            OnResponsePanelUpdateMessage?.Invoke(JsonUtility.FromJson(panelUpdateMessage,Type.GetType(baseMessage.MessageName))as ModelPanelUpdateBaseMessage);
            lastUpdateMessage = panelUpdateMessage;
        }
    }

    private void OnDestroy()
    {
        if (networkPlayingRoomGameModel)
        {
            networkPlayingRoomGameModel.RemoveModelPlayer(this);
            networkPlayingRoomGameModel.CurrentGameModelPanelPrefabPathChanged.RemoveListener(SCallbackSetGamePanelPath);
        }
        //EventsManager<string>.RemoveListener("GameModelPanelUpdateEvent", SetPanelUpdateMessage);

    }

    /// <summary>
    /// 客户端请求服务器对GamePlayer重定位
    /// </summary>
    [Command]
    public void CmdGamePlayerRelocation()
    {
        networkPlayingRoomGameModel.GamePlayerRelocation(this);
    }



    [ServerCallback]
    public void SCallbackSetGamePanelPath(string path)
    {
        gameModelPanelPath = path;
    }

    //[ServerCallback]
    //public void SCallbackDeadKillerCount(Queue<DamagePackage> killerCount)
    //{
    //    networkPlayingRoomGameModel.SendDeadInfo(this, killerCount);
    //}

    public void SetPanelUpdateMessage(string updateMessage)
    {
        panelUpdateMessage = updateMessage;
    }
}
