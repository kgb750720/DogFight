using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UIPlayerRowContentTowCol : UIPlayerRowContentBase
{
    public enum Team
    {
        Team1,
        Team2
    }
    public struct RoomPlayerTeamItemInfo
    {
        public Team team;
        public Transform playerRowTran;
    }

    public Dictionary<NetworkPlayingRoomStolsPlayer, RoomPlayerTeamItemInfo> PlayerRowLsit = new Dictionary<NetworkPlayingRoomStolsPlayer, RoomPlayerTeamItemInfo>();

    [Serializable]
    public struct TeamRelationNode
    {
        public Team team;
        public string bindTag;
    }

    public TeamRelationNode[] TeamRelation;

    private Dictionary<Team, LinkedList<RectTransform>> TeamRemainPos = new Dictionary<Team, LinkedList<RectTransform>>();
    private Dictionary<string, Team> strTeamToTeam = new Dictionary<string, Team>();


    private void Awake()
    {
        //初始化容器
        TeamRemainPos.Add(Team.Team1, new LinkedList<RectTransform>());
        TeamRemainPos.Add(Team.Team2, new LinkedList<RectTransform>());
        foreach (var item in TeamRelation)
        {
            strTeamToTeam[item.bindTag] = item.team;
        }
    }

    //delegate void ChangedEventFunc(NetworkPlayingRoomPlayer roomPlayer);

    public override void UpdateContent(Dictionary<string, HashSet<NetworkPlayingRoomStolsPlayer>> teamGroup)
    {
        ClearList();
        foreach (var team in teamGroup)
        {
            string teamTag = team.Key;
            foreach (var roomStolsPlayer in team.Value)
            {
                if (roomStolsPlayer.isLocalPlayer)
                    localPlayer = roomStolsPlayer;
                PlayerInfo playerInfo = roomStolsPlayer.GetComponent<PlayerInfo>();
                NetworkPlayingRoomPlayer roomPlayer = roomStolsPlayer.GetComponent<NetworkPlayingRoomPlayer>();
                if(TeamRemainPos[strTeamToTeam[teamTag]].Count>0)
                {
                    RectTransform rectPlayerContent = TeamRemainPos[strTeamToTeam[teamTag]].First.Value;
                    TeamRemainPos[strTeamToTeam[teamTag]].RemoveFirst();
                    UIFriendButtonController playerShowController = rectPlayerContent.GetComponentInChildren<UIFriendButtonController>();
                    InitPlayerRowContent(rectPlayerContent, playerShowController, playerInfo, roomPlayer);
                    RoomPlayerTeamItemInfo node = new RoomPlayerTeamItemInfo();
                    node.playerRowTran = rectPlayerContent.parent;
                    node.team = strTeamToTeam[teamTag];
                    PlayerRowLsit.Add(roomStolsPlayer, node);
                }
                else
                {
                    GameObject playerRow = Instantiate(PrefabPlayerRow, ListContent);
                    RoomPlayerTeamItemInfo node = new RoomPlayerTeamItemInfo();
                    node.playerRowTran = playerRow.transform;
                    node.team = strTeamToTeam[teamTag];
                    PlayerRowLsit.Add(roomStolsPlayer, node);
                    Transform playerContent = node.team == Team.Team1 ? playerRow.transform.Find("Team1Content") : playerRow.transform.Find("Team2Content");
                    Transform anotherContent = node.team == Team.Team1 ? playerRow.transform.Find("Team2Content") : playerRow.transform.Find("Team1Content");
                    playerContent.gameObject.SetActive(true);
                    InitPlayerRowContent((RectTransform)playerContent, playerContent.GetComponentInChildren<UIFriendButtonController>(), playerInfo, roomPlayer);
                    TeamRemainPos[anotherTeam(strTeamToTeam[teamTag])].AddLast((RectTransform)anotherContent);
                }
            }
        }
    }

    private static void InitPlayerRowContent(RectTransform playerContent, UIFriendButtonController playerShowController, PlayerInfo playerInfo, NetworkPlayingRoomPlayer roomPlayer)
    {
        playerContent.gameObject.SetActive(true);
        playerShowController.SetPlaeyrName(playerInfo.PlayerId);
        string StateToString(NetworkPlayingRoomPlayer.RoomReadyState state)
        {
            switch (state)
            {
                case NetworkPlayingRoomPlayer.RoomReadyState.Ready:
                    return "准备";
                    break;
                case NetworkPlayingRoomPlayer.RoomReadyState.NotReady:
                    return "未准备";
                    break;
                case NetworkPlayingRoomPlayer.RoomReadyState.Playing:
                    return "游戏中";
                    break;
                default:
                    return "";
                    break;
            }
        }
        playerShowController.SetPlayerStatus(StateToString(roomPlayer.readyState));
        playerInfo.OnPlayerIdChangedEvent.AddListener(playerShowController.SetPlaeyrName);
        playerShowController.OnInstanceDestroy += delegate
        {
            playerInfo.OnPlayerIdChangedEvent.RemoveListener(playerShowController.SetPlaeyrName);
        };
        void StateChanged(NetworkPlayingRoomPlayer roomPlayer)
        {
            playerShowController.SetPlayerStatus(StateToString(roomPlayer.readyState));
        }
        roomPlayer.OnReadyStateChanged.AddListener(StateChanged);
        playerShowController.OnInstanceDestroy += delegate
        {
            roomPlayer.OnReadyStateChanged.RemoveListener(StateChanged);
        };
    }

    public override void ClearList()
    {
        PlayerRowLsit.Clear();
        TeamRemainPos[Team.Team1].Clear();
        TeamRemainPos[Team.Team2].Clear();
        base.ClearList();
    }

    Team anotherTeam(Team team)
    {
        if (team == Team.Team1)
            return Team.Team2;
        else
            return Team.Team1;
    }


}
