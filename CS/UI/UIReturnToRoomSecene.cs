using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIReturnToRoomSecene : MonoBehaviour
{
    public NetworkPlayingRoomManager networkPlayingRoomManager;

    private void Awake()
    {
        if (!networkPlayingRoomManager)
            networkPlayingRoomManager = GameObject.FindObjectOfType<NetworkPlayingRoomManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnToRoomPage()
    {
        foreach (var roomPlayer in networkPlayingRoomManager.roomSlots)
        {
            if(roomPlayer.isLocalPlayer)
            {
                roomPlayer.CmdClientReturnToRoom();
                break;
            }
        } 
    }
}
