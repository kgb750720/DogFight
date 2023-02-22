/// <summary>
/// Player controller.
/// </summary>
using UnityEngine;
using System.Collections;
using System;
using UnityEngine.InputSystem;



namespace Flight
{
	[RequireComponent(typeof(FlightSystem))]
	public class PlayerController : MonoBehaviour
	{

		public FlightSystem flight;// Core plane system
		FlightView View;
		public bool Active = true;
		public bool SimpleControl;// make it easy to control Plane will turning easier.
		public bool Acceleration;// Mobile*** enabled gyroscope controller
		public float AccelerationSensitivity = 5;// Mobile*** gyroscope sensitivity
		private TouchScreenVal controllerTouch;// Mobile*** move
		private TouchScreenVal fireTouch;// Mobile*** fire
		private TouchScreenVal switchTouch;// Mobile*** swich
		private TouchScreenVal sliceTouch;// Mobile*** slice
		private bool directVelBack;
		public GUISkin skin;
		public bool ShowHowto;

		public ItemUse Item;



		//新输入系统
		Controls actions;
		bool FirePress = false;
		float UpSpeed = 0;  //0不变 ，>0加速，<0减速
		Vector2 RollPitchTarget = Vector2.zero;
		Vector2 RollPitchInput = Vector2.zero;
		float PichInput = 0;
		float RollInput = 0;    //备用
		Vector2 RollPitchValue = Vector2.zero;
		float YawInput = 0;
		float YawValue = 0;
		public float RollPitchSpeed = 2;
		public float YawSpeed = 2;
		float TurnTargetAngle = 0;
		float TurnAngle = 0;
		public float LandWheelsTurnSpeed = 30;


		private void Awake()
		{
			
		}

		

		private void SetInput()
		{
			actions = new Controls();
			actions.Enable();


			//绑定新输入系统输入回调

			//翻滚俯仰动作
			//按下时
			actions.FlightControl.Roll_Pitch.started += ActionRollPitch;
			//按住时
			actions.FlightControl.Roll_Pitch.performed += ActionRollPitch;
			//松开时
			actions.FlightControl.Roll_Pitch.canceled += ActionRollPitch;

			//单独的俯仰动作
			//按下时
			actions.FlightControl.Pitch.started += ActionPitch;
			//按住时
			actions.FlightControl.Pitch.performed += ActionPitch;
			//松开时
			actions.FlightControl.Pitch.canceled += ActionPitch;

			//偏航动作
			//按下时
			actions.FlightControl.Yaw.started += ActionYaw;
			//按住时
			actions.FlightControl.Yaw.performed += ActionYaw;
			//松开时
			actions.FlightControl.Yaw.canceled += ActionYaw;

			// 陆地转弯动作
			//按下时
			actions.FlightControl.LandTurn.started += ActionLandTurn;
			//按住时
			actions.FlightControl.LandTurn.performed += ActionLandTurn;
			//松开时
			actions.FlightControl.LandTurn.canceled += ActionLandTurn;


			//油门挡位动作
			//按下时
			actions.FlightControl.Throttle.started += ActionThrottle;
			//按住时
			actions.FlightControl.Throttle.performed += ActionThrottle;
			//松开时
			actions.FlightControl.Throttle.canceled += ActionThrottle;

			//即时灵敏度调整
			//按下时
			actions.FlightControl.CurrentSensitiveSet.started += ActionCurrentSensitiveSet;
			//按住时
			actions.FlightControl.CurrentSensitiveSet.performed += ActionCurrentSensitiveSet;
			//松开时
			actions.FlightControl.CurrentSensitiveSet.canceled += ActionCurrentSensitiveSet;

			//使用推进器
			//按下时
			actions.FlightControl.AfterBurner.started += ActionAfterBurner;
			//按住时
			actions.FlightControl.AfterBurner.performed += ActionAfterBurner;
			//松开时
			actions.FlightControl.AfterBurner.canceled += ActionAfterBurner;

			//使用刹车
			//按下时
			actions.FlightControl.Break.started += ActionBreak;
			//按住时
			actions.FlightControl.Break.performed += ActionBreak;
			//松开时
			actions.FlightControl.Break.canceled += ActionBreak;

			//切换视角
			//按下时
			actions.FlightControl.SwitchView.started += ActionSwitchView;

			//切换武器
			//按下时
			actions.FlightControl.SwtichWeapon.started += ActionSwtichWeapon;

			//开火
			//按下时
			actions.FlightControl.Fire.started += ActionFire;
			//按住时
			actions.FlightControl.Fire.performed += ActionFire;
			//松开时
			actions.FlightControl.Fire.canceled += ActionFire;

			//后视
			//按下时
			actions.FlightControl.BackView.started += ActionBackView;
			//按住时
			actions.FlightControl.BackView.performed += ActionBackView;
			//松开时
			actions.FlightControl.BackView.canceled += ActionBackView;

			//瞄准
			//按下时
			actions.FlightControl.Aim.started += ActionAim;
			//按住时
			actions.FlightControl.Aim.performed += ActionAim;
			//松开时
			actions.FlightControl.Aim.canceled += ActionAim;

			//使用道具
			//按下时
			actions.FlightControl.ItemUse.started += ActionUseItem;
			//按住时
			actions.FlightControl.ItemUse.performed += ActionUseItem;
			//松开时
			actions.FlightControl.ItemUse.canceled += ActionUseItem;

			//切换起落架状态
			//按下时
			actions.FlightControl.SwitchLandGear.started += ActionSwitchLandGear;

			//切换发射器发射数量
			actions.FlightControl.SwitchLauncherFireNub.started += ActionSwitchLauncherFireNub;

			//切换锁定模式
			actions.FlightControl.SwitchLauncherLockModel.started += ActionSwitchLauncherLockModel;

		}

		private void ActionSwitchLauncherLockModel(InputAction.CallbackContext obj)
		{
			if (GetComponent<JetSync>()!=null)
			{
				GetComponent<JetSync>().CmdSyncSwitchLauncherLockModel();
			}
			else
			{
				WeaponController controller = flight.WeaponControl;
				controller.SwitchLauncherLockModel(controller.CurrentWeaponIdx);
			}
		}

		private void ActionSwitchLauncherFireNub(InputAction.CallbackContext obj)
		{
			if (GetComponent<JetSync>()!=null)
			{
				GetComponent<JetSync>().CmdSyncAddLauncherOnceFireNub((int)obj.ReadValue<float>());
			}
			else
			{
				WeaponController controller = flight.WeaponControl;
				if (controller.CurrLauncher && controller.CurrLauncher.MultiLockModel)
					controller.AddLauncherOnceFireNub(controller.CurrentWeaponIdx, (int)obj.ReadValue<float>());
			}
		}

		private void ActionSwitchLandGear(InputAction.CallbackContext obj)
		{
			flight.SwitchLandGear();
		}

		private void ActionLandTurn(InputAction.CallbackContext obj)
		{
			TurnTargetAngle = obj.ReadValue<float>() * 45;
		}

		private void ActionPitch(InputAction.CallbackContext obj)
		{
			PichInput = obj.ReadValue<float>();
		}

		private void ActionUseItem(InputAction.CallbackContext obj)
		{
			if (GetComponent<JetSync>()!=null)
			{
				if (obj.started)
					GetComponent<JetSync>().CmdUsed(true);
				else if (obj.canceled)
					GetComponent<JetSync>().CmdUsed(false);
			}
			else if (Item != null)
			{
				if (obj.started)
					Item.Used = true;
				else if (obj.canceled)
					Item.Used = false;
			}

		}

		private void ActionAim(InputAction.CallbackContext obj)
		{
			if (obj.started)
				View.ViewAim = true;
			else if (obj.canceled)
				View.ViewAim = false;
		}

		private void ActionBackView(InputAction.CallbackContext obj)
		{
			if (obj.started)
				View.ViewBack = true;
			else if (obj.canceled)
				View.ViewBack = false;

		}

		private void ActionFire(InputAction.CallbackContext obj)
		{
			if (obj.started)
			{
				FirePress = true;
				if (GetComponent<JetSync>() != null)
					GetComponent<JetSync>().PressTrigger(true);
			}
			else if (obj.canceled)
			{
				FirePress = false;
				if (GetComponent<JetSync>() != null)
					GetComponent<JetSync>().PressTrigger(false);
			}
		}

		private void ActionSwtichWeapon(InputAction.CallbackContext obj)
		{
			if (GetComponent<JetSync>()!=null)
				GetComponent<JetSync>().CmdSwtichWeapon();
			else
				flight.WeaponControl.SwitchWeapon();
		}

		private void ActionSwitchView(InputAction.CallbackContext obj)
		{
			if (obj.started && View)
				View.SwitchCamera = true;
		}

		private void ActionBreak(InputAction.CallbackContext obj)
		{
			flight.BrakeControl(obj.ReadValue<float>());
		}

		private void ActionAfterBurner(InputAction.CallbackContext obj)
		{
			flight.AfterBurnerControl(obj.ReadValue<float>());
		}

		private void ActionCurrentSensitiveSet(InputAction.CallbackContext obj)
		{
			flight.SensitiveControl(obj.ReadValue<float>());
		}

		private void ActionThrottle(InputAction.CallbackContext obj)
		{
			//print(obj.ReadValue<float>());
			UpSpeed = obj.ReadValue<float>();
			//flight.SpeedUp(obj.ReadValue<float>());
			//print(obj.ReadValue<float>());
		}

		private void ActionYaw(InputAction.CallbackContext obj)
		{
			YawInput = obj.ReadValue<float>();
		}

		private void ActionRollPitch(InputAction.CallbackContext obj)
		{
			RollPitchInput = obj.ReadValue<Vector2>();
			//print(inputAxis);

		}

		void Start()
		{
			SetInput();
			Item = GetComponent<ItemInterfere>();
			flight = this.GetComponent<FlightSystem>();
			View = (FlightView)GameObject.FindObjectOfType(typeof(FlightView));


			// setting all Touch screen controller in the position
			controllerTouch = new TouchScreenVal(new Rect(0, 0, Screen.width / 2, Screen.height - 100));
			fireTouch = new TouchScreenVal(new Rect(Screen.width / 2, 0, Screen.width / 2, Screen.height));
			switchTouch = new TouchScreenVal(new Rect(0, Screen.height - 100, Screen.width / 2, 100));
			sliceTouch = new TouchScreenVal(new Rect(0, 0, Screen.width / 2, 50));

			if (flight)
				directVelBack = flight.DirectVelocity;
		}

		void Update()
		{
			//if (!flight || !Active)
			//	return;
			//#if UNITY_EDITOR || UNITY_WEBPLAYER || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
			//// On Desktop
			//DesktopController();
			//#else
			//// On Mobile device
			//MobileController ();
			//#endif


			InputSystemUpdate();

		}

		private void FixedUpdate()
		{
			//InputSystemUpdate();
		}

        private void OnDestroy()
        {
			if (actions != null)
			{
				actions.Disable();
				actions.Dispose();
			}
        }

        private void InputSystemUpdate()
		{
			if (flight)
			{
				RollPitchTarget.x = Mathf.Abs(RollPitchInput.x) > Mathf.Abs(RollInput) ? RollPitchInput.x : RollInput;
				RollPitchTarget.y = Mathf.Abs(RollPitchInput.y) > Mathf.Abs(PichInput) ? RollPitchInput.y : PichInput;

				if (Vector2.Distance(RollPitchValue, RollPitchTarget) > 0.01f)
					RollPitchValue = Vector2.Lerp(RollPitchValue, RollPitchTarget, RollPitchSpeed * Time.deltaTime);
				if (Mathf.Abs(YawInput - YawValue) > 0.01f)
					YawValue = Mathf.Lerp(YawValue, YawInput, YawSpeed * Time.deltaTime);

				flight.SimpleControl = SimpleControl;   //同步简单操控模式状态
				flight.RollPitchControl(RollPitchValue);
				if (SimpleControl)
				{
					flight.DirectVelocity = false;
					flight.YawControl(YawValue);
				}
				else
				{
					flight.DirectVelocity = directVelBack;
					flight.YawControl(YawValue);
				}
				if (FirePress)
					flight.WeaponControl.LaunchWeapon();
				flight.SpeedUp(UpSpeed);
				if (flight.LandGearOpen)
				{
					TurnAngle = Mathf.Lerp(TurnAngle, TurnTargetAngle, LandWheelsTurnSpeed * Time.deltaTime);
					flight.TurnWheelsControll(TurnAngle);
				}
			}
		}

		void DesktopController()
		{
			// Desktop controller
			flight.SimpleControl = SimpleControl;

			// lock mouse position to the center.
			MouseLock.MouseLocked = true;

			//flight.AxisControl (new Vector2 (Input.GetAxis ("Mouse X"), Input.GetAxis ("Mouse Y")));
			flight.RollPitchControl(new Vector2(Input.GetAxis("Roll"), Input.GetAxis("Pitch")));
			if (SimpleControl)
			{
				flight.DirectVelocity = false;
				flight.YawControl(Input.GetAxis("Roll"));
			}
			else
			{
				flight.DirectVelocity = directVelBack;
			}

			flight.YawControl(Input.GetAxis("Yaw"));
			flight.SpeedUp(Input.GetAxis("Throttle"));
			flight.SensitiveControl(Input.GetAxis("Sensitive"));
			flight.AfterBurnerControl(Input.GetAxis("AfterBurner"));
			flight.BrakeControl(Input.GetAxis("Brake"));

			if (Input.GetButton("Fire1"))
			{
				flight.WeaponControl.LaunchWeapon();
			}

			if (Input.GetButtonDown("Fire2"))
			{
				flight.WeaponControl.SwitchWeapon();
			}

			if (Input.GetKeyDown(KeyCode.C))
			{
				if (View)
					View.SwitchCameras();
			}
		}

		void MobileController()
		{
			// Mobile controller

			flight.SimpleControl = SimpleControl;

			if (Acceleration)
			{
				// get axis control from device acceleration
				Vector3 acceleration = Input.acceleration;
				Vector2 accValActive = new Vector2(acceleration.x, (acceleration.y + 0.3f) * 0.5f) * AccelerationSensitivity;
				flight.FixedX = false;
				flight.FixedY = false;
				flight.FixedZ = true;

				flight.RollPitchControl(accValActive);
				flight.YawControl(accValActive.x);
			}
			else
			{
				flight.FixedX = true;
				flight.FixedY = false;
				flight.FixedZ = true;
				// get axis control from touch screen
				Vector2 dir = controllerTouch.OnDragDirection(true);
				dir = Vector2.ClampMagnitude(dir, 1.0f);
				flight.RollPitchControl(new Vector2(dir.x, -dir.y) * AccelerationSensitivity * 0.7f);
				flight.YawControl(dir.x * AccelerationSensitivity * 0.3f);
			}
			sliceTouch.OnDragDirection(true);
			// slice speed
			flight.SpeedUp(sliceTouch.slideVal.x);

			if (fireTouch.OnTouchPress())
			{
				flight.WeaponControl.LaunchWeapon();
			}
			if (switchTouch.OnTouchPress())
			{

			}
		}


		// you can remove this part..
		void OnGUI()
		{
			if (!ShowHowto)
				return;

			if (skin)
				GUI.skin = skin;
			/*
			if (GUI.Button (new Rect (20, 150, 200, 40), "Gyroscope " + Acceleration)) {
				Acceleration = !Acceleration;
			}

			if (GUI.Button (new Rect (20, 200, 200, 40), "Change View")) {
				if (View)
					View.SwitchCameras ();	
			}

			if (GUI.Button (new Rect (20, 250, 200, 40), "Change Weapons")) {
				if (flight)
					flight.WeaponControl.SwitchWeapon ();
			}

			if (GUI.Button (new Rect (20, 300, 200, 40), "Simple Control " + SimpleControl)) {
				if (flight)
					SimpleControl = !SimpleControl;
			}
			*/
		}

	}
}