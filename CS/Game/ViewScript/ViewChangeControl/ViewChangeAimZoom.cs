using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewChangeAimZoom : ViewChangeAim
{

    GameObject cameraObj;
    float originFOV = 60f;  //原FOV
    float AimFOV = 30f;     //瞄准的FOV

    float targetFOV;        //插值目标的FOV
    float lateFOV;          //当前处于插值的FOV
    public float LerpSpeed = 60f;   //插值速度

    private bool finishedLerp = true;
    public bool FinishedLerp { get => finishedLerp; }

    public override void Update()
    {
        //LerpToAimFOV();
    }

    private bool LerpToAimFOV()
    {
        bool result = false;
        if (Mathf.Abs(targetFOV - lateFOV) > 0.1f)
        {
            lateFOV = Mathf.Lerp(lateFOV, targetFOV, LerpSpeed * Time.deltaTime);
            result = false;
        }
        else
        {
            lateFOV = targetFOV;
            result = true;
        }
        if (cameraObj)
        {
            Camera camera = cameraObj.GetComponent<Camera>();
            if (camera)
                camera.fieldOfView = lateFOV;
        }
        return result;
    }

    ViewChangeAimZoom(GameObject cameraObj)
    {
        this.cameraObj = cameraObj;
        originFOV = cameraObj.GetComponent<Camera>().fieldOfView;
        targetFOV = originFOV;
        lateFOV = originFOV;
    }

    public ViewChangeAimZoom(GameObject cameraObj, float aimFov)
    {
        this.cameraObj = cameraObj;
        this.AimFOV = aimFov;
        this.originFOV = cameraObj.GetComponent<Camera>().fieldOfView;
        targetFOV = originFOV;
        lateFOV = originFOV;
    }

    public ViewChangeAimZoom(GameObject cameraObj,float originFov, float aimFov)
    {
        this.cameraObj = cameraObj;
        this.AimFOV = aimFov;
        this.originFOV = originFov;
        targetFOV = originFOV;
        lateFOV = originFOV;
    }

    public override void Canceled()
    {
        if (originFOV <= 0 || originFOV > 179)
            originFOV = 60;
        targetFOV = originFOV;
        finishedLerp = LerpToAimFOV();
    }

    public override void Close()
    {
        Camera cam = this.cameraObj.GetComponent<Camera>();
        if (cam)
        {
            cam.fieldOfView = originFOV;
            cam.enabled = false;
        }
        AudioListener lis = cameraObj.GetComponent<AudioListener>();
        if (lis)
            lis.enabled = false;
    }

    public override void Started()
    {
        Camera cam = this.cameraObj.GetComponent<Camera>();
        if (cam)
        {
            cam.fieldOfView = originFOV;
            cam.enabled = true;
        }
        AudioListener lis = cameraObj.GetComponent<AudioListener>();
        if (lis)
            lis.enabled = true;
        if (originFOV <= 0 || originFOV > 179)
            targetFOV = 30f;
        targetFOV = AimFOV;
        finishedLerp = LerpToAimFOV();
    }

    public override void Open()
    {
        base.Open();
        Camera cam = this.cameraObj.GetComponent<Camera>();
        if (cam)
            cam.enabled = true;
        AudioListener lis = cameraObj.GetComponent<AudioListener>();
        if (lis)
            lis.enabled = true;
    }
}
