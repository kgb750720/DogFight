using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using Mirror;

public class UIMinigunConfgPanel : UIBaseConfgPanel
{
    [SerializeField]
    private Toggle noCoolingToggle;
    

    protected override void Awake()
    {
        base.Awake();
        if (!noCoolingToggle)
            noCoolingToggle = GetComponentInChildren<Toggle>();
    }
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (SyncString != "")
            noCoolingToggle.isOn = JsonUtility.FromJson<WeaponLauncher.SyncValueData>(SyncString).InfinityAmmo;
        btnSaveConfg.onClick.AddListener(delegate
        {
            if (equipmentPrefabPath != null)
            {
                WeaponLauncher.SyncValueData syncData = Resources.Load<GameObject>(GameManager.RemovePathPrefixAndSuffix(equipmentPrefabPath)).GetComponent<WeaponLauncher>().Sync;
                syncData.InfinityAmmo = noCoolingToggle.isOn;
                ApplyEquipmentConfg(syncData);
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
