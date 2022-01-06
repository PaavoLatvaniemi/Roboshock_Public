using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using MLAPI;

public class DroneManager : NetworkBehaviour
{
    static DroneManager _singleton;
    public static DroneManager Singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindObjectOfType<DroneManager>();
            }
            return _singleton;
        }
    }
    public DronePath[] paths;

    [SerializeField] int minDroneCountOnOnePath;
    [SerializeField] int maxDroneCountOnOnePath;
    [SerializeField] float minDroneSpawnInterval;
    [SerializeField] float maxDroneSpawnInterval;

    DroneSpawner droneSpawner;

    void Awake()
    {

        paths = GetComponentsInChildren<DronePath>();
        droneSpawner = FindObjectOfType<DroneSpawner>();
    }

    public DronePath GetFirstFreePath()
    {
        foreach (var path in paths)
        {
            if (path.isFree) 
            {
                return path;
            }
        }
        return null;
    }

    public void RequestSpawnFromPathComponent(DronePath path)
    {
        if (!IsServer) return;
        if (droneSpawner == null)
        {
            Debug.LogWarning("Missing Drone Spawner Component");
            return;
        }
        StartCoroutine(SpawnDrones(path));
    }

    private IEnumerator SpawnDrones(DronePath path)
    {
        int droneCount = Random.Range(minDroneCountOnOnePath, maxDroneCountOnOnePath + 1);
        for (int i = 0; i < droneCount; i++)
        {
            float spawnInterval = Random.Range(minDroneSpawnInterval, maxDroneSpawnInterval);
            yield return new WaitForSeconds(spawnInterval);
            droneSpawner.SpawnDrone(path.GetComponent<PathCreator>());
            path.AddDroneToPath();
        }
    }
}
