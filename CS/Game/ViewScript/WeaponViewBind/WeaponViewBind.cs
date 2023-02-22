using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponViewBind : MonoBehaviour
{
    /// <summary>
    /// 绑定武器的行为事件
    /// </summary>
    protected Action BindWeapon;



    // Start is called before the first frame update
    protected virtual void Start()
    {
        //启动绑定协程，为了使各个视角的相机都准备充分所以延迟0.1s绑定。
        StartCoroutine(WaitFor());
    }

    IEnumerator WaitFor()
    {
        yield return new WaitForSeconds(0.5f);
        BindWeapon.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
