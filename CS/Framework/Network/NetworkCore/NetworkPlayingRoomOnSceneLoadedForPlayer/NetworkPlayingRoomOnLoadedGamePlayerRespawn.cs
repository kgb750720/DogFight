using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ≈‰∫œPlayingRoomManager π”√
/// </summary>
[SerializeField]
public class NetworkPlayingRoomOnLoadedGamePlayerRespawn : NetworkPlayingRoomOnLoadedGamePlayerBase
{
    public override void OnLoadedGamePlayer(GameObject gamePalyer, NetworkPlayingRoomPlayer roomPlayer)
    {
        base.OnLoadedGamePlayer(gamePalyer, roomPlayer);
        gamePalyer.GetComponent<Respawn>().RoomPlayer = roomPlayer;
    }
}
