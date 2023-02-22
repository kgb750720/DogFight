using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pool
{
    public Pool(Func<PoolInstanceBase> CreateProcessCallback)
    {
        createProcessCallback = CreateProcessCallback;
    }
    private Func<PoolInstanceBase> createProcessCallback;
    private Queue<PoolInstanceBase> queue = new Queue<PoolInstanceBase>();
    public void Release(PoolInstanceBase obj)
    {
        obj.OnRelease();
        queue.Enqueue(obj);
    }

    public PoolInstanceBase Get()
    {
        while (queue.Count > 0 && queue.Peek() == null)
            queue.Dequeue();
        if (queue.Count > 0)
        {
            PoolInstanceBase res = queue.Dequeue();
            res.OnGet();
            return res;
        }
        return createProcessCallback();
    }

    /// <summary>
    /// 清空删除所有脚本依附的实例化物体
    /// </summary>
    public void Clear()
    {
        while (queue.Count > 0)
        {
            GameObject.Destroy(queue.Dequeue().gameObject);
        }
    }

    public int Count { get => queue.Count; }

    //弃用：暂时无法解决create和Release在同一帧时ReleaseProcess无法操作到未初始化组件的问题
    public void EnsureQuantity(int size)
    {
        if (queue.Count < size)
        {
            for (int i=queue.Count;i<=size;i++)
            {
                PoolInstanceBase go = createProcessCallback();
                Release(go);
                //go.WaitForReleaseOnEndOfFrame(this);
                //go.WaitForRelease(0.1f, this);
            }
        }
    }
}

public abstract class PoolInstanceBase : MonoBehaviour
{
    //public static Func<PoolInstanceBase> GetProcessCallback;
    //public static Action<PoolInstanceBase> ReleaseProcessCallback;
    [ResourcesPrefab]
    public string PrefabPath;
    SyncBase syncBase;
    protected virtual void Awake()
    {
        syncBase = GetComponent<SyncBase>();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void OnRelease()
    {
        if (syncBase)
        {
            syncBase.SCallbackReleaseCallback();
        }
        else
            DoReleaseProcess();
    }

    public void DoReleaseProcess()
    {
        ReleaseProcessCallback();
    }

    public virtual void OnGet() 
    {
        if (syncBase)
        {
            syncBase.SCallbackGetCallback();
        }
        else
            DoGetProcess();
    }

    public void DoGetProcess()
    {
        GetProcessCallback();
    }

    protected virtual void ReleaseProcessCallback(){}

    protected virtual void GetProcessCallback() {}

    private WaitForSeconds _wfr = null;
    private IEnumerator _waitForRelease(float seonds, Pool pool)
    {
        if (_wfr == null)
            _wfr = new WaitForSeconds(seonds);
        yield return _wfr;
        _wfr = null;
        pool.Release(this);
    }

    public void WaitForRelease(float seonds, Pool pool)
    {
        StartCoroutine(_waitForRelease(seonds, pool));
    }

    WaitForEndOfFrame _waitForEndOfFrame = new WaitForEndOfFrame();
    IEnumerator _waitForReleaseOnEndOfFrame(Pool pool)
    {
        yield return _waitForEndOfFrame;
        pool.Release(this);
    }
    public void WaitForReleaseOnEndOfFrame(Pool pool)
    {
        StartCoroutine(_waitForReleaseOnEndOfFrame(pool));
    }

    private void OnDestroy()
    {
    }
}
