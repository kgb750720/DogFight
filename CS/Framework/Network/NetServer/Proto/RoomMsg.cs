using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 获取战绩协议
/// </summary>
public class MsgGetAchieve:MsgBase
{
    public MsgGetAchieve()
    {
        protoName = "MsgGetAchieve";
    }
    public int win = 0;
    public int lost = 0;
}

[System.Serializable]
public class RoomInfo
{
    public int id = 0;
    public int count = 0;
    public int status = 0;   //1为游戏进行状态
    public string RoomInfoJson = "";
}


public class MsgGetRoomList:MsgBase
{
    public MsgGetRoomList()
    {
        protoName = "MsgGetRoomList";
    }
    public RoomInfo[] rooms;
}
public class MsgCreateRoom:MsgBase
{
    public MsgCreateRoom()
    {
        protoName = "MsgCreateRoom";
    }
    public string RoomInfoJson = "";
    public int result = 0;
}

public class MsgEnterRoom:MsgBase
{
    public MsgEnterRoom()
    {
        protoName = "MsgEnterRoom";
    }
    public int id = 0;
    public int result = 0;
}

public class MsgGetRoomInfo:MsgBase
{
    public MsgGetRoomInfo()
    {
        protoName = "MsgGetRoomInfo";
    }
    public PlayerInfo[] players;
    public string RoomInfoJson = "";
}



public class MsgLeaveRoom:MsgBase
{
    public MsgLeaveRoom()
    {
        protoName = "MsgLeaveRoom";
    }
    public int result = 0;
}

public class MsgStartBattle:MsgBase
{
    public MsgStartBattle()
    {
        protoName = "MsgStartBattle";
    }
    public int result = 0;
}

public class MsgUpdateRoomInfoJson : MsgBase
{
    public MsgUpdateRoomInfoJson()
    {
        protoName = "MsgUpdateRoomInfoJson";
    }

    public string RoomInfoJson = "";
}