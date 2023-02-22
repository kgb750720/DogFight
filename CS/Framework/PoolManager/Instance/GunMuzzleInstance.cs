using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunMuzzleInstance : PoolInstanceBase
{
    ParticleSystem[] _particles;
    Light _light;
    FlashLight _flash;

    protected override void Awake()
    {
        base.Awake();
        _particles = GetComponentsInChildren<ParticleSystem>();
        _light = GetComponent<Light>();
        _flash = GetComponent<FlashLight>();
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
        foreach (var item in _particles)
        {
            item.Play();
        }
        _flash.enabled = false;
        _light.enabled = true;
    }

    protected override void ReleaseProcessCallback()
    {
        foreach (var item in _particles)
        {
            item.Stop();
        }
        _flash.enabled = true;
        _light.enabled = false;
    }
}
