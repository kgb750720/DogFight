using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using Mirror;

public class UIEquipmentPanel : MonoBehaviour
{
    Toggle[] toggles;
    [SerializeField]
    GameObject missileSelectView;
    [SerializeField]
    Transform content;

    string lastSelectName = "";
    public string CurrentSelectEquipmentName { get => lastSelectName; }

    GameObject panel = null;

    private void Awake()
    {
        toggles = GetComponentsInChildren<Toggle>();
        if (!missileSelectView)
            missileSelectView = transform.Find("Missile Select Scroll View").gameObject;
        if (!content)
            content = transform.Find("Missile Select Scroll View/Viewport/Content");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        string selectWeaponName = "";
        foreach (var item in toggles)
        {
            if(item.isOn)
                selectWeaponName = item.name;
        }

        if (selectWeaponName != lastSelectName)
        {

            if (panel != null)
                GameObject.Destroy(panel);
            if (missileSelectView)
            {
                if (selectWeaponName == "")
                {
                    missileSelectView.SetActive(false);
                }
                else
                {
                    missileSelectView.SetActive(true);
                    ReflushMissileView(selectWeaponName);
                }
            }
            lastSelectName = selectWeaponName;
        }
    }

    private void OnDestroy()
    {
        if (panel != null)
            GameObject.Destroy(panel);
    }

    private void OnDisable()
    {
        if (panel != null)
            GameObject.Destroy(panel);
    }

    public void ReflushMissileView(string selectWeaponName)
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            GameObject.Destroy(content.GetChild(i).gameObject);
        XmlElement equipemnt = GameManager.LoadMXL("Equipment");
        XmlElement missileInfo = equipemnt.SelectSingleNode("MissileInfo") as XmlElement;
        bool isNetwork = GameObject.FindObjectOfType<NetworkManager>();
        foreach (XmlElement missile in missileInfo.ChildNodes)
        {
            string[] launcheNameparts = missile.GetAttribute("LauncherWeapon").Split('_');
            if (launcheNameparts[0] == selectWeaponName)
            {
                string missileName = missile.GetAttribute("ShowName");
                string missilePrefabPath = isNetwork ? missile.GetAttribute("MissileNetworkPrefabPath") : missile.GetAttribute("MissilePrefabPath");    //ShowHanger显示的均为非Network预制体
                GameObject missileCell = Instantiate(Resources.Load<GameObject>("UI/MissileShowCell"), content);
                missileCell.GetComponentInChildren<Text>().text = missileName;
                UIHangerManger HM = GameObject.FindObjectOfType<UIHangerManger>();
                //WeaponLauncher[] launchers = HM.CurrentJetObj.GetComponentsInChildren<WeaponLauncher>();
                //List<WeaponLauncher> launchers = new List<WeaponLauncher>();
                foreach (var item in HM.CurrentWeaponPrefabList)
                {
                    GameObject prefab = Resources.Load<GameObject>(GameManager.RemovePathPrefixAndSuffix(item));
                    if(prefab.GetComponent<WeaponLauncher>().MissilePrefab== missilePrefabPath)
                        missileCell.GetComponent<Image>().color = new Color(120, 120, 120);
                }
                missileCell.GetComponent<Button>().onClick.AddListener(delegate
                {
                    if (panel!=null)
                        GameObject.Destroy(panel);
                    switch (launcheNameparts[0])
                    {
                        case "Minigun":
                            panel = Instantiate(Resources.Load<GameObject>("UI/MiniGunConfgPanel"), GameObject.Find("Canvas").transform);
                            break;
                        case "Rocket":
                            panel = Instantiate(Resources.Load<GameObject>("UI/MissileConfgPanel"), GameObject.Find("Canvas").transform);
                            break;
                        default:
                            break;
                    }
                    if (panel != null)
                    {
                        panel.GetComponent<UIBaseConfgPanel>().xmlElementMissile = missile;
                    }
                });
            }
        }
    }
}
