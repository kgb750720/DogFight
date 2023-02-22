using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

public class UIBattlePanel : MonoBehaviour
{
    public FlightView view;

    public bool DrawPanel = true;
    public GameObject ShowPanel;
    public GameObject SpeedPanel;
    public GameObject SattlTip;
    public Text Throttle;
    public Text Speed;
    public GameObject ItemPanel;
    public Text ItemName;
    public Text ItemNub;
    public GameObject WeaponPanel;
    public Text WeaponTittle;
    public Text AmmoText;
    public GameObject CoolingCoolingSlider;
    public GameObject WeaponModelPanel;
    public Text Model;
    private bool multiModelEnable = false;
    WeaponLauncher lastLauncher;
    public Text FireNub;
    public Image WeaponIcon;
    public UIBattleWeaponTip WeaponTip;
    public Text WeaponLauncherName;
    public GameObject HpPanel;
    public GameObject HpSlider;
    public GameObject WarringPanel;
    public GameObject BeLocked;
    public GameObject MissileComing;
    private void Awake()
    {
        
    }

    private void InitBindUI()
    {
        if (!view)
            view = GameObject.FindObjectOfType<FlightView>().GetComponent<FlightView>();
        if (!ShowPanel)
            ShowPanel = transform.Find("ShowPanel").gameObject;
        if (!SpeedPanel)
            SpeedPanel = ShowPanel.transform.Find("SpeedPanel").gameObject;
        if (!SattlTip)
            SattlTip = SpeedPanel.transform.Find("ImageStall").gameObject;
        if (!Throttle)
            Throttle = SpeedPanel.transform.Find("ThrottleShow/Image/Text").GetComponent<Text>();
        if (!Speed)
            Speed = SpeedPanel.transform.Find("SpeedShow/Image/Text").GetComponent<Text>();
        if (!ItemPanel)
            ItemPanel = ShowPanel.transform.Find("ItemPanel").gameObject;
        if (!ItemName)
            ItemName = ItemPanel.transform.Find("Tittle").GetComponent<Text>();
        if (!ItemNub)
            ItemNub = ItemPanel.transform.Find("Image/Text").GetComponent<Text>();
        if (!WeaponPanel)
            WeaponPanel = ShowPanel.transform.Find("WeaponPanel").gameObject;
        if (!CoolingCoolingSlider)
            CoolingCoolingSlider = WeaponPanel.transform.Find("CoolingSlider").gameObject;
        if (!AmmoText)
            AmmoText = WeaponPanel.transform.Find("WeaponAmmo/AmmoText").GetComponent<Text>();
        if (!WeaponModelPanel)
            WeaponModelPanel = WeaponPanel.transform.Find("WeaponModel").gameObject;
        if (!Model)
            Model = WeaponPanel.transform.Find("WeaponModel/Model/ModelText").GetComponent<Text>();
        if (!FireNub)
            FireNub = WeaponPanel.transform.Find("WeaponModel/FireNub/NubText").GetComponent<Text>();
        if (!WeaponIcon)
            WeaponIcon = WeaponPanel.transform.Find("WeaponName/WeaponIcon").GetComponent<Image>();
        if (!WeaponTittle)
            WeaponTittle = WeaponPanel.transform.Find("WeaponName/Tittle").GetComponent<Text>();
        if (!WeaponTip)
            WeaponTip = WeaponPanel.GetComponentInChildren<UIBattleWeaponTip>();
        if (!WeaponLauncherName)
            WeaponLauncherName = WeaponPanel.transform.Find("WeaponName/Tittle").GetComponent<Text>();
        if (!HpPanel)
            HpPanel = ShowPanel.transform.Find("HpPanel").gameObject;
        if (!HpSlider)
            HpSlider = HpPanel.transform.Find("HpSlider").gameObject;
        if (!WarringPanel)
            WarringPanel = ShowPanel.transform.Find("WarringPanel").gameObject;
        if (!BeLocked)
            BeLocked = WarringPanel.transform.Find("BeLocked").gameObject;
        if (!MissileComing)
            MissileComing = WarringPanel.transform.Find("MissileComing").gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitBindUI();
    }

    // Update is called once per frame
    void Update()
    {
        DoDrawUI();
    }


    void DoDrawUI()
    {
        if (!DrawPanel||!view||!view.Target)
        {
            ShowPanel.SetActive(false);
            return;
        }
        else
            ShowPanel.SetActive(true);

        //速度面板
        FlightSystem flight = view.Target.GetComponent<FlightSystem>();
        if (flight.Stall)
            SattlTip.SetActive(true);
        else
            SattlTip.gameObject.SetActive(false);
        Throttle.GetComponent<Text>().text = ((int)(flight.AfterBurner * flight.throttle * 100)).ToString() + "%";
        float speed = flight.Speed * 3.6f;
        int dotLeft = (int)speed;
        int dotRight = (int)(speed * 10) % 10;
        //string speedStr = ().ToString("#,##");   //保留两位
        //int len = speedStr.IndexOf('.') + 3;
        //Speed.GetComponent<Text>().text = speedStr.Substring(0, len) + "km/h";
        Speed.GetComponent<Text>().text = dotLeft + "." + dotRight + "km/h";
        //道具面板
        ItemUse item = view.Target.GetComponent<ItemUse>();
        if (item != null)
        {
            ItemInterfere interfere = item as ItemInterfere;
            if (interfere)
            {
                ItemPanel.SetActive(true);
                ItemName.text = interfere.ItemName + "：";
                ItemNub.GetComponent<Text>().text = interfere.CurrInterfereNub.ToString();
            }
        }
        else
            ItemPanel.SetActive(false);
        //武器面板
        WeaponLauncher weapon = flight.WeaponControl.CurrLauncher;
        if (weapon != null)
        {
            WeaponPanel.SetActive(true);
            //武器名
            if (weapon!=lastLauncher)
            {
                WeaponIcon.sprite = Sprite.Create(weapon.Icon, new Rect(0, 0, weapon.Icon.width, weapon.Icon.height), Vector2.zero);
                XmlNodeList nodeList = GameManager.LoadMxlNodeList("Equipment", "MissileInfo");
                foreach (XmlElement missile in nodeList)
                {
                    if (weapon.MissilePrefab == missile.GetAttribute("MissilePrefabPath") || weapon.MissilePrefab == missile.GetAttribute("MissileNetworkPrefabPath"))
                    {
                        WeaponTittle.text = missile.GetAttribute("ShowName");
                        break;
                    }
                }
                lastLauncher = weapon;
            }
            //锁定模式
            if (weapon.EnableMultiLock)
            {
                WeaponModelPanel.SetActive(true);
                Model.text = weapon.MultiLockModel ? "多锁定" : "单锁定";
                if(weapon.MultiLockModel^multiModelEnable)
                {
                    multiModelEnable = weapon.MultiLockModel;
                    WeaponTip.SetTipForTime("开启" + (weapon.MultiLockModel ? "多锁定" : "单锁定"), 2f);
                }
            }
            else
                WeaponModelPanel.SetActive(false);
            FireNub.text = weapon.FireOnceOutBulletNub.ToString();
            //武器弹药
            AmmoText.text = weapon.Ammo + "/" + weapon.AmmoMax;
            if (weapon.Overheating)
            {
                CoolingCoolingSlider.SetActive(true);
                CoolingCoolingSlider.GetComponent<Slider>().value = weapon.CoolingProcess;
                CoolingCoolingSlider.GetComponentInChildren<Text>().text = "武器温度：" + Mathf.Floor(weapon.CoolingProcess * 100) + "%";
            }
            else
                CoolingCoolingSlider.SetActive(false);
        }
        else
            WeaponPanel.SetActive(false);

        //血量面板
        float hpPercentage= (float)flight.DamageManage.HP / flight.DamageManage.HPmax;
        Slider hpSlider = HpSlider.GetComponent<Slider>();
        hpSlider.value = hpPercentage;
        HpSlider.GetComponentInChildren<Text>().text = flight.DamageManage.HP + "/" + flight.DamageManage.HPmax;
        hpSlider.fillRect.GetComponent<Image>().color = new Color(255, 255f * hpPercentage, 255 * hpPercentage);

        //锁定警告面板
        LockWarring lockWarring = flight.GetComponent<LockWarring>();
        if(lockWarring)
        {
            if(lockWarring.LockerLaunchers.Count>0&&lockWarring.LockerMissiles.Count==0)
            {
                MissileComing.SetActive(false);
                BeLocked.SetActive(true);
            }
            else if(lockWarring.LockerMissiles.Count>0)
            {
                BeLocked.SetActive(false);
                MissileComing.SetActive(true);
            }
            else
            {
                BeLocked.SetActive(false);
                MissileComing.SetActive(false);
            }
        }
        else
        {
            BeLocked.SetActive(false);
            MissileComing.SetActive(false);
        }
    }

    IEnumerator WaitForSetActive(GameObject panel,bool val,float second)
    {
        yield return new WaitForSeconds(second);
        panel.SetActive(val);
    }
}
