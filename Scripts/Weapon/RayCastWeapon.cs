using MLAPI;
using MLAPI.Messaging;
using System.Linq;
using UnityEngine;
public class RayCastWeapon : BaseWeapon
{

    protected override void CreateWeaponClassFX(Vector3 screenFiringPosition, Vector3 firingPositionDirection, bool firingSpecial = false)
    {


        RaycastHit hit; // paikallista Raycastia käytetään efekteihin.
        if (Physics.Raycast(screenFiringPosition, firingPositionDirection, out hit, 1000, ~onShootIgnoreLayers))
        {
            if (!hit.collider.CompareTag("Player"))
                CreateDecalsServerRpc(hit.point, hit.normal);
            // Local metodi synnyttää efektit paikallisesti.
            // ServerRpc metodi synnyttää samat efektit muille clienteille.
            CreateWeaponFXLocal(hit.point);
            CreateWeaponFXServerRpc(hit.point);
        }
        else
        {
            CreateWeaponFXLocal(barrelOrigin.TransformDirection(Vector3.forward) * 1000);
        }
    }

    protected void addRecoil()
    {
        if (shotNumber > 0)
        {
            horizontalRecoil += recoilIncreaseAmount;

        }
    }

    [ServerRpc] // Damage Raycast tehdään palvelimella.
    protected override void CreateWeaponClassDamageServerRpc(ulong firingPlayerNetworkId, InterpolationStateInfo[] playerInts, int frameId, Vector3 screenFiringPosition, Vector3 firingPositionDirection, bool firingSpecial = false)
    {

        ServerFrameSimulator.Singleton.Simulate(frameId, playerInts, () =>
        {
            //Tallennetaan kaikki hitit raycastista, johon kuuluu skene ja pelaajat, ei pickuppeja.
            //Koko tapahtuma taltioidaan serverlillä, ja samaan aikaan voi tapahtua useampikin ampuminen, ei voida siksi
            //vaan alkaa muuttelemaan pelaajien layereita, koska sitten muut potentiaaliset hiti eivät osu.
            FireRaycasts(firingPlayerNetworkId, screenFiringPosition, firingPositionDirection);

        });

    }

    protected void FireRaycasts(ulong firingPlayerNetworkId, Vector3 screenFiringPosition, Vector3 firingPositionDirection)
    {
        RaycastHit[] hits = (Physics.RaycastAll(screenFiringPosition, firingPositionDirection, 1000, ~onShootIgnoreLayers));
        Debug.DrawRay(screenFiringPosition, firingPositionDirection * 1000, Color.blue, 5f);
        RaycastHit hit;

        RaycastHit[] orderedHits = hits.OrderBy(hit => hit.distance).ToArray();


        if (orderedHits.Length > 0)
        {
            //Käydään läpi raycastit, jos raycast ei osu mihinkään, ei tarvi tehdä mitään.
            //Lasketaan osumista, että kuinka moni niista on potentiaalisesti pelaajan oma collider tai subcollider luissa.
            int selfHits = 0;

            //Iteroidaan osumat, tarkistetaan että onko osuma mihinkä
            for (int i = 0; i < orderedHits.Length; i++)
            {
                //Tallennetaan tieto osumasta, tarvitaan tieto, että onko osuma pelaajahahmo, joten tallennetaan sen komponentti
                ConnectPlayer theoreticalPlayerComponent = orderedHits[i].transform.root.GetComponent<ConnectPlayer>();
                IDamageable damageable = orderedHits[i].transform.root.GetComponent<IDamageable>();
                //Osuttiin muuhun kuin pelaajaan, break! ei tarvitse mitata enää raycastia.
                if (theoreticalPlayerComponent == null)
                {
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
                    break;
                }
            }

        }
        addRecoilClientRpc(ownerParams);
    }

    [ClientRpc]
    private void addRecoilClientRpc(ClientRpcParams owner)
    {
        addRecoil();
        WeaponVerticalRecoil += verticalRecoilIncreaseAmount;
    }

    protected void DoDamageIfHit(ulong firingPlayerNetworkId, RaycastHit hit)
    {
        Transform rootOfHit = hit.transform.root;
        IDamageable damageable = rootOfHit.GetComponent<IDamageable>();
        if (damageable != null)
        {
            float todoDamage = (rootOfHit.gameObject.CompareTag("HeadShot") == true) ? -headShotDamage : -baseDamage;
            damageable.ChangeHealth(todoDamage, firingPlayerNetworkId, getWeaponSpriteWithIndex());
            CreateHitPlayerIndicators(rootOfHit, hit.transform.position);
        }

    }

    private void CreateHitPlayerIndicators(Transform rootOfHit, Vector3 hitPoint)
    {
        HitDirectionIndication hitDirectionIndication = rootOfHit.GetComponent<HitDirectionIndication>();
        if (hitDirectionIndication != null)
        {
            ulong hitID = rootOfHit.GetComponent<NetworkObject>().OwnerClientId;
            ClientRpcParams clientRpcParams = new ClientRpcParams()
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { hitID }
                }
            };
            hitDirectionIndication.ShowDamageFromDirectionClientRpc(new Vector3(transform.root.position.x, 0, transform.root.position.z), hitPoint, clientRpcParams);
        }
    }

    protected void AddForceToHit(RaycastHit hit)
    {
        Rigidbody rb = hit.transform.root.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddTorque(-hit.normal * hitForce);
        }

    }

    [ServerRpc]
    protected override void CreateDecalsServerRpc(Vector3 hitPoint, Vector3 hitNormal)
    {
        GameObject decal = Instantiate(bulletImpactEffect, hitPoint + hitNormal * 0.02f, Quaternion.LookRotation(-hitNormal));
        decal.GetComponent<NetworkObject>().Spawn();

        Destroy(decal, 10f);
    }

    protected void CreateWeaponFXLocal(Vector3 position)
    {
        GameObject trail = Instantiate(bulletTrail, barrelOrigin.position, Quaternion.identity);
        LineRenderer lineComponent = trail.GetComponent<LineRenderer>();
        lineComponent.SetPosition(0, barrelOrigin.position);
        lineComponent.SetPosition(1, position);
        if (barrelVisualEffects != null)
        {
            barrelVisualEffects.Play();
            barrelVisualEffectParticleLights.Emit(1);
        }
    }

    [ServerRpc]
    protected override void CreateWeaponFXServerRpc(Vector3 position)
    {
        CreateWeaponFXClientRpc(position);
    }

    [ClientRpc]
    protected override void CreateWeaponFXClientRpc(Vector3 position)
    {
        if (IsOwner) return;

        CreateWeaponFXLocal(position);
    }
}
