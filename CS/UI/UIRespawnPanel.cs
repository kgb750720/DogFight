using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using UnityEngine.UI;

public class UIRespawnPanel : MonoBehaviour
{
    public bool DrawPanel = true;
    public GameObject ShowPanel;
    public GameObject EquipmentPanel;


    string jetPath = "";
    private void Awake()
    {
        if(!ShowPanel)
            ShowPanel = transform.Find("ShowPanel").gameObject;
        if (!EquipmentPanel)
            EquipmentPanel = ShowPanel.transform.Find("EquipmentPanel").gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        DoDramPanel();
    }

    void DoDramPanel()
    {
        if (!DrawPanel)
        {
            ShowPanel.SetActive(false);
            return;
        }
        else
            ShowPanel.SetActive(true);
        Transform jetItem = EquipmentPanel.transform.GetChild(0);
        jetItem.gameObject.SetActive(false);
        //for (int i = 0; i < jetItem.childCount; i++)
        //    jetItem.GetChild(i).gameObject.SetActive(false);
        jetPath = GameManager.SpawnPrefabResourcesPath;
        foreach (XmlElement item in GameManager.LoadMxlNodeList("Equipment", "JetInfo"))
        {
            if (item.GetAttribute("JetPrefabPath") ==jetPath||item.GetAttribute("JetNetworkPrefabPath") ==jetPath)
            {
                //for(int i=0;i<jetItem.childCount;i++)
                //{
                //    jetItem.GetChild(i).gameObject.SetActive(true);
                //    jetItem.GetChild(i).GetComponent<Text>().text = item.GetAttribute("JetName");
                //    jetItem.GetChild(i).GetComponent<Image>().sprite = Instantiate(Resources.Load<Sprite>(item.GetAttribute("JetImagePath")));
                //}
                jetItem.gameObject.SetActive(true);
                jetItem.GetComponentInChildren<Text>().text= item.GetAttribute("JetName");
                jetItem.transform.Find("Image").GetComponent<Image>().sprite = Instantiate(Resources.Load<Sprite>(item.GetAttribute("JetImagePath")));
                break;
            }
        }
        string[] weaponList = GameManager.GetWeaponResourcesPathList();
        for (int i=1;i<EquipmentPanel.transform.childCount;i++)
        {
            Transform Item = EquipmentPanel.transform.GetChild(i);
            if (i-1 < weaponList.Length)
            {
                foreach (XmlElement weapon in GameManager.LoadMxlNodeList("Equipment", "WeaponInfo"))
                {
                    if (weapon.GetAttribute("WeaponPrefabPath") == weaponList[i-1] || weapon.GetAttribute("WeaponNetworkPrefabPath") == weaponList[i-1])
                    {
                        Item.gameObject.SetActive(true);
                        Item.GetComponentInChildren<Text>().text = weapon.GetAttribute("ShowName");
                        Texture2D luancherIcon = Resources.Load<GameObject>(GameManager.RemovePathPrefixAndSuffix(weaponList[i-1])).GetComponent<WeaponLauncher>().Icon;
                        Item.transform.Find("Image").GetComponent<Image>().sprite = Sprite.Create(luancherIcon, new Rect(0, 0, luancherIcon.width, luancherIcon.height), Vector2.zero);
                        break;
                    }

                }
            }
            else
                Item.gameObject.SetActive(false);
        }
    }
}
