using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlightViewBack : MonoBehaviour
{
	public GameObject Target;// player ( Your Plane)
	public float FollowSpeedMult = 0.5f; // camera following speed 
	public float TurnSpeedMult = 5; // camera turning speed 
	public Vector3 Offset = new Vector3(-30, -0.85f, -30);// position offset between plan and camera
	public FlightView.CameraInfo CameraInfo;

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	Vector3 positionTargetUp;
	void FixedUpdate()
	{
		if (!Target)
			return;

		// rotation , moving along the player	
		//Quaternion lookAtRotation = Quaternion.LookRotation (Target.transform.position);
		this.transform.LookAt(Target.transform.position + Target.transform.forward * Offset.x);
        positionTargetUp = Vector3.Lerp(positionTargetUp, (-Target.transform.forward + (Target.transform.up * Offset.y)), Time.fixedDeltaTime * TurnSpeedMult);
        Vector3 positionTarget = Target.transform.position + (positionTargetUp * Offset.z);
        float distance = Vector3.Distance(positionTarget, this.transform.position);
        this.transform.position = Vector3.Lerp(this.transform.position, positionTarget, Time.fixedDeltaTime * (distance * FollowSpeedMult));
    }
}
