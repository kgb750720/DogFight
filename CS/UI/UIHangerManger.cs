using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
using UnityEngine.UI;

public class UIHangerManger : MonoBehaviour
{
    public CinemachineVirtualCamera VCM;
    public float MinAxisY = 1f;
    public float MaxAxisY = 10f;
    public float CameraHorizonalMoveSpeed = 8f;
    public float CameraVerticalMoveSpeed = 2f;
    public float ZoomSpeed = 2f;
    public float ZoomLerpSpeed = 2f;
    public float ZoomMaxVFov = 60f;
    public float ZoomMinVFov = 10f;
    private float TargetZoomVFovVal = 60;
    public Transform CameraFollow;
    private Vector3 CameraOffset;
    public Camera AroundCamera;
    public float AroundSpeed = 6f;
    public bool Around = false;
    public bool Touch = false;
    public bool EquipmentUIShow = false;
    public Button BtnBack;

    private string currentJetPrefabPath = "";
    public string CurrentJetPrefabPath { get => currentJetPrefabPath; }
    private GameObject currentJetObj;
    public GameObject CurrentJetObj { get => currentJetObj; }
    private List<string> currentWeaponPrefabList = new List<string>();
    public List<string> CurrentWeaponPrefabList { get => new List<string>(currentWeaponPrefabList); }
    private List<GameObject> currentWeaponObjList = new List<GameObject>();
    //<装备预制体路径,装备Sync同步配置>
    private Dictionary<string, string> currentPrefabEquipmentConf = new Dictionary<string, string>();

    private CinemachineTransposer ct;
    private Vector3 defultOffest;

    [SerializeField]
    private Transform canvas;
    private void Awake()
    {
        if (!VCM)
            VCM = GetComponentInChildren<CinemachineVirtualCamera>();
        if (!CameraFollow)
            CameraFollow = transform.Find("CameraFollow");
        VCM.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = new Vector3(10, 10, 10);
        if (!AroundCamera)
            AroundCamera = GetComponentInChildren<Camera>();
        TargetZoomVFovVal = ZoomMaxVFov;
        if (!canvas)
            canvas = transform.Find("Canvas");
        if (!BtnBack)
            BtnBack = transform.Find("Canvas/BtnBack").GetComponent<Button>();
        ct = VCM.GetCinemachineComponent<CinemachineTransposer>();
        defultOffest = ct.m_FollowOffset;
    }
    // Start is called before the first frame update
    void Start()
    {
        ChangeJetPrefab(GameManager.SpawnPrefabResourcesPath);
        GameManager.SpawnPrefabChanged += ChangeJetPrefab;
    }

    // Update is called once per frame
    void Update()
    {
        if (Touch && Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            if (!Physics.Raycast(AroundCamera.ScreenPointToRay(Input.mousePosition), float.MaxValue, 1 << LayerMask.NameToLayer("UI")))
            {
                Vector2 mouseForward = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                Vector3 offset = ct.m_FollowOffset;
                CameraFollow.Rotate(transform.rotation * (mouseForward.x * CameraHorizonalMoveSpeed * Time.deltaTime * transform.up));
                float addY = -mouseForward.y * CameraVerticalMoveSpeed * Time.deltaTime;
                if (offset.y == 10)
                {
                    offset.x -= addY; offset.z -= addY;
                    if (offset.x < 1 || offset.z < 1) { offset.x = 1; offset.z = 1; }
                    else if (offset.x > 10 || offset.z > 10)
                    {
                        offset.y -= offset.x - 10;
                        offset.x = 10; offset.z = 10;
                    }
                }
                else
                {
                    offset.y += addY;
                    if (offset.y < 1)
                        offset.y = 1;
                    else if (offset.y > 10)
                    {
                        offset.x -= offset.y - 10; offset.z -= offset.y - 10;
                        offset.y = 10;
                    }
                }
                ct.m_FollowOffset = offset;
            }
        }
        else if (!Touch && Around)
        {
            //旋转
            CameraFollow.Rotate(CameraFollow.rotation * CameraFollow.up * AroundSpeed * Time.deltaTime);
            ResetCameraZoom();
        }
        float scrollWheelVal = Input.GetAxis("MouseScrollWheel");
        //float scrollWheelVal = DefaultControls.factory.
        if (Touch && scrollWheelVal != 0)
        {
            float addZoom = -scrollWheelVal * ZoomSpeed;
            TargetZoomVFovVal = Mathf.Clamp(TargetZoomVFovVal + addZoom, ZoomMinVFov, ZoomMaxVFov);
        }
        if (EquipmentUIShow)
            canvas.gameObject.SetActive(true);
        else
            canvas.gameObject.SetActive(false);
    }

    private void ResetCameraZoom()
    {
        ct.m_FollowOffset = defultOffest;
        TargetZoomVFovVal = ZoomMaxVFov;
    }

    private void FixedUpdate()
    {
        
    }

    private void LateUpdate()
    {
        float fov = VCM.GetComponent<CinemachineVirtualCamera>().m_Lens.FieldOfView;
        LensSettings lens = VCM.GetComponent<CinemachineVirtualCamera>().m_Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, TargetZoomVFovVal, Time.deltaTime * ZoomLerpSpeed);
        VCM.GetComponent<CinemachineVirtualCamera>().m_Lens = lens;
    }

    private void OnDestroy()
    {
        GameManager.SpawnPrefabChanged -= ChangeJetPrefab;
        GameManager.RemoveSpawnEquipmentConfChanged(currentJetPrefabPath, ChangeEquimpemtConf);
        GameManager.RemoveSpawnEquipmentConfRemoved(currentJetPrefabPath, RemoveEquimpemtConf);
        GameManager.RemoveWeaponListChanged(GameManager.SpawnPrefabResourcesPath, ChangeJetWeaponList);
    }

    private Camera sourceCamera;

    public void OpenShowHanger(Camera sourceCamera)
    {
        if (sourceCamera!=null&&sourceCamera != AroundCamera)
        {
            this.sourceCamera = sourceCamera;
            if (sourceCamera.GetComponent<FlightView>())
            {
                sourceCamera.GetComponent<FlightView>().enabled = false;
                //由于FlightView在对Camera的操作在LateUpdate里，使用协程延时关闭防止关闭操作被覆盖
                StartCoroutine(WaitForCloseCamera());
            }
            else
            {
                sourceCamera.enabled = false;
                if (sourceCamera.GetComponent<AudioListener>())
                    sourceCamera.GetComponent<AudioListener>().enabled = false;
            }
        }
        ResetCameraZoom();
        AroundCamera.GetComponent<AudioListener>().enabled = true;
        AroundCamera.enabled = true;
        //Touch = true;
    }

    IEnumerator WaitForCloseCamera()
    {
        yield return new WaitForSeconds(0.1f);
        sourceCamera.enabled = false;
        if (sourceCamera.GetComponent<AudioListener>())
            sourceCamera.GetComponent<AudioListener>().enabled = false;
    }

    /// <summary>
    /// 还原到sourceCamera
    /// </summary>
    public void CloseShowHanger()
    {
        ResetCameraZoom();
        if (AroundCamera)
        {
            AroundCamera.GetComponent<AudioListener>().enabled = false;
            AroundCamera.enabled = false;
        }
        if(sourceCamera)
        {
            sourceCamera.enabled = true;
            if(sourceCamera.GetComponent<AudioListener>())
                sourceCamera.GetComponent<AudioListener>().enabled = true;
            if (sourceCamera.GetComponent<FlightView>())
                sourceCamera.GetComponent<FlightView>().enabled = true;
        }
        //Touch = false;
    }

    void ChangeJetPrefab(string path)
    {
        ClearWeaponList();
        if (currentJetObj)
            GameObject.Destroy(currentJetObj);
        if (currentJetPrefabPath!="")
        {
            //GameManager.SpawnPrefabConfigData.EquipmentConfigChangeCallBack changeCallbackDestroy = GameManager.AddSpawnEquipmentConfChanged(currentJetPrefab);
            //GameManager.SpawnPrefabConfigData.EquipmentConfigRemoveCallBack removeCallBackDestroy = GameManager.AddSpawnEquipmentConfRemoved(currentJetPrefab);
            //changeCallbackDestroy -= ChangeEquimpemtConf;
            //removeCallBackDestroy -= RemoveEquimpemtConf;
            GameManager.RemoveSpawnEquipmentConfChanged(currentJetPrefabPath, ChangeEquimpemtConf);
            GameManager.RemoveSpawnEquipmentConfRemoved(currentJetPrefabPath, RemoveEquimpemtConf);
            GameManager.RemoveWeaponListChanged(path, ChangeJetWeaponList);
        }
        GameObject prefab = Resources.Load<GameObject>(GameManager.RemovePathPrefixAndSuffix(path));
        currentJetPrefabPath = path; ;
        currentJetObj = CreatOfflineObj(prefab);
        currentJetObj.GetComponent<FlightSystem>().EffectChangeEnable = false;
        currentJetObj.GetComponent<Flight.PlayerController>().enabled = false;
        currentJetObj.GetComponent<PlayerManager>().enabled = false;
        currentJetObj.GetComponent<RadarSystem>().enabled = false;
        currentJetObj.layer = LayerMask.NameToLayer("Ignore Raycast");
        //GameManager.SpawnPrefabConfigData.EquipmentConfigChangeCallBack changeCallback = GameManager.AddSpawnEquipmentConfChanged(currentJetPrefab);
        //GameManager.SpawnPrefabConfigData.EquipmentConfigRemoveCallBack removeCallBack = GameManager.AddSpawnEquipmentConfRemoved(currentJetPrefab);
        //changeCallback += ChangeEquimpemtConf;
        //removeCallBack += RemoveEquimpemtConf;
        GameManager.AddSpawnEquipmentConfChanged(currentJetPrefabPath, ChangeEquimpemtConf);
        GameManager.AddSpawnEquipmentConfRemoved(currentJetPrefabPath, RemoveEquimpemtConf);
        currentPrefabEquipmentConf = GameManager.GetSpawnEpuimentConfig(path);
        //foreach (var item in GameManager.GetSpawnEpuimentConfig(currentJetPrefabPath))
        //{
        //    //    if (item.Key.GetComponent<SyncBase>())
        //    //        currentPrefabEquipmentConf.Add(item.Key.GetComponent<SyncBase>().OfflinePrefab, item.Value);
        //    //    else
        //    currentPrefabEquipmentConf.Add(item.Key, item.Value);
        //}
        GameManager.AddWeaponListChanged(path, ChangeJetWeaponList);
        StartCoroutine(WaitForFreezeJetRig(1f));
        ChangeJetWeaponList(GameManager.GetWeaponResourcesPathList());
    }

    private GameObject CreatOfflineObj(GameObject prefab)
    {
        if (prefab.GetComponent<SyncBase>())
            prefab = prefab.GetComponent<SyncBase>().OfflinePrefab;
        GameObject obj = Instantiate(prefab, CameraFollow.position, Quaternion.identity);
        GameObject.DontDestroyOnLoad(obj);
        return obj;
    }

    void ChangeJetWeaponList(string []paths)
    {
        ClearWeaponList();
        currentWeaponObjList.Clear();
        foreach (string path in paths)
        {
            GameObject prefab = Resources.Load(GameManager.RemovePathPrefixAndSuffix(path)) as GameObject;
            GameObject weaponObj = CreatOfflineObj(prefab);
            //GameObject.DontDestroyOnLoad(weaponObj);
            weaponObj.transform.parent = currentJetObj.transform;
            currentWeaponPrefabList.Add(path);
            if (currentPrefabEquipmentConf.ContainsKey(path))
            {
                WeaponLauncher.SyncValueData sync = JsonUtility.FromJson<WeaponLauncher.SyncValueData>(currentPrefabEquipmentConf[path]);
                weaponObj.GetComponent<WeaponLauncher>().Sync = sync;
            }
            currentWeaponObjList.Add(weaponObj);
        }
    }

    private void ClearWeaponList()
    {
        currentWeaponPrefabList.Clear();
        for (int i = currentWeaponObjList.Count - 1; i >= 0; i--)
            if (currentWeaponObjList[i])
                GameObject.Destroy(currentWeaponObjList[i]);
    }

    private void ChangeEquimpemtConf(string equipmentPrefab,string JonsSyncData)
    {
        if (currentPrefabEquipmentConf.ContainsKey(equipmentPrefab))
            currentPrefabEquipmentConf[equipmentPrefab] = JonsSyncData;
        else
            currentPrefabEquipmentConf.Add(equipmentPrefab, JonsSyncData);
        checkReload(equipmentPrefab);
    }

    private void RemoveEquimpemtConf(string equipmentPrefab)
    {
        if (currentPrefabEquipmentConf.ContainsKey(equipmentPrefab))
            currentPrefabEquipmentConf.Remove(equipmentPrefab);
        checkReload(equipmentPrefab);
    }

    private void checkReload(string equipmentPrefab)
    {
        if (equipmentPrefab == currentJetPrefabPath)
            ChangeJetPrefab(GameManager.SpawnPrefabResourcesPath);
        else if (currentWeaponPrefabList.Contains(equipmentPrefab))
            ChangeJetWeaponList(GameManager.GetWeaponResourcesPathList());
    }

    IEnumerator WaitForFreezeJetRig(float second)
    {
        yield return new WaitForSeconds(second);
        if(currentJetObj)
            currentJetObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }
}
