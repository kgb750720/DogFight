using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WingLineControl : MonoBehaviour
{
    public TrailRenderer trail;

    float lateForwardAngle;
    private void Awake()
    {
        if (!trail)
            trail = GetComponent<TrailRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        lateForwardAngle = transform.rotation.eulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
