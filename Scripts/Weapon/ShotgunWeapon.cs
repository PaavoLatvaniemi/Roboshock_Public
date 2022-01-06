
using MLAPI.Messaging;
using UnityEngine;
public class ShotgunWeapon : RayCastWeapon
{
    [SerializeField]
    int pelletCount = 11;
    [SerializeField] float deviationMax = 0.08f;
    (float, float, float)[] generatedDeviations;

    protected override void CreateWeaponClassFX(Vector3 screenFiringPosition, Vector3 firingPositionDirection, bool firingSpecial = false)
    {
        CreateLocalDeviations(pelletCount);
        CreateDeviationsServerRpc(pelletCount);

        for (int i = 0; i < pelletCount; i++)
        {
            float randX = Random.Range(-deviationMax, deviationMax);
            float randY = Random.Range(-deviationMax, deviationMax);
            Vector3 forwardVectorPelletDeviation = playerCamera.transform.TransformDirection(new Vector3(randX, randY, 0));
            //Visuaalinen ja damagerayn asiat ei mätsää jos niitä ei tallenna etukäteen...
            SaveDeviationLocal(i, forwardVectorPelletDeviation.x, forwardVectorPelletDeviation.y, forwardVectorPelletDeviation.z);
            SaveDeviationServerRpc(i, forwardVectorPelletDeviation.x, forwardVectorPelletDeviation.y, forwardVectorPelletDeviation.z);
            RaycastHit hit;
            if (Physics.Raycast(screenFiringPosition, firingPositionDirection + forwardVectorPelletDeviation, out hit, 1000, ~onShootIgnoreLayers))
            {
                if (!hit.collider.CompareTag("Player"))
                    CreateDecalsServerRpc(hit.point, hit.normal);

                CreateWeaponFXLocal(hit.point);
                CreateWeaponFXServerRpc(hit.point);


            }
            else
            {
                CreateWeaponFXLocal(barrelOrigin.TransformDirection(Vector3.forward + new Vector3(randX, randY, 0)) * 1000);
                CreateWeaponFXServerRpc(barrelOrigin.TransformDirection(Vector3.forward + new Vector3(randX, randY, 0)) * 1000);
            }


        }
    }

    private void SaveDeviationLocal(int i, float randX, float randY, float randZ)
    {
        generatedDeviations[i] = (randX, randY, randZ);
    }

    private void CreateLocalDeviations(int pellets)
    {
        generatedDeviations = new (float, float, float)[pellets];
    }

    [ServerRpc]
    private void SaveDeviationServerRpc(int i, float randX, float randY, float randZ)
    {
        generatedDeviations[i] = (randX, randY, randZ);
        SaveDeviationClientRpc(i, randX, randY, randZ);
    }
    [ClientRpc]
    private void SaveDeviationClientRpc(int i, float randX, float randY, float randZ)
    {
        if (IsOwner) return;
        SaveDeviationLocal(i, randX, randY, randZ);
    }

    [ServerRpc]
    private void CreateDeviationsServerRpc(int pellets)
    {
        generatedDeviations = new (float, float, float)[pellets];
        CreateDeviationsClientRpc(generatedDeviations.Length);
    }
    [ClientRpc]
    private void CreateDeviationsClientRpc(int pelletCount)
    {
        if (IsOwner) return;
        CreateLocalDeviations(pelletCount);
    }

    [ServerRpc]
    protected override void CreateWeaponClassDamageServerRpc(ulong firingPlayerNetworkId, InterpolationStateInfo[] intStates, int frameid, Vector3 screenFiringPosition, Vector3 firingPositionDirection, bool firingSpecial = false)
    {
        ServerFrameSimulator.Singleton.Simulate(frameid, intStates, () =>
        {
            for (int i = 0; i < pelletCount; i++)
            {
                FireRaycasts(firingPlayerNetworkId, screenFiringPosition, (firingPositionDirection + new Vector3(generatedDeviations[i].Item1, generatedDeviations[i].Item2, generatedDeviations[i].Item3)));

            }




        });


    }





    protected override void SpecialFireWeapon()
    {
        //Helppo tapa juksuttaa asetta ampumaan kahdet setit on se että tallennan vaan
        //alkuperäisen pellettimäärän ja vaihdan vanhan memoryadressin tilalle kaksi kertaa sen vanhan arvon
        PlayerEnergy.ChangeEnergy(-specialFireCost);
        CurrentAmmo -= neededAmmoForSpecialFire;
        shotNumber++;
        int standardPelletCount = pelletCount;
        pelletCount = pelletCount * 2;
        CreateWeaponClassFX(screenFiringPosition, playerCamera.transform.TransformDirection(Vector3.forward + recoilVector));
        CreateWeaponClassDamageServerRpc(Owner.NetworkObjectId,
                                         Owner.ClientSideInterpolationWatcher.getWrappedPlayersAndInterpolations(),
                                         Owner.GetNetworkTimeWithInputDelay(),
                                         screenFiringPosition,
                                         playerCamera.transform.TransformDirection(Vector3.forward + recoilVector));
        WeaponVerticalRecoil += verticalRecoilIncreaseAmount;
        OnSpecialFire();
        pelletCount = standardPelletCount;
    }

}