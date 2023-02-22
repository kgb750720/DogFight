using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIBtnEquipmentPanel : MonoBehaviour
{
    public List<RectTransform> ControlUIList = new List<RectTransform>();

    public UIHangerManger UIHanger;
    public UIMainPanelManager panelManager;
    public Animator MainPanelAnimator;
    Button btn;
    public string AroundPanelName = "";
    private void Awake()
    {
        //if(!panelManager)
        //    transform.parent.GetComponentInChildren<UIMainPanelManager>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if(!UIHanger)
            UIHanger= GameManager.Hanger.GetComponent<UIHangerManger>();
        UIHanger.OpenShowHanger(null);
        btn = GetComponent<Button>();
        //btn.enabled = false;
        btn.onClick.AddListener(delegate
        {
            //panelManager.CloseCurrentUIProcess();
            foreach (var item in ControlUIList)
                {
                    item.gameObject.SetActive(false);
                }
                btn.enabled = false;
                GameManager.Hanger.Touch = true;
                GameManager.Hanger.Around = false;
                GameManager.Hanger.EquipmentUIShow = true;
            }
        );
        UIHanger.BtnBack.onClick.AddListener(BackHomePanel);
    }

    // Update is called once per frame
    void Update()
    {
        //if (panelManager.currentPanel.name == AroundPanelName&& !GameManager.Hanger.EquipmentUIShow)
        //    GameManager.Hanger.Around = true;
    }

    public void BackHomePanel()
    {
        foreach (var item in ControlUIList)
        {
            item.gameObject.SetActive(true);
        }
        btn.enabled = true;
        GameManager.Hanger.Touch = false;
        GameManager.Hanger.Around = true;
        GameManager.Hanger.EquipmentUIShow = false;
        MainPanelAnimator.Play("Start");
        panelManager.OpenFirstTab();
        //panelManager.panels[panelManager.currentPanelIndex].panelObject.GetComponent<Animator>().Play("Start");
    }

    //public void BtnEnable(bool enable)
    //{
    //    btn.enabled = enable;
    //}

    private void OnDestroy()
    {
        UIHanger.CloseShowHanger();
    }

    public void SetHangerArround(bool enableArround)
    {
        UIHanger.Around = enableArround;
    }

    public void SetHangerTouch(bool enableTouch)
    {
        UIHanger.Touch = enableTouch;
    }
}
