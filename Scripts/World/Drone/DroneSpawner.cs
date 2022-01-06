using UnityEngine;
using MLAPI;
using PathCreation;

public class DroneSpawner : NetworkBehaviour
{
    [SerializeField] GameObject dronePrefab;

    public void SpawnDrone(PathCreator dronePath)
    {
        if (!IsServer) return;
        //Spawnaa aina keskelle m‰ppi‰, turha sit‰ on sin‰ns‰ instasiirt‰‰ pathille, joten spawnataan se jonnekin muualle
        GameObject go = Instantiate(dronePrefab, Vector3.up*-1000, Quaternion.identity);
        go.GetComponent<DroneMoveAlongPath>().StartPath(dronePath);
        go.GetComponent<NetworkObject>().Spawn();
    }
}
