using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Events;

public class TeamDeathMatchPanelUpdateMessage: ModelPanelUpdateBaseMessage
{
    public TeamDeathMatchPanelUpdateMessage()
    {
        MessageName = "TeamDeathMatchPanelUpdateMessage";
    }

    public int TotallTicket;
    public int Team1Ticket;
    public int Team2Ticket;
    //public PlayerScoresInfo[] ModelPlaeyrsInfo;
    public string ModelPlaeyrsInfoJosn;

    public PlayerScoresInfo[] ModelPlaeyrsInfo 
    {
        set
        {
            ModelPlaeyrsInfoJosn = GetModelPlaeyrsInfoJosn(value);
        }
        get
        {
            if (ModelPlaeyrsInfoJosn == null)
                return null;
            return GetModelPlaeyrsInfo(ModelPlaeyrsInfoJosn);
        }
    }
    public static string GetModelPlaeyrsInfoJosn(PlayerScoresInfo[] ModelPlaeyrsInfo)
    {
        //将排行数组信息序列化从xml再以字符串形式保存在ModelPlaeyrsInfo中再转换成Json
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(PlayerScoresInfo[]));
        MemoryStream ms = new MemoryStream();
        xmlSerializer.Serialize(ms, ModelPlaeyrsInfo);
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    public static PlayerScoresInfo[] GetModelPlaeyrsInfo(string ModelPlaeyrsInfoJosn)
    {
        //将Json信息转成ModelPlaeyrsInfoJosn还原成数组
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(PlayerScoresInfo[]));
        MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(ModelPlaeyrsInfoJosn));
        return xmlSerializer.Deserialize(ms) as PlayerScoresInfo[];
    }
}


public class GameModelTeamDeathMatch : GameModelCtrl
{
    public struct RespawnPosRota
    {
        public Vector3 pos;
        public Quaternion quaternion;
        public GameObject ocuppyObj;    //该位置的占用生成物体信息
    }

    //<阵营tag,可用出生点表<出生点信息 >>>
    public Dictionary<string, List<RespawnPosRota>> RelocationInfo = new Dictionary<string, List<RespawnPosRota>>(2);
    private Dictionary<GameObject, int> hashIdx = new Dictionary<GameObject, int>();    //辅助记录已经出生物体在其阵营tag下的索引记录

    public int TotallTicket = 50;
    public int Team1Ticket = 0;
    public int Team2Ticket = 0;

    public override bool SetGamePlaying 
    {
        set 
        {
            if (value)
            {
                ResetTicket();
            }
        } 
    }

    UnityEvent<string> onGameFinished = new UnityEvent<string>();
    public override UnityEvent<string> OnGameFinished { get => onGameFinished; }

    string modelPanelPrefabPath = "UI/GameModelDeathMatchPanel";
    /// <summary>
    /// 无前后缀预制体路径
    /// </summary>
    public override string GameModelPanelPrefabPath => modelPanelPrefabPath;

    UnityEvent<string> onModelPanelPrefabPathChanged = new UnityEvent<string>();
    public override UnityEvent<string> GameModelPanelPrefabPathChanged => onModelPanelPrefabPathChanged;

    public override string ModelName => "团队死亡竞赛";

    public Dictionary<NetworkPlayingRoomGameModelPlayer, PlayerScoresInfo> ScoresTab = new Dictionary<NetworkPlayingRoomGameModelPlayer, PlayerScoresInfo>();

    GameObject prefabAISpawnerTeam1;
    GameObject AISpawnerTeam1;
    GameObject prefabAISpawnerTeam2;
    GameObject AISpawnerTeam2;
    Quaternion quaternionTeam1 = new Quaternion();
    Quaternion quaternionTeam2 = new Quaternion();
    public GameModelTeamDeathMatch()
    {
        //生成出生点位置信息
        RelocationInfo.Add("Team1", new List<RespawnPosRota>(50));
        RelocationInfo.Add("Team2", new List<RespawnPosRota>(50));
        quaternionTeam1.eulerAngles = new Vector3(0, 0, 0);
        quaternionTeam2.eulerAngles = new Vector3(0, 180, 0);
        for (int i = 0; i < 50; i++)
        {
            if (i == 0)
            {
                RelocationInfo["Team1"].Add(new RespawnPosRota { pos = new Vector3(0, 1500, -4000), quaternion = quaternionTeam1, ocuppyObj = null });
                RelocationInfo["Team2"].Add(new RespawnPosRota { pos = new Vector3(0, 1500, 4000), quaternion = quaternionTeam2, ocuppyObj = null });
                continue;
            }
            RelocationInfo["Team1"].Add(new RespawnPosRota { pos = new Vector3(i * 10, 1500,  - 4000 - i * 10), quaternion = quaternionTeam1, ocuppyObj = null });
            RelocationInfo["Team1"].Add(new RespawnPosRota { pos = new Vector3(-i * 10, 1500,  - 4000 - i * 10), quaternion = quaternionTeam1, ocuppyObj = null });
            RelocationInfo["Team2"].Add(new RespawnPosRota { pos = new Vector3(i * 10, 1500, 4000 + i * 10), quaternion = quaternionTeam2, ocuppyObj = null });
            RelocationInfo["Team2"].Add(new RespawnPosRota { pos = new Vector3(-i * 10, 1500, 4000 + i * 10), quaternion = quaternionTeam2, ocuppyObj = null });
        }

        //生成票数信息
        ResetTicket();

        //获取模式界面预制体
        //modelPanelPrefab = Resources.Load<GameObject>();

        //生成AI产生器预制体
        prefabAISpawnerTeam1 = Resources.Load<GameObject>("NetworkPrefabs/AISpawner/AISpawnerTeam1");
        prefabAISpawnerTeam2 = Resources.Load<GameObject>("NetworkPrefabs/AISpawner/AISpawnerTeam2");
        //注册死亡事件
        EventsManager<GameObject, Queue<DamagePackage>>.AddListener(GameModelCtrl.DeadEvet, OnJetDead);
        //注册游戏房间地图切换完成事件
        void roomSceneChangedCallback(string newScene)
        {
            NetworkPlayingRoomManager room = NetworkPlayingRoomManager.singleton as NetworkPlayingRoomManager;
            if (newScene == room.GameplayScene)
                CreateSpawner();
        }
        EventsManager<string>.AddListener(GameModelCtrl.GameRoomSceneChanged, roomSceneChangedCallback);
    }

    private void CreateSpawner()
    {
        AISpawnerTeam1 = GameObject.Instantiate(prefabAISpawnerTeam1, new Vector3(-300, 1500, 0), quaternionTeam1);
        AISpawnerTeam2 = GameObject.Instantiate(prefabAISpawnerTeam2, new Vector3(300, 1500, 0), quaternionTeam2);
    }


    ~GameModelTeamDeathMatch()
    {
        if (AISpawnerTeam1)
            GameObject.Destroy(AISpawnerTeam1);
        if (AISpawnerTeam2)
            GameObject.Destroy(AISpawnerTeam2);
        EventsManager<GameObject,Queue<DamagePackage>>.RemoveListener(GameModelCtrl.DeadEvet, OnJetDead);
    }

    private void ResetTicket()
    {
        Team1Ticket = TotallTicket;
        Team2Ticket = TotallTicket;
    }


    /// <summary>
    /// 出生点设置
    /// </summary>
    /// <param name="modelPlayer"></param>
    public override void GamePlayerRelocation(NetworkPlayingRoomGameModelPlayer modelPlayer)
    {
        //NetworkPlayingRoomStolsPlayer groupPlayer = modelPlayer.GetComponent<NetworkPlayingRoomStolsPlayer>();
        NetworkPlayingRoomStolsPlayer playerGroup = modelPlayer.GetComponent<NetworkPlayingRoomStolsPlayer>();
        if (RelocationInfo.ContainsKey(playerGroup.TeamTag))
        {
            for (int i = 0; i < RelocationInfo[playerGroup.TeamTag].Count; i++)
            {
                if(!RelocationInfo[playerGroup.TeamTag][i].ocuppyObj)
                {
                    //设置新位置并在位置容器中添加占位
                    NetworkPlayingRoomPlayer roomPlayer = modelPlayer.GetComponent<NetworkPlayingRoomPlayer>();
                    if (hashIdx.ContainsKey(roomPlayer.GamePlayer))
                    {
                        int lastIdx = hashIdx[roomPlayer.GamePlayer];
                        if(RelocationInfo["Team1"][lastIdx].ocuppyObj==roomPlayer.GamePlayer)
                            RelocationInfo["Team1"][lastIdx] = new RespawnPosRota { pos = RelocationInfo["Team1"][lastIdx].pos, quaternion = RelocationInfo["Team1"][lastIdx].quaternion, ocuppyObj = null };
                        else
                            RelocationInfo["Team2"][lastIdx] = new RespawnPosRota { pos = RelocationInfo["Team2"][lastIdx].pos, quaternion = RelocationInfo["Team2"][lastIdx].quaternion, ocuppyObj = null };
                    }
                    roomPlayer.GamePlayer.transform.position = RelocationInfo[playerGroup.TeamTag][i].pos;
                    roomPlayer.GamePlayer.transform.rotation = RelocationInfo[playerGroup.TeamTag][i].quaternion;
                    RelocationInfo[playerGroup.TeamTag][i] = new RespawnPosRota { pos = RelocationInfo[playerGroup.TeamTag][i].pos, quaternion = RelocationInfo[playerGroup.TeamTag][i].quaternion, ocuppyObj = roomPlayer.GamePlayer };
                    hashIdx[roomPlayer.GamePlayer] = i;

                    //设置出生点机体出生初始项
                    FlightInit flightInit = roomPlayer.GamePlayer.GetComponent<FlightInit>();
                    flightInit.OpenLandGear = false;
                    flightInit.Speed = 200f;
                    flightInit.Throttle = 0.5f;
                    break;
                }
            }
        }
    }

    public override ModelPanelUpdateBaseMessage GetPanelUpdateMessage(NetworkPlayingRoomGameModelPlayer modelPlayer)
    {
        PlayerScoresInfo[] scoresInfo = new PlayerScoresInfo[ScoresTab.Count];
        ScoresTab.Values.CopyTo(scoresInfo, 0);
        return new TeamDeathMatchPanelUpdateMessage { ModelPlaeyrsInfoJosn =TeamDeathMatchPanelUpdateMessage.GetModelPlaeyrsInfoJosn(scoresInfo), TotallTicket = this.TotallTicket, Team1Ticket = this.Team1Ticket, Team2Ticket = this.Team2Ticket };
    }

    public override void AddModelPlayer(NetworkPlayingRoomGameModelPlayer modelPlayer)
    {
        ScoresTab.Add(modelPlayer, new PlayerScoresInfo { modelPlayerNetId = modelPlayer.netId, Kills = 0, Dead = 0, Score = 0 });
        EventsManager.Invoke(GameModelCtrl.PanelUpdateEvent);
    }

    public override void RemoveModelPlayer(NetworkPlayingRoomGameModelPlayer modelPlayer)
    {
        ScoresTab.Remove(modelPlayer);
    }

    public void PlayerDeadScoresUpdate(NetworkPlayingRoomGameModelPlayer deadPlayer, Queue<DamagePackage> killerCount)
    {
        
        if (ScoresTab.ContainsKey(deadPlayer))
        {
            PlayerScoresInfo scores = ScoresTab[deadPlayer];
            scores.Dead++;
            ScoresTab[deadPlayer] = scores;
        }
        
    }

    private void DeadForTickets(GameObject deadObj)
    {
        switch (deadObj.tag)
        {
            case "Team1":
                Team1Ticket--;
                break;
            case "Team2":
                Team2Ticket--;
                break;
            default:
                break;
        }
        //添加更新模式界面的消息
        if (Team1Ticket == 0)
            onGameFinished.Invoke("Team2");
        else if (Team2Ticket == 0)
            onGameFinished.Invoke("Team1");
    }

    //public T GetPanelUpdateMessage<T>(NetworkPlayingRoomGameModelPlayer modelPlayer) where T : ModelPanelUpdateBaseMessage
    //{
    //    PlayerScoresInfo[] scoresInfo = new PlayerScoresInfo[ScoresTab.Count];
    //    ScoresTab.Values.CopyTo(scoresInfo, 0);
    //    return new TeamDeathMatchPanelUpdateMessage { ModelPlaeyrsInfo = scoresInfo, TotallTicket = this.TotallTicket, Team1Ticket = this.Team1Ticket, Team2Ticket = this.Team2Ticket };
    //}

    private void OnJetDead(GameObject jetObj,Queue<DamagePackage> killerCount)
    {
        DeadForTickets(jetObj);

        JetSync jetSync=null;
        if(jetObj)
            jetSync = jetObj.GetComponent<JetSync>();
        if (jetSync!=null&&jetSync.ownerPlayerObj)
        {
            NetworkPlayingRoomGameModelPlayer modelPlayer = jetSync.ownerPlayerObj.GetComponent<Respawn>().modelPlayer;
            PlayerDeadScoresUpdate(modelPlayer, killerCount);
        }
        KillerAddScoresUpdate(jetObj,killerCount);
        EventsManager.Invoke(GameModelCtrl.PanelUpdateEvent);
    }

    private void KillerAddScoresUpdate(GameObject deadObj,Queue<DamagePackage> killerCount)
    {

        var modelPlayers = NetworkPlayingRoomGameModel.singleton.ModelPlayers;
        int maxHP = deadObj.GetComponent<DamageManager>().HPmax;
        while (killerCount.Count>0)
        {
            DamagePackage dm = killerCount.Dequeue();
            if (dm.Owner == null)
                continue;
            GameObject objOwnerPlayer = null;
            if (dm.Owner.GetComponent<JetSync>())
                objOwnerPlayer= dm.Owner.GetComponent<JetSync>().ownerPlayerObj;
            if (objOwnerPlayer)
            {
                Respawn respawn = objOwnerPlayer.GetComponent<Respawn>();
                if (modelPlayers.ContainsKey(respawn.modelPlayer.gameObject))
                {
                    var scoresInfo = ScoresTab[modelPlayers[respawn.modelPlayer.gameObject]];
                    scoresInfo.Score += (int)(((float)dm.Damage / maxHP) * 100f);
                    if (killerCount.Count == 0)
                        scoresInfo.Kills++;
                    ScoresTab[modelPlayers[respawn.modelPlayer.gameObject]] = scoresInfo;
                }
            }
        }
    }
}
