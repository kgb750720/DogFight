using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Flight;



public partial struct ItemUseSyncData
{
    public int MaxInterfereNub;
    public float ReloadIntervalTime;
    public int ReloadInterfereNub;
    public float InterfereAliveTime;

    public ItemUseSyncData(int maxInterfereNub, float reloadIntervalTime, int reloadInterfereNub, float interfereAliveTime)
    {
        MaxInterfereNub = maxInterfereNub;
        ReloadIntervalTime = reloadIntervalTime;
        ReloadInterfereNub = reloadInterfereNub;
        InterfereAliveTime = interfereAliveTime;
    }
}

[RequireComponent(typeof(LockWarring))]
public class ItemInterfere : ItemUse
{
    public GameObject InterferePrefab;
    public GameObject LuancherPoint;
    public float UseCD = 0.2f;
    public int CurrInterfereNub=-1;

    public int MaxInterfereNub=10;
    public float ReloadIntervalTime = 10f;
    public int ReloadInterfereNub = 2;

    public float InterfereAliveTime = 10f;

    
    public override string ItemName { get => "ÈÈÓÕµ¯";}
    public override ItemUseSyncData Sync 
    { 
        get => new ItemUseSyncData(MaxInterfereNub,ReloadIntervalTime,ReloadInterfereNub,InterfereAliveTime); 
        set
        {
            MaxInterfereNub = value.MaxInterfereNub;
            ReloadIntervalTime = value.ReloadIntervalTime;
            ReloadInterfereNub = value.ReloadInterfereNub;
            InterfereAliveTime = value.InterfereAliveTime;
        }
    }


    public LockWarring Warring;

    public AudioSource FireSound;

    bool Using = false;
    float ReloadTimeCount;
    bool luancherLeft = true;
    float lastUseTime;
    private void Awake()
    {
        if (CurrInterfereNub < 0)
            CurrInterfereNub = MaxInterfereNub;
        lastUseTime -= UseCD;
        ReloadTimeCount -= ReloadIntervalTime;

        PlayerController controller = gameObject.GetComponent<PlayerController>();
        if (controller)
            controller.Item = this;
        if (gameObject.GetComponent<LockWarring>())
            Warring = gameObject.GetComponent<LockWarring>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    public override bool Used { get => base.Used; set => base.Used = value; }

    // Update is called once per frame
    void Update()
    {
        if (Used)
            UseItem();
    }

    private void LateUpdate()
    {
        if (Using && Time.time - lastUseTime > UseCD && CurrInterfereNub > 0)
        {
            lastUseTime = Time.time;
            Using = false;
            float force = 1000;
            CurrInterfereNub -= 1;
            GameObject obj = Instantiate(InterferePrefab, transform.position, transform.rotation);
            Rigidbody objRig = obj.GetComponent<Rigidbody>();
            objRig.velocity = gameObject.GetComponent<Rigidbody>().velocity;
            if (luancherLeft)
                objRig.AddForce(transform.right * (-force));
            else
                objRig.AddForce(transform.right * force);
            luancherLeft = !luancherLeft;

            if (GetComponent<JetSync>())
                GetComponent<JetSync>().BroadCastInstance(obj);
            obj.GetComponentInChildren<AudioSource>().spatialBlend = 0;
            Destroy(obj, InterfereAliveTime);
            if (Warring && Warring.LockerLaunchers.Count > 0)
            {
                WeaponLauncher[] launchers = new WeaponLauncher[Warring.LockerLaunchers.Count];
                Warring.LockerLaunchers.CopyTo(launchers);
                for (int i = launchers.Length - 1; i >= 0; i--)
                {
                    if (launchers[i])
                        launchers[i].Unlock(gameObject);
                }
            }
            if (Warring && Warring.LockerMissiles.Count > 0)
            {
                MoverMissile[] missiles = new MoverMissile[Warring.LockerMissiles.Count];
                Warring.LockerMissiles.CopyTo(missiles);
                if (missiles.Length > 0 && missiles[missiles.Length - 1])
                {
                    missiles[missiles.Length - 1].Target = obj;
                    Warring.LockerMissiles.Remove(missiles[missiles.Length - 1]);
                }
            }
        }

        if (Time.time - ReloadTimeCount > ReloadIntervalTime&&Time.time-lastUseTime>ReloadIntervalTime)
        {
            ReloadTimeCount = Time.time;
            if (CurrInterfereNub + ReloadInterfereNub > MaxInterfereNub)
                CurrInterfereNub = MaxInterfereNub;
            else
                CurrInterfereNub += ReloadInterfereNub;
        }
    }

    public override void UseItem()
    {
        if (Time.time - lastUseTime > UseCD&&CurrInterfereNub>0)
            Using = true;
    }

   
}
