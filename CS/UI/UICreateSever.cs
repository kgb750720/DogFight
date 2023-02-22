using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using UnityEngine;

public class UICreateSever : MonoBehaviour
{
    [Serializable]
    public enum CreateType
    {
        Sever,
        Host
    }

    public CreateType CreateMode = CreateType.Host;

    public UIRoomController RoomController;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHostMode()
    {
        CreateMode = CreateType.Host;
    }

    public void SetSeverMode()
    {
        CreateMode = CreateType.Sever;
    }

    public void SetRoomServerPassword(TMP_InputField inputFieldRoomPassword)
    {
        RoomController.SetServerPassword(inputFieldRoomPassword.text);
    }

    public void SetRoomClientConnectPassword(TMP_InputField inputFieldRoomPassword)
    {
        RoomController.SetClientConnectPassword(inputFieldRoomPassword.text);
    }

    public void SetCreateRoomName(TMP_InputField inputFieldRoomName)
    {
        RoomController.SetRoomName(inputFieldRoomName.text);
    }

    public void DoCreate(TMP_InputField inputField)
    {
        UriBuilder uriB = new UriBuilder();
        uriB.Port = int.Parse(inputField.text);
        uriB.Host = Dns.GetHostName();
        Uri uri = uriB.Uri;
        switch (CreateMode)
        {
            case CreateType.Sever:
                RoomController.StartRoomSever(uri);
                break;
            case CreateType.Host:
                RoomController.StartRoomHost(uri);
                break;
            default:
                break;
        }
    }
}
