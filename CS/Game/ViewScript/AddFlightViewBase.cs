using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class AddFlightViewBase : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void AddFlightViewCS(GameObject go)
    {
        if (!go.GetComponent<FlightView>())
            go.AddComponent<FlightView>();
        else
            go.GetComponent<FlightView>().enabled = true;
    }
}
