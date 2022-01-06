using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreLeadController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI myKills;
    [SerializeField] TextMeshProUGUI otherKills;
    bool inTheLead = false;
    PlayerInfo playerInfo;
    private void OnEnable()
    {
        playerInfo = transform.root.GetComponent<PlayerInfo>();
        PlayerInfo.onKill += updateKills;
        PlayerInfo.onKillAdded += updateOtherInfo;
        PlayerManager.onPlayerDisconnection += checkIfNowInLeadDueToDisconnection;
    }


    private void OnDisable()
    {
        PlayerInfo.onKill -= updateKills;
        PlayerInfo.onKillAdded -= updateOtherInfo;
        PlayerManager.onPlayerDisconnection -= checkIfNowInLeadDueToDisconnection;
    }

    private void checkIfNowInLeadDueToDisconnection(ulong objectID, ulong networkID)
    {
        CheckLead();
        if (PlayerManager.Singleton.AllPlayerInfos.Count > 1)
        {
            otherKills.text = PlayerManager.Singleton.getNthPlayerByNetworkObject(1).PlayerKills.ToString();
        }
        else
        {
            otherKills.text = "0";
        }
    }

    private void updateOtherInfo(PlayerInfo KillerPlayerInfo)
    {
        CheckLead();

        if (inTheLead)
        {

            if (PlayerManager.Singleton.AllPlayerInfos.Count > 1)
            {
                if (KillerPlayerInfo == PlayerManager.Singleton.getNthPlayerByNetworkObject(1))
                {
                    otherKills.text = KillerPlayerInfo.PlayerKills.ToString();
                }

            }
 
        }
        else
        {
            if (KillerPlayerInfo == PlayerManager.Singleton.getNthPlayerByNetworkObject(0))
            {
                otherKills.text = KillerPlayerInfo.PlayerKills.ToString();
            }
        }



    }

    private void updateKills(string killName, int killCount)
    {
        myKills.text = killCount.ToString();
        CheckLead();
    }

    private void CheckLead()
    {
        if (playerInfo == PlayerManager.Singleton.getNthPlayerByNetworkObject(0))
        {
            inTheLead = true;
        }
        else
        {
            inTheLead = false;
        }
    }
}
