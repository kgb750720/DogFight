using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ViewChangeAimSwitchZoomCam : ViewChangeAimZoom
{

    GameObject aimCam;
    public GameObject AimCamera { get => aimCam; }
    //bool Aimed = false;
    FlightView.CameraInfo mainCameraInfo;
    public ViewChangeAimSwitchZoomCam(FlightView.CameraInfo mainCameraInfo, GameObject aimCam, float aimFov) : base(mainCameraInfo.MainView, aimFov)
    {
        this.aimCam = aimCam;
        this.mainCameraInfo = mainCameraInfo;
    }

    public override void Canceled()
    {
        
        AimCameraClose();
        mainCameraInfo.Open();
        //Aimed = false;
        base.Canceled();

    }

    public override void Close()
    {
        AimCameraClose();
        base.Close();
    }

    public override void Open()
    {
        base.Open();
        if (FinishedLerp)
        {
            mainCameraInfo.CloseMainCam();
            AimCameraOpen();
        }
    }

    public override void Started()
    {
        
        base.Started();
        if (FinishedLerp)
        {
            mainCameraInfo.CloseMainCam();
            AimCameraOpen();
        }
        //Aimed = true;
    }

    public override void Update()
    {
        base.Update();
    }

    void AimCameraOpen()
    {
        if (aimCam)
        {
            Camera camA = aimCam.GetComponent<Camera>();
            if (camA)
                camA.enabled = true;
            AudioListener listener = aimCam.GetComponent<AudioListener>();
            if (listener)
                listener.enabled = true;
        }
    }

    private void AimCameraClose()
    {
        if (aimCam)
        {
            Camera camA = aimCam.GetComponent<Camera>();
            if (camA)
                camA.enabled = false;
            AudioListener listener = aimCam.GetComponent<AudioListener>();
            if (listener)
                listener.enabled = false;
        }
    }
}


//public class ViewChangeAimSwitchCam : ViewChangeAim
//{
//    GameObject aimCam;
//    FlightView flightView;
//    Transform aimTrans;
//    Transform restTrans;
//    Transform targetTrans;
//    float LerpSpeed = 0.1f;
//    bool Aimed = false;
//    bool AimFinished = true;
//    public ViewChangeAimSwitchCam(FlightView flightView, GameObject aimCam,float lerpSpeed)
//    {

//        this.aimCam = aimCam;
//        GameObject restObj = new GameObject("CameraRestPoint");
//        GameObject aimObj = new GameObject("CameraAimPoint");
//        restObj.transform.position = aimCam.transform.position;
//        restObj.transform.parent = aimCam.transform.parent;
//        aimObj.transform.position = aimCam.transform.position;
//        aimObj.transform.parent = aimCam.transform.parent;
//        restTrans = restObj.transform;
//        aimTrans = aimObj.transform;
//        this.flightView = flightView;
//        LerpSpeed = lerpSpeed;
//        targetTrans = restTrans;
//    }

//    public override void Started()
//    {
//        if (!Aimed)
//        {
//            restTrans.position = flightView.CurrCamera.MainView.transform.position;
//            targetTrans = aimTrans;
//            //aimCam.transform.parent = aimTrans;
//            aimCam.transform.position = restTrans.position;
//            AimFinished = false;
//        }
//        Aimed = true;
//        LerpToTarget();
//        flightView.CurrCamera.CloseMainCam();
//        AimCameraOpen();
//    }

//    public override void Canceled()
//    {
//        if (Aimed)
//        {
//            restTrans.position = flightView.transform.position;
//            targetTrans = restTrans;
//            Vector3 pos = aimCam.transform.position;
//            //aimCam.transform.parent = targetTrans;
//            aimCam.transform.position = pos;

//        }
//        Aimed = false;
//        AimCameraClose();
//        flightView.CurrCamera.Open();
//        bool lerpFinished = LerpToTarget();
//        if (lerpFinished&&!AimFinished)
//        {
//            flightView.CurrCamera.CloseMainCam();
//            AimCameraOpen();
//            lerpFinished = true;
//            AimFinished = true;
//        }
//    }

//    private bool LerpToTarget()
//    {
//        //Debug.Log(aimCam.transform.parent.name);
//        //if (Vector3.Distance(aimCam.transform.localPosition, Vector3.zero) > 0)
//        //{
//        //    aimCam.transform.localPosition = Vector3.Lerp(aimCam.transform.localPosition, Vector3.zero, LerpSpeed * Time.deltaTime/*aimCam.transform.localScale.magnitude*/);
//        //    //Vector3 forward = -aimCam.transform.localPosition / LerpSpeed * Time.time;
//        //    //Vector3 forward = -aimCam.transform.localPosition * Time.deltaTime;
//        //    //Debug.Log("Distance" + Vector3.Distance(aimCam.transform.localPosition, Vector3.zero) + "\t" + "LocalPosition:" + aimCam.transform.localPosition + "\t" + "Forward:" + forward);
//        //    //aimCam.transform.localPosition += forward;

//        //}
//        //else
//        //    aimCam.transform.localPosition = Vector3.zero;
//        if (Vector3.Distance(aimCam.transform.position, targetTrans.position) > 0.5f)
//            aimCam.transform.position = Vector3.Lerp(aimCam.transform.position, targetTrans.position, LerpSpeed * Time.deltaTime);
//        else
//        {
//            aimCam.transform.parent = targetTrans;
//            aimTrans.localPosition = Vector3.zero;
//        }

//        return Vector3.Distance(aimCam.transform.position , targetTrans.position)<0.1f;
//    }

//    public override void Close()
//    {

//        AimCameraClose();
//        aimCam.transform.position = restTrans.position;
//        Aimed = false;
//        AimFinished = true;
//    }

//    void AimCameraOpen()
//    {
//        Camera camA = aimCam.GetComponent<Camera>();
//        if (camA)
//            camA.enabled = true;
//        AudioListener listener = aimCam.GetComponent<AudioListener>();
//        if (listener)
//            listener.enabled = true;
//    }

//    private void AimCameraClose()
//    {
//        Camera camA = aimCam.GetComponent<Camera>();
//        if (camA)
//            camA.enabled = false;
//        AudioListener listener = aimCam.GetComponent<AudioListener>();
//        if (listener)
//            listener.enabled = false;
//    }



//    public override void Update()
//    {

//        //if (Vector3.Distance(aimCam.transform.localPosition, Vector3.zero) > 1f)
//        //    aimCam.transform.localPosition = Vector3.Lerp(aimCam.transform.localPosition, Vector3.zero, LerpSpeed * Time.deltaTime);
//        //else
//        //    aimCam.transform.localPosition = Vector3.zero;
//        //if (Aimed)
//        //{
//        //    flightView.CurrCamera.Close();
//        //    AimCameraOpen();
//        //}
//        //else if (!Aimed&&aimCam.transform.localPosition==Vector3.zero)
//        //{
//        //    AimCameraClose();
//        //    flightView.CurrCamera.Open();
//        //}

//    }
//}
