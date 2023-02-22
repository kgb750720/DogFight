using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBGMSrouceCtrl : MonoBehaviour
{
    public AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        NetworkPlayingRoomManager manager = Transform.FindObjectOfType<NetworkPlayingRoomManager>();
        manager.OnRoomStateChanged.AddListener(delegate {
            if (audioSource)
            {
                if (NetworkPlayingRoomManager.IsSceneActive(manager.RoomScene))
                    audioSource.enabled = true;
                else
                    audioSource.enabled = false;
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
