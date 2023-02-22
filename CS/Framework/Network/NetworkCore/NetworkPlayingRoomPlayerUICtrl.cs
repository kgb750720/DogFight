using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class NetworkPlayingRoomPlayerUICtrl : MonoBehaviour
{

    public GameObject PrefabRoomUI;

    public UnityEvent OnInstanceUI = new UnityEvent();
    public UnityEvent OnDestroyUI = new UnityEvent();

    private GameObject UIInscetanceObj;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InstanceUI()
    {
        DestroyUI();
        UIInscetanceObj = Instantiate(PrefabRoomUI, transform);
        OnInstanceUI?.Invoke();
    }

    public void DestroyUI()
    {
        if (UIInscetanceObj)
        {
            Destroy(UIInscetanceObj);
            OnDestroyUI?.Invoke();
        }
    }
}
