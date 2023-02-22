using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.UI;

public class UIJetSelectPanel : MonoBehaviour
{
    [SerializeField]
    Transform content;


    private void Awake()
    {

        if (!content)
            content = transform.Find("Viewport/Content");
        GameObject prefabJetCell = Resources.Load("UI/JetShowCell") as GameObject;
        XmlElement equipment = GameManager.LoadMXL("Equipment");
        XmlElement jetInfo = equipment.SelectSingleNode("JetInfo") as XmlElement;
        bool isNetwork = GameObject.FindObjectOfType<NetworkManager>()!=null;
        foreach (XmlElement item in jetInfo.ChildNodes)
        {
            string prefabPath = isNetwork ? item.GetAttribute("JetNetworkPrefabPath") : item.GetAttribute("JetPrefabPath");
            string imagePath = item.GetAttribute("JetImagePath");
            string jetName = item.GetAttribute("JetName");
            GameObject cellObj = Instantiate(prefabJetCell, content);
            cellObj.GetComponentInChildren<Text>().text = jetName;
            Sprite sp = Instantiate(Resources.Load<Sprite>(imagePath));
            cellObj.transform.Find("Image").GetComponent<Image>().sprite = sp;
            if (prefabPath == GameManager.SpawnPrefabResourcesPath)
                cellObj.GetComponent<Image>().color = new Color(0, 0, 0);
            cellObj.GetComponent<Button>().onClick.AddListener(delegate{ 
                GameManager.SpawnPrefabResourcesPath = prefabPath;
                for(int i=0;i<content.childCount;i++)
                {
                    if (content.GetChild(i) == cellObj.transform)
                        cellObj.transform.GetComponent<Image>().color = new Color(0, 0, 0);
                    else
                        content.GetChild(i).GetComponent<Image>().color = new Color(255, 255, 255);
                }
            });
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
    }

}
