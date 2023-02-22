using JetNetwork;
using Michsky.UI.Shift;
using Mirror;
using Mirror.Discovery;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISeverBrowserListContent : MonoBehaviour
{
    public GameObject PrefabSeverButton;
    public BlurManager ModalWinBlurManager;
    public ModalWindowManager SelectSeverModalWindow;
    public NetworkRoomInfoDiscovery RoomInfoDiscovery;
    Dictionary<long, ServerRoomInfoResponse> discoveredServers = new Dictionary<long, ServerRoomInfoResponse>();
    Dictionary<long, GameObject> severItems = new Dictionary<long, GameObject>();
    private long selectedResponseID = -1;

    private void Awake()
    {
        if (!RoomInfoDiscovery)
            NetworkManager.singleton.GetComponent<NetworkRoomInfoDiscovery>();
    }

    // Start is called before the first frame update
    void Start()
    {
        RoomInfoDiscovery.StartDiscovery();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ClearList()
    {
        discoveredServers.Clear();
        selectedResponseID = -1;
        for (int i = transform.childCount-1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    public void OnDiscoveredServer(ServerRoomInfoResponse info)
    {
        if (!gameObject.activeSelf)
            return;
        // Note that you can check the versioning to decide if you can connect to the server or not using this metho
        if (!discoveredServers.ContainsKey(info.serverId))
        {
            discoveredServers[info.serverId] = info;
            GameObject severItem = Instantiate(PrefabSeverButton, transform);
            severItems[info.serverId] = severItem;
            updateSeverButton(info, severItem);
            Button btn = severItem.GetComponent<Button>();
            btn.onClick.AddListener(
                delegate
                {
                    ModalWinBlurManager.BlurInAnim();
                    SelectSeverModalWindow.ModalWindowIn();
                    selectedResponseID = info.serverId;
                }
            );
        }
        else
        {
            GameObject item = severItems[info.serverId];
            updateSeverButton(info, item);
        }
    }

    private void updateSeverButton(ServerRoomInfoResponse serverInfo, GameObject serverItem)
    {
        //更新玩家数量
        serverItem.transform.Find("Content/Players").GetComponent<TMP_Text>().text = serverInfo.roomInfo.roomPlayersCount + "/" + serverInfo.roomInfo.roomMaxPlayers;
        //更新Ping
        Ping ping = new Ping(serverInfo.uri.Host);
        TMP_Text pingText = serverItem.transform.Find("Content/Ping").GetComponentInChildren<TMP_Text>();
        StartCoroutine(WaitForPingUpdate(pingText, ping));
        //更新服务器名称
        TMP_Text titleText = serverItem.transform.Find("Content/Title & Description/Title").GetComponent<TMP_Text>();
        titleText.text = serverInfo.roomInfo.roomName;
        //更新服务器模式
        TMP_Text modelText = serverItem.transform.Find("Content/Game Mode").GetComponent<TMP_Text>();
        modelText.text = serverInfo.roomInfo.modelName;
    }

    IEnumerator WaitForPingUpdate(TMP_Text pingText,Ping ping)
    {
        while (!ping.isDone)
        {
            yield return null;
        }
        pingText.text = ping.time.ToString();
    }

    /// <summary>
    /// 连接至当前选中的selectedResponseID
    /// </summary>
    public void Connect()
    {
        if (selectedResponseID != -1)
        {
            RoomInfoDiscovery.StopDiscovery();
            NetworkManager.singleton.StartClient(discoveredServers[selectedResponseID].uri);
        }
    }
}
