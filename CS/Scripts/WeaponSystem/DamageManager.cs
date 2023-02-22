/// <summary>
/// Damage manager. 
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Mirror;

public class DamageManager : MonoBehaviour
{
    public AudioClip[] HitSound;
    public GameObject Effect;
    public int HP = 100;
	public int HPmax;
	public ParticleSystem OnFireParticle;
	public bool Recover = false;
	public float RecoverStartUpTime = 20f;
	public float RecoverIntervalTime = 0.2f;
	public int OnceRecoverHp = 1;
    private float lastDemageTimeCount;
	private float lastRecoverTimeCount;


    //伤害积分计算设计
    Queue<DamagePackage> KillerCount = new Queue<DamagePackage>();

    public UnityEvent<Queue<DamagePackage>> DeadEvent = new UnityEvent<Queue<DamagePackage>>();

    public struct SyncData
    {
        public int HP;
        public int HPmax;
        //public float RecoverStartUpTime;
        //public float RecoverIntervalTime;
        public int OnceRecoverHp;
        public SyncData(int hp,int hpmax/*,float recoverStartUpTime,float recoverIntervalTime*/,int onceRecoverHp)
        {
            HP = hp;
            HPmax = hpmax;
            //RecoverStartUpTime = recoverStartUpTime;
            //RecoverIntervalTime = recoverIntervalTime;
            OnceRecoverHp = onceRecoverHp;
        }
    }

    public SyncData Sync
    {
        get => new SyncData(HP, HPmax/*, RecoverStartUpTime, RecoverIntervalTime*/, OnceRecoverHp);
        set
        {
            HP = value.HP;
            if (HP <= 0)
                Dead();
            HPmax = value.HPmax;
            //RecoverStartUpTime = value.RecoverStartUpTime;
            //RecoverIntervalTime = value.RecoverIntervalTime;
            OnceRecoverHp = value.OnceRecoverHp;
        }
    }


    private void Awake()
    {
		lastRecoverTimeCount -= RecoverIntervalTime;
        lastDemageTimeCount -= RecoverStartUpTime;
    }
    private void Start()
    {
		HPmax = HP;
		if(OnFireParticle){
			OnFireParticle.Stop();
		}
    }

    private void Update()
    {
        if (Time.time - lastDemageTimeCount > RecoverStartUpTime && Time.time - lastRecoverTimeCount > RecoverIntervalTime)   //启动回血
        {
            lastRecoverTimeCount = Time.time;
            if (HP + OnceRecoverHp > HPmax)
                HP = HPmax;
            else
                HP += OnceRecoverHp;

            int recoverHp = OnceRecoverHp;
            while (KillerCount.Count>0 && recoverHp>= KillerCount.Peek().Damage)
            {
                recoverHp -= KillerCount.Peek().Damage;
                KillerCount.Dequeue();
            }
            if (KillerCount.Count > 0)
            {
                DamagePackage first = KillerCount.Peek();
                first.Damage -= recoverHp;
            }

        }

        if (OnFireParticle)
        {
            if (HP < (int)(HPmax / 2.0f) && !OnFireParticle.isPlaying)
                OnFireParticle.Play();
            else if (HP >= (int)(HPmax / 2.0f) && OnFireParticle.isPlaying)
                OnFireParticle.Stop();
        }
    }

    // Damage function
    public void ApplyDamage(DamagePackage dm)
    {
		if(HP<0||!NetworkServer.active)
		    return;
	
        if (HitSound.Length > 0)
        {
            AudioSource.PlayClipAtPoint(HitSound[Random.Range(0, HitSound.Length)], transform.position);
        }
        //计算伤害积分
        KillerCount.Enqueue(dm);

        HP -= dm.Damage;
        lastDemageTimeCount = Time.time;
		
        if (HP <= 0)
        {
			this.gameObject.SendMessage("OnDead",dm.Owner,SendMessageOptions.DontRequireReceiver);
            Dead();
        }
    }

    private void Dead()
    {
        DeadEvent?.Invoke(KillerCount);
        EventsManager<GameObject, Queue<DamagePackage>>.Invoke(GameModelCtrl.DeadEvet, gameObject, KillerCount);
        if (Effect){
            GameObject obj = (GameObject)GameObject.Instantiate(Effect, transform.position, transform.rotation);
			if(this.GetComponent<Rigidbody>()){
				if(obj.GetComponent<Rigidbody>()){
					obj.GetComponent<Rigidbody>().velocity = this.GetComponent<Rigidbody>().velocity;
					obj.GetComponent<Rigidbody>().AddTorque(Random.rotation.eulerAngles * Random.Range(100,2000));
				}
			}
		}

        if (NetworkServer.active)
            NetworkServer.UnSpawn(this.gameObject);
        Destroy(this.gameObject);
    }

}
