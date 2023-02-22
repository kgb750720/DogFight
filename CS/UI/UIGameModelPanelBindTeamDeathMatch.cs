using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGameModelPanelBindTeamDeathMatch : UIGameModelPanelBindBase
{
    public UIScoreboardPanel scoreboardPanel;
    public UITeamsTicketsPanel ticketsPanel;
    public override void SetBindModelPlayer(NetworkPlayingRoomGameModelPlayer modelPlayer)
    {
        base.SetBindModelPlayer(modelPlayer);
    }

    public override void SetPanelUpdateMessage(ModelPanelUpdateBaseMessage msg)
    {
        base.SetPanelUpdateMessage(msg);
        TeamDeathMatchPanelUpdateMessage TDMMsg = (TeamDeathMatchPanelUpdateMessage)msg;
        ticketsPanel.SetTeamTicket("Team1", TDMMsg.Team1Ticket, TDMMsg.TotallTicket);
        ticketsPanel.SetTeamTicket("Team2", TDMMsg.Team2Ticket, TDMMsg.TotallTicket);
        foreach (var item in TDMMsg.ModelPlaeyrsInfo)
        {
            if (item.modelPlayerNetId == _modelPlayer.netId)
            {
                scoreboardPanel.UpdatePersonScoreInfo(item);
                break;
            }
        }
    }
}
