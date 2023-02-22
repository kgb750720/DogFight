using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileBind : WeaponViewBind
{

    public string[] MissileFirePointNames;
    // Start is called before the first frame update
    protected override void Start()
    {
        BindWeapon += DoBind;
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void DoBind()
    {
        if (transform.parent != null)
        {
            WeaponController weaponController = transform.parent.GetComponent<WeaponController>();
            if (!weaponController.WeaponList.Contains(GetComponent<WeaponLauncher>()))
                weaponController.WeaponList.Add(GetComponent<WeaponLauncher>());
        }
        else
            Destroy(gameObject);
    }
}
