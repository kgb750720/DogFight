/// <summary>
/// Player manager. this script will attached all Necessary components to the Plane automatically
/// </summary>
using UnityEngine;
using System.Collections;
using Flight;

// add all necessary components.
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(Indicator))]
[RequireComponent(typeof(RadarSystem))]

public class PlayerManager : MonoBehaviour {
	[HideInInspector]
	public PlayerController PlayerControl;
	[HideInInspector]
	public Indicator Indicate;

	public bool DoViewBind=true;

	FlightView.CameraInfo addCameraInfo;

	void Awake(){
		Indicate = this.GetComponent<Indicator>();
		PlayerControl = this.GetComponent<PlayerController>();
	}
	
	void Start () {

		if (DoViewBind)
		{
			FlightView view = (FlightView)GameObject.FindObjectOfType(typeof(FlightView));
			// setting cameras 将可切换的摄像机加入视角切换集合里
			if (Indicate.CockpitCamera.Length > 0)
			{
				//将所有摄像机加入集合
				//for(int i=0;i<Indicate.CockpitCamera.Length;i++){
				//	if(Indicate.CockpitCamera[i]!=null){
				//		view.AddCamera(Indicate.CockpitCamera[i].gameObject);
				//	}
				//}

				//只将主座舱主摄像机加入切换集合
				if (Indicate.CockpitCamera.Length > 0 &&
					Indicate.CockpitCamera.Length > Indicate.PrimaryCameraIndex &&
					Indicate.CockpitCamera[Indicate.PrimaryCameraIndex] != null
					)
					addCameraInfo = view.AddCamera(FlightView.CameraInfo.ViewTypeSet.Cockpit, Indicate.CockpitCamera[Indicate.PrimaryCameraIndex].gameObject,
						new ViewChangeAimZoom(Indicate.CockpitCamera[Indicate.PrimaryCameraIndex].gameObject, 30),
						new ViewChangeBackSwitchCam(Indicate.CockpitCamera[Indicate.PrimaryCameraIndex].gameObject, Indicate.CockpitCamera[1].gameObject),
						GetComponent<FlightSystem>()
						);
			}
		}
	}

	void Update () {
	
	}

    private void OnDestroy()
    {
		if (addCameraInfo != null)
		{
			FlightView view = GameObject.FindObjectOfType<FlightView>();
			if(view)
				view.RemoveCamera(addCameraInfo);
		}
    }
}
