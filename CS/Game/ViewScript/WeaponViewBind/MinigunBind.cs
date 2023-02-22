using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(WeaponLauncher))]
public class MinigunBind : WeaponViewBind
{
    protected override void Start()
    {
        BindWeapon += Binder;
        base.Start();
    }

    void Binder()
    {
        
        if (transform.parent != null)
        {
            Transform point = transform.parent.Find("MinigunFirePoint");
            if (point)
                transform.position = point.position;
            Transform cameraPoint = point.Find("CameraPoint");
            CameraSway cs = transform.GetComponentInChildren<CameraSway>();
            if (cameraPoint && cs)
                cs.positionTemp = cameraPoint.localPosition;
            if (!transform.parent.GetComponent<WeaponController>().WeaponList.Contains(GetComponent<WeaponLauncher>()))
                transform.parent.GetComponent<WeaponController>().WeaponList.Add(GetComponent<WeaponLauncher>());
        }
        else
            Destroy(gameObject);

        //∞Û∂® ”Ω«
        GameObject cam = GetComponentInChildren<Camera>().gameObject;
        WeaponLauncher launcher = GetComponent<WeaponLauncher>();
        FlightView flightView = FlightView.singleton;
        if (cam && launcher && flightView)
        {
            //flightView.WeaponAimChanges[launcher] = new Dictionary<FlightView.CameraInfo.ViewTypeSet, ViewChangeAim>();
            //flightView.WeaponAimChanges[launcher][FlightView.CameraInfo.ViewTypeSet.Third] = new ViewChangeAimSwitchCam(flightView, cam, 0.5f);
            FlightView.CameraInfo.ViewTypeSet targetViewType = FlightView.CameraInfo.ViewTypeSet.Third;
            List<FlightView.CameraInfo> needAdds = flightView.GetCmaeras(targetViewType);
            foreach (FlightView.CameraInfo item in needAdds)
            {
                flightView.AddCameraPrepare(cam);
                item.AddAimWeaponChange(launcher, new ViewChangeAimSwitchZoomCam(item, cam, 30f));
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
