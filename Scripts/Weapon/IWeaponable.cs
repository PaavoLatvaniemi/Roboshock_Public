using UnityEngine;

public interface IWeaponable
{
    void addAmmo(int amount);
    void FireWeapon();
    void ResetWeapon();
    WeaponType getWeaponType();
    GameObject getGameObject();
    int getCurrentAmmo();
    int getMaxMagazineAmmo();
    int getSurplusAmmo();
    int getMaxDefaultSurplusAndCurrentMagazineAmmo();
    bool hasInfiniteAmmo();
    Sprite getWeaponSprite();
    int getWeaponSpriteWithIndex();

}