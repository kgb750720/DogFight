using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MoverBullet : WeaponBase
{
    public int Lifetime;
    public float Speed = 80;
    public float SpeedMax = 80;
    public float SpeedMult = 1;

    private PoolInstanceBase pi;    //��������PoolInstanceBase��������ű�����Start�����л��������


    protected override void Awake()
    {
        base.Awake();
        pi = GetComponent<PoolInstanceBase>();
    }

    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        if (pi)
        {
            //���Ի�ó�����ӦԤ��������
            Pool instancePool = PoolManager.GetPool(SceneManager.GetActiveScene(), pi.PrefabPath);
            if (instancePool == null) //�����������ڶ�ӦԤ��������
            {
                instancePool = PoolManager.CreatePool(SceneManager.GetActiveScene(), pi.PrefabPath, delegate {
                    GameObject go = Instantiate(Resources.Load<GameObject>(GameManager.RemovePathPrefixAndSuffix(pi.PrefabPath)));
                    return go.GetComponent<PoolInstanceBase>();
                });
            }
            //StartCoroutine(WaitForRelease(Lifetime, instancePool));
            pi.WaitForRelease(Lifetime, instancePool);
        }
        else
            Destroy(gameObject, Lifetime);
    }

    // move bullet by force
    private void FixedUpdate()
    {
		if(!this.GetComponent<Rigidbody>())
			return;
		
        if (!RigidbodyProjectile)
        {
            GetComponent<Rigidbody>().velocity = transform.forward*Speed;
        }else{
			if(this.GetComponent<Rigidbody>().velocity.normalized!=Vector3.zero)
			this.transform.forward = this.GetComponent<Rigidbody>().velocity.normalized;	
		}
        if (Speed < SpeedMax)
        {
            Speed += SpeedMult * Time.fixedDeltaTime;
        }
    }

    //WaitForSeconds wfs = null;
    //IEnumerator WaitForRelease(float seconds, Pool instancePool)
    //{
    //    if(wfs==null)
    //    {
    //        wfs = new WaitForSeconds(seconds);
    //    }
    //    yield return wfs;
    //    instancePool.Release(pi);
    //    wfs = null;
    //}
}
