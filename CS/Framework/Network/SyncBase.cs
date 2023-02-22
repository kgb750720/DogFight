using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Reflection;
using System;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class SyncBase : NetworkBehaviour
{
    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// Invoked on the server when the object is unspawned
    /// <para>Useful for saving object data in persistent storage</para>
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() { }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() { }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() { }

    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnection parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() { }

    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }

    #endregion
    
    public GameObject OfflinePrefab;

    [SyncVar(hook = nameof(OnSyncTagChanged))]
    protected string syncTag;
    private void OnSyncTagChanged(string oldSyncTag, string newSyncTag)
    {
        syncTag = newSyncTag;
    }
    [ServerCallback]
    public void SCallbackSetSyncTag(string tag)
    {
        syncTag = tag;
    }

    Transform lastParent;
    [SyncVar]
    NetworkIdentity parentIdenty = null;


    [ServerCallback]
    public void BroadCastInstance(GameObject obj)
    {
        NetworkServer.Spawn(obj);
    }

    //当玩家加入其他玩家主持的服务器时无法在同步加入端玩家无法正常同步transform父物体
    //public void BroadCastSetParent(Transform parent)
    //{
    //    if (isClient)
    //        ClientBroadCastExecute("setParent", parent.GetComponent<NetworkIdentity>());
    //    else if (isServer)
    //        ServerBroadCastExecute("setParent", parent.GetComponent<NetworkIdentity>());
    //}

    protected void setParent(NetworkIdentity parentIdentity)
    {
        transform.parent = parentIdentity.transform;
    }


    //public void BroadCastSetTag(string tag)
    //{
    //    if (netIdentity.isClient)
    //        CmdSetTag(tag);
    //    else if (isServer)
    //        SCallbackSetTag(tag);
    //}

    //[Command]
    //void CmdSetTag(string tag)
    //{
    //    gameObject.tag = tag;
    //    CRpcSetTag(tag);
    //}

    //[ServerCallback]
    //void SCallbackSetTag(string tag)
    //{
    //    gameObject.tag = tag;
    //    CRpcSetTag(tag);
    //}

    //[ClientRpc]
    //void CRpcSetTag(string tag)
    //{
    //    gameObject.tag = tag;
    //}

    [ServerCallback]
    void SCallBack(string methodName, params NetworkIdentity[] methodParams)
    {
        MethodInfo method = this.GetType().GetMethod(methodName,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
        method.Invoke(this, methodParams);
    }
    [ClientRpc]
    void CRpc(string methodName, params NetworkIdentity[] methodParams)
    {

        MethodInfo method = this.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        method.Invoke(this, methodParams);
    }

    /// <summary>
    /// 客户端发起的广播函数执行
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="methodParams"></param>
    [Command]
    protected void ClientBroadCastExecute(string methodName, params NetworkIdentity[] methodParams)
    {
        SCallBack(methodName, methodParams);
        CRpc(methodName, methodParams);
    }

    /// <summary>
    /// 服务器发起的广播函数执行
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="methodParams"></param>
    [ServerCallback]
    protected void ServerBroadCastExecute(string methodName, params NetworkIdentity[] methodParams)
    {
        SCallBack(methodName, methodParams);
        CRpc(methodName, methodParams);
    }

    protected virtual void Awake()
    {
        syncTag = gameObject.tag;
    }

    protected virtual void Update()
    {
        if (gameObject.tag != syncTag)
            OnSyncTagDifTag(syncTag);
        if(isServer)
        {
            if (transform.parent != lastParent)
            {
                lastParent = transform.parent;
                NetworkIdentity newIdenty = transform.parent.GetComponent<NetworkIdentity>();
                parentIdenty = newIdenty;
            }
        }
        else
        {
            if(parentIdenty!=null&&parentIdenty.transform!=transform.parent)
            {
                transform.parent = parentIdenty.transform;
            }
        }
    }

    protected virtual void OnSyncTagDifTag(string syncTag)
    {
        gameObject.tag = syncTag;
    }


    [ServerCallback]
    public virtual void SCallbackReleaseCallback()
    {
        GetComponent<PoolInstanceBase>().DoGetProcess();
        CRpcReleaseCallback();
    }

    [ServerCallback]
    public virtual void SCallbackGetCallback() 
    {
        GetComponent<PoolInstanceBase>().DoGetProcess();
        CRpcGetallback();
    }

    [ClientRpc]
    public virtual void CRpcGetallback()
    {
        GetComponent<PoolInstanceBase>().DoGetProcess();
    }

    [ClientRpc]
    public virtual void CRpcReleaseCallback() 
    {
        GetComponent<PoolInstanceBase>().DoReleaseProcess();
    }

}
