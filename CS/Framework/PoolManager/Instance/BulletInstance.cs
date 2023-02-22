using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletInstance : PoolInstanceBase
{
    Collider _collider;
    Rigidbody _rigidbody;
    Damage _damage;
    TrailRenderer _trailRenderer;
    bool _trailRendererLertpTrrigerOn = false;
    float _trailRendererTime;
    MoverBullet _moverBullet;
    ParticleSystem _particleSystem;
    Animator _animator;
    SyncBase sync;
    protected override void Awake()
    {
        base.Awake();
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        _damage = GetComponent<Damage>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _trailRendererTime = _trailRenderer.time;
        _moverBullet = GetComponent<MoverBullet>();
        _animator = GetComponent<Animator>();
        _particleSystem = GetComponent<ParticleSystem>();
        sync = GetComponent<SyncBase>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (sync&&sync.isClientOnly)
        {
            _collider.enabled = false;
            //_rigidbody.useGravity = false;
            _damage.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(_trailRendererLertpTrrigerOn&&Mathf.Abs(_trailRenderer.time- _trailRendererTime)>0.001f)
        {
            _trailRenderer.time = Mathf.Lerp(_trailRenderer.time, _trailRendererTime, Time.deltaTime*10);
        }
    }

    protected override void GetProcessCallback()
    {
        
        _moverBullet.enabled = true;

        if (NetworkServer.active)
        {
            _particleSystem.Play();
            _trailRenderer.enabled = true;
            _trailRenderer.time = 0;
            _trailRendererLertpTrrigerOn = true;
            _animator.SetBool("Show", true);
            _collider.enabled = true;
            _damage.enabled = true;
        }
        else
            StartCoroutine(WaitForColiderEnabled(0.2f, true));
        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;
            
    }

    protected override void ReleaseProcessCallback()
    {
        _trailRendererLertpTrrigerOn = false;
        _trailRenderer.time = -1;
        _trailRenderer.enabled = false;
        _moverBullet.enabled = false;
        _moverBullet.StopAllCoroutines();
        _particleSystem.Stop();
        _animator.SetBool("Show", false);
        _collider.enabled = false;
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
        _damage.enabled = false;
        _damage.StopAllCoroutines();
    }

    WaitForSeconds wtfColiderEnable = null;
    IEnumerator WaitForColiderEnabled(float seconds,bool enbaled)
    {
        if(wtfColiderEnable==null)
            wtfColiderEnable = new WaitForSeconds(seconds);
        yield return wtfColiderEnable;
        wtfColiderEnable = null;
        _particleSystem.Play();
        _trailRenderer.enabled = true;
        _trailRenderer.time = 0;
        _trailRendererLertpTrrigerOn = true;
        _animator.SetBool("Show", true);
        _collider.enabled = enbaled;
    }

    //WaitForSeconds wtfShow = null;
    //IEnumerator WaitForShow()
    //{
    //    if (wtfShow == null)
    //        wtfShow = new WaitForSeconds(0.02f);
    //    yield return wtfShow;
    //    _trailRenderer.enabled = true;
    //    _trailRendererLertpTrrigerOn = true;
    //    //_trailRenderer.time = _trailRendererTime;
    //    _animator.SetBool("Show", true);
    //}
}
