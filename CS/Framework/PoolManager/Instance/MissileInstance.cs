using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileInstance : PoolInstanceBase
{
    MoverMissile _moverMissile;
    Rigidbody _rigidbody;
    Damage _damage;
    Collider _collider;
    TrailRenderer[] _trails;
    Light[] _lights;
    ParticleSystem[] _particles;
    AudioSource[] _sources;
    [SerializeField]
    List<GameObject> hideList = new List<GameObject>();


    SyncBase sync;
    protected override void Awake()
    {
        base.Awake();
        _moverMissile = GetComponent<MoverMissile>();
        _rigidbody = GetComponent<Rigidbody>();
        _damage = GetComponent<Damage>();
        _collider = GetComponent<Collider>();
        _particles = gameObject.GetComponentsInChildren<ParticleSystem>();
        _sources = gameObject.GetComponents<AudioSource>();
        _trails = gameObject.GetComponentsInChildren<TrailRenderer>();
        _lights = gameObject.GetComponentsInChildren<Light>();
        sync = GetComponent<SyncBase>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (sync && sync.isClientOnly)
        {
            _collider.enabled = false;
            _rigidbody.useGravity = false;
            _damage.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void GetProcessCallback()
    {
        _collider.enabled = true;
        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;
        _moverMissile.enabled = true;

        foreach (var item in hideList)
        {
            item.gameObject.SetActive(true);
        }
    }

    protected override void ReleaseProcessCallback()
    {
        _collider.enabled = false;
        _rigidbody.useGravity = false;
        _damage.enabled = false;
        _damage.StopAllCoroutines();
        _collider.enabled = false;
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
        _moverMissile.enabled = false;
        _moverMissile.StopAllCoroutines();
        for (int i = 0; i < _trails.Length; i++)
            _trails[i].enabled = false;
        foreach (var item in _sources)
            item.enabled = false;
        foreach (var item in _lights)
            item.enabled = false;
        foreach (var item in _particles)
            item.Pause();

        foreach (var item in hideList)
        {
            item.gameObject.SetActive(false);
        }
    }
}
