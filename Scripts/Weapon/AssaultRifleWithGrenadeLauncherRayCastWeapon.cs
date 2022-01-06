
using MLAPI;
using MLAPI.Messaging;
using UnityEngine;


public class AssaultRifleWithGrenadeLauncherRayCastWeapon : RaycastAutomaticWeapon
{
    //Pelaaja näkee hyvin oudosti kulkevan kranaatin jos sitä ei jaa visuaaliseen smoothiin kranaattin
    //ja "servukranaattiin".
    [SerializeField] GameObject grenadePrefabNetwork;
    [SerializeField] GameObject grenadePrefabVisual;
    [SerializeField] float grenadeFlingForceInNewtons = 500f;
    public override void TryShootWithInputMethod()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (Reloading == false && currentAmmo != maxMagazineAmmo)
            {
                ReloadWeapon();
            }
        }
        if (Input.GetKey(KeyCode.Mouse0))
        {
            TryShootMainFire();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            TryShootAltFire();
        }
    }
    protected override void SpecialFireWeapon()
    {
        PlayerEnergy.ChangeEnergy(-specialFireCost);
        CurrentAmmo -= neededAmmoForSpecialFire;
        Vector3 firingDirection = playerCamera.transform.TransformDirection(Vector3.forward);
        Vector3 lagCompensatedFiringPosition = barrelOrigin.position + barrelOrigin.TransformDirection(Vector3.forward * 0.50f);
        //Tarvitaan paikallinen FX, Servun tekemä muiden paikallinen FX, sekä Server Damage kranaatti

        CreateServerGrenadeServerRpc(ConnectPlayer.localPlayerNetworkObject.NetworkObjectId, firingDirection, lagCompensatedFiringPosition);
        CreateGrenadeFXLocal(firingDirection, lagCompensatedFiringPosition, barrelOrigin.transform.rotation);
        CreateGrenadeFXServerRpc(firingDirection, lagCompensatedFiringPosition, barrelOrigin.transform.rotation);

        OnSpecialFire();
    }
    [ServerRpc]
    private void CreateServerGrenadeServerRpc(ulong firingPlayerNetworkId, Vector3 firingDirection, Vector3 firingPosition)
    {
        GameObject go = Instantiate(grenadePrefabNetwork, firingPosition, Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn();

        Rigidbody grenadeRB = go.GetComponent<Rigidbody>();
        go.GetComponent<Grenade>().Initialize(firingPlayerNetworkId);
        grenadeRB.AddForce(firingDirection * grenadeFlingForceInNewtons);
    }
    [ServerRpc]
    private void CreateGrenadeFXServerRpc(Vector3 firingDirection, Vector3 firingPosition, Quaternion rotation)
    {
        CreateGrenadeFXClientRpc(firingDirection, firingPosition, rotation);
    }
    [ClientRpc]
    void CreateGrenadeFXClientRpc(Vector3 firingDirection, Vector3 firingPosition, Quaternion rotation)
    {
        if (IsOwner) return;
        CreateGrenadeFXLocal(firingDirection, firingPosition, rotation);
    }
    void CreateGrenadeFXLocal(Vector3 firingDirection, Vector3 firingPosition, Quaternion rotation)
    {
        GameObject go = Instantiate(grenadePrefabVisual, firingPosition, rotation);
        Rigidbody grenadeRB = go.GetComponent<Rigidbody>();

        grenadeRB.AddForce(firingDirection * grenadeFlingForceInNewtons);
    }

}
