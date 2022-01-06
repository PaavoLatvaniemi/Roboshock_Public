using MLAPI.Messaging;
using System.Linq;
using UnityEngine;
public class SniperRayCastWeapon : RayCastWeapon
{
    CameraController cam;
    protected override void OnEnable()
    {
        base.OnEnable();
        cam = transform.root.GetComponentInChildren<CameraController>();
    }

    private void OnDisable()
    {
        if (cam != null)
        {
            if (cam.ZoomedIn)
            {
                //Aseenvaihdon yhteydessä zoom ei poistu jos se on togglattu..
                cam.ToggleZoom();
            }
        }

    }
    //Ensimmäinen piste kun saavutaan ampumiseen
    public override void TryShootWithInputMethod()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (Reloading == false && currentAmmo != maxMagazineAmmo)
            {
                ReloadWeapon();
            }
        }
        if (cam.ZoomedIn && CurrentAmmo >= 1)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                TryShootAltFire();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                TryShootMainFire();
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            ToggleZoomOnorOffForSniper();
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            ToggleZoomOnorOffForSniper();
        }

    }

    private void ToggleZoomOnorOffForSniper()
    {
        cam.ToggleZoom();
    }

    protected override void SpecialFireWeapon()
    {

        PlayerEnergy.ChangeEnergy(-specialFireCost);
        CurrentAmmo -= neededAmmoForSpecialFire;
        shotNumber++;
        setFiringSpecialServerRpc(true);
        CreateWeaponClassFX(screenFiringPosition, playerCamera.transform.TransformDirection(Vector3.forward + recoilVector), true);
        CreateWeaponClassDamageServerRpc(Owner.NetworkObjectId,
                                         Owner.ClientSideInterpolationWatcher.getWrappedPlayersAndInterpolations(),
                                         Owner.GetNetworkTimeWithInputDelay(),
                                         screenFiringPosition,
                                         playerCamera.transform.TransformDirection(Vector3.forward + recoilVector),
                                         true);

        OnSpecialFire();
        setFiringSpecialServerRpc(false);

    }

    [ServerRpc] // Damage Raycast tehdään palvelimella.
    protected override void CreateWeaponClassDamageServerRpc(ulong firingPlayerNetworkId, InterpolationStateInfo[] intStates, int frameId, Vector3 screenFiringPosition, Vector3 firingPositionDirection, bool firingSpecial = false)
    {
        ServerFrameSimulator.Singleton.Simulate(frameId, intStates, () =>
         {

             RaycastHit[] hits = (Physics.RaycastAll(screenFiringPosition, firingPositionDirection, 1000, ~onShootIgnoreLayers));
             RaycastHit[] orderedHits = hits.OrderBy(hit => hit.distance).ToArray();
             if (orderedHits.Length > 0)
             {
                 //Käydään läpi raycastit, jos raycast ei osu mihinkään, ei tarvi tehdä mitään.
                 //Lasketaan osumista, että kuinka moni niista on potentiaalisesti pelaajan oma collider tai subcollider luissa.
                 int selfHits = 0;
                 Debug.DrawLine(screenFiringPosition, orderedHits[0].point, Color.blue, 5f);
                 //Iteroidaan osumat, tarkistetaan että onko osuma mihinkä
                 for (int i = 0; i < orderedHits.Length; i++)
                 {
                     //Tallennetaan tieto osumasta, tarvitaan tieto, että onko osuma pelaajahahmo, joten tallennetaan sen komponentti
                     ConnectPlayer theoreticalPlayerComponent = orderedHits[i].transform.root.GetComponent<ConnectPlayer>();
                     IDamageable damageable = orderedHits[i].transform.root.GetComponent<IDamageable>();
                     //Osuttiin muuhun kuin pelaajaan, break! ei tarvitse mitata enää raycastia.
                     if (theoreticalPlayerComponent == null)
                     {
                         TryPenetrativeShot(firingPlayerNetworkId, screenFiringPosition, firingPositionDirection, firingSpecial, orderedHits, i);
                         if (damageable != null)
                         {
                             DoDamageIfHit(firingPlayerNetworkId, orderedHits[i]);
                             AddForceToHit(orderedHits[i]);
                         }
                         break;
                     }
                     //Pelaaja tässä tilanteessa on kulkenut teoreettisesti tosi kovaa vauhtia (mm. warp tai korkea hyppy)
                     //Ignorataan tulos jos sama id kun ampuja, ja jatketaaan eteenpäin.
                     if (theoreticalPlayerComponent.NetworkObjectId == firingPlayerNetworkId)
                     {
                         selfHits++;
                         continue;
                     }
                     //Iteraatio voi olla pisteessä esim iteraatio 3, (i = 2), pelaajaan on osuttu kahdesti, joten selfhits = 2
                     //seuraavaa osumaa mitataan, että onko se toinen pelaaja tai maailma. jos osuma on edelleen
                     //ampuva pelaaja, siirrytään seuraavaan iteraation kunnes löytyy joko maastoa tai toinen pelaaja, ja sitten lopetetaan for loop
                     if (i <= selfHits)
                     {
                         DoDamageIfHit(firingPlayerNetworkId, orderedHits[i]);
                         TryPenetrativeShot(firingPlayerNetworkId, screenFiringPosition, firingPositionDirection, firingSpecial, orderedHits, i);
                         break;
                     }
                 }

             }
         });
        addRecoilClientRpc(ownerParams);

    }
    [ClientRpc]
    private void addRecoilClientRpc(ClientRpcParams owner)
    {
        addRecoil();
        WeaponVerticalRecoil += verticalRecoilIncreaseAmount;
    }
    private void TryPenetrativeShot(ulong firingPlayerNetworkId, Vector3 screenFiringPosition, Vector3 firingPositionDirection, bool firingSpecial, RaycastHit[] hits, int i)
    {
        if (firingSpecial)
        {
            RaycastHit nextHit;
            Ray ray = new Ray(screenFiringPosition, firingPositionDirection);
            //Otetaan saadusta raysta vastakkainen piste hemmetin kaukana ja käännetään sen suunta takaisin. Sitten saadaan vastakkainen seinä raylle helposti
            Ray reverseRay = new Ray(ray.GetPoint(10000), -ray.direction);
            if (hits[i].collider.Raycast(reverseRay, out nextHit, 100000))
            {
                CreateDecalsServerRpc(nextHit.point, nextHit.normal);
                if (Physics.Raycast(nextHit.point, firingPositionDirection, out nextHit, 1000, ~onShootHitLayers))
                {
                    DoDamageIfHit(firingPlayerNetworkId, nextHit);
                }
            }

        }
    }

    protected override void CreateWeaponClassFX(Vector3 screenFiringPosition, Vector3 firingPositionDirection, bool firingSpecial = false)
    {


        Vector3 recoilVector = new Vector3(horizontalRecoil, WeaponVerticalRecoil, 0);

        RaycastHit hit; // paikallista Raycastia käytetään efekteihin.
        RaycastHit newHit;

        if (Physics.Raycast(screenFiringPosition, firingPositionDirection, out hit, 1000, ~onShootIgnoreLayers))
        {
            if (firingSpecial)
            {
                Ray ray = new Ray(screenFiringPosition, firingPositionDirection);
                //Otetaan saadusta raysta vastakkainen piste hemmetin kaukana ja käännetään sen suunta takaisin.
                Ray reverseRay = new Ray(ray.GetPoint(10000), -ray.direction);
                if (hit.collider.Raycast(reverseRay, out newHit, 100000))
                {
                    CreateDecalsServerRpc(newHit.point, newHit.normal);
                    RaycastHit penetrationHit;
                    if (Physics.Raycast(newHit.point, firingPositionDirection, out penetrationHit, 1000, ~onShootIgnoreLayers))
                    {
                        CreateHitFX(penetrationHit);
                    }
                }

            }

            CreateHitFX(hit);
        }
        else
        {
            CreateWeaponFXLocal(barrelOrigin.TransformDirection(Vector3.forward) * 1000);
        }
    }

    private void CreateHitFX(RaycastHit hit)
    {
        if (!hit.collider.CompareTag("Player"))
        {
            CreateDecalsServerRpc(hit.point, hit.normal);
        }

        // Local metodi synnyttää efektit paikallisesti.
        // ServerRpc metodi synnyttää samat efektit muille clienteille.
        CreateWeaponFXLocal(hit.point);
        CreateWeaponFXServerRpc(hit.point);
    }
}
