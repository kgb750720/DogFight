using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewChangeAimSwitchCam : ViewChangeAim
{
    FlightView.CameraInfo mainCameraInfo;
    GameObject aimCamera;
    public GameObject AimCamera { get => aimCamera; }

    public ViewChangeAimSwitchCam(FlightView.CameraInfo mainCameraInfo, GameObject aimCamera)
    {
        this.mainCameraInfo = mainCameraInfo;
        this.aimCamera = aimCamera;
    }

    public override void Close()
    {
        Camera cam = aimCamera.GetComponent<Camera>();
        if (cam)
            cam.enabled = false;
        AudioListener lis = aimCamera.GetComponent<AudioListener>();
        if (lis)
            lis.enabled = false;

    }

    public override void Open()
    {
    }

    public override void Started()
    {
        mainCameraInfo.CloseMainCam();
        Camera cam = aimCamera.GetComponent<Camera>();
        if (cam)
            cam.enabled = true;
        AudioListener lis = aimCamera.GetComponent<AudioListener>();
        if (lis)
            lis.enabled = true;

    }

    public override void Canceled()
    {
        Camera cam = aimCamera.GetComponent<Camera>();
        if (cam)
            cam.enabled = false;
        AudioListener lis = aimCamera.GetComponent<AudioListener>();
        if (lis)
            lis.enabled = false;
        mainCameraInfo.Open();

    }

    public override void Update()
    {
        base.Update();
    }
}
