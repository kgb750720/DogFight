using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using Mirror;
using System.Linq;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public enum WeaponType
{
	PrimaryWeapon,
	SecondaryWeapon,
	ThirdWeapon,
	FourthWeapon,
	Other
}

[RequireComponent(typeof(AudioSource))]
public class WeaponLauncher : WeaponBase
{
	[SerializeField]
	private bool onActive;
	public bool OnActive 
	{
		get => onActive;
		set
        {
			if(value&&lastWeaponControllerLauncher!=this)
            {
				UnlockAll();
				foreach (var item in SoundSwitchWeaponSources)
					item.Play();
            }
			onActive = value;
        }
	}
	public Texture2D Icon;
	public Transform[] MissileOuter;
	private Dictionary<string, Transform> _missileOuterHash = new Dictionary<string, Transform>();
	[SerializeField]
	private List<string> missileOuterPosName = new List<string>();
	public List<string> MissileOuterPosName
    {
		get => missileOuterPosName;
		set
        {
			if (!Enumerable.SequenceEqual(missileOuterPosName, value))
			{
				missileOuterPosName = value;
				ReflushShowHangMissile();
			}
        }
    }
	[ResourcesPrefab]
	public string MissilePrefab;          //发射弹药
	private GameObject missilePrefab;
	[ResourcesPrefab]
	public string HangMissileAppearancePrefab;
	public float FireRate = 0.1f;
	public float Spread = 1;
	public float ForceShoot = 8000;
	public int FireOnceOutBulletNub = 1;    //每次射击发射导弹数量
	public int FireOnceOutBulletMaxNub = 1;
	public int Ammo = 10;
	public int AmmoMax = 10;
	public bool InfinityAmmo = false;
	public float CoolingTime;
	public bool ShowHUD = true;
	public int MaxAimRange = 10000;
	public bool ShowCrosshair;
	public Texture2D CrosshairTexture;
	public Texture2D TargetLockOnTexture;
	public Texture2D TargetLockedTexture;
	public float DistanceLock = 200;
	public float TimeToLock = 2;
	private HashSet<GameObject> Targets = new HashSet<GameObject>();	//多锁定模式下的目标
	[Range(0f,0.99f)]
	public float AimDirection = 0.8f;		//锁定需要保持的角度范围(点乘性质：a dot b>0时为90度~0度， a dot b<0时为90~180度)
	public bool Seeker;			//是否开启锁定追踪
	public GameObject Shell;
	public float ShellLifeTime = 4;
	public Transform[] ShellOuter;
	public int ShellOutForce = 300;
	[ResourcesPrefab]
	public string MuzzlePrefab;     //枪焰预制体
	private GameObject muzzlePrefab;
	public float MuzzleLifeTime = 2;
	public AudioClip[] SoundGun;
	public AudioSource[] SoundOverheatSources;
	public AudioSource[] SoundCoolingFinishedSources;
	public AudioSource[] SoundSwitchWeaponSources;
	WeaponLauncher lastWeaponControllerLauncher;





	private float timetolockcount = 0;
	private float lastFireTimeCount = 0;
	private GameObject target;      //待锁定目标
	private Dictionary<GameObject, float> targets = new Dictionary<GameObject, float>();    //<锁定对象, 锁定时间>
	private Vector3 torqueTemp;
	private float overheatingStartTime;
	private AudioSource audioSource;
	[HideInInspector]
	public bool Overheating;
	[HideInInspector]
	public float CoolingProcess;

	public int ReloadNub = 1;
	public float ReloadIntervalTime = 10f;
	private float lastReloadTimeCount;
	//锁定扫描间隔时间
	public float ScanningIntervalTime = 0.5f;
	private float lastScanningTime;
	public bool MultiLockModel
	{
		get
        {
			if (!EnableMultiLock)
				multiLockModel = false;
			return multiLockModel;
        }
		set
        {
			if (EnableMultiLock)
			{
				multiLockModel = value;
				if (!multiLockModel)
					FireOnceOutBulletNub = 1;
			}
			else
				multiLockModel = false;
        }
	}
	private bool multiLockModel = false;
	public bool EnableMultiLock = true;
	//悬挂导弹效果
	private List<GameObject> hangMissilesAppearanceList = new List<GameObject>();
	private Queue<Transform> NoMissileOuters = new Queue<Transform>();

    public struct SyncValueData
    {
		public bool onActive;
		//public Transform[] MissileOuter;
		public string MissileOuterPosNameJons;
		public List<string> MissileOuterPosName { get=> JsonConvert.DeserializeObject<List<string>>(MissileOuterPosNameJons); }
		public string MissilePrefab;          //发射弹药
		public string HangMissileAppearancePrefabPath;
		public float FireRate;
		public float Spread;
		public float ForceShoot;
		public int FireOnceOutBulletNub;    //每次射击发射导弹数量
		public int FireOnceOutBulletMaxNub;
		public int Ammo;
		public int AmmoMax;
		public bool InfinityAmmo;
		public float CoolingTime;
		public bool ShowHUD;
		public int MaxAimRange;
		public float DistanceLock;
		public float TimeToLock;
		public float AimDirection;       //锁定需要保持的角度范围(点乘性质：a dot b>0时为90度~0度， a dot b<0时为90~180度)
		public bool Seeker;         //是否开启锁定追踪
		//public GameObject Shell;
		public float ShellLifeTime;
		//public Transform[] ShellOuter;
		public int ShellOutForce;
		public string MuzzlePrefab;       //枪焰预制体
		public float MuzzleLifeTime;
		public float timetolockcount;
		//public float lastFireTimeCount;
		public GameObject small_target;      //待锁定目标
		public Vector3 torqueTemp;
		public float overheatingStartTime;
		public bool Overheating;
		public float CoolingProcess;
		public int ReloadNub;
		public float ReloadIntervalTime;
		//public float lastReloadTimeCount;	//涉及具体机器时间的要不同步
		//锁定扫描间隔时间
		public float ScanningIntervalTime;
		//public float lastScanningTime;	//涉及具体机器时间的要不同步
		public bool multiLockModel;
		public bool EnableMultiLock;

        public SyncValueData(bool onActive, /*Transform[] missileOuter,*/List<string> missileOuterPosName ,string missile, string hangMissileAppearance, float fireRate, float spread, float forceShoot, int fireOnceOutBulletNub, int fireOnceOutBulletMaxNub, int ammo, int ammoMax, bool infinityAmmo, float coolingTime, bool showHUD, int maxAimRange, float distanceLock, float timeToLock, float aimDirection, bool seeker/*, GameObject shell*/, float shellLifeTime, /*Transform[] shellOuter,*/ int shellOutForce, string muzzle, float muzzleLifeTime, float timetolockcount/*, float lastFireTimeCount*/, GameObject small_target, Vector3 torqueTemp, float overheatingStartTime, bool overheating, float coolingProcess, int reloadNub, float reloadIntervalTime/*, float lastReloadTimeCount*/, float scanningIntervalTime, /*float lastScanningTime,*/ bool multiLockModel, bool enableMultiLock)
        {
            this.onActive = onActive;
            //MissileOuter = missileOuter;
			MissileOuterPosNameJons = JsonConvert.SerializeObject(missileOuterPosName);
            MissilePrefab = missile;
            HangMissileAppearancePrefabPath = hangMissileAppearance;
            FireRate = fireRate;
            Spread = spread;
            ForceShoot = forceShoot;
            FireOnceOutBulletNub = fireOnceOutBulletNub;
            FireOnceOutBulletMaxNub = fireOnceOutBulletMaxNub;
            Ammo = ammo;
            AmmoMax = ammoMax;
            InfinityAmmo = infinityAmmo;
            CoolingTime = coolingTime;
            ShowHUD = showHUD;
            MaxAimRange = maxAimRange;
            DistanceLock = distanceLock;
            TimeToLock = timeToLock;
            AimDirection = aimDirection;
            Seeker = seeker;
            //Shell = shell;
            ShellLifeTime = shellLifeTime;
            //ShellOuter = shellOuter;
            ShellOutForce = shellOutForce;
            MuzzlePrefab = muzzle;
            MuzzleLifeTime = muzzleLifeTime;
            this.timetolockcount = timetolockcount;
            //this.lastFireTimeCount = lastFireTimeCount;
            this.small_target = small_target;
            this.torqueTemp = torqueTemp;
            this.overheatingStartTime = overheatingStartTime;
            Overheating = overheating;
            CoolingProcess = coolingProcess;
            ReloadNub = reloadNub;
            ReloadIntervalTime = reloadIntervalTime;
            //this.lastReloadTimeCount = lastReloadTimeCount;
            ScanningIntervalTime = scanningIntervalTime;
            //this.lastScanningTime = lastScanningTime;
            this.multiLockModel = multiLockModel;
            EnableMultiLock = enableMultiLock;
        }
    }
	public SyncValueData Sync
	{
		get => new SyncValueData(onActive, /*MissileOuter,*/MissileOuterPosName, MissilePrefab, HangMissileAppearancePrefab, FireRate, Spread, ForceShoot, FireOnceOutBulletNub, FireOnceOutBulletMaxNub, Ammo, AmmoMax, InfinityAmmo, CoolingTime, ShowHUD, MaxAimRange, DistanceLock, TimeToLock, AimDirection, Seeker/*, Shell*/, ShellLifeTime, /*ShellOuter,*/ ShellOutForce, MuzzlePrefab, MuzzleLifeTime, timetolockcount/*, lastFireTimeCount*/, target, torqueTemp, overheatingStartTime, Overheating, CoolingProcess, ReloadNub, ReloadIntervalTime,/* lastReloadTimeCount,*/ ScanningIntervalTime, /*lastScanningTime,*/ MultiLockModel, EnableMultiLock);
		set
        {
			OnActive = value.onActive;
			//MissileOuter = value.MissileOuter;
			MissileOuterPosName = JsonConvert.DeserializeObject<List<string>>(value.MissileOuterPosNameJons);
			if (MissilePrefab != value.MissilePrefab)
				missilePrefab = Resources.Load(GameManager.RemovePathPrefixAndSuffix(value.MissilePrefab)) as GameObject;
			MissilePrefab = value.MissilePrefab;
			HangMissileAppearancePrefab = value.HangMissileAppearancePrefabPath;
			FireRate = value.FireRate;
			Spread = value.Spread;
			ForceShoot = value.ForceShoot;
			FireOnceOutBulletNub = value.FireOnceOutBulletNub;
			FireOnceOutBulletMaxNub = value.FireOnceOutBulletMaxNub;
			Ammo = value.Ammo;
			AmmoMax = value.AmmoMax;
			InfinityAmmo = value.InfinityAmmo;
			CoolingTime = value.CoolingTime;
			ShowHUD = value.ShowHUD;
			MaxAimRange = value.MaxAimRange;
			DistanceLock = value.DistanceLock;
			TimeToLock = value.TimeToLock;
			AimDirection = value.AimDirection;
			Seeker = value.Seeker;
			//Shell = value.Shell;
			ShellLifeTime = value.ShellLifeTime;
			//ShellOuter = value.ShellOuter;
			ShellOutForce = value.ShellOutForce;
			if (MuzzlePrefab != value.MuzzlePrefab)
				muzzlePrefab = Resources.Load(GameManager.RemovePathPrefixAndSuffix(MuzzlePrefab)) as GameObject;
			MuzzlePrefab = value.MuzzlePrefab;
			MuzzleLifeTime = value.MuzzleLifeTime;
			timetolockcount = value.timetolockcount;
			//lastFireTimeCount = value.lastFireTimeCount;
			target = value.small_target;
			torqueTemp = value.torqueTemp;
			overheatingStartTime = value.overheatingStartTime;
			Overheating = value.Overheating;
			CoolingProcess = value.CoolingProcess;
			ReloadNub = value.ReloadNub;
			ReloadIntervalTime = value.ReloadIntervalTime;
			//lastReloadTimeCount = value.lastReloadTimeCount;
			ScanningIntervalTime = value.ScanningIntervalTime;
			//lastScanningTime = value.lastScanningTime;
			MultiLockModel = value.multiLockModel;
			EnableMultiLock = value.EnableMultiLock;
		}
	}

	
	public SyncHashSet<GameObject> SyncTargets
    {
		get
		{
			SyncHashSet<GameObject> result = new SyncHashSet<GameObject>();
            foreach (var item in Targets)
				result.Add(item);
			return result;
		}
		set
		{
            foreach (var item in value)
				Targets.Add(item);
		}
    }

	public SyncDictionary<GameObject, float> Synctargets
	{
		get => new SyncDictionary<GameObject, float>(targets);
		set 
		{
			targets.Clear();
			foreach (var item in value)
				targets.Add(item.Key, item.Value);
		}
    }

	//public List<GameObject> SynchangMissilesAppearanceList 
	//{
	//	get => hangMissilesAppearanceList;
	//	set => hangMissilesAppearanceList = value;
	//}

	public List<string> SyncNoMissileOutersName
    {
		get
		{
			List<string> result = new List<string>(NoMissileOuters.Count);
			foreach (var item in NoMissileOuters)
				result.Add(item.name);
			return result;
		}
		set
        {
			NoMissileOuters.Clear();
			foreach (var missilePosName in value)
			{
				if (_missileOuterHash.ContainsKey(missilePosName))
				{
					NoMissileOuters.Enqueue(_missileOuterHash[missilePosName]);
				}
			}
        }
    }

	PoolInstanceBase _missilePI;    //MissilePrefab是否挂载了PoolInstanceBase脚本
	Pool _missilePool;
	PoolInstanceBase _muzzlePI;     //MuzzlePrefab是否挂载了PoolInstanceBase脚本
	Pool _muzzlePool;

	protected override void Awake()
    {
        base.Awake();
		lastScanningTime = -ScanningIntervalTime;
		lastReloadTimeCount -= ReloadIntervalTime;
		lastFireTimeCount -= FireRate;
		if (CoolingTime <= 0)
			CoolingTime = ReloadIntervalTime * AmmoMax;
		missilePrefab = Resources.Load(GameManager.RemovePathPrefixAndSuffix(MissilePrefab)) as GameObject;
		muzzlePrefab = Resources.Load(GameManager.RemovePathPrefixAndSuffix(MuzzlePrefab)) as GameObject;
		ReflushShowHangMissile();
		_missilePI = missilePrefab.GetComponent<PoolInstanceBase>();
		LauncherSync sync = GetComponent<LauncherSync>();
		if (_missilePI&&(!sync||(sync&& NetworkServer.active)))
        {
			_missilePool = PoolManager.GetPool(SceneManager.GetActiveScene(), _missilePI.PrefabPath);
			if (_missilePool == null)
				_missilePool = PoolManager.CreatePool(SceneManager.GetActiveScene(), _missilePI.PrefabPath,
					delegate
					{
						GameObject go = Instantiate(missilePrefab, Vector3.zero, Quaternion.identity);
						if (NetworkServer.active & go.GetComponent<SyncBase>())
							NetworkServer.Spawn(go);
						return go.GetComponent<PoolInstanceBase>();
					}
				);
            if (_missilePool.Count < AmmoMax)
            {
                _missilePool.EnsureQuantity(AmmoMax);
            }
        }

		if(muzzlePrefab)
			_muzzlePI = muzzlePrefab.GetComponent<PoolInstanceBase>();
		if(_muzzlePI)
        {
			_muzzlePool = PoolManager.GetPool(SceneManager.GetActiveScene(), _muzzlePI.PrefabPath);
			if (_muzzlePool == null)
				_muzzlePool = PoolManager.CreatePool(SceneManager.GetActiveScene(), _muzzlePI.PrefabPath,
					delegate {
						GameObject go = Instantiate(Resources.Load<GameObject>(GameManager.RemovePathPrefixAndSuffix(_muzzlePI.PrefabPath)), Vector3.zero, Quaternion.identity);
						return go.GetComponent<PoolInstanceBase>();
					}
				);
            if (_muzzlePool.Count < AmmoMax)
            {
                _muzzlePool.EnsureQuantity(AmmoMax);
            }
        }
	}

	protected virtual void Start ()
	{
		if (!Owner) 
			Owner = this.transform.root.gameObject;
		
		if (!audioSource) {
			audioSource = this.GetComponent<AudioSource> ();
			if (!audioSource) {
				this.gameObject.AddComponent<AudioSource> ();	
			}
		}
		
		
		StartCoroutine(WaitForToInit(0.5f));
	}

    public void SwitchLockModel()
	{
		if (MultiLockModel&&targets.Count > 0)
		{
			KeyValuePair<GameObject, float> temp;
			temp = targets.GetEnumerator().Current;
			target = temp.Key;
			timetolockcount = temp.Value;
			if (Targets.Contains(target))
				Target = target;
			targets.Clear();
			Targets.Clear();
		}
		else if(!MultiLockModel&&target)
        {
			targets.Add(target, timetolockcount);
        }

		MultiLockModel = !MultiLockModel;
    }

    [HideInInspector]
	public Vector3 AimPoint;
	[HideInInspector]
	public GameObject AimObject;



	/// <summary>
	/// 射线检测前方获取瞄准点（用于给准星坐标和发射目标位置定位）
	/// </summary>
	protected virtual void rayAiming ()
	{
		RaycastHit hit;
		if (Physics.Raycast (transform.position, this.transform.forward, out hit, MaxAimRange)) {
			if (missilePrefab != null && hit.collider.tag != missilePrefab.tag) {
				AimPoint = hit.point;
				AimObject = hit.collider.gameObject;
			}
		} else {
			AimPoint = this.transform.position + (this.transform.forward * MaxAimRange);
			AimObject = null;
		}
		
	}

	protected virtual void FixedUpdate()
	{
		if (onActive)
		{
			rayAiming();

			if (TorqueObject)
			{       //给扭矩物体根据torqueTemp值做对应旋转，torqueTemp值在每帧对V3zore做插值，以此模拟扭矩复位
				TorqueObject.transform.Rotate(torqueTemp * Time.fixedDeltaTime);
				torqueTemp = Vector3.Lerp(torqueTemp, Vector3.zero, Time.fixedDeltaTime);
			}
			DoSeeker();
			LockProcess();
		}

		//弹药和过热功能
		if (Overheating)  //是否过热
		{
			if(!GetComponent<LauncherSync>()|| GetComponent<LauncherSync>().isServer)
				CoolingProcess = ((1 / CoolingTime) * (overheatingStartTime + CoolingTime - Time.time));
			if (Time.time > overheatingStartTime + CoolingTime)
			{
				if (!GetComponent<LauncherSync>() || GetComponent<LauncherSync>().isServer)
					Overheating = false;
                foreach (var item in SoundCoolingFinishedSources)
					item.Play();
				//Ammo = AmmoMax;
			}
		}
		else if (Ammo <= 0)
		{
			UnlockAll();
			Overheating = true;
			overheatingStartTime = Time.time;
			foreach (var item in SoundOverheatSources)
				item.Play();
		}
		//print(Time.time - lastReloadTimeCount);
		if (Ammo < AmmoMax && Time.time - lastReloadTimeCount > ReloadIntervalTime && Time.time - lastFireTimeCount > ReloadIntervalTime)
		{
			UnlockAll();
			lastReloadTimeCount = Time.time;
			if (Ammo + ReloadNub > AmmoMax&&(!GetComponent<LauncherSync>() || GetComponent<LauncherSync>().isServer))
				Ammo = AmmoMax;
			else
			{
				if(!GetComponent<LauncherSync>() || GetComponent<LauncherSync>().isServer)
					Ammo += ReloadNub;
				for (int i = 0; i < ReloadNub && NoMissileOuters.Count > 0; i++)
				{
					Transform point = NoMissileOuters.Dequeue();
					for (int j = point.childCount - 1; j >= 0; j--)
						if (point.GetChild(j))
							point.GetChild(j).gameObject.SetActive(true);
				}
			}
		}
	}

	protected virtual void Update()
	{
		if (CurrentCamera == null) 
		{

			CurrentCamera = Camera.main;

			if (CurrentCamera == null)
				CurrentCamera = Camera.current;
		}
		if (Camera.current != null) {
			if (CurrentCamera != Camera.current) {
				CurrentCamera = Camera.current;
			}
		}

		if(transform.parent)
			lastWeaponControllerLauncher = transform.parent.GetComponent<WeaponController>().CurrLauncher;
	}

    private void UnlockAll()
    {
		if(target!=null)
			Unlock(target);
		target = null;
		Target = null;
		timetolockcount = 0;
        foreach (var item in targets.Keys)
        {
			if (item == null)
				continue;
			LockWarring warring = item.GetComponent<LockWarring>();
			if (warring)
				warring.Unlocked(this);
        }
		Targets.Clear();
		targets.Clear();
    }

    private void LockProcess()
    {
		if (!Seeker)
			return;
		
		if (MultiLockModel&&targets.Count>0)
        {
			List<GameObject> unlockTargets = new List<GameObject>();
			GameObject[] targetsList = new GameObject[targets.Count];
			targets.Keys.CopyTo(targetsList, 0);
			for(int i=0;i<targetsList.Length;i++)
            {
				if (targetsList[i] == null)
				{
					targets[targetsList[i]] = 0;
					continue;
				}
				targets[targetsList[i]] += Time.time;
				if (targets[targetsList[i]] > TimeToLock)   //锁定完成
					Targets.Add(targetsList[i]);
				LockWarring warring = targetsList[i].GetComponent<LockWarring>();
				if (warring)
					warring.Locked(this);
				float targetdistance = Vector3.Distance(transform.position, targetsList[i].transform.position);
				Vector3 dir = (targetsList[i].transform.position - transform.position).normalized;
				float direction = Vector3.Dot(dir, transform.forward);
				//print(direction);
				if (targetdistance > DistanceLock || direction <= AimDirection) //脱锁
				{
					unlockTargets.Add(targetsList[i]);
					Targets.Remove(targetsList[i]);
				}
			}
			foreach (var item in unlockTargets)
				Unlock(item);

		}
        else if (target) //有锁定对象
        {
            timetolockcount += Time.deltaTime;
			if (timetolockcount > TimeToLock)   //锁定完成
				Target = target;
			LockWarring warring = target.GetComponent<LockWarring>();
            if (warring)
                warring.Locked(this);
            float targetdistance = Vector3.Distance(transform.position, target.transform.position);
            Vector3 dir = (target.transform.position - transform.position).normalized;
            float direction = Vector3.Dot(dir, transform.forward);
            //print(direction);
            if (targetdistance > DistanceLock || direction <= AimDirection) //脱锁
                Unlock(Target);
		}
    }

    private void DoSeeker()
    {
		if (!Seeker)
			return;
		if (MultiLockModel&& Time.time - lastScanningTime >= ScanningIntervalTime)
		{
			float radius = DistanceLock * Mathf.Tan(/*角度*/(90 - 90 * AimDirection) / 180 * Mathf.PI);
			float distance = int.MaxValue;
			RaycastHit[] hits = Physics.SphereCastAll(transform.position - transform.forward * (radius > 10 ? radius - 10 : 0), radius, transform.forward, DistanceLock + 30, 1 << LayerMask.NameToLayer("Scanning"));
			for(int i=0;i<hits.Length;i++)
			{
				if (TargetTag.Contains(hits[i].collider.tag))
				{
					Vector3 dir = (hits[i].transform.position - transform.position).normalized;
					float direction = Vector3.Dot(dir, transform.forward);
					float dis = Vector3.Distance(hits[i].transform.position, transform.position);
					if (direction >= AimDirection && DistanceLock > dis && distance > dis /*&& timetolockcount + TimeToLock < Time.time*/)
					{
						distance = dis;
						target = hits[i].collider.gameObject;
						if(!targets.ContainsKey(hits[i].collider.gameObject))
							targets.Add(hits[i].collider.gameObject,0);
					}
				}
				lastScanningTime = Time.time;
			}
		}
		else if (!target && Time.time - lastScanningTime >= ScanningIntervalTime)
		{
			float radius = DistanceLock * Mathf.Tan(/*角度*/(90 - 90 * AimDirection) / 180 * Mathf.PI);
			float distance = int.MaxValue;
			RaycastHit m_targetSeekerHit;
			if (AimObject != null && TargetTag.Contains(AimObject.tag))
			{
				float dis = Vector3.Distance(AimObject.transform.position, transform.position);
				if (DistanceLock > dis && distance > dis/* && timetolockcount + TimeToLock < Time.time*/)
				{
					distance = dis;
					target = AimObject;
				}
			}
			else if (Physics.SphereCast(transform.position - transform.forward * (radius > 10 ? radius - 10 : 0), radius, transform.forward, out m_targetSeekerHit, DistanceLock + 30, 1 << LayerMask.NameToLayer("Scanning")))
			{
				if (TargetTag.Contains(m_targetSeekerHit.collider.tag))
				{
					Vector3 dir = (m_targetSeekerHit.transform.position - transform.position).normalized;
					float direction = Vector3.Dot(dir, transform.forward);
					float dis = Vector3.Distance(m_targetSeekerHit.transform.position, transform.position);
					if (direction >= AimDirection && DistanceLock > dis && distance > dis /*&& timetolockcount + TimeToLock < Time.time*/)
					{
						distance = dis;
						target = m_targetSeekerHit.collider.gameObject;
					}
				}
				lastScanningTime = Time.time;
			}
		}
		
    }

    private Camera CurrentCamera;
	protected virtual void DrawTargetLockon (Transform aimtarget, bool locked)
	{
		if (!ShowHUD)
			return;
		
		if (CurrentCamera) {
			Vector3 dir = (aimtarget.position - CurrentCamera.transform.position).normalized;
			float direction = Vector3.Dot (dir, CurrentCamera.transform.forward);
			if (direction > 0.5f)
			{
				Vector3 screenPos = CurrentCamera.WorldToScreenPoint(aimtarget.transform.position);
				float distance = Vector3.Distance(transform.position, aimtarget.transform.position);
				if (locked)
				{
					if (TargetLockedTexture)
						GUI.DrawTexture(new Rect(screenPos.x - (TargetLockedTexture.width / 2), Screen.height - screenPos.y - (TargetLockedTexture.height / 2), TargetLockedTexture.width, TargetLockedTexture.height), TargetLockedTexture);
					GUI.Label(new Rect(screenPos.x + 40, Screen.height - screenPos.y, 200, 30), aimtarget.name + " " + Mathf.Floor(distance) + "m.");
				}
				if (TargetLockOnTexture)
					GUI.DrawTexture(new Rect(screenPos.x - TargetLockOnTexture.width / 2, Screen.height - screenPos.y - TargetLockOnTexture.height / 2, TargetLockOnTexture.width, TargetLockOnTexture.height), TargetLockOnTexture);
			}
		} else {
			//Debug.Log("Can't Find camera");
		}
	}

	private Vector3 crosshairPos;

	protected virtual void DrawCrosshair ()
	{
		if(!ShowCrosshair)
			return;
		
		if (CurrentCamera) {
			Vector3 screenPosAim = CurrentCamera.WorldToScreenPoint (AimPoint);
			crosshairPos += ((screenPosAim - crosshairPos) / 5);
			if (CrosshairTexture) {
				GUI.DrawTexture (new Rect (crosshairPos.x - CrosshairTexture.width / 2, Screen.height - crosshairPos.y - CrosshairTexture.height / 2, CrosshairTexture.width, CrosshairTexture.height), CrosshairTexture);
				
			}
		}
	}

	private FlightView view;

	protected virtual void OnGUI()
	{
		if (!view)
			view = GameObject.FindObjectOfType<FlightView>();
		if (onActive&& view && view.Target==transform.parent.gameObject)
		{
			if (Seeker)
			{
				if (!MultiLockModel)
				{
					if (target && !Target)
						DrawTargetLockon(target.transform, false);
					else if (target && Target)
						DrawTargetLockon(target.transform, true);
				}
                else
                {
                    foreach (var item in targets.Keys)
                    {
						if (item == null)
							continue;
						if(Targets.Contains(item))
							DrawTargetLockon(item.transform, true);
						else
							DrawTargetLockon(item.transform, false);
					}
                }
			}
			//GameObject.FindObjectOfType<FlightView>().Target!=transform.parent.gameObject
			DrawCrosshair();
		}
	}
	public void Unlock (GameObject UnlockTarget)
	{
		//timetolockcount = Time.time;

		if (!MultiLockModel && UnlockTarget == target)
		{
			timetolockcount = 0;
			if (target)
			{
				LockWarring warring = target.GetComponent<LockWarring>();
				if (warring)
					warring.Unlocked(this);
			}
			target = null;
			Target = null;
		}
		else if (MultiLockModel && UnlockTarget &&targets.ContainsKey(UnlockTarget))
		{
			if (Targets.Contains(UnlockTarget))
				Targets.Remove(UnlockTarget);
			targets.Remove(UnlockTarget);
			LockWarring warring= UnlockTarget.GetComponent<LockWarring>();
			if (warring)
				warring.Unlocked(this);
		}
	}
	
	private int currentOuter = 0;

	public void Shoot ()
	{
		if (InfinityAmmo) {
			Ammo = AmmoMax;	
		}
		if (Ammo > 0&& !Overheating) {
			if (Time.time > lastFireTimeCount + FireRate&&Ammo-FireOnceOutBulletNub>=0) {
				lastFireTimeCount = Time.time;
				torqueTemp = TorqueSpeedAxis;
				Ammo -= FireOnceOutBulletNub;
				Vector3 missileposition = this.transform.position;
				Quaternion missilerotate = this.transform.rotation;
				int idxTargets = 0;
				GameObject[] multiModelTargets = new GameObject[Targets.Count];
				Targets.CopyTo(multiModelTargets);
				for (int i = 0; i < FireOnceOutBulletNub; i++) 
				{
					if (MissileOuter.Length > 0)
					{
						missilerotate = MissileOuter[currentOuter].transform.rotation;
						missileposition = MissileOuter[currentOuter].transform.position;
						if (MissileOuter[currentOuter].transform.childCount > 0)
							for (int j = 0; j < MissileOuter[currentOuter].transform.childCount; j++)
								MissileOuter[currentOuter].transform.GetChild(j).gameObject.SetActive(false);
						NoMissileOuters.Enqueue(MissileOuter[currentOuter]);
					}

					if (MissileOuter.Length > 0)
					{
						currentOuter += 1;
						if (currentOuter >= MissileOuter.Length)
							currentOuter = 0;
					}

					if (muzzlePrefab)
					{
						GameObject muzzle;
						if (_muzzlePI)
						{
							PoolInstanceBase instanceMuzzle = _muzzlePool.Get();
							muzzle = instanceMuzzle.gameObject;
							instanceMuzzle.transform.position = missileposition;
							instanceMuzzle.transform.rotation = missilerotate;
							instanceMuzzle.transform.parent = this.transform;
							StartCoroutine(waitForRelease(MuzzleLifeTime, instanceMuzzle, _muzzlePool));
						}
						else
						{
							muzzle = (GameObject)GameObject.Instantiate(muzzlePrefab, missileposition, missilerotate);
							muzzle.transform.parent = this.transform;
							GameObject.Destroy(muzzle, MuzzleLifeTime);
						}
						if (MissileOuter.Length > 0)
						{
							muzzle.transform.parent = MissileOuter[currentOuter].transform;
						}
					}


					if (missilePrefab && (!GetComponent<LauncherSync>() || GetComponent<LauncherSync>().isServer)) 
					{
						Vector3 spread = new Vector3 (Random.Range (-Spread, Spread), Random.Range (-Spread, Spread), Random.Range (-Spread, Spread)) / 100;
						Vector3 direction = this.transform.forward + spread;
						GameObject bullet;
						if (_missilePI)
						{

							bullet = _missilePool.Get().gameObject;
							bullet.transform.position = missileposition;
							bullet.transform.rotation = missilerotate;
						}
						else
							bullet = (GameObject)Instantiate(missilePrefab, missileposition, missilerotate);
						DamageBase damangeBase = bullet.GetComponent<DamageBase> ();
						if (damangeBase) {
							damangeBase.Owner = Owner;
							damangeBase.TargetTag = TargetTag;
						}
						WeaponBase weaponBase = bullet.GetComponent<WeaponBase> ();
						if (weaponBase) {
							weaponBase.Owner = Owner;
							if (!MultiLockModel)
								weaponBase.Target = Target;
							else if (multiModelTargets.Length > 0)
                            {
								idxTargets %= multiModelTargets.Length;
								weaponBase.Target = multiModelTargets[idxTargets];
								idxTargets++;
                            }
							weaponBase.TargetTag = TargetTag;
						}

                        if (bullet.GetComponent<MoverMissile>())
							bullet.GetComponent<MoverMissile>().MissileDown = -transform.up;	//为什么现在改成up又是对的了？？？？？
							//bullet.GetComponent<MoverMissile>().MissileDown = -transform.right;	//别问我为什么是right不是up，我也迷惑，这里用right才是对的。

						bullet.transform.forward = direction;
						if (RigidbodyProjectile) {
							if (bullet.GetComponent<Rigidbody>()) {
								if (Owner != null && Owner.GetComponent<Rigidbody>()) {
									bullet.GetComponent<Rigidbody>().velocity = Owner.GetComponent<Rigidbody>().velocity;
								}
								Rigidbody rigidbody = bullet.GetComponent<Rigidbody>();
								if (rigidbody)
									rigidbody.AddForce(direction * ForceShoot);
							}
						}
						//SyncBase bulletSync = bullet.GetComponent<SyncBase>();
						//if (!bulletSync.isClient)
						//{
						//	//if (bullet.GetComponent<MoverBullet>())
						//	//	StartCoroutine(WaitForSpawn(0.1f, bullet.GetComponent<NetworkIdentity>()));
						//	//else
						//	//	NetworkServer.Spawn(bullet);
						//	NetworkServer.Spawn(bullet);
						//}
					}

					if (Shell)
					{
						Transform shelloutpos = this.transform;
						if (ShellOuter.Length > 0)
						{
							shelloutpos = ShellOuter[currentOuter];
						}

						GameObject shell = (GameObject)Instantiate(Shell, shelloutpos.position, Random.rotation);
						GameObject.Destroy(shell.gameObject, ShellLifeTime);
						if (shell.GetComponent<Rigidbody>())
						{
							shell.GetComponent<Rigidbody>().AddForce(shelloutpos.forward * ShellOutForce);
						}
						//if (GetComponent<LauncherSync>())
						//	NetworkServer.Spawn(shell);
					}
					if (SoundGun.Length > 0)
					{
						if (audioSource)
						{
							audioSource.PlayOneShot(SoundGun[Random.Range(0, SoundGun.Length)]);
						}
					}
				}
				lastFireTimeCount += FireRate;
			}
		} 
		
	}

	protected virtual void OnDestroy()
    {
		UnlockAll();
        foreach (var item in hangMissilesAppearanceList)
			Destroy(item);
	}

	public void AddFireOnceNub(int addNub)
    {
		if (FireOnceOutBulletNub + addNub > FireOnceOutBulletMaxNub)
			FireOnceOutBulletNub = FireOnceOutBulletMaxNub;
		else if (FireOnceOutBulletNub + addNub < 1)
			FireOnceOutBulletNub = 1;
		else
			FireOnceOutBulletNub += addNub;
    }

	WaitForSeconds wfr = null;
	IEnumerator waitForRelease(float seconds, PoolInstanceBase pi, Pool pool)
	{
		if (wfr == null)
			wfr = new WaitForSeconds(seconds);
		yield return wfr;
		wfr = null;
		pool.Release(pi);
	}

	WaitForSeconds wfti = null;
	IEnumerator WaitForToInit(float second)
    {
		if(wfti==null)
			wfti= new WaitForSeconds(second);
		yield return wfti;
		wfti = null;
        ResetTargetTag(transform.parent.tag);
        if (transform.parent.GetComponent<PlayerManager>())
            GetComponent<AudioSource>().spatialBlend = 0;

        

        if (EnableMultiLock)
            MultiLockModel = false;
        if (!multiLockModel)
            FireOnceOutBulletNub = 1;
    }

    public void ReflushShowHangMissile()
    {
        NoMissileOuters.Clear();
        foreach (var item in hangMissilesAppearanceList)
            Destroy(item);

        List<Transform> tempList = new List<Transform>();
        foreach (string posName in MissileOuterPosName)
        {
            Transform pos = transform.parent.Find("RocketFirePoints/" + posName);
            if (pos)
                tempList.Add(pos);
        }
        MissileOuter = tempList.ToArray();
		_missileOuterHash.Clear();
        foreach (var item in tempList)
			_missileOuterHash.Add(item.name, item);

		ShellOuter = tempList.ToArray();

        GameObject hangPrefab = Resources.Load(GameManager.RemovePathPrefixAndSuffix(HangMissileAppearancePrefab)) as GameObject;
        if (hangPrefab && MissileOuter.Length > 0)
        {
            foreach (var item in MissileOuter)
            {
                hangMissilesAppearanceList.Add(Instantiate(hangPrefab, item.position, Quaternion.identity, item));
            }
        }
    }

	//private WaitForSeconds waitForSpawn = null;

	//IEnumerator WaitForSpawn(float second,NetworkIdentity spawnObj)
 //   {
	//	if (waitForSpawn == null)
	//		waitForSpawn = new WaitForSeconds(second);
	//	yield return new WaitForSeconds(second);
	//	if(spawnObj&&spawnObj.gameObject)
	//		NetworkServer.Spawn(spawnObj.gameObject);
	//	waitForSpawn = null;
 //   }
}


