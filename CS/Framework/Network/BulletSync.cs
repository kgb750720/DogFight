using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/guides/networkbehaviour
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkBehaviour.html
*/

// NOTE: Do not put objects in DontDestroyOnLoad (DDOL) in Awake.  You can do that in Start instead.

public class BulletSync : SyncBase
{
    #region Start & Stop Callbacks

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// Invoked on the server when the object is unspawned
    /// <para>Useful for saving object data in persistent storage</para>
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() { }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() { }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() { }

    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority">AssignClientAuthority</see> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnection parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() { }

    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }

    #endregion

    Collider _collider;
    Rigidbody _rigidbody;
    Damage _damage;
    TrailRenderer _trailRenderer;
    MoverBullet _moverBullet;
    ParticleSystem _particleSystem;
    Animator _animator;

    protected override void Awake()
    {
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
        _damage = GetComponent<Damage>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _moverBullet = GetComponent<MoverBullet>();
        _animator = GetComponent<Animator>();
        _particleSystem = GetComponent<ParticleSystem>();
        base.Awake();
    }

    void Start()
    {
        if (isClientOnly)
        {
            _collider.enabled = false;
            _rigidbody.useGravity = false;
            _damage.enabled = false;
        }
    }

    protected override void Update()
    {
        base.Update();
    }


    //[ServerCallback]
    //public override void SCalllbackReleaseCallback()
    //{
    //    ReleaseProcessCallback();
    //    CRpcReleaseCallback();
    //}

    //[ServerCallback]
    //public override void SCalllbackGetCallback()
    //{
    //    ReleaseProcessCallback();
    //    CRpcGetCallback();
    //}

    //[ClientRpc]
    //public override void CRpcReleaseCallback()
    //{
    //    GetProcessCallback();
    //    CRpcGetCallback();
    //}

    //[ClientRpc]
    //public virtual void CRpcGetCallback()
    //{
    //    GetProcessCallback();
    //}

    //private void ReleaseProcessCallback()
    //{
    //    _trailRenderer.enabled = false;
    //    _moverBullet.enabled = false;
    //    _particleSystem.Stop();
    //    _animator.SetBool("Show", false);
    //}

    //private void GetProcessCallback()
    //{
    //    _trailRenderer.enabled = true;
    //    _moverBullet.enabled = true;
    //    _particleSystem.Play();
    //    _animator.SetBool("Show", true);
    //}

}
