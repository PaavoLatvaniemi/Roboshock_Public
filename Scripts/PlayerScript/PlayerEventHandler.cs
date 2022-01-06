using MLAPI;
using MLAPI.Messaging;
using UnityEngine;


public class PlayerEventHandler : NetworkBehaviour
{
    [SerializeField]
    NetworkObject networkObject;
    [SerializeField]
    WeaponController weaponController;
    [SerializeField]
    PlayerController playerController;
    [SerializeField]
    CameraController cameraController;
    [SerializeField]
    PlayerHpController playerHpController;
    [SerializeField]
    PlayerEnergyController playerEnergyController;
    [SerializeField]
    PlayerUIController playerUIController;
    // Use this for initialization
    void Start()
    {
        PlayerRespawn.onPlayerRespawn += checkIfSamePlayer;
        GameModeManager.onGameModeStart += EnablePlayer;
        GameModeManager.onGameModeEnd += DisablePlayer;
        DisablePlayer();
    }
    private void OnDisable()
    {
        PlayerRespawn.onPlayerRespawn -= checkIfSamePlayer;
        GameModeManager.onGameModeStart -= EnablePlayer;
        GameModeManager.onGameModeEnd -= DisablePlayer;
    }

    private void checkIfSamePlayer(ulong respawningPlayerNetworkId)
    {
        if (networkObject.NetworkObjectId == respawningPlayerNetworkId)
        {
            handlePlayerRespawnVariables();
        }
    }

    private void handlePlayerRespawnVariables()
    {
        playerHpController.ResetHealth();
        playerEnergyController.ResetEnergy();

        handlePlayerRespawnVariablesClientRpc();
    }
    [ClientRpc] // Aseet resetoidaan clientillä, koska player "inventory" ei ole automaattisesti synkronoitu.
    private void handlePlayerRespawnVariablesClientRpc()
    {
        weaponController.ResetWeapons();
        weaponController.SetWeaponEnabledState(true);
    }
    private void EnablePlayer()
    {
        playerController.EnableMovement();
        weaponController.SetWeaponEnabledState(true);
        cameraController.enabled = true;
        playerUIController.ShowUI();
    }

    private void DisablePlayer()
    {
        playerController.DisableMovement();
        weaponController.SetWeaponEnabledState(false);
        cameraController.enabled = false;
        playerUIController.HideUI();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
