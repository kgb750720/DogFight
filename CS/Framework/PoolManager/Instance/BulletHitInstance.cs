using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHitInstance : HitInstance
{
    ParticleSystem _particle;
    AudioSource _audioSource;
    protected override void Awake()
    {
        base.Awake();
        _particle = GetComponent<ParticleSystem>();
        _audioSource = GetComponent<AudioSource>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void GetProcessCallback()
    {
        _particle.Play();
        _audioSource.enabled = true;
        _audioSource.Play();
    }

    protected override void ReleaseProcessCallback()
    {
        _particle.Stop();
        _audioSource.Stop();
        _audioSource.enabled = false;
    }
}
