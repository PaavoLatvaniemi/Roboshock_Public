using UnityEngine;

public class CameraMount : MonoBehaviour
{
    /* Scenessä on vain yksi kamera per client, 
     * joten peliin liittyessä siirrämme kameran aina omalle pelaajalle.*/

    Vector3 initPos;
    Quaternion initRot;

    Transform localPlayerTransform;

    private void Start()
    {
        initPos = transform.position;
        initRot = transform.rotation;

        GameModeManager.onGameModeEnd += ResetCamera;
        GameModeManager.onGameModeStart += MountCamera;
    }

    private void OnDestroy()
    {
        GameModeManager.onGameModeEnd -= ResetCamera;
        GameModeManager.onGameModeStart -= MountCamera;
    }

    public void Mount(Transform mountPoint)
    {
        transform.position = mountPoint.position;
        transform.rotation = mountPoint.rotation;
        transform.parent = mountPoint;
    }

    public void MountCamera()
    {
        if (!ConnectPlayer.localPlayerNetworkObject.IsOwner) return;
        if (localPlayerTransform == null)
        {
            localPlayerTransform = ConnectPlayer.localPlayerNetworkObject
                .GetComponentInChildren<CameraController>().transform;
        }
        Mount(localPlayerTransform);
    }

    public void ResetCamera()
    {
        transform.parent = null;
        transform.position = initPos;
        transform.rotation = initRot;
    }
}
