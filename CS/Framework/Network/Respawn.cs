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

    bool isHasAuthority = false;    //����OnDestroy�ж�hasAuthority

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
            
            //��ʼ��FlightView�ű�
            StartCoroutine(WaitForInit());
        }
    }

    //�ӳ�0.1s��ʼ������ű�����ֹ������ر�MianCamera����OnDestroy��ԭʱʧЧ��OnDestroy��Unityִ��ʱ�����ģ��ÿӣ�
    IEnumerator WaitForInit()
    {
        yield return new WaitForSeconds(0.1f);
        //gameObject.AddComponent<FlightInit>();

        //����UI����
        //�����ս�µ�UICanvas
        networkCanvas = Instantiate(Resources.Load<GameObject>("UI/CanvasNetwork"));

        //��ͣ����
        if (!pauseMenu)
            pauseMenu = Instantiate(Resources.Load<GameObject>("UI/NetworkPauseMenu"));
        //��ս������
        battlePanel = networkCanvas.transform.Find("BattlePanel").gameObject;

        //�Ӹ������
        respawnPanel = networkCanvas.transform.Find("RespawnPanel").gameObject;
        respawnPanel.GetComponent<UIRespawnPanel>().DrawPanel = true;

        //��ʾTipCanvas
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


        //�ض��������λ�úͳ�ʼ����Ϸģʽ����
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
        //hasAuthority��OnDestroy����ʧЧ�ģ��޷������ж�
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

    [Command]   //�ͻ��˷�����÷�������(��Command���Command��ͬ�ڷ��������������������ʧЧ)
    public void CmdDoRespawn()
    {
        //���ɻ���
        string loadPath = GameManager.RemovePathPrefixAndSuffix(RespawnPrefabLoadPath);
        GameObject jetPrefab = Resources.Load<GameObject>(loadPath);
        jetObj= Instantiate(jetPrefab, transform.position, transform.rotation);
        jetObj.GetComponent<JetSync>().ownerPlayerObj = gameObject;
        //��������¼��ص�
        jetObj.GetComponent<JetSync>().OnDestroyEvent.AddListener(OnJetDestroy);
        NetworkServer.Spawn(jetObj, RoomPlayer.gameObject);
        //������������
        SCallbackShowRespawnPanel(false);
        //��ʾ������ʾ
        ScallbackShowTipCanvas(true);
        //���� PlayerGroup �� syncTeam ��ǩ���ɻ����ǩ
        NetworkPlayingRoomStolsPlayer playerGroup = RoomPlayer.GetComponent<NetworkPlayingRoomStolsPlayer>();
        jetObj.GetComponent<JetSync>().SCallbackSetSyncTag(playerGroup.TeamTag);
        //��������
        foreach (string weaponPath in WeaponLoadPathList)
        {
            loadPath = GameManager.RemovePathPrefixAndSuffix(weaponPath);
            GameObject weapon = Instantiate(Resources.Load(loadPath) as GameObject, transform.position, transform.rotation);
            NetworkServer.Spawn(weapon, RoomPlayer.gameObject);

            //�ڷ������Ϳͻ����ϰ������ͻ����Tansform��ϵ
            weapon.transform.parent = jetObj.transform;
            //�ڷ������Ϳͻ���������������ʼ���ò���
            SCallBackSetEquipmentInitSyncData(weapon.GetComponent<NetworkIdentity>(),weaponPath);
            //ֱ����ͻ������û����
            //CRpcSetEquipmentInitSyncData(weapon.GetComponent<NetworkIdentity>(), weaponPath);
            //����RoomPlayer��ǩ����������ǩ
            SCallbackSetGameObjectTag(weapon, RoomPlayer.tag);
        }
    }

    [ServerCallback]    //���ڷ������˵���
    void SCallBackSetTranformParentTo(Transform child, Transform parent)
    {
        child.parent = parent;
    }

    [ClientRpc]         //���ڿͻ��˵���
    void CRpcSetTranformParentTo(Transform child, Transform parent)
    {
        child.parent = parent;
    }

    [ServerCallback]
    void SCallBackSetEquipmentInitSyncData(NetworkIdentity equipmentIndentity,string equipmentPrefabPath)
    {
        if (EquipmentConfig.ContainsKey(equipmentPrefabPath))
        {
            //�ڴ����ͬ��װ������
            //����ͬ���������
            if (equipmentIndentity.GetComponent<WeaponLauncher>())
            {
                WeaponLauncher.SyncValueData syncData = JsonUtility.FromJson<WeaponLauncher.SyncValueData>(EquipmentConfig[equipmentPrefabPath]);
                equipmentIndentity.GetComponent<WeaponLauncher>().Sync = syncData;
            }
        }
    }

    public void ToDoChangeRespawnPrefab(string newPrefabPath)
    {
        //��һ�θı���������������������˶�Ҫ���½�����¼�������°��¼�
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
        //������ʱ���ÿͻ��˻ᱨ��
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

    //���ݶ�Ӧģʽ����Ԥ����·�����ؽ���Ԥ����
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
