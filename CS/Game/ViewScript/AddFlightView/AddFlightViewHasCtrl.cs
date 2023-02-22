using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class AddFlightViewHasCtrl : AddFlightViewBase
{
    public bool InitControllerEnable=true;
    public override void AddFlightViewCS(GameObject go)
    {
        base.AddFlightViewCS(go);
        ViewController vc = go.AddComponent<ViewController>();
        if (!InitControllerEnable)
            vc.actions.Disable();
        else
            vc.actions.Enable();
    }

    public virtual void AddFlightViewCS(GameObject go,bool initEnable)
    {
        bool temp = InitControllerEnable;
        InitControllerEnable = initEnable;
        AddFlightViewCS(go);
        InitControllerEnable = temp;
    }
}
