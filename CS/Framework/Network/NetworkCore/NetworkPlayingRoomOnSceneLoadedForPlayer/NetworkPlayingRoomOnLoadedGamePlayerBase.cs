using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class NetworkPlayingRoomOnLoadedGamePlayerBase : MonoBehaviour
{
    public virtual void OnLoadedGamePlayer(GameObject gamePalyer,NetworkPlayingRoomPlayer roomPlayer){}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
