using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[SerializeField]
public struct PlayerScoresInfo
{
    //public NetworkPlayingRoomGameModelPlayer modelPlayer;
    public uint modelPlayerNetId;
    public int Score;
    public int Kills;
    public int Dead;
}

public class NetworkPlayingRoomGameModel : MonoBehaviour
{
    public NetworkPlayingRoomManager networkPlayingRoomManager;
    public NetworkPlayingRoomStolsGroup networkPlayingRoomStolsGroup;

    //<ModelPlayer GameObject,ModelPlayer>
    public Dictionary<GameObject, NetworkPlayingRoomGameModelPlayer> ModelPlayers = new Dictionary<GameObject, NetworkPlayingRoomGameModelPlayer>();

    public GameModels gameModels = GameModels.TeamDeathMatch;

    private GameModelCtrl modelCtrl;
    public string GameModelPanel { get => modelCtrl.GameModelPanelPrefabPath; }

    public UnityEvent<string> CurrentGameModelPanelPrefabPathChanged = new UnityEvent<string>();

    static NetworkPlayingRoomGameModel instance;

    public static NetworkPlayingRoomGameModel singleton { get => instance; }

    public string GameModelName { get => modelCtrl.ModelName; }

    private void Awake()
    {
        if (instance != null)
            return;
        instance = this;
        if(!networkPlayingRoomManager)
            networkPlayingRoomManager = GetComponent<NetworkPlayingRoomManager>();
        networkPlayingRoomManager.OnStartServerEvent.AddListener(StartRoomGameModel);
        networkPlayingRoomManager.OnStopServerEvent.AddListener(StopRoomGameModel);
        networkPlayingRoomManager.OnStartHostEvent.AddListener(StartRoomGameModel);
        networkPlayingRoomManager.OnStopHostEvent.AddListener(StopRoomGameModel);
        networkPlayingRoomManager.OnStartClientEvent.AddListener(StartRoomGameModel);
        networkPlayingRoomManager.OnStopClientEvent.AddListener(StopRoomGameModel);
        if (!networkPlayingRoomStolsGroup)
            networkPlayingRoomStolsGroup = GetComponent<NetworkPlayingRoomStolsGroup>();

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void InitModelCtrl()
    {
        if (modelCtrl != null)
            return;
        EventsManager.AddListener(GameModelCtrl.PanelUpdateEvent, UpdataAllModelPleyerPanel);
        switch (gameModels)
        {
            case GameModels.TeamDeathMatch:
                SetModelCtrl(new GameModelTeamDeathMatch());
                break;
            default:
                break;
        }
    }

    private void SetModelCtrl(GameModelCtrl ctrl)
    {
        modelCtrl = ctrl;
        modelCtrl.GameModelPanelPrefabPathChanged.AddListener(InvokeCurrentGameModelPanelPrefabPathChanged);
        //modelCtrl.OnGameFinished.AddListener()
    }

    void InvokeCurrentGameModelPanelPrefabPathChanged(string path)
    {
        CurrentGameModelPanelPrefabPathChanged?.Invoke(path);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Ω·À„
    /// </summary>
    [ServerCallback]
    public void Settlement()
    {
        networkPlayingRoomManager.ServerRoomGamePlayingStop();
    }

    [ServerCallback]
    public void GamePlayerRelocation(NetworkPlayingRoomGameModelPlayer modelPlayer)
    {
        modelCtrl.GamePlayerRelocation(modelPlayer);
    }

    [ServerCallback]
    public void StartRoomGameModel()
    {
        if (NetworkServer.active)
        {
            InitModelCtrl();
        }
    }



    [ServerCallback]
    public void StopRoomGameModel()
    {
        modelCtrl = null;
        EventsManager.RemoveListener(GameModelCtrl.PanelUpdateEvent, UpdataAllModelPleyerPanel);
    }


    public string GetPanelUpdateMessage(NetworkPlayingRoomGameModelPlayer modelPlayer)
    {
        ModelPanelUpdateBaseMessage msg = modelCtrl.GetPanelUpdateMessage(modelPlayer);
        return JsonUtility.ToJson(Convert.ChangeType(msg,(Type.GetType(msg.MessageName))));
    }

    private void UpdataAllModelPleyerPanel()
    {
        foreach (var item in ModelPlayers.Values)
        {
            item.SetPanelUpdateMessage(GetPanelUpdateMessage(item));
        }
    }

    public void AddModelPlayer(NetworkPlayingRoomGameModelPlayer modelPalyer)
    {
        ModelPlayers.Add(modelPalyer.gameObject, modelPalyer);
        if(modelCtrl!=null)
            modelCtrl.AddModelPlayer(modelPalyer);
    }

    public void RemoveModelPlayer(NetworkPlayingRoomGameModelPlayer modelPlayer)
    {
        if(modelCtrl!=null)
            modelCtrl.RemoveModelPlayer(modelPlayer);
        ModelPlayers.Remove(modelPlayer.gameObject);
    }

    //public void SendDeadInfo(NetworkPlayingRoomGameModelPlayer modelPlayer, Queue<DamagePackage> killerCount)
    //{
    //    modelCtrl.SendDeadInfo(modelPlayer, killerCount);
    //}
}
