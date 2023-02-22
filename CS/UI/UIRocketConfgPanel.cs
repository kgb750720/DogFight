using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRocketConfgPanel : UIBaseConfgPanel
{
    [SerializeField]
    Toggle multiToggle;
    [SerializeField]
    Transform content;
    Transform rocketFirePoints;

    UIHangerManger hangerManger;
    protected override void Awake()
    {
        base.Awake();
        if (!multiToggle)
            multiToggle = GetComponentInChildren<Toggle>();
        if (!content)
            content = transform.Find("Scroll View/Viewport/Content");
        hangerManger = GameManager.Hanger;
        rocketFirePoints = hangerManger.CurrentJetObj.transform.Find("RocketFirePoints");

        //加载空缺导弹挂载位选项
        for (int i = 0; i < rocketFirePoints.childCount; i++)
        {
            if (rocketFirePoints.GetChild(i).childCount == 0)
            {
                GameObject cell = Instantiate(Resources.Load<GameObject>("UI/RocketPositionShowCell"), content);
                cell.GetComponentInChildren<Text>().text = rocketFirePoints.GetChild(i).name;
            }
        }
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (SyncString!="")
        {
            WeaponLauncher.SyncValueData syncData = JsonUtility.FromJson<WeaponLauncher.SyncValueData>(SyncString);
            multiToggle.isOn = syncData.EnableMultiLock;
            foreach (string item in syncData.MissileOuterPosName)
            {
                if(rocketFirePoints.Find(item))
                {
                    GameObject cell = Instantiate(Resources.Load<GameObject>("UI/RocketPositionShowCell"), content);
                    cell.GetComponentInChildren<Toggle>().isOn = true;
                    cell.GetComponentInChildren<Text>().text = item;
                }
            }
        }
        btnSaveConfg.onClick.AddListener(delegate {
            if (equipmentPrefabPath != null)
            {
                WeaponLauncher.SyncValueData syncData = Resources.Load<GameObject>(GameManager.RemovePathPrefixAndSuffix(equipmentPrefabPath)).GetComponent<WeaponLauncher>().Sync;
                List<string> rocketPostion = new List<string>();
                for (int i = 0; i < content.childCount; i++)
                {
                    Transform cell = content.GetChild(i);
                    if (cell.GetComponent<Toggle>().isOn)
                        rocketPostion.Add(cell.GetComponentInChildren<Text>().text);
                }
                syncData.EnableMultiLock = multiToggle.isOn;
                syncData.AmmoMax = rocketPostion.Count;
                syncData.Ammo = rocketPostion.Count;
                syncData.FireOnceOutBulletMaxNub = rocketPostion.Count;
                syncData.MissileOuterPosNameJons = JsonConvert.SerializeObject(rocketPostion);
                //GameManager.AddEquimentConf(prefab, JsonUtility.ToJson(syncData));
                if (rocketPostion.Count > 0)
                    ApplyEquipmentConfg(syncData);
                else
                    UninstallEquipment();
            }
            ReflushEquipmentPanel();
            GameObject.Destroy(gameObject);
        });
        btnClose.onClick.AddListener(delegate { GameObject.Destroy(gameObject); });
        btnUninstall.onClick.AddListener(delegate {
            UninstallEquipment();
            ReflushEquipmentPanel();
            GameObject.Destroy(gameObject);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
