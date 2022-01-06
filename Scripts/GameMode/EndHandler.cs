using Assets.Scripts.PlayerScript;
using MLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndHandler : MonoBehaviour
{
    [SerializeField] GameObject endCanvas;
    [SerializeField] TextMeshProUGUI winnerText;
    private void OnEnable()
    {
        DeathmatchGM.onGameEnd += DoEndHandle;
    }
    private void OnDisable()
    {
        DeathmatchGM.onGameEnd -= DoEndHandle;
    }
    public void ReactivateMenuOpening()
    {
        PlayerGlobals.canOpenMenus = true;
    }
    private void DoEndHandle(ulong winner)
    {
        string playerName = PlayerManager.Singleton.getNthPlayerByNetworkObject(0).PlayerName;
        endCanvas.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


        PlayerGlobals.isNotInMenu = false;
        PlayerGlobals.canOpenMenus = false;
        winnerText.text = playerName + " is the winner!";
    }
}
