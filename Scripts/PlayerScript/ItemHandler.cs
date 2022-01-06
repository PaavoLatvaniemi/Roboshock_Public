using UnityEngine;


public class ItemHandler : MonoBehaviour
{
    WeaponController weaponController;
    private void OnEnable()
    {
        weaponController = GetComponent<WeaponController>();
    }
    public void HandlePickUpWeapon(WeaponType weaponable)
    {
        //Tsekataan että onko pelaajalla asetta vielä tällä fieldillä
        IWeaponable weaponFromPlayer = weaponController.getWeaponFromPlayer(weaponable);

        if (weaponFromPlayer != null)
        {
            weaponController.addAmmo(weaponFromPlayer.getMaxDefaultSurplusAndCurrentMagazineAmmo(), weaponable);
        }
        else
        {
            weaponController.GiveWeaponServerRpc(weaponable);
        }

    }
}
