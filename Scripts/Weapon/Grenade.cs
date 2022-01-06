using MLAPI;
using UnityEngine;

public class Grenade : NetworkBehaviour
{

    [SerializeField] float explosionRadius = 3.75f;
    [SerializeField] float explosionPowerMultiplier = 2f;
    [SerializeField] float grenadeDamage = 15f;
    [SerializeField] LayerMask playerLayer;
    ulong PlayerID;


    public void Initialize(ulong playerID)
    {
        PlayerID = playerID;
    }

    private void OnTriggerEnter(Collider other)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, playerLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            IDamageable damageable = hits[i].transform.gameObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.ChangeHealth(-grenadeDamage, PlayerID);
                CharacterController charC = hits[i].transform.root.GetComponent<CharacterController>();
                //Knockback efekti
                //Luodaan ensiksi vektori, jossa alkupää on kranaatin törmäyskohta (transform pos tällä framella) ja loppupää on tässä tilanteessa
                //Hahmon root transform.

                Vector3 distanceVector = charC.bounds.center - transform.position;
                //Sitten luodaan normalisoidun datan perusteella lineaarisesti laskeva knockback efekti räjähdyksen keskeltä
                //(x-min)/(max-min) esim. matka = 3, räjähdysetäisyys 5m => (3m-0m)/(5m-0m)
                float power = 1 - (distanceVector.magnitude / (explosionRadius));
                //Sitten vielä päätetään että etäisyysvektori normalisoidaan ykkösen pituseksi, jonka jälkeen voima voidaan lisätä vektoriin.
                distanceVector = (distanceVector.normalized * power) * explosionPowerMultiplier;
                hits[i].transform.root.GetComponent<PlayerController>().addForceToCharacterController(distanceVector);
            }
        }
        Destroy(gameObject);
    }


}
