using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Events;
using Michsky.UI.Shift;

public class UILoginPanel : MonoBehaviour
{
    public TMP_InputField EmailInput;
    public TMP_InputField pwInput;

    public SwitchManager switchManager;

    public UnityEvent OnLoginEvents = new UnityEvent();

    public struct UserLoginInfoCache
    {
        public string email;
        public string pw;
    }

    // Start is called before the first frame update
    void Start()
    {
        NetManager.AddEventListener(NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddEventListener(NetEvent.ConnectFail, OnConnectFail);
        NetManager.AddMsgListener("MsgLoginResult", OnMsgLoginResult);
    }

    private void OnConnectFail(string err)
    {
        print("connect fail");
    }

    private void OnConnectSucc(string err)
    {
        print("connect succ");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnLoginClick()
    {
        if (EmailInput.text == "" || pwInput.text == "")
        {
            //PanelManager.Open<TipPanel>("用户名密码不能为空");
            return;
        }
        MsgLogin msg = new MsgLogin();
        msg.email = EmailInput.text;
        msg.pw = pwInput.text;
        NetManager.Send(msg);
    }
    private void OnMsgLoginResult(MsgBase msgBase)
    {
        MsgLoginResult msg = (MsgLoginResult)msgBase;

        if (msg.result == 0)
        {
            print("login succ");
            GameManager.Manager.PlayerId = msg.id;
            OnLoginEvents?.Invoke();
            if (switchManager.isOn)
                WirteLoginCacheInfo();
        }
        else
        {
            print("login fail");
        }
    }

    public void ReadLoginCacheInfo()
    {
        if (!PlayerPrefs.HasKey("LoginCacheInfo")|| EmailInput.text!=""|| pwInput.text!="")
            return;
        //当存在LoginCacheInfo缓存且EmailInput.text和pwInput.text都没有内容时（页面第一次载入时）启动
        string json = PlayerPrefs.GetString("LoginCacheInfo");
        UserLoginInfoCache cache = JsonUtility.FromJson<UserLoginInfoCache>(json);
        EmailInput.text = cache.email;
        pwInput.text = cache.pw;
    }

    private void WirteLoginCacheInfo()
    {
        UserLoginInfoCache cache = new UserLoginInfoCache { email = EmailInput.text, pw = pwInput.text };
        PlayerPrefs.SetString("LoginCacheInfo", JsonUtility.ToJson(cache));
    }
}
