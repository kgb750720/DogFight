using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRoomPanelController : MonoBehaviour
{
    public NetworkPlayingRoomStolsGroup RoomStolsGropMangaer;

    
    public UIPlayerRowContentBase PlayerListContentController;

    public RectTransform btnReady;
    public RectTransform btnnCancelReady;

    public Text textTitle;

    private Camera mainCamera;
    NetworkPlayingRoomManager networkPlayingRoomManager;

    private void Awake()
    {
        networkPlayingRoomManager = NetworkPlayingRoomManager.singleton as NetworkPlayingRoomManager;
        if (!RoomStolsGropMangaer)
            RoomStolsGropMangaer = networkPlayingRoomManager.GetComponent<NetworkPlayingRoomStolsGroup>();
        RoomStolsGropMangaer.OnStolsGroupUpdate.AddListener(UpdatePlayerList);
        //SeverUpdateRoomName();
    }

    private void SetRoomName(string roomName)
    {
        textTitle.text = roomName;
    }

    //WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();

    //IEnumerator _waitForUpdateRoomNameOnEndOfFrame()
    //{
    //    yield return _waitForEndOfFrame;
    //    textTitle.text = networkPlayingRoomManager.RoomName;
    //}

    //[Server]
    //public void SeverUpdateRoomName()
    //{
    //    StartCoroutine(_waitForUpdateRoomNameOnEndOfFrame());
    //}
    bool firstUpdateList = true;
    public void UpdatePlayerList(Dictionary<string, HashSet<NetworkPlayingRoomStolsPlayer>> TeamGroup)
    {
        PlayerListContentController.UpdateContent(TeamGroup);
        if(PlayerListContentController.localPlayer.GetComponent<NetworkPlayingRoomPlayer>().readyState!=NetworkPlayingRoomPlayer.RoomReadyState.NotReady)
        {
            btnReady.gameObject.SetActive(false);
            btnnCancelReady.gameObject.SetActive(true);
        }
        else
        {
            btnReady.gameObject.SetActive(true);
            btnnCancelReady.gameObject.SetActive(false);
        }

        if (firstUpdateList)
        {
            PlayerListContentController.localPlayer.OnRoomNameChangedEvent.AddListener(SetRoomName);
            firstUpdateList = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        RoomStolsGropMangaer.DoStolsGroupUpdate();
        mainCamera = UnityEngine.Camera.main;
        if (mainCamera)
        {
            mainCamera.enabled = false;
            AudioListener audio = mainCamera.GetComponent<AudioListener>();
            if (audio)
                audio.enabled = false;
        }
        GameManager.Hanger.OpenShowHanger(mainCamera);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void BeforeDestroy()
    //{
    //    if (mainCamera)
    //    {
    //        mainCamera.enabled = true;
    //        AudioListener audio = mainCamera.GetComponent<AudioListener>();
    //        if (audio)
    //            audio.enabled = true;
    //    }
    //    GameManager.Hanger.CloseShowHanger();
    //}

    private void OnDestroy()
    {
        if (mainCamera)
        {
            mainCamera.enabled = true;
            AudioListener audio = mainCamera.GetComponent<AudioListener>();
            if (audio)
                audio.enabled = true;
        }
        GameManager.Hanger.CloseShowHanger();
        PlayerListContentController.localPlayer.OnRoomNameChangedEvent.RemoveListener(SetRoomName);
    }
    public void SetLocalPlayerReadState(NetworkPlayingRoomPlayer.RoomReadyState state)
    {
        foreach (var roomPlayer in NetworkManager.singleton.GetComponent<NetworkPlayingRoomManager>().roomSlots)
        {
            if (roomPlayer.isLocalPlayer)
            {
                PlayerListContentController.localPlayer = roomPlayer.GetComponent<NetworkPlayingRoomStolsPlayer>();
                roomPlayer.CmdChangeReadyState(state);
                break;
            }
        }
    }

    public void SetLocalPlayerReady()
    {
        SetLocalPlayerReadState(NetworkPlayingRoomPlayer.RoomReadyState.Ready);
    }

    public void SetLocalPlayerNotReady()
    {
        SetLocalPlayerReadState(NetworkPlayingRoomPlayer.RoomReadyState.NotReady);
    }

    public void SetlocalPlayerToTeam(string teamTag)
    {
        if(!PlayerListContentController.localPlayer)
        {
            foreach (var roomPlayer in NetworkManager.singleton.GetComponent<NetworkPlayingRoomManager>().roomSlots)
            {
                if (roomPlayer.isLocalPlayer)
                {
                    PlayerListContentController.localPlayer = roomPlayer.GetComponent<NetworkPlayingRoomStolsPlayer>();
                    break;
                }
            }
        }
        PlayerListContentController.localPlayer.CmdTurnRoomPlayerTo(teamTag);
    }

}
