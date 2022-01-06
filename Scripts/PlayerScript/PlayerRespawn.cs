using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : NetworkBehaviour
{
    static PlayerRespawn singleton;
    public static PlayerRespawn Singleton
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindObjectOfType<PlayerRespawn>();
            }
            return singleton;
        }
    }
    [SerializeField] float respawnTime = 5f;
    [SerializeField] float spawnPointCooldownTime = 5f;

    /* Yrityksien määrä löytää vapaa spawn point, 
       jos yrityksien määrä täyttyy pakotetaan spawnaus randomiin spawn pointiin. */
    const int ForceSpawnThreshold = 10;

    List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    public delegate void NewPlayerRespawn(ulong respawningPlayerNetworkId);
    public static event NewPlayerRespawn onPlayerRespawn;
    public delegate void OnPlayerRespawnFinish();
    public static event OnPlayerRespawnFinish onPlayerSpawnFinish;

    private void OnEnable()
    {
        // Haetaan kaikki Spawn pointit (tämän transformin childit)
        spawnPoints.Clear();
        foreach (Transform child in transform)
        {
            SpawnPoint spawnPoint = child.GetComponent<SpawnPoint>();
            spawnPoint.cooldownTime = spawnPointCooldownTime;
            spawnPoints.Add(spawnPoint);
        }

        PlayerHpController.onPlayerDeath += StartRespawnAfterDeath;
    }

    private void OnDisable()
    {
        spawnPoints.Clear();
        PlayerHpController.onPlayerDeath -= StartRespawnAfterDeath;
    }

    void StartRespawnAfterDeath(PlayerInfo killedPlayerInfo, PlayerInfo killerPlayerInfo, int killWeapon = 0)
    {
        Transform player = killedPlayerInfo.transform;

        StartCoroutine(RespawnCoroutine(player));
    }

    public void RespawnAllPlayers()
    {
        if (!IsServer) return;

        EnableAllSpawnpoints();

        foreach (ulong id in PlayerManager.Singleton.AllNetworkIds)
            RespawnServerRpc(id);
    }

    public void EnableAllSpawnpoints()
    {
        if (!IsServer) return;

        foreach (SpawnPoint spawn in spawnPoints)
            spawn.EnableSpawnPoint();
    }

    IEnumerator RespawnCoroutine(Transform player)
    {
        NetworkObject playerNetworkObject = player.GetComponent<NetworkObject>();
        ulong respawningPlayerNetworkId = playerNetworkObject.NetworkObjectId;
        WeaponController weaponController = player.GetComponent<WeaponController>();

        DisablePlayerVisibility(respawningPlayerNetworkId);
        weaponController.SetWeaponEnabledState(false);

        yield return new WaitForSeconds(respawnTime);
        //Pelaaja voi disconnectaa tällöin, joten varmistetaan että tämä ei ole tapahtunut
        if (player != null) RespawnServerRpc(respawningPlayerNetworkId);

    }

    [ServerRpc]
    public void RespawnServerRpc(ulong playerNetworkId)
    {
        Transform player = PlayerManager.Singleton.GetPlayerInfoByNetworkObject(playerNetworkId).transform;

        // Valitaan random spawn point. Mikäli ei ole spawn pointteja pelaaja syntyy 0,0,0.
        if (spawnPoints.Count > 0)
        {
            int randSpawn;
            int i = 0;
            while (true)
            {
                randSpawn = Random.Range(0, spawnPoints.Count);
                if (spawnPoints[randSpawn].canSpawn || i >= ForceSpawnThreshold)
                {
                    spawnPoints[randSpawn].DisableSpawnPoint();
                    break;
                }
                i++;
            }

            Vector3 pos = spawnPoints[randSpawn].transform.position;
            Quaternion rot = spawnPoints[randSpawn].transform.rotation;

            SetPlayerLocationClientRpc(playerNetworkId, pos, rot);
        }
        else SetPlayerLocationClientRpc(playerNetworkId, new Vector3(0, 0, 0), player.rotation);

        onPlayerRespawn?.Invoke(playerNetworkId);


    }

    [ClientRpc] // Pelaajan position asetetaan clientillä, koska NetworkTransform on client auth.
    void SetPlayerLocationClientRpc(ulong playerNetworkId, Vector3 pos, Quaternion rot)
    {
        PlayerInfo player = PlayerManager.Singleton.GetPlayerInfoByNetworkObject(playerNetworkId);

        if (player.GetComponent<NetworkObject>().IsOwner)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            cc.enabled = false;
            player.transform.position = pos;
            player.transform.rotation = rot;
            cc.enabled = true;
            player.GetComponent<PlayerController>().ResetCharacterMovement();
            player.GetComponent<InterpolationObjectController>().ResetTransforms();
            onPlayerSpawnFinish?.Invoke();
        }
        EnablePlayerVisibility(playerNetworkId);
    }



    void DisablePlayerVisibility(ulong playerNetworkId)
    {
        Transform player = PlayerManager.Singleton.GetPlayerInfoByNetworkObject(playerNetworkId).transform;

        player.GetComponent<CharacterController>().enabled = false;
        //Vaati että kolmas child on playerCharacter, voi olla joku fieldiki jossain, mutta indeksin nappaus on myös nopeeta
        player.gameObject.transform.GetChild(2).gameObject.SetActive(false);
    }

    void EnablePlayerVisibility(ulong playerNetworkId)
    {
        Transform player = PlayerManager.Singleton.GetPlayerInfoByNetworkObject(playerNetworkId).transform;

        player.GetComponent<CharacterController>().enabled = true;
        player.gameObject.transform.GetChild(2).gameObject.SetActive(true);
    }
}
