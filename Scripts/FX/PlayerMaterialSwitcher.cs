using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMaterialSwitcher : MonoBehaviour
{
    [SerializeField] Material onTheLeadMaterial;
    [SerializeField] SkinnedMeshRenderer playerScreen;
    PlayerInfo playerInfo;
    Material _defaultMaterial;
    private void Start()
    {
        _defaultMaterial = playerScreen.material;
        playerInfo = GetComponent<PlayerInfo>();
    }
    private void OnEnable()
    {
        PlayerHpController.onPlayerDeath += checkIfLead;
    }
    private void OnDisable()
    {
        PlayerHpController.onPlayerDeath -= checkIfLead;
    }
    private void checkIfLead(PlayerInfo KilledPlayerInfo, PlayerInfo KillerPlayerInfo, int killWeapon)
    {
        if (playerInfo == PlayerManager.Singleton.getNthPlayerByNetworkObject(0))
        {
            playerScreen.material = onTheLeadMaterial;
        }
        else
        {
            playerScreen.material = _defaultMaterial;
        }
    }
}
