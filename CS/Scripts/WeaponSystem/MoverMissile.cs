using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MoverMissile : WeaponBase
{
	public float Damping = 10;	//转向速度
	public float Speed = 80;
	public float SpeedMax = 80;
	public float SpeedMult = 1;
	public Vector3 Noise = new Vector3 (20, 20, 20);
	public float TargetLockDirection = 0.5f;	//脱锁角度差
	public int DistanceLock = 70;
	public int DurationLock = 40;
	public bool Seeker;
	public float LifeTime = 5.0f;
	private bool locked;
	private int timetorock;
	private float timeCount = 0;

	public float startUpTime = 0f;
	public float MissileFireDownForce = 0f;
	public Vector3 MissileDown = Vector3.zero;
	private bool startUp = false;


	private ParticleSystem[] particles;
	private AudioSource[] sources;

	PoolInstanceBase pi;

	bool started = false;   //保证不会在start函数调用前调用Init()的标志变量

    protected override void Awake()
    {
		base.Awake();
		pi = GetComponent<PoolInstanceBase>();
	}

    private void Start ()
    {
		started = true;
        
        particles = gameObject.GetComponentsInChildren<ParticleSystem>();
        sources = gameObject.GetComponents<AudioSource>();
        Init();
    }

    private void OnEnable()
    {
		if(started)
			Init();
    }


    private void Init()
    {
		startUp = false;//重置启动状态
        
		if (pi)
        {
			Pool instancePool = PoolManager.GetPool(SceneManager.GetActiveScene(), pi.PrefabPath);
			if (instancePool == null) //若场景不存在对应预制体对象池
			{
				instancePool = PoolManager.CreatePool(SceneManager.GetActiveScene(), pi.PrefabPath, delegate {
					GameObject go = Instantiate(Resources.Load<GameObject>(GameManager.RemovePathPrefixAndSuffix(pi.PrefabPath)));
					return go.GetComponent<PoolInstanceBase>();
				});
			}
			//StartCoroutine(WaitForRelease(LifeTime, instancePool));
			pi.WaitForRelease(LifeTime, instancePool);
		}
        else
            Destroy(gameObject, LifeTime);


        timeCount = Time.time;
        foreach (var item in particles)
            item.Pause();
        StartCoroutine(WaitForTime(startUpTime));
    }

 //   WaitForSeconds wfs = null;
 //   private IEnumerator WaitForRelease(float seconds, Pool instancePool)
 //   {
	//	if (wfs == null)
	//	{
	//		wfs = new WaitForSeconds(seconds);
	//	}
	//	yield return wfs;
	//	instancePool.Release(pi);
	//	wfs = null;
	//}

    WaitForSeconds waitForSecods;
	private IEnumerator WaitForTime(float startUpTime)
    {
		if (waitForSecods == null)
			waitForSecods = new WaitForSeconds(startUpTime);
		yield return waitForSecods;
		if(gameObject)
        {
			startUp = true;
            for (int i = 0; i < particles.Length; i++)
				particles[i].Play();
			TrailRenderer[] trails = gameObject.GetComponentsInChildren<TrailRenderer>();
			for (int i = 0; i < trails.Length; i++)
				trails[i].enabled = true;
			foreach (var item in sources)
			{
				item.enabled = true;
				item.Play();
			}
			Light[] lights= gameObject.GetComponentsInChildren<Light>();
			foreach (var item in lights)
				item.enabled = true;
		}
		waitForSecods = null;
    }

    private void FixedUpdate ()
	{
		if (startUp)
		{
			GetComponent<Rigidbody>().velocity = new Vector3(transform.forward.x * Speed * Time.fixedDeltaTime, transform.forward.y * Speed * Time.fixedDeltaTime, transform.forward.z * Speed * Time.fixedDeltaTime);
			GetComponent<Rigidbody>().velocity += new Vector3(Random.Range(-Noise.x, Noise.x), Random.Range(-Noise.y, Noise.y), Random.Range(-Noise.z, Noise.z));
			if (Speed < SpeedMax)
			{
				Speed += SpeedMult * Time.fixedDeltaTime;
			}
		}
		else
        {
			Rigidbody rigidbody = GetComponent<Rigidbody>();
			if (rigidbody)
				rigidbody.AddForce(MissileDown * MissileFireDownForce);
		}
	}

	private void Update ()
	{
		if (Time.time >= (timeCount + LifeTime) - 0.5f) {
			if (GetComponent<Damage> ()) {
				GetComponent<Damage> ().Active ();
			}
		}
		
		if (Target) 
		{
			Transform targetTra = Target.transform.Find("TargetTrackPoint");
			Vector3 attackPosition = targetTra ? targetTra.position : Target.transform.position;
			Quaternion rotation = Quaternion.LookRotation(attackPosition  - transform.transform.position);
			//transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * Damping);
			transform.rotation = Quaternion.Lerp (transform.rotation, rotation, Time.deltaTime * Damping);
			Vector3 dir = (Target.transform.position - transform.position).normalized;
			float direction = Vector3.Dot (dir, transform.forward);
			if (direction < TargetLockDirection) {
				LockWarring warring = Target.GetComponent<LockWarring>();
				if(warring)
					warring.Unlocked(this);
				Target = null;
			}
		}
		
		if (Seeker) {
			if (timetorock > DurationLock && !locked && !Target)
			{
				float distance = int.MaxValue;
				RaycastHit hit;
				if (Physics.SphereCast(transform.position - transform.forward * 15, 20, transform.forward, out hit, DistanceLock,1<<LayerMask.NameToLayer("Scanning")))
					if (TargetTag.Contains(hit.collider.tag))
					{
						Vector3 dir = (hit.transform.position - transform.position).normalized;
						float direction = Vector3.Dot(dir, transform.forward);
						float dis = Vector3.Distance(hit.transform.position, transform.position);
						if (direction >= TargetLockDirection && DistanceLock > dis)
						{
							if (distance > dis)
							{
								distance = dis;
								if (Target != hit.collider.gameObject)
									Target = hit.collider.gameObject;
							}
							locked = true;

						}
					}
			}
			else
			{
				timetorock += 1;
			}

			if (Target) {
				LockWarring warring = Target.GetComponent<LockWarring>();
				if(warring)
					warring.Locked(this);
			} else {
				locked = false;

			}
		}
	}

    private void OnDestroy()
    {
		if (Target)
		{
			LockWarring warring = Target.GetComponent<LockWarring>();
			if (warring)
				warring.Unlocked(this);
		}
	}

}
