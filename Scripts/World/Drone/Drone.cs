using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

public class Drone : NetworkBehaviour, IDamageable
{
    [SerializeField]
    NetworkVariableFloat health = new NetworkVariableFloat(10f);
    [SerializeField] GameObject explosionPrefab;
    [SerializeField] GameObject explosionFXPrefab;
    [SerializeField] AudioSource warningAudioSource;
    [SerializeField] AudioSource engineAudioSource;

    Rigidbody rb;
    DroneMoveAlongPath droneMoveAlongPath;
    bool isAlive = true;
    ulong shooterNetworkId;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        droneMoveAlongPath = GetComponent<DroneMoveAlongPath>();
        engineAudioSource.Play();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isAlive || !IsServer) return;

        ContactPoint contact = collision.contacts[0];
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 position = contact.point;

        GameObject explosionGO = Instantiate(explosionPrefab, position, rotation);
        explosionGO.GetComponent<DroneExplosion>().Initialize(shooterNetworkId);
        explosionGO.GetComponent<NetworkObject>().Spawn();

        GameObject explosionFXGO = Instantiate(explosionFXPrefab, position, rotation);
        explosionFXGO.GetComponent<NetworkObject>().Spawn();

        Destroy(gameObject);
    }

    public void ChangeHealth(float amount, ulong healthChangerNetworkId, int killWeapon = 4)
    {
        if(!IsServer || !isAlive) return;

        health.Value += amount;

        if (health.Value <= 0)
        {
            shooterNetworkId = healthChangerNetworkId;
            HandleDeath();
        }
    }

    public void ChangeHealth(float amount, bool isToSet = false)
    {
        
    } 

    void HandleDeath()
    {
        droneMoveAlongPath.StopPath();
        rb.isKinematic = false;
        isAlive = false;
        PlayWarningSoundClientRpc();
    }

    [ClientRpc]
    void PlayWarningSoundClientRpc()
    {
        engineAudioSource.Stop();
        warningAudioSource.Play();
    }
}
