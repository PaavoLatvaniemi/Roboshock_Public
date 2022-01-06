using UnityEngine;

public class WeaponPickup : BasePickUp
{
    [SerializeField]
    [Tooltip("Osoita aseen enum. On sama prefabissa")]
    WeaponType weaponType;

    protected override void doPickUp(Collider other)
    {
        other.GetComponent<ItemHandler>().HandlePickUpWeapon(weaponType);
    }

}
