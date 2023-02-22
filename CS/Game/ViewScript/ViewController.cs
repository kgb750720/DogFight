using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(FlightView))]
public class ViewController : MonoBehaviour
{
    public FlightView View;
    Vector2 forwardInput = Vector2.zero;
    Vector2 moveInput = Vector2.zero;
    float rise = 0f;
    float AccelerateCoef = 1;
    public float MoveSpeed=500f;
    public float RotateSpeed = 90f;
    public Controls actions { get; private set; }
    private void Awake()
    {
        View = GetComponent<FlightView>();
        InitInputSetting();
    }

    void InitInputSetting()
    {
        actions = new Controls();
        actions.Enable();

        //视角
        actions.CameraControl.Forward.started += ActionForward;
        actions.CameraControl.Forward.performed += ActionForward;
        actions.CameraControl.Forward.canceled += ActionForward;

        //移动
        actions.CameraControl.Move.started += ActionMove;
        actions.CameraControl.Move.performed += ActionMove;
        actions.CameraControl.Move.canceled += ActionMove;

        //上升
        actions.CameraControl.Rise.started += ActionRise;
        actions.CameraControl.Rise.performed += ActionRise;
        actions.CameraControl.Rise.canceled += ActionRise;

        //缩放
        actions.CameraControl.ViewZoom.started += ActionViewZoom;
        actions.CameraControl.ViewZoom.performed += ActionViewZoom;
        actions.CameraControl.ViewZoom.canceled += ActionViewZoom;

        //缩放
        actions.CameraControl.Accelerate.started += ActionAccelerate;
        actions.CameraControl.Accelerate.performed += ActionAccelerate;
        actions.CameraControl.Accelerate.canceled += ActionAccelerate;
    }

    private void ActionAccelerate(InputAction.CallbackContext obj)
    {
        if (obj.started || obj.performed)
            AccelerateCoef = 2;
        else
            AccelerateCoef = 1;
    }

    private void ActionViewZoom(InputAction.CallbackContext obj)
    {
        if (obj.started || obj.performed)
            View.ViewAim = true;
        else
            View.ViewAim = false;
    }

    private void ActionRise(InputAction.CallbackContext obj)
    {
        rise = obj.ReadValue<float>();
    }

    private void ActionMove(InputAction.CallbackContext obj)
    {
        moveInput = obj.ReadValue<Vector2>();
    }

    private void ActionForward(InputAction.CallbackContext obj)
    {
        forwardInput = obj.ReadValue<Vector2>();
    }



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        if (!View.Target || (View.Target && !View.Target.activeSelf))
        {
            actions.Enable();
            transform.Translate(transform.forward * moveInput.y * MoveSpeed* AccelerateCoef * Time.fixedDeltaTime, Space.World);
            transform.Translate(transform.right * moveInput.x * MoveSpeed* AccelerateCoef * Time.fixedDeltaTime, Space.World);
            float x = transform.rotation.eulerAngles.x;
            float y = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(new Vector3(x, y, 0));
            //这里要将输入的坐标映射做一下转换以得到正确的转向角度
            x = forwardInput.y;
            y = forwardInput.x;
            //float z = transform.rotation.z;
            transform.Rotate(transform.rotation * new Vector3(x, y, 0) * RotateSpeed * Time.fixedDeltaTime, Space.World);
            //if (transform.rotation.eulerAngles.x > 90 && transform.rotation.eulerAngles.x < 270)
            //{
            //    Vector3 elur = transform.eulerAngles;
            //    if (Mathf.Abs(transform.rotation.eulerAngles.x - 90) < Mathf.Abs(transform.rotation.eulerAngles.x - 270))
            //        elur.x = 90f;
            //    else
            //        elur.y = 270f;
            //    transform.rotation=Quaternion.Euler(elur);
            //}
            transform.Translate(transform.up * rise * MoveSpeed * Time.fixedDeltaTime, Space.World);
        }
        else
            actions.Disable();
    }
}
