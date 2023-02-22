using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class NetworkPlayingRoomStolsGroup : MonoBehaviour
{
    private Dictionary<string, HashSet<NetworkPlayingRoomStolsPlayer>> StolsGroup;   //<阵营标签,队伍房间玩家成员>
    private Dictionary<string, string[]> GroupEnemyTags;    //阵营的敌对标签关系<队伍标签,队伍敌对阵营标签>

    public class StolsGroupUpdateEvent : UnityEvent<Dictionary<string, HashSet<NetworkPlayingRoomStolsPlayer>>> { }

    [Serializable]
    public struct TeamGroup
    {
        public string TeamTag;
        public string[] EnemyTags;
    }

    public List<TeamGroup> TeamTagGroups = new List<TeamGroup>();

    public StolsGroupUpdateEvent OnStolsGroupUpdate =new StolsGroupUpdateEvent();

    public bool GroupBalance = true;


    private void Awake()
    {
        InitTagRelation();
        GetComponent<NetworkPlayingRoomManager>().OnRoomStateChanged.AddListener(delegate{ OnStolsGroupUpdate.Invoke(StolsGroup); });
    }

    private void InitTagRelation()
    {
        StolsGroup = new Dictionary<string, HashSet<NetworkPlayingRoomStolsPlayer>>();
        GroupEnemyTags = new Dictionary<string, string[]>();
        GameManager.Manager.TargetTags.Clear();
        foreach (TeamGroup team in TeamTagGroups)
        {
            StolsGroup.Add(team.TeamTag, new HashSet<NetworkPlayingRoomStolsPlayer>());
            GroupEnemyTags.Add(team.TeamTag, team.EnemyTags);
            GameManager.Manager.TargetTags[team.TeamTag] = team.EnemyTags;
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// 添加房间玩家到阵营分组容器中
    /// </summary>
    /// <param name="roomPlayer"></param>
    public void AddStols(NetworkPlayingRoomStolsPlayer roomPlayer)
    {
        //默认加入到人少的一边
        string minGroupTag = null;
        foreach (var groupTag in StolsGroup.Keys)
        {
            if(StolsGroup[groupTag].Count==0)
            {
                minGroupTag = groupTag;
                break;
            }    
            if (minGroupTag == null || StolsGroup[groupTag].Count < StolsGroup[minGroupTag].Count)
                minGroupTag = groupTag;
        }
        StolsGroup[minGroupTag].Add(roomPlayer);
        if (NetworkServer.active)
            roomPlayer.SCallbackChangeTagTo(minGroupTag);
        //UpdateTeamGroups();
        OnStolsGroupUpdate?.Invoke(StolsGroup);
    }

    /// <summary>
    /// 阵营分组容器中删除房间玩家
    /// </summary>
    /// <param name="roomPlayer"></param>
    public void RemoveStols(NetworkPlayingRoomStolsPlayer roomPlayer)
    {
        foreach (var team in StolsGroup.Values)
        {
            if(team.Contains(roomPlayer))
            {
                team.Remove(roomPlayer);
                break;
            }
        }
        OnStolsGroupUpdate?.Invoke(StolsGroup);
    }

    /// <summary>
    /// 将房间玩家转换到指定的阵营
    /// </summary>
    /// <param name="roomPlayer"></param>
    /// <param name="TeamTag"></param>
    public void TurnToTeam(NetworkPlayingRoomStolsPlayer roomPlayer,string TeamTag)
    {
        if(StolsGroup.ContainsKey(TeamTag))
        {
            if (StolsGroup[TeamTag].Contains(roomPlayer))
                return;
            foreach (var team in StolsGroup.Values)
            {
                if(team.Contains(roomPlayer))
                {
                    team.Remove(roomPlayer);
                    break;
                }
            }
            StolsGroup[TeamTag].Add(roomPlayer);
            OnStolsGroupUpdate?.Invoke(StolsGroup);
        }
    }

    public void DoStolsGroupUpdate()
    {
        OnStolsGroupUpdate.Invoke(StolsGroup);
    }
}
