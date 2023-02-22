using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using UnityEngine.Events;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class JetSync : SyncBase
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

    public UnityEvent OnDestroyEvent;

    [SyncVar]
    public GameObject ownerPlayerObj;

    [SyncVar(hook = nameof(OnFilghtSyncDataChanged))]
    FlightSystem.SyncData flightSyncData;
    public void OnFilghtSyncDataChanged(FlightSystem.SyncData oldData, FlightSystem.SyncData newData)
    {
        flightSyncData = newData;
    }
    //[Command]
    //void CmdSetFlightSyncData(FlightSystem.SyncData data)
    //{
    //    flightSyncData = data;
    //}

    [SyncVar(hook = nameof(OnDamegeSyncDataChanged))]
    DamageManager.SyncData damageSyncData;
    public void OnDamegeSyncDataChanged(DamageManager.SyncData oldData, DamageManager.SyncData newData)
    {
        damageSyncData = newData;
    }

    [SyncVar(hook = nameof(OnWeaponControllerSyncDataChanged))]
    WeaponController.SyncData weaponControllerSyncData;
    public void OnWeaponControllerSyncDataChanged(WeaponController.SyncData oldData, WeaponController.SyncData newData)
    {
        weaponControllerSyncData = newData;
    }

    [SyncVar(hook = nameof(OnItemSyncDataChanged))]
    ItemUseSyncData itemSyncData;
    public void OnItemSyncDataChanged(ItemUseSyncData oldData, ItemUseSyncData newData)
    {
        itemSyncData = newData;
    }

    [SyncVar(hook = nameof(OnSyncLockerLaunchersChanged))]
    SyncList<GameObject> lockwarringSyncLockerLaunchers = new SyncList<GameObject>();
    public void OnSyncLockerLaunchersChanged(SyncList<GameObject> oldData, SyncList<GameObject> newData)
    {
        lockwarringSyncLockerLaunchers = newData;
    }

    [SyncVar(hook = nameof(OnSyncLockerMissilesChanged))]
    SyncList<GameObject> lockwarringSyncLockerMissiles = new SyncList<GameObject>();
    public void OnSyncLockerMissilesChanged(SyncList<GameObject> oldData, SyncList<GameObject> newData)
    {
        lockwarringSyncLockerMissiles = newData;
    }



    FlightSystem flight;
    NetworkPlayingRoomGameModelPlayer modelPlayer;

    AIController _AICtrl = null;

    protected override void Awake()
    {
        base.Awake();
        flight = GetComponent<FlightSystem>();
        _AICtrl = GetComponent<AIController>();
    }

    private void Start()
    {
        flightSyncData = flight.Sync;
        damageSyncData = flight.DamageManage.Sync;
        weaponControllerSyncData = flight.WeaponControl.Sync;
        itemSyncData = GetComponent<ItemUse>().Sync;
        if (netIdentity.hasAuthority)
        {
            flight.EngineSound1.spatialBlend = 0;
            flight.EngineSound2.spatialBlend = 0;
            if (!GetComponent<PlayerManager>())
                gameObject.AddComponent<PlayerManager>();
            GetComponent<RadarSystem>().enabled = true;
            GameObject.FindObjectOfType<FlightView>().Target = gameObject;
        }
        if (ownerPlayerObj)
        {
            //以下语句需要在项目设置中设置JetSync脚本先于FlightSystem执行时才有效果
            flight.Speed = ownerPlayerObj.GetComponent<FlightInit>().Speed;
            flight.throttle = ownerPlayerObj.GetComponent<FlightInit>().Throttle;
            flight.LandGearOpen = ownerPlayerObj.GetComponent<FlightInit>().OpenLandGear;
        }

        if (_AICtrl && isClientOnly)
            _AICtrl.enabled = false;
        //绑定死亡事件
        //Respawn rp = ownerPlayerObj.GetComponent<Respawn>();
        //NetworkPlayingRoomGameModelPlayer modelPlayer;
        //if (rp)
        //{
        //    modelPlayer = rp.modelPlayer;
        //    void deadCallback(Queue<DamagePackage> killerCount)
        //    {
        //        EventsManager<NetworkPlayingRoomGameModelPlayer, Queue<DamagePackage>>.Invoke(GameModelCtrl.DeadEvet, modelPlayer, killerCount);
        //    }
        //    flight.DamageManage.DeadEvent.AddListener(deadCallback);
        //}
    }

    

    protected override void Update()
    {
        base.Update();
        FlightSystem.SyncData flightSync = flight.Sync;
        if (!netIdentity.hasAuthority)
        {
            flight.Sync = flightSyncData;
            if (flight.isGround||(_AICtrl&&isServer))
                flight.AerodynamicSimulationEnable = true;
            else
                flight.AerodynamicSimulationEnable = false;
        }
        else
            flightSyncData = flightSync;
        //    CmdSetFlightSyncData(flight.Sync);

        if (!_AICtrl)
        {
            if (isServer)
            {
                damageSyncData = flight.DamageManage.Sync;
                weaponControllerSyncData = flight.WeaponControl.Sync;
                if (GetComponent<ItemUse>() != null)
                    itemSyncData = GetComponent<ItemUse>().Sync;
                lockwarringSyncLockerLaunchers = GetComponent<LockWarring>().SyncLockerLaunchers;
                lockwarringSyncLockerMissiles = GetComponent<LockWarring>().SyncLockerMissiles;
            }
            else
            {
                flight.DamageManage.Sync = damageSyncData;
                flight.WeaponControl.Sync = weaponControllerSyncData;
                if (GetComponent<ItemUse>() != null)
                    GetComponent<ItemUse>().Sync = itemSyncData;
                GetComponent<LockWarring>().SyncLockerLaunchers = lockwarringSyncLockerLaunchers;
                GetComponent<LockWarring>().SyncLockerMissiles = lockwarringSyncLockerMissiles;
            }
        }
    }

    bool Trigger = false;

    private void FixedUpdate()
    {
        if (Trigger)
            flight.WeaponControl.LaunchWeapon();
    }

    private void OnDestroy()
    {
        if (GetComponent<Flight.PlayerController>())
            GetComponent<Flight.PlayerController>().enabled = false;
        OnDestroyEvent?.Invoke();
    }


    [Command]
    public void CmdSwtichWeapon()
    {
        flight.WeaponControl.SwitchWeapon();
    }

    [Command]
    public void CmdUsed(bool isUsed)
    {
        ItemUse item = GetComponent<ItemUse>();
        if(item!=null)
        {
            item.Used = isUsed;
        }
    }

    [Command]
    public void PressTrigger(bool isPress)
    {
        Trigger = isPress;
    }

    [Command]
    public void CmdSyncAddLauncherOnceFireNub(int v)
    {
        WeaponController controller = flight.WeaponControl;
        if (controller.CurrLauncher && controller.CurrLauncher.MultiLockModel)
            controller.AddLauncherOnceFireNub(controller.CurrentWeaponIdx, v);
    }

    [Command]
    public void CmdSyncSwitchLauncherLockModel()
    {
        WeaponController controller = flight.WeaponControl;
        controller.SwitchLauncherLockModel(controller.CurrentWeaponIdx);
    }

    protected override void OnSyncTagDifTag(string syncTag)
    {
        base.OnSyncTagDifTag(syncTag);
        RadarSystem radar = GetComponent<RadarSystem>();
        if(radar&&hasAuthority)
        {
            radar.ResetEnemyTag();
        }
    }

    //可能会改变运行时的MainRot属性，不建议粗暴的同步Sync
    //[ServerCallback]
    //public void SCallbackSetSync(FlightSystem.SyncData syncData)
    //{
    //    if (flight)
    //        flight.Sync = syncData;
    //    else
    //        GetComponent<FlightSystem>().Sync = syncData;
    //    CRpcSetSync(syncData);
    //}

    //[ClientRpc]
    //void CRpcSetSync(FlightSystem.SyncData syncData)
    //{
    //    if (flight)
    //        flight.Sync = syncData;
    //    else
    //        GetComponent<FlightSystem>().Sync = syncData;
    //}

    //依然有问题
    //[ServerCallback]
    //public void SetFlightInit(float Speed,float Throttle,bool LandgearOpen)
    //{
    //    //if (flight)
    //    //{
    //    //    flight.Speed = Speed;
    //    //    flight.throttle = Throttle;
    //    //    flight.LandGearOpen = LandgearOpen;
    //    //}
    //    //else
    //    //{
    //    //    GetComponent<FlightSystem>().Speed = Speed;
    //    //    GetComponent<FlightSystem>().throttle = Throttle;
    //    //    GetComponent<FlightSystem>().LandGearOpen = LandgearOpen;
    //    //}
    //    CRpcSetInit(Speed, Throttle, LandgearOpen);
    //}

    //[ClientRpc]
    //private void CRpcSetInit(float Speed, float Throttle, bool LandgearOpen)
    //{
    //    if (netIdentity.hasAuthority)
    //    {
    //        if (flight)
    //        {
    //            flight.Speed = Speed;
    //            flight.throttle = Throttle;
    //            flight.LandGearOpen = LandgearOpen;
    //        }
    //        else
    //        {
    //            GetComponent<FlightSystem>().Speed = Speed;
    //            GetComponent<FlightSystem>().throttle = Throttle;
    //            GetComponent<FlightSystem>().LandGearOpen = LandgearOpen;
    //        }
    //    }
    //}

    //[ServerCallback]
    //public void SCallbackSetJetTag(string tag)
    //{
    //    gameObject.tag = tag;
    //    foreach (var item in flight.WeaponControl.WeaponList)
    //        item.ResetTargetTag(tag);
    //    CRpcSetJetTag(tag);
    //}

    //[ClientRpc]
    //private void CRpcSetJetTag(string tag)
    //{
    //    gameObject.tag = tag;
    //    foreach (var item in flight.WeaponControl.WeaponList)
    //        item.ResetTargetTag(tag);
    //    if (netIdentity.hasAuthority && GetComponent<RadarSystem>().enabled)
    //        GetComponent<RadarSystem>().ResetEnemyTag();
    //}
}
