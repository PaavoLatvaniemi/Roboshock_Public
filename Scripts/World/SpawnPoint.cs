using MLAPI;
using UnityEngine;

public class SpawnPoint : NetworkBehaviour
{
    [HideInInspector] public float cooldownTime;
    public bool canSpawn { get; private set; } = true;

    float cooldownTimer;

    private void Update()
    {
        if (!IsServer) return;

        if (!canSpawn)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                EnableSpawnPoint();
            }
        }
    }

    public void EnableSpawnPoint()
    {
        if (!IsServer) return;

        cooldownTimer = 0;
        canSpawn = true;
    }

    public void DisableSpawnPoint()
    {
        if (!IsServer) return;

        cooldownTimer = cooldownTime;
        canSpawn = false;
    }

}
