using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponIconsOverlayControl : MonoBehaviour
{
    [SerializeField] Transform transformToParentWeaponIconsTo;
    [SerializeField] GameObject weaponIcon;
    int newestWeaponIndex = 1;
    private void OnEnable()
    {
        PlayerHpController.onClientDeath += clearIcons;
        WeaponController.onWeaponAdded += addNewIcon;
    }

    private void clearIcons()
    {
        for (int i = 0; i < transformToParentWeaponIconsTo.childCount; i++)
        {
            Destroy(transformToParentWeaponIconsTo.GetChild(i).gameObject);
        }
        newestWeaponIndex = 1;
    }

    private void OnDisable()
    {
        WeaponController.onWeaponAdded -= addNewIcon;
        PlayerHpController.onClientDeath -= clearIcons;
    }

    private void addNewIcon(IWeaponable weaponable)
    {
        GameObject weapIcon = Instantiate(weaponIcon, transformToParentWeaponIconsTo);
        weapIcon.GetComponent<WeaponIconController>().Initialize(newestWeaponIndex.ToString(), weaponable.getWeaponSprite());
        newestWeaponIndex++;
    }
}
