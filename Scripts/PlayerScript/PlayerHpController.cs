using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using UnityEngine;

public class PlayerHpController : NetworkBehaviour, IDamageable
{
    /* NetworkVariable - 
        * Kun PALVELIN muutta var arvoa,
        * se synkronoituu automaattisesti kaikilla clienteillä.*/
    //COMPO JA FIELD
    [SerializeField]
    NetworkVariableFloat health = new NetworkVariableFloat(100f);
    NetworkVariableBool isAlive = new NetworkVariableBool(true);
    public float Health => health.Value;
    float _maxHP = 100;
    public bool IsAlive => isAlive.Value;

    public float MaxHP { get => _maxHP; set => _maxHP = value; }

    PlayerEnergyController energyController;
    PlayerInfo playerInfo;
    //EVENTS
    public delegate void HealthChange(float amount);
    public event HealthChange onHealthChange;
    public delegate void PlayerDeath(PlayerInfo KilledPlayerInfo, PlayerInfo KillerPlayerInfo, int killWeapon = 5);
    public static event PlayerDeath onPlayerDeath;
    public delegate void ClientDeath();
    public static event ClientDeath onClientDeath;
    public delegate void ClientSpawn();
    public static event ClientSpawn onClientSpawn;
    public delegate void PlayerDeathEvent();
    public event PlayerDeathEvent onPlayerDeathEvent;
    public delegate void DamageNotify(ulong healthChanger);
    public static event DamageNotify onDamageNotify;

    public void ChangeHealth(float amount, ulong healthChangerNetworkId, int killWeapon = 5) // Vain palvelin suorittaa.
    {
        if (!IsServer || !IsAlive) return;
        float amountToChangeBy = amount;
        if (amountToChangeBy < 0)
        {
            amountToChangeBy = compareWithEnergyDamageReductionAndGetNewDamage(amountToChangeBy);
        }
        health.Value += amountToChangeBy;

        SendClientHitClientRpc(healthChangerNetworkId);
        if (health.Value >= 100)
            health.Value = 100;

        if (health.Value <= 0)
        {
            HandleDeath(healthChangerNetworkId, killWeapon);
            isAlive.Value = false;
        }


    }
    [ClientRpc]
    private void SendClientHitClientRpc(ulong healthCh)
    {
        onDamageNotify?.Invoke(healthCh);
    }

    private float compareWithEnergyDamageReductionAndGetNewDamage(float damage)
    {
        return energyController.returnEnergyDamageReductionDamage(damage);
    }

    public void ChangeHealth(float amount, bool isToset = false) // Vain palvelin suorittaa.
    {
        if (!IsServer || !IsAlive) return;

        if (isToset)
        {
            health.Value = amount;
        }
        else
        {
            health.Value += amount;
        }
        health.SetDirty(true);

        if (health.Value >= 100)
            health.Value = 100;

        if (health.Value <= 0)
        {
            isAlive.Value = false;
            HandleDeath(null);
        }

    }
    [ServerRpc]
    public void ChangeHealthServerRpc(float amount)
    {
        ChangeHealth(amount);
    }
    void HandleDeath(ulong? killerNetworkId, int killWeapon = 5)
    {
        if (!IsServer) return;

        playerInfo.AddDeath();

        if (killerNetworkId != null)
        {
            PlayerInfo killerPlayerInfo = PlayerManager.Singleton.GetPlayerInfoByNetworkObject(killerNetworkId);

            //Nullable vs non-nullable, implicit cast vaaditaan.
            ClientRpcParams OwnerRPCParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new ulong[] { (ulong)killerPlayerInfo.OwnerClientId } } };
            killerPlayerInfo.AddKill(playerInfo.PlayerName, OwnerRPCParams);
            HandleDeathClientRpc((ulong)killerNetworkId, killWeapon);
            return;
        }

        HandleDeathClientRpc();
    }

    public void ResetHealth() // Vain palvelin suorittaa.
    {
        if (!IsServer) return;
        isAlive.Value = true;
        ChangeHealth(100, true);

    }




    [ClientRpc]
    private void HandleDeathClientRpc(ulong killerNetworkId, int killWeapon = 5) // Kutsutaan, jos vahingon tekijä oli pelaaja.
    {
        PlayerInfo killedPlayerInfo = GetComponent<PlayerInfo>();
        PlayerInfo killerPlayerInfo = PlayerManager.Singleton.GetPlayerInfoByNetworkObject(killerNetworkId);
        if (killedPlayerInfo.IsLocalPlayer)
        {
            onClientDeath?.Invoke();
        }
        onPlayerDeath?.Invoke(killedPlayerInfo, killerPlayerInfo, killWeapon);
        onPlayerDeathEvent?.Invoke();
    }
    [ClientRpc]
    private void HandleDeathClientRpc()
    {

        PlayerInfo killedPlayerInfo = GetComponent<PlayerInfo>();
        if (killedPlayerInfo.IsLocalPlayer)
        {
            onClientDeath?.Invoke();
        }
        onPlayerDeath?.Invoke(killedPlayerInfo, null);
        onPlayerDeathEvent?.Invoke();
    }
    private void OnEnable()
    {
        playerInfo = GetComponent<PlayerInfo>();
        energyController = GetComponent<PlayerEnergyController>();
        health.OnValueChanged += OnHealthChanged;
    }

    private void OnHealthChanged(float previousValue, float newValue)
    {
        if (IsOwner && IsClient)
        {
            onHealthChange.Invoke(newValue);
        }
    }
}
