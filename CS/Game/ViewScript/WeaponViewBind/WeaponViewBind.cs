using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponViewBind : MonoBehaviour
{
    /// <summary>
    /// ����������Ϊ�¼�
    /// </summary>
    protected Action BindWeapon;



    // Start is called before the first frame update
    protected virtual void Start()
    {
        //������Э�̣�Ϊ��ʹ�����ӽǵ������׼����������ӳ�0.1s�󶨡�
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
