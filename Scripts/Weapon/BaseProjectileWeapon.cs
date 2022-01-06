using UnityEngine;
//Turha? Tai uusiokäyttöön noobituubiin
public class BaseProjectileWeapon : BaseWeapon
{
    //Kentät
    [SerializeField]
    //Prefabi, joka on projektiiliaseiden luomien projektiili-instanssien malli.
    GameObject projectilePrefab;
    public override void FireWeapon()
    {
        base.FireWeapon();
        GameObject projectileObject = Instantiate(projectilePrefab, barrelOrigin.position, barrelOrigin.transform.rotation);
    }
}