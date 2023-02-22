using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System;
using System.Reflection;
using UnityEngine.Events;
using Michsky.UI.Shift;

public class Respawn : NetworkBehaviour
{
    //public string RespawnBtnPath = "Canvas/RespawnPanel/BtnRespwan";
    public GameObject networkCanvas;
    public GameObject battlePanel;
    public GameObject respawnPanel;
    public GameObject pauseMenu;
    public GameObject gameModelPanel;
    public GameObject tipCanvas;
    public AddFlightViewBase addFlightViewCS;
    [SyncVar(hook = nameof(OnRespawnObjLoadPathChanged))]
    public string RespawnPrefabLoadPath;
    public string lastRespawnPrefabLoadPath;
    //private GameObject respawnPrefab;
    void OnRespawnObjLoadPathChanged(string oldPath, string newPath)
    {
        RespawnPrefabLoadPath = newPath;
    }


    [SyncVar(hook = nameof(OnWeaponLoadPathListChanged))]
    public SyncList<string> WeaponLoadPathList = new SyncList<string>();
    void OnWeaponLoadPathListChanged(SyncList<string> oldList, SyncList<string> newList)
    {
        WeaponLoadPathList = newList;
    }

    //[SyncVar(hook =nameof(OnPlayerObjChanged))]
    //public GameObject PlayerObj;
    //void OnPlayerObjChanged(GameObject oldObj,GameObject newObj)
    //{
    //    PlayerObj = newObj;
    //}

    [SyncVar(hook = nameof(OnEquipmentCofChanged))]
    public SyncDictionary<string, string> EquipmentConfig = new SyncDictionary<string, string>();
    void OnEquipmentCofChanged(SyncDictionary<string, string> oldConf, SyncDictionary<string, string> newConf)
    {
        EquipmentConfig = newConf;
    }

    [SyncVar(hook =nameof(OnRoomPlayerChanged))]
    public NetworkPlayingRoomPlayer RoomPlayer;
    void OnRoomPlayerChanged(NetworkPlayingRoomPlayer oldRoomPlayer, NetworkPlayingRoomPlayer newRoomPlayer)
    {
        RoomPlayer = newRoomPlayer;
    }
    public NetworkPlayingRoomGameModelPlayer modelPlayer;
    public Button btnRespan;
    public Button btnEquipment;
    private UnityAction equipmentPanelBack;

    UIHangerManger hanger;

    GameObject jetObj;

    Camera mainCam;

    bool isHasAuthority = false;    //辅助OnDestroy判断hasAuthority

    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        if (hasAuthority)
        {
            isHasAuthority = true;
            if (!NetworkClient.ready)
                NetworkClient.Ready();
            
            //初始化FlightView脚本
            StartCoroutine(WaitForInit());
        }
    }

    //延迟0.1s初始化相机脚本，防止其他类关闭MianCamera并在OnDestroy复原时失效（OnDestroy是Unity执行时序最后的，好坑）
    IEnumerator WaitForInit()
    {
        yield return new WaitForSeconds(0.1f);
        //gameObject.AddComponent<FlightInit>();

        //加载UI界面
        //网络对战下的UICanvas
        networkCanvas = Instantiate(Resources.Load<GameObject>("UI/CanvasNetwork"));

        //暂停界面
        if (!pauseMenu)
            pauseMenu = Instantiate(Resources.Load<GameObject>("UI/NetworkPauseMenu"));
        //子战斗界面
        battlePanel = networkCanvas.transform.Find("BattlePanel").gameObject;

        //子复活界面
        respawnPanel = networkCanvas.transform.Find("RespawnPanel").gameObject;
        respawnPanel.GetComponent<UIRespawnPanel>().DrawPanel = true;

        //提示TipCanvas
        tipCanvas = Instantiate(Resources.Load<GameObject>("UI/TipCanvas"));
        tipCanvas.SetActive(false);

        mainCam = UnityEngine.Camera.main;
        if (mainCam && !mainCam.enabled)
        {
            mainCam.enabled = true;
            mainCam.GetComponent<AudioListener>().enabled = true;
        }
        if (addFlightViewCS && mainCam)
            addFlightViewCS.AddFlightViewCS(mainCam.gameObject);

        btnRespan = respawnPanel.transform.Find("ShowPanel/BtnRespawn").GetComponent<Button>();
        btnRespan.onClick.AddListener(CmdDoRespawn);
        btnEquipment = respawnPanel.transform.Find("ShowPanel/EquipmentPanel").GetComponent<Button>();
        hanger = GameManager.Hanger;
        equipmentPanelBack = delegate
        {
            hanger.EquipmentUIShow = false;
            hanger.CloseShowHanger();
            respawnPanel.GetComponent<UIRespawnPanel>().DrawPanel = true;
        };
        hanger.transform.Find("Canvas/BtnBack").GetComponent<Button>().onClick.AddListener(equipmentPanelBack);

        btnEquipment.onClick.AddListener(delegate
        {
            respawnPanel.GetComponent<UIRespawnPanel>().DrawPanel = false;
            hanger.EquipmentUIShow = true;
            hanger.Around = false;
            hanger.Touch = true;
            hanger.OpenShowHanger(mainCam);
        });
        ToDoChangeRespawnPrefab(GameManager.SpawnPrefabResourcesPath);
        //SetWeaponList(GameManager.GetWeaponResourcesPathList());
        UpdateWeaponList(GameManager.GetWeaponResourcesPathList());
        GameManager.SpawnPrefabChanged += ToDoChangeRespawnPrefab;


        //重定向出生点位置和初始化游戏模式界面
        modelPlayer = RoomPlayer.GetComponent<NetworkPlayingRoomGameModelPlayer>();
        if (modelPlayer)
        {
            modelPlayer.CmdGamePlayerRelocation();
            LoadGameModelPanel(modelPlayer);
            modelPlayer.OnGameModelPanelChanged.AddListener(LoadGameModelPanel);
            modelPlayer.OnResponsePanelUpdateMessage.AddListener(SendMessageToModelPanel);
        }
    }







    // Update is called once per frame
    void Update()
    {
    }



    private void OnDestroy()
    {
        //hasAuthority在OnDestroy中是失效的，无法正常判断
        if (isHasAuthority)
        {
            GameManager.SpawnPrefabChanged -= ChangeRespawnPrefab;
            GameManager.RemoveSpawnEquipmentConfChanged(lastRespawnPrefabLoadPath, ChangeEquipmentConfig);
            GameManager.RemoveSpawnEquipmentConfRemoved(lastRespawnPrefabLoadPath, RemoveEquipmentConfig);
            GameManager.RemoveWeaponListChanged(lastRespawnPrefabLoadPath, UpdateWeaponList);
            UIHangerManger hanger = GameManager.Hanger;
            hanger.transform.Find("Canvas/BtnBack").GetComponent<Button>().onClick.RemoveListener(equipmentPanelBack);

            if (mainCam && mainCam.GetComponent<FlightView>().enabled)
            {
                mainCam.GetComponent<FlightView>().enabled = false;
            }


            if (modelPlayer)
            {
                modelPlayer.OnGameModelPanelChanged.RemoveListener(LoadGameModelPanel);
                modelPlayer.OnResponsePanelUpdateMessage.RemoveListener(SendMessageToModelPanel);
            }
            if (gameModelPanel)
                Destroy(gameModelPanel);
            Cursor.lockState = CursorLockMode.None;
        }

        if (isServer&&jetObj)
            NetworkServer.Destroy(jetObj);
        if (networkCanvas)
            Destroy(networkCanvas);
        if (pauseMenu)
            Destroy(pauseMenu);
        if (tipCanvas)
            Destroy(tipCanvas);
    }

    [Command]   //客户端发起调用服务器端(在Command里调Command等同于服务器发起调服务器，会失效)
    public void CmdDoRespawn()
    {
        //生成机体
        string loadPath = GameManager.RemovePathPrefixAndSuffix(RespawnPrefabLoadPath);
        GameObject jetPrefab = Resources.Load<GameObject>(loadPath);
        jetObj= Instantiate(jetPrefab, transform.position, transform.rotation);
        jetObj.GetComponent<JetSync>().ownerPlayerObj = gameObject;
        //添加死亡事件回调
        jetObj.GetComponent<JetSync>().OnDestroyEvent.AddListener(OnJetDestroy);
        NetworkServer.Spawn(jetObj, RoomPlayer.gameObject);
        //隐藏重生界面
        SCallbackShowRespawnPanel(false);
        //显示操作提示
        ScallbackShowTipCanvas(true);
        //根据 PlayerGroup 的 syncTeam 标签生成机体标签
        NetworkPlayingRoomStolsPlayer playerGroup = RoomPlayer.GetComponent<NetworkPlayingRoomStolsPlayer>();
        jetObj.GetComponent<JetSync>().SCallbackSetSyncTag(playerGroup.TeamTag);
        //生成武器
        foreach (string weaponPath in WeaponLoadPathList)
        {
            loadPath = GameManager.RemovePathPrefixAndSuffix(weaponPath);
            GameObject weapon = Instantiate(Resources.Load(loadPath) as GameObject, transform.position, transform.rotation);
            NetworkServer.Spawn(weapon, RoomPlayer.gameObject);

            //在服务器和客户端上绑定武器和机体的Tansform关系
            weapon.transform.parent = jetObj.transform;
            //在服务器和客户端上设置武器初始配置参数
            SCallBackSetEquipmentInitSyncData(weapon.GetComponent<NetworkIdentity>(),weaponPath);
            //直接向客户端配置会出错
            //CRpcSetEquipmentInitSyncData(weapon.GetComponent<NetworkIdentity>(), weaponPath);
            //根据RoomPlayer标签生成武器标签
            SCallbackSetGameObjectTag(weapon, RoomPlayer.tag);
        }
    }

    [ServerCallback]    //仅在服务器端调用
    void SCallBackSetTranformParentTo(Transform child, Transform parent)
    {
        child.parent = parent;
    }

    [ClientRpc]         //仅在客户端调用
    void CRpcSetTranformParentTo(Transform child, Transform parent)
    {
        child.parent = parent;
    }

    [ServerCallback]
    void SCallBackSetEquipmentInitSyncData(NetworkIdentity equipmentIndentity,string equipmentPrefabPath)
    {
        if (EquipmentConfig.ContainsKey(equipmentPrefabPath))
        {
            //在此添加同步装备配置
            //武器同步配置情况
            if (equipmentIndentity.GetComponent<WeaponLauncher>())
            {
                WeaponLauncher.SyncValueData syncData = JsonUtility.FromJson<WeaponLauncher.SyncValueData>(EquipmentConfig[equipmentPrefabPath]);
                equipmentIndentity.GetComponent<WeaponLauncher>().Sync = syncData;
            }
        }
    }

    public void ToDoChangeRespawnPrefab(string newPrefabPath)
    {
        //第一次改变重生对象或者重生对象换了都要重新解绑老事件，添加新绑定事件
        if (newPrefabPath != lastRespawnPrefabLoadPath)
        {
            if (lastRespawnPrefabLoadPath!="")
            {
                GameManager.RemoveSpawnEquipmentConfChanged(lastRespawnPrefabLoadPath, ChangeEquipmentConfig);
                GameManager.RemoveSpawnEquipmentConfRemoved(lastRespawnPrefabLoadPath, RemoveEquipmentConfig);
                GameManager.RemoveWeaponListChanged(lastRespawnPrefabLoadPath, UpdateWeaponList);
            }
            lastRespawnPrefabLoadPath = newPrefabPath;
            SetEquipmentConfig(GameManager.GetSpawnEpuimentConfig(newPrefabPath));
            GameManager.AddSpawnEquipmentConfChanged(newPrefabPath, ChangeEquipmentConfig);
            GameManager.AddSpawnEquipmentConfRemoved(newPrefabPath, RemoveEquipmentConfig);
            GameManager.AddWeaponListChanged(newPrefabPath, UpdateWeaponList);
        }
        ChangeRespawnPrefab(newPrefabPath);
    }

    [Command]
    public void ChangeRespawnPrefab(string newPrefabPath)
    {
        RespawnPrefabLoadPath = newPrefabPath;
    }

    public void SetEquipmentConfig(Dictionary<string,string> equipmentConfig)
    {
        ClearEquipmmentConfig();
        foreach (var item in equipmentConfig)
            ChangeEquipmentConfig(item.Key, item.Value);
    }

    [Command]
    public void ClearEquipmmentConfig()
    {
        EquipmentConfig.Clear();
    }

    [Command]
    private void ChangeEquipmentConfig(string equipmentPrefabPath, string syncDataJons)
    {
        if (EquipmentConfig.ContainsKey(equipmentPrefabPath))
            EquipmentConfig[equipmentPrefabPath] = syncDataJons;
        else
            EquipmentConfig.Add(equipmentPrefabPath, syncDataJons);
    }

    [Command]
    private void RemoveEquipmentConfig(string equipmentPrefab)
    {
        if (EquipmentConfig.ContainsKey(equipmentPrefab))
            EquipmentConfig.Remove(equipmentPrefab);
    }

    [Command]
    private void AddWeaponList(string weapon)
    {
        if(!WeaponLoadPathList.Contains(weapon))
            WeaponLoadPathList.Add(weapon);
    }

    [Command]
    void CLearWeaponList()
    {
        WeaponLoadPathList.Clear();
    }

    private void UpdateWeaponList(string[] list)
    {
        foreach (var item in list)
        {
            AddWeaponList(item);
        }
    }


    [ServerCallback]
    private void SCallbackSetGameObjectTag(GameObject go, string tag)
    {
        go.gameObject.tag = tag;
        //在生成时调用客户端会报错
    }


    [ServerCallback]
    void SCallbackShowRespawnPanel(bool show)
    {
        CRpcShowRespawnPanel(show);
    }

    [ClientRpc]
    void CRpcShowRespawnPanel(bool show)
    {
        if (respawnPanel)
        {
            respawnPanel.GetComponent<UIRespawnPanel>().DrawPanel = show;
            if (show)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }
    }


    public void OnJetDestroy()
    {
        SCallbackShowRespawnPanel(true);
        ScallbackShowTipCanvas(false);
        NetworkPlayingRoomGameModelPlayer modelPlayer = null;
        if (RoomPlayer)
            modelPlayer = RoomPlayer.GetComponent<NetworkPlayingRoomGameModelPlayer>();
        if (modelPlayer)
        {
            modelPlayer.CmdGamePlayerRelocation();
        }
    }

    [ServerCallback]
    private void ScallbackShowTipCanvas(bool show)
    {
        CRpcShowTipCanvas(show);
    }

    [ClientRpc]
    private void CRpcShowTipCanvas(bool show)
    {
        if (tipCanvas)
        {
            
            tipCanvas.SetActive(show);
            if (show)
            {
                StartCoroutine(WaitForFixTip());
            }
        }
    }

    WaitForSeconds _wtfft = new WaitForSeconds(0.1f);

    IEnumerator WaitForFixTip()
    {
        yield return _wtfft;
        tipCanvas.GetComponentInChildren<LayoutGroupPositionFix>().FixPos();
        tipCanvas.GetComponentInChildren<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperLeft;
    }

    //根据对应模式界面预制体路径加载界面预制体
    void LoadGameModelPanel(NetworkPlayingRoomGameModelPlayer player)
    {
        if (gameModelPanel)
            Destroy(gameModelPanel);
        gameModelPanel = Instantiate(Resources.Load<GameObject>(player.GameModelPanelPath));
        gameModelPanel.GetComponent<UIGameModelPanelBindBase>().SetBindModelPlayer(player);
    }

    void SendMessageToModelPanel(ModelPanelUpdateBaseMessage msg)
    {
        if (gameModelPanel)
            gameModelPanel.GetComponent<UIGameModelPanelBindBase>().SetPanelUpdateMessage(msg);
    }
}
