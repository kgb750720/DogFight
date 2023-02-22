using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public enum GameModels
{
    TeamDeathMatch, //团队死亡竞技
}

public class ModelPanelUpdateBaseMessage : NetworkMessage
{
    public string MessageName = "";
}

//public class GameModelPanelUpdateEvent:EventNameBase
//{
//    public GameModelPanelUpdateEvent()
//    {
//        EventName = "GameModelPanelUpdateEvent";
//    }
//}


public abstract class GameModelCtrl
{
    public abstract string ModelName { get; }
    public abstract void GamePlayerRelocation(NetworkPlayingRoomGameModelPlayer modelPlayer);

    //public abstract void JetObjDead(NetworkPlayingRoomGameModelPlayer modelPlayer);
    public static string PanelUpdateEvent { get; } = "GameModelPanelUpdateEvent";
    public static string DeadEvet { get; } = "DeadEvet";

    public static string GameRoomSceneChanged { get; } = "GameRoomSceneChanged";

    public static string GameBeginEvent { get; } = "GameBeginEvent";

    //设置是否开始房间游戏
    public abstract bool SetGamePlaying { set;}

    /// <summary>
    /// 游戏完成获胜阵营tag作为参数
    /// </summary>
    public abstract UnityEvent<string> OnGameFinished { get; }

    /// <summary>
    /// 获取游戏模式的UI预制体
    /// </summary>
    public abstract string GameModelPanelPrefabPath { get; }
    public abstract UnityEvent<string> GameModelPanelPrefabPathChanged { get; }

    //public abstract T GetPanelUpdateMessage<T>(NetworkPlayingRoomGameModelPlayer modelPlayer) where T : ModelPanelUpdateBaseMessage;
    public abstract ModelPanelUpdateBaseMessage GetPanelUpdateMessage(NetworkPlayingRoomGameModelPlayer modelPlayer);

    public abstract void AddModelPlayer(NetworkPlayingRoomGameModelPlayer modelPlayer);

    public abstract void RemoveModelPlayer(NetworkPlayingRoomGameModelPlayer modelPlayer);
}