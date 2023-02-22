using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NapalmBind : WeaponViewBind
{

    public Camera GunCamera;
    public Texture2D Crosshair;
    bool CrosshairShow = false;
    protected override void Start()
    {
        BindWeapon += Binder;
        base.Start();
    }

    void Binder()
    {
        GunCamera = GetComponentInChildren<Camera>();
        GameObject cam = null;
        if (GunCamera)
             cam=GunCamera.gameObject;
        WeaponLauncher launcher = GetComponent<WeaponLauncher>();
        FlightView flightView = GameObject.FindObjectOfType<FlightView>();
        if (cam && launcher && flightView)
        {
            //flightView.WeaponAimChanges[launcher] = new Dictionary<FlightView.CameraInfo.ViewTypeSet, ViewChangeAim>();
            //flightView.WeaponAimChanges[launcher][FlightView.CameraInfo.ViewTypeSet.Third] = new ViewChangeAimSwitchCam(flightView, cam, 0.5f);
            List<FlightView.CameraInfo> needAdds = flightView.GetCmaeras(FlightView.CameraInfo.ViewTypeSet.Cockpit);
            foreach (FlightView.CameraInfo item in needAdds)
            {
                item.AddAimWeaponChange(launcher, new ViewChangeAimSwitchCam(item, cam));
            }
            needAdds = flightView.GetCmaeras(FlightView.CameraInfo.ViewTypeSet.Third);
            foreach (FlightView.CameraInfo item in needAdds)
            {
                item.AddAimWeaponChange(launcher, new ViewChangeAimSwitchZoomCam(item, cam, 10));
            }
        }
    }

    private void Update()
    {
        if (GunCamera && GunCamera.enabled)
            CrosshairShow = true;
        else
            CrosshairShow = false;
    }

    protected void OnGUI()
    {
        if (CrosshairShow&&Crosshair)
            GUI.DrawTexture(new Rect(Screen.width / 2 - Crosshair.width / 2, Screen.height / 2 - Crosshair.height / 2, Crosshair.width, Crosshair.height), Crosshair);
    }
}
