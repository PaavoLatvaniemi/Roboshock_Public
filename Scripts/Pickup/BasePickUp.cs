using MLAPI;
using MLAPI.Messaging;
using System.Collections;
using UnityEngine;


public abstract class BasePickUp : NetworkBehaviour
{
    [SerializeField]
    protected float respawnTime = 15f;
    [SerializeField]
    protected Transform pickupVisual;
    public delegate void PickUp();
    public static event PickUp onPickUp;
    protected bool isPickupable = true;
    AudioSource pickUpAudio;

    private void Start()
    {
        pickUpAudio = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPickupable)
        {

                if (other.gameObject.CompareTag("Player"))
                {
                if (specificPlayerCanPickup(other.gameObject))
                {
                    onPickUp?.Invoke();
                    DisablePickupServerRpc();
                    doPickUp(other);
                    if (pickUpAudio != null) pickUpAudio.Play();
                }
            }


        }

    }

    [ServerRpc(RequireOwnership = false)]
    void DisablePickupServerRpc()
    {
        DisablePickupClientRpc();
    }

    [ClientRpc]
    void DisablePickupClientRpc()
    {
        StartCoroutine(spawnDelayThread());
        pickupVisual.gameObject.SetActive(false);
        isPickupable = false;
    }

    protected IEnumerator spawnDelayThread()
    {
        yield return new WaitForSeconds(respawnTime);
        isPickupable = true;
        pickupVisual.gameObject.SetActive(true);
    }
    protected virtual bool specificPlayerCanPickup(GameObject picker)
    {
        return checkIfCanPickup(picker);
    }
    public virtual bool checkIfCanPickup(GameObject picker)
    {
        return true;
    }
    protected abstract void doPickUp(Collider other);

}
