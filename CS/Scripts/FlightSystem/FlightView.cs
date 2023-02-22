/// <summary>
/// Flight view. this script is the Camera Follower
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mewlist;

[RequireComponent(typeof(Camera))]

public class FlightView : MonoBehaviour
{
	public class CameraInfo
	{
		public enum ViewTypeSet
		{
			Cockpit,
			Third
		}
		public CameraInfo(ViewTypeSet viewType, GameObject mainView, ViewChangeAim defultAimChange, ViewChangeBack defultBackChange, FlightSystem effectiveFlight)
		{
			this.MainView = mainView;
			this.DefultAimChange = defultAimChange;
			lateAimChange = defultAimChange;
			this.DefultBackChange = defultBackChange;
			lateBackChange = defultBackChange;
			this.ViewType = viewType;
			this.EffectiveFlight = effectiveFlight;
		}
		public GameObject MainView; //主摄像头

		ViewChangeAim DefultAimChange;
		ViewChangeAim lateAimChange;
		/// <summary>
		/// 相机瞄准方案绑定
		/// </summary>
		public ViewChangeAim AimChange { 
			get
			{
				ViewChangeAim res = DefultAimChange;
				if(EffectiveFlight!=null&&EffectiveFlight.WeaponControl!=null&& EffectiveFlight.WeaponControl.WeaponList.Count>0 && AimWeaponChangeSet.ContainsKey(EffectiveFlight.WeaponControl.CurrLauncher))
					res = AimWeaponChangeSet[EffectiveFlight.WeaponControl.CurrLauncher];
				if (res != lateAimChange && lateAimChange != DefultAimChange)
				{
					if(lateAimChange!=null)
						lateAimChange.Close();
					DefultAimChange.Open();
					//DefultAimChange.Started();
				}
				lateAimChange = res;
				//if (lateBackChange != null&&lateBackChange != DefultBackChange)
				//	lateBackChange.Close();
				return res;
			}
		}
		
		ViewChangeBack DefultBackChange;
		ViewChangeBack lateBackChange;
		/// <summary>
		/// 相机后视方案绑定
		/// </summary>
		public ViewChangeBack BackChange { 
			get
			{
				ViewChangeBack res = DefultBackChange;
				//if (lateAimChange != null && lateAimChange != DefultAimChange)
				//	lateAimChange.Close();
				lateBackChange = res;
				return res;
			} 
		}

		FlightSystem EffectiveFlight;

		private Dictionary<WeaponLauncher, ViewChangeAim> AimWeaponChangeSet=new Dictionary<WeaponLauncher, ViewChangeAim>();	//摄像机存储的武器瞄准替代方案

		public void AddAimWeaponChange(WeaponLauncher launcher, ViewChangeAim viewChange)
        {
			AimWeaponChangeSet[launcher] = viewChange;
        }

		public ViewChangeAim GetAimWeaponChange(WeaponLauncher launcher)
        {
			if (AimWeaponChangeSet.ContainsKey(launcher))
				return AimWeaponChangeSet[launcher];
			return null;

        }

		public ViewTypeSet ViewType { get; }
		public virtual void Open()
		{
			if (MainView.GetComponent<Camera>())
				MainView.GetComponent<Camera>().enabled = true;
			if (MainView.GetComponent<AudioListener>())
				MainView.GetComponent<AudioListener>().enabled = true;
		}
		public virtual void CloseAll()
        {
			//AimChange.Close();
			//BackChange.Close();
			DefultAimChange.Close();
			DefultAimChange.Close();
			if (lateBackChange != null && lateBackChange != DefultBackChange)
				lateBackChange.Close();
			if (lateAimChange != null && lateAimChange != DefultAimChange)
				lateAimChange.Close();
			foreach (ViewChangeAim item in AimWeaponChangeSet.Values)
            {
                item.Close();
            }
            CloseMainCam();
        }

        public void CloseMainCam()
        {
            if (MainView.GetComponent<Camera>())
                MainView.GetComponent<Camera>().enabled = false;
            if (MainView.GetComponent<AudioListener>())
                MainView.GetComponent<AudioListener>().enabled = false;
        }
    }


	private static FlightView _instance = null;
	public static FlightView singleton { get => _instance; }

	[SerializeField]
	private GameObject target;// player ( Your Plane)
	public GameObject Target
    {
		set
        {
			target = value;
			if(backViewCS)
				backViewTargetRest(backViewCS.gameObject);
        }
		get
        {
			if (target)
				return target;
			return null;
        }
    }
	//public GameObject[] Cameras;// all cameras in the game ( in case of able to swith the views).
	public List<CameraInfo> Cameras=new List<CameraInfo>();//主摄像机集合
	public int firstCameraIdx = 0;	//初始摄像机索引
	public float FollowSpeedMult = 10f; // camera following speed 
	public float TurnSpeedMult = 5; // camera turning speed 
	private int indexCamera;// current camera index
	public Vector3 Offset = new Vector3 (14, 0.3f, 14);// position offset between plan and camera


	//瞄准相机绑定<前视相机,瞄准方案>
	//public Dictionary<GameObject, ViewChangeAim> aimCamera = new Dictionary<GameObject, ViewChangeAim>();

	//前视后视相机绑定<前视相机，后视方案>
	//public Dictionary<GameObject, ViewChangeBack> backCamera = new Dictionary<GameObject, ViewChangeBack>();

	//第三人称后视摄像头脚本
	private FlightViewBack backViewCS;

	//根据武器加入的额外瞄准效果
	//public Dictionary<WeaponLauncher, Dictionary<CameraInfo.ViewTypeSet, ViewChangeAim>> WeaponAimChanges = new Dictionary<WeaponLauncher, Dictionary<CameraInfo.ViewTypeSet, ViewChangeAim>>();

	//当前存在的瞄准效果更新,相机更新放在lateUpdate
	//ViewChangeAim viewChangeAimLateUpdate;

	//上一个武器瞄准方案
	//ViewChangeAim lateWeaponChange = null;

	public CameraInfo CurrCamera 
	{
		get 
		{
			if (indexCamera > 0 && indexCamera < Cameras.Count)
				return Cameras[indexCamera];
			else
				return null;
		}
	}


	/// <summary>
	/// 根据类型返回Cameras里对应相机类型的相机
	/// </summary>
	/// <param name="viewType">相机类型</param>
	/// <returns>相机数组</returns>
	public List<CameraInfo> GetCmaeras(CameraInfo.ViewTypeSet viewType)
    {
		List<CameraInfo> result = new List<CameraInfo>();
        foreach (CameraInfo item in Cameras)
        {
			if(item.ViewType==viewType)
				result.Add(item);
        }
		return result;
    }

	// camera swith
	public void SwitchCameras ()
	{
		//indexCamera += 1;
		//if (indexCamera >= Cameras.Length) {
		//	indexCamera = 0;
		//}

		////将对应的后视视角关闭
		//if (backCamera.ContainsKey(Cameras[indexCamera]))
		//          backCamera[Cameras[indexCamera]].Close();

		//      //将对应的瞄准视角关闭
		//      if (aimCamera.ContainsKey(Cameras[indexCamera]))
		//          aimCamera[Cameras[indexCamera]].Close();


		//将所有集合中的摄像机和收音器关闭
		//for (int i = 0; i < Cameras.Count; i++)
		//{
		//    if (Cameras[i] && Cameras[i].GetComponent<Camera>())
		//        Cameras[i].GetComponent<Camera>().enabled = false;
		//    if (Cameras[i] && Cameras[i].GetComponent<AudioListener>())
		//        Cameras[i].GetComponent<AudioListener>().enabled = false;

		//    //将对应的后视视角关闭
		//    if (backCamera.ContainsKey(Cameras[i]))
		//        backCamera[Cameras[i]].Close();

		//    //将对应的瞄准视角关闭
		//    if (aimCamera.ContainsKey(Cameras[i]))
		//        aimCamera[Cameras[i]].Close();
		//}

		Cameras[indexCamera].CloseAll();
		//解除武器额外瞄准
		//if(Target)
  //      {
		//	WeaponController controller = Target.GetComponent<WeaponController>();
		//	if (controller && controller.CurrentWeapon < controller.WeaponLists.Count && WeaponAimChanges.ContainsKey(controller.WeaponLists[controller.CurrentWeapon]))
		//		if (WeaponAimChanges[controller.WeaponLists[controller.CurrentWeapon]].ContainsKey(Cameras[indexCamera].ViewType))
		//			WeaponAimChanges[controller.WeaponLists[controller.CurrentWeapon]][Cameras[indexCamera].ViewType].Close();

		//}
		indexCamera++;
		indexCamera %= Cameras.Count;
		Cameras[indexCamera].Open();


		//将当开启的主摄像机所在物体的摄像机和收音器关闭
		//Camera.main.gameObject.GetComponent<AudioListener>().enabled = false;
		//Camera.main.enabled = false;

		//开启对应摄像机
		//if (Cameras[indexCamera])
		//      {
		//          if (Cameras[indexCamera] && Cameras[indexCamera].GetComponent<Camera>())
		//              Cameras[indexCamera].GetComponent<Camera>().enabled = true;
		//          if (Cameras[indexCamera] && Cameras[indexCamera].GetComponent<AudioListener>())
		//              Cameras[indexCamera].GetComponent<AudioListener>().enabled = true;
		//      }
	}

	void Awake ()
    {
		// add this camera to primery
		//SetFirstCamera(this.gameObject);
		if (_instance==null)
		{
			_instance = this;
			GameObject obj = new GameObject();
			obj.name = "FlightCameraBack";
			obj.AddComponent<Camera>().enabled = false;
			obj.AddComponent<AudioListener>().enabled = false;
			obj.AddComponent<FlareLayer>().enabled = true;
			backViewCS = obj.AddComponent<FlightViewBack>();
			//backViewCS.Offset = this.Offset;
			backViewTargetRest(obj);
			firstCameraIdx = Cameras.Count - 1;
			ViewBack = false;
		}
    }

    private void backViewTargetRest(GameObject CamObj)
    {
		if (Target)
		{
			if (backViewCS.CameraInfo != null)
				RemoveCamera(backViewCS.CameraInfo);
			backViewCS.Target = Target;
			backViewCS.CameraInfo=AddCamera(CameraInfo.ViewTypeSet.Third, gameObject, new ViewChangeAimZoom(gameObject, 30), new ViewChangeBackSwitchCam(gameObject, CamObj), Target.GetComponent<FlightSystem>());
		}
    }

	//private void SetFirstCamera(GameObject cam){
	//GameObject[] temp = new GameObject[Cameras.Length+1];
	//Cameras.CopyTo(temp, 0);
	//Cameras = temp;
	//Cameras[temp.Length-1] = cam;
	//}

	public CameraInfo AddCamera(CameraInfo.ViewTypeSet cameraType, GameObject primeryCam, ViewChangeAim defultAimChange, ViewChangeBack defultBackChange, FlightSystem effectiveFlight)
    {
        //GameObject[] temp = new GameObject[Cameras.Length + 1];
        //Cameras.CopyTo(temp, 0);
        //Cameras = temp;
        //Cameras[temp.Length - 1] = forwrardCam;
        //backCamera[gameObject] = backCam;

        //Cameras.Add(primeryCam);
        //aimCamera[primeryCam] = aimChange;
        //backCamera[primeryCam] = backChange;
        var Info = new CameraInfo(cameraType, primeryCam, defultAimChange, defultBackChange, effectiveFlight);
        AddCameraPrepare(primeryCam);
		if(defultAimChange is ViewChangeAimSwitchCam)
        {
			AddCameraPrepare((defultAimChange as ViewChangeAimSwitchCam).AimCamera);
		}else if(defultAimChange is ViewChangeAimSwitchZoomCam)
        {
			AddCameraPrepare((defultAimChange as ViewChangeAimSwitchZoomCam).AimCamera);
		}
		AddCameraPrepare(defultBackChange.OriginCamera);
		AddCameraPrepare(defultBackChange.ChangeCamera);
		Cameras.Add(Info);
        return Info;
    }

	/// <summary>
	/// 为加入相机做准备
	/// </summary>
	/// <param name="addCamera"></param>
    public void AddCameraPrepare(GameObject addCamera)
    {
		Camera camera = addCamera.GetComponent<Camera>();
		camera.cullingMask &= ~(1 << 8);
		tryAddMassiveCloud(addCamera);
    }

	//尝试未摄相机添加体积云组件
	private void tryAddMassiveCloud(GameObject addCloudsCamera)
    {
        MassiveClouds massiveClouds = this.GetComponent<MassiveClouds>();
        if (!massiveClouds)
            return;
        List<MassiveCloudsProfile> list = massiveClouds.Profiles;
        if (list.Count == 0 || list.Contains(null))
            return;
        if (massiveClouds && !addCloudsCamera.GetComponent<MassiveClouds>())
        {
            MassiveClouds changeCloud = addCloudsCamera.AddComponent<MassiveClouds>();
            changeCloud.ambientMode = massiveClouds.ambientMode;
            changeCloud.ambient = massiveClouds.ambient;
            changeCloud.parameters = new List<MassiveCloudsParameter>();
            changeCloud.SetParameters(changeCloud.Parameters);
            changeCloud.profiles = new List<MassiveCloudsProfile>();
            changeCloud.SetProfiles(massiveClouds.Profiles);

            //changeCloud.SetProfiles(massiveClouds.Profiles);
            //changeCloud.SetParameters(massiveClouds.Parameters);

            MassiveCloudsCameraEffect originEffect = massiveClouds.GetComponent<MassiveCloudsCameraEffect>();
            if (originEffect)
            {
                MassiveCloudsCameraEffect changeEffect = addCloudsCamera.AddComponent<MassiveCloudsCameraEffect>();
                changeEffect.cameraEvent = originEffect.cameraEvent;
            }

            MassiveCloudsScriptableScrollSample originScroll = massiveClouds.GetComponent<MassiveCloudsScriptableScrollSample>();
            if (originScroll)
            {
                MassiveCloudsScriptableScrollSample changeScroll = addCloudsCamera.AddComponent<MassiveCloudsScriptableScrollSample>();
                changeScroll.massiveClouds = changeCloud;
                changeScroll.velocity = originScroll.velocity;
                changeScroll.direction = originScroll.direction;
                changeScroll.densities = originScroll.densities;
            }
        }
    }

    public bool RemoveCamera(CameraInfo info)
    {
        for(int i=0;i<Cameras.Count;i++)
        {
			if(Cameras[i]==info)
            {
				if (i <= indexCamera && indexCamera != 0)
				{
					Cameras[indexCamera].CloseAll();
					indexCamera--;
				}
                Cameras.Remove(info);
				if(Cameras.Count>0)
					Cameras[(indexCamera%=Cameras.Count)].Open();
				return true;
            }
        }
		return false;
    }

	void Start ()
	{
		// if player is not included try to find a player
		//if(!Target){
		//	PlayerManager player = (PlayerManager)GameObject.FindObjectOfType(typeof(PlayerManager));
		//	if(player)
		//		Target = player.gameObject;
		//	backViewCS.Target = Target;
		//}
	}
	Vector3 positionTargetUp;

	bool viewBack = false;
	/// <summary>
	/// 后视开启信号量
	/// </summary>
	public bool ViewBack { get { return viewBack; } 
		set 
		{
			if (ViewAim)
				viewBack = false;
			else
				viewBack = value;
		} 
	}

	bool viewAim = false;
	//bool lateViewAim = false;
	/// <summary>
	/// 瞄准开启信号量
	/// </summary>
	public bool ViewAim { get { return viewAim; }
        set
        {
			if (ViewBack)
				viewAim = false;
			else
				viewAim = value;
        }
	}

	/// <summary>
	/// 切换视角
	/// </summary>
	public bool SwitchCamera { set; get; }

	void FixedUpdate ()
	{
		if (!Target||!Target.activeSelf)
			return;
		
		// rotation , moving along the player	
		//Quaternion lookAtRotation = Quaternion.LookRotation (Target.transform.position);
		this.transform.LookAt (Target.transform.position + Target.transform.forward * Offset.x);
		positionTargetUp = Vector3.Lerp(positionTargetUp,(-Target.transform.forward + (Target.transform.up * Offset.y)),Time.fixedDeltaTime * TurnSpeedMult);
		Vector3 positionTarget = Target.transform.position + (positionTargetUp * Offset.z);
		float distance = Vector3.Distance (positionTarget, this.transform.position);
		this.transform.position = Vector3.Lerp (this.transform.position, positionTarget, Time.fixedDeltaTime * (distance  * FollowSpeedMult));

		//后视视角位置同步
		if (Target)
		{
			Vector3 offset = Target.transform.position - transform.position;
			backViewCS.transform.position = Target.transform.position + offset;
		}
	}

	void Update()
	{
		//检查机舱内几个副摄像机是否可用，若无可用摄像机切回主摄像机
		//bool activecheck = false;
		//for (int i =0; i<Cameras.Count; i++) {
		//	if (Cameras [i] && Cameras [i].GetComponent<Camera>().enabled) {
		//		activecheck = true;
		//		break;	
		//	}
		//}
		//if (!activecheck) {	
		//	this.GetComponent<Camera>().enabled = true;
		//	if (this.gameObject.GetComponent<AudioListener> ())
		//		this.gameObject.GetComponent<AudioListener> ().enabled = true;
		//}

		//检查是否处于后视模式
		if (ViewBack&& indexCamera<Cameras.Count)
			Cameras[indexCamera].BackChange.Started();
		else if(!ViewAim&& indexCamera < Cameras.Count)
			Cameras[indexCamera].BackChange.Canceled();

		//检查是否处于瞄准模式
		//ViewChangeAim viewChange = Cameras[indexCamera].AimChange;
		//if (Target)
		//{
		//	WeaponController controller = Target.GetComponent<WeaponController>();
		//	ViewChangeAim changeAim = CurrCamera.GetAimWeaponChange(controller.CurrLauncher);
		//	if (changeAim != null)
		//	{
		//		viewChange = changeAim;
		//		lateWeaponChange = viewChange;
		//	}
		//	//if (controller.WeaponLists.Count > 0 && WeaponAimChanges.ContainsKey(controller.WeaponLists[controller.CurrentWeapon]))
		//	//    if (WeaponAimChanges[controller.WeaponLists[controller.CurrentWeapon]].ContainsKey(Cameras[indexCamera].ViewType))
		//	//    {
		//	//        viewChange = WeaponAimChanges[controller.WeaponLists[controller.CurrentWeapon]][Cameras[indexCamera].ViewType];
		//	//        lateWeaponChange = viewChange;
		//	//    }
		//}
        //if (lateWeaponChange != null && viewChange != lateWeaponChange)
        //{
        //    lateWeaponChange.Close();
        //    lateWeaponChange = null;
        //}
        if (viewAim && indexCamera < Cameras.Count)
			Cameras[indexCamera].AimChange.Started();
		//else if (viewBack&&lateWeaponChange!=null)
		//	lateWeaponChange.Close();
		else if(!viewBack && indexCamera < Cameras.Count)
			Cameras[indexCamera].AimChange.Canceled();
		//viewChangeAimLateUpdate = viewChange;


		//切换相机
		if (SwitchCamera)
		{
			SwitchCameras();
			SwitchCamera = false;
		}

	}

    private void LateUpdate()
    {

		//viewChangeAimLateUpdate.Update();
	}
}
