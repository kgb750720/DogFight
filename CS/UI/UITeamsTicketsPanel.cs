using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UITeamsTicketsPanel : MonoBehaviour
{
    [Serializable]
    public struct TeamTicketInfo
    {
        public string TeamTag;
        public TMP_Text TicketNub;
        public Slider slider;
    }

    [SerializeField]
    private List<TeamTicketInfo> teamTickets = new List<TeamTicketInfo>();

    public Dictionary<string, TeamTicketInfo> teamToTickets = new Dictionary<string, TeamTicketInfo>();
    private void Awake()
    {
        foreach (var item in teamTickets)
        {
            teamToTickets[item.TeamTag] = item;
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

    public void SetTeamTicket(string teamTag,int ticketNub,int maxTicketNub)
    {
        teamToTickets[teamTag].TicketNub.text = ticketNub.ToString();
        teamToTickets[teamTag].slider.value = (float)ticketNub / maxTicketNub;
    }
}
