using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;

public class UIRegisterPanel : MonoBehaviour
{
    public TMP_InputField inputEmail;
    public TMP_InputField inputId;
    public TMP_InputField inputPassword;

    public UnityEvent OnMsgRegisterEvents = new UnityEvent();

    // Start is called before the first frame update
    void Start()
    {
        NetManager.AddMsgListener("MsgRegister", OnMsgRegister);
    }

    private void OnMsgRegister(MsgBase msgBase)
    {
        OnMsgRegisterEvents?.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRefisterClick()
    {
        if (inputEmail.text == "" || inputId.text == "" ||inputPassword.text == "")
        {
            return;
        }
        MsgRegister msg = new MsgRegister();
        msg.email = inputEmail.text;
        msg.id = inputId.text;
        msg.pw = inputPassword.text;
        NetManager.Send(msg);
    }
}
