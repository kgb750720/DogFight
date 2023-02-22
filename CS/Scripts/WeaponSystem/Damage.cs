using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Mirror;

public struct DamagePackage{
	public int Damage;
	public GameObject Owner;
}

public class Damage : DamageBase
{
    public bool Explosive;  //伤害类型是否为爆炸溅射
    public float ExplosionRadius = 20;
    public float ExplosionForce = 1000;
	public bool HitedActive = true;
	public float TimeActive = 0;
	public bool RandomTimeActive;
	private float timetemp = 0;
	
	public GameObject DestroyAfterObject;
	public float DestryAfterDuration = 10;

    public float InsuranceTime = 0.1f;
    float timeCount = 0;

    private PoolInstanceBase mi;    //子弹预制体池对象

    bool isNetwork = false;
    protected override void Awake()
    {
        base.Awake();
        mi = GetComponent<PoolInstanceBase>();
        if (GetComponent<SyncBase>())
            isNetwork = true;
    }

    private void Start()
    {
		timetemp = Time.time;
		if(RandomTimeActive)
			TimeActive = Random.Range(TimeActive/2f,TimeActive);
		
        if (!Owner || !Owner.GetComponent<Collider>()) return;
        Physics.IgnoreCollision(GetComponent<Collider>(), Owner.GetComponent<Collider>());
		
		
    }

    private void Update()
    {
		if(TimeActive>0){
			if(Time.time >= (timetemp + TimeActive)){
				Active();
			}
		}
    }

    private void FixedUpdate()
    {
        timeCount += Time.fixedDeltaTime;
    }

    public void Active()
    {
        if (Effect)
        {
            if (ei)
            {
                Pool hitPool = PoolManager.GetPool(SceneManager.GetActiveScene(), ei,
                    delegate
                    {
                        GameObject go = Instantiate(Effect);
                        return go.GetComponent<HitInstance>();
                    }
                );
                PoolInstanceBase hitInstance = hitPool.Get();
                hitInstance.transform.position = transform.position;
                hitInstance.transform.rotation = transform.rotation;
                hitInstance.WaitForRelease(LifeTimeEffect, hitPool);
            }
        }
        else
        {
            GameObject obj = (GameObject)Instantiate(Effect, transform.position, transform.rotation);
            Destroy(obj, LifeTimeEffect);
        }

        if (Explosive && (!isNetwork || (isNetwork && NetworkServer.active)))
            ExplosionDamage();

        if (DestroyAfterObject)
        {

            DestroyAfterObject.transform.parent = null;
            if (DestroyAfterObject.GetComponent<ParticleSystem>())
            {
                var emission = DestroyAfterObject.GetComponent<ParticleSystem>().emission;
                emission.rateOverTime = 0;
            }
            Destroy(DestroyAfterObject, DestryAfterDuration);
        }

        if (!isNetwork||(isNetwork && NetworkServer.active))
        {
            if (mi)
            {
                Pool pool = PoolManager.GetPool(SceneManager.GetActiveScene(), mi.PrefabPath);
                if (pool == null)
                {
                    pool = PoolManager.CreatePool(SceneManager.GetActiveScene(), mi.PrefabPath, delegate
                    {
                        GameObject go = Instantiate(Resources.Load<GameObject>(GameManager.RemovePathPrefixAndSuffix(mi.PrefabPath)));
                        return go.GetComponent<PoolInstanceBase>();
                    });
                }
                pool.Release(mi);
            }
            else
                Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        //Debug.Log("Destroy " + name);
    }

    private void ExplosionDamage()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, ExplosionRadius);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            Collider hit = hitColliders[i];
            if (!hit)
                continue;
			
			
			DamagePackage dm = new DamagePackage();
			dm.Damage = Damage;
			dm.Owner = Owner;
			hit.gameObject.SendMessage("ApplyDamage",dm,SendMessageOptions.DontRequireReceiver);
           
            if (hit.GetComponent<Rigidbody>())
                hit.GetComponent<Rigidbody>().AddExplosionForce(ExplosionForce, transform.position, ExplosionRadius, 3.0f);
        }

    }

    private void NormalDamage(Collision collision)
    {
        DamagePackage dm = new DamagePackage();
		dm.Damage = Damage;
		dm.Owner = Owner;
		collision.collider.gameObject.SendMessage("ApplyDamage",dm,SendMessageOptions.DontRequireReceiver);
    }

    private void OnCollisionEnter(Collision collision)
    {
		if(HitedActive&&timeCount>InsuranceTime){
        	if (collision.gameObject.tag != "Particle" && collision.gameObject.tag != this.gameObject.tag)
        	{
            	if (!Explosive&&(!isNetwork||(isNetwork&&NetworkServer.active)))
                	NormalDamage(collision);
            	Active();
        	}
		}
    }
}
