using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using kcp2k;
using JetNetwork;
using Mirror.Authenticators;

public class UIRoomController : MonoBehaviour
{
    public NetworkManager networkManager;
    public NetworkRoomInfoDiscovery networkDiscovery;

    public bool InitClearBasicAuthenticatorInifo = false;
    // Start is called before the first frame update
    void Start()
    {
        if(!networkManager)
            networkManager = NetworkManager.singleton;
        if (!networkDiscovery)
            networkDiscovery = NetworkManager.singleton.GetComponent<NetworkRoomInfoDiscovery>();
        if(InitClearBasicAuthenticatorInifo&&networkManager.authenticator is BasicAuthenticator)
        {
            BasicAuthenticator ba = networkManager.authenticator as BasicAuthenticator;
            ba.serverUsername = "";
            ba.serverPassword = "";
            ba.username = "";
            ba.password = "";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ExitRoom()
    {
        if (NetworkServer.active)
        {
            if (networkManager.mode == NetworkManagerMode.ServerOnly)
                networkManager.StopServer();
            else
                networkManager.StopHost();
        }
        else
            networkManager.StopClient();
    }

    public void StartRoomHost(Uri uri)
    {
        networkManager.networkAddress = uri.Host;
        if (uri.Port != 0)
        {
            KcpTransport transport = networkManager.GetComponent<KcpTransport>();
            if(transport)
            {
                transport.Port = (ushort)uri.Port;
            }
        }
        networkManager.StartHost();
        networkDiscovery.AdvertiseServer();
    }

    public void SetServerPassword(string password)
    {
        if (networkManager.authenticator is BasicAuthenticator)
        {
            BasicAuthenticator ba = networkManager.authenticator as BasicAuthenticator;
            ba.serverPassword = password;
            ba.password = password;
        }
    }

    public void SetClientConnectPassword(string password)
    {
        if (networkManager.authenticator is BasicAuthenticator)
        {
            BasicAuthenticator ba = networkManager.authenticator as BasicAuthenticator;
            ba.serverPassword = password;
            ba.password = password;
        }
    }

    public void SetRoomName(string RoomName)
    {
        if(networkManager is NetworkPlayingRoomManager)
        {
            NetworkPlayingRoomManager manager = networkManager as NetworkPlayingRoomManager;
            manager.RoomName = RoomName;
        }
    }

    public void StartRoomSever(Uri uri)
    {
        networkManager.networkAddress = uri.Host;
        if (uri.Port != 0)
        {
            KcpTransport transport = networkManager.GetComponent<KcpTransport>();
            if (transport)
            {
                transport.Port = (ushort)uri.Port;
            }
        }
        networkManager.StartServer();
        networkDiscovery.AdvertiseServer();
    }

    public void StartClient(Uri uri)
    { 
        networkManager.StartClient(uri);
    }
}
