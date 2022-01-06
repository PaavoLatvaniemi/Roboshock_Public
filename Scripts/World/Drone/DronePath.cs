using UnityEngine;

public class DronePath : MonoBehaviour
{
    public bool isFree = true;

    int droneCountOnPath;

    private void OnEnable()
    {
        GameModeManager.onGameModeStartServer += RequestSpawn;
    }
    private void OnDisable()
    {
        GameModeManager.onGameModeStartServer -= RequestSpawn;
    }

    public void AddDroneToPath() 
    {
        droneCountOnPath++;
        isFree = false;
    }

    public void RemoveDroneFromPath()
    {
        droneCountOnPath--;
        if (droneCountOnPath <= 0)
        {
            isFree = true;
            RequestSpawn();
        }
    }

    void RequestSpawn() => DroneManager.Singleton.RequestSpawnFromPathComponent(this);
}
