using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class UIBaseConfgPanel : MonoBehaviour
{

    public XmlElement xmlElementMissile;
    public string equipmentPrefabPath = null;
    public string prefabLoadPath = "";
    protected string SyncString = "";

    [SerializeField]
    protected Button btnSaveConfg;
    [SerializeField]
    protected Button btnClose;
    [SerializeField]
    protected Button btnUninstall;

    protected string spawnPrefab = "";

    protected virtual void Awake()
    {
        Button[] btns = GetComponentsInChildren<Button>();
        foreach (var item in btns)
        {
            if (item.name == "BtnSave" && !btnSaveConfg)
                btnSaveConfg = item;
            else if (item.name == "BtnClose" && !btnClose)
                btnClose = item;
            else if (item.name == "BtnUninstall" && !btnUninstall)
                btnUninstall = item;
        }
        spawnPrefab = GameObject.FindObjectOfType<UIHangerManger>().CurrentJetPrefabPath;
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        getPrefabAndSyncString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected void getPrefabAndSyncString()
    {
        string launcher = xmlElementMissile.GetAttribute("LauncherWeapon");
        XmlElement missileInfo = GameManager.LoadMXL("Equipment").SelectSingleNode("WeaponInfo") as XmlElement;
        foreach (XmlElement item in missileInfo.ChildNodes)
        {
            if (item.GetAttribute("WeaponName") == launcher)
            {
                if (GameObject.FindObjectsOfType<NetworkManager>() != null)
                {
                    //equipmentPrefabPath = Resources.Load<GameObject>(GameManager.RemovePathPrefixAndSuffix(item.GetAttribute("WeaponNetworkPrefabPath")));
                    equipmentPrefabPath = item.GetAttribute("WeaponNetworkPrefabPath");
                    prefabLoadPath = item.GetAttribute("WeaponNetworkPrefabPath");
                }
                else
                {
                    //equipmentPrefabPath = Resources.Load<GameObject>(GameManager.RemovePathPrefixAndSuffix(item.GetAttribute("WeaponPrefabPath")));
                    equipmentPrefabPath = item.GetAttribute("WeaponPrefabPath");
                    prefabLoadPath = item.GetAttribute("WeaponPrefabPath");
                }
                if (equipmentPrefabPath != null)
                {
                    Dictionary<string, string> config = GameManager.GetSpawnEpuimentConfig(spawnPrefab);
                    if (config!=null&&config.ContainsKey(equipmentPrefabPath))
                    {
                        SyncString = config[equipmentPrefabPath];
                    }
                }
                break;
            }

        }
    }

    protected void ApplyEquipmentConfg(WeaponLauncher.SyncValueData syncData)
    {
        GameManager.SpawnPrefabAddEquimentConfig(spawnPrefab,equipmentPrefabPath, JsonUtility.ToJson(syncData));
        //UIHangerManger HM = GameObject.FindObjectOfType<UIHangerManger>();
        List<string> currentWeaponPath = new List<string>(GameManager.GetWeaponResourcesPathList());
        if (!currentWeaponPath.Contains(prefabLoadPath))
        {
            currentWeaponPath.Add(prefabLoadPath);
            GameManager.SetWeaponResourcesPathList(currentWeaponPath.ToArray());
        }
    }

    protected void UninstallEquipment()
    {
        List<string> currentWeaponPath = new List<string>(GameManager.GetWeaponResourcesPathList());
        GameManager.SpawnPrefabRemoveEquimentConfig(spawnPrefab, equipmentPrefabPath);
        if (currentWeaponPath.Contains(prefabLoadPath))
        {
            GameManager.SpawnPrefabRemoveEquimentConfig(spawnPrefab, prefabLoadPath);
            currentWeaponPath.Remove(prefabLoadPath);
        }
        GameManager.SetWeaponResourcesPathList(currentWeaponPath.ToArray());
    }

    protected static void ReflushEquipmentPanel()
    {
        UIEquipmentPanel panel = GameObject.FindObjectOfType<UIEquipmentPanel>().GetComponent<UIEquipmentPanel>();
        panel.ReflushMissileView(panel.CurrentSelectEquipmentName);
    }
}
