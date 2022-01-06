using MLAPI.Messaging;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    Canvas canvas;
    Camera camera;
    //Pelaajan komponentit
    WeaponController wController;
    PlayerHpController hpController;
    PlayerEnergyController energyController;
    //Kanvas
    //Tekstikentät

    [SerializeField] TextMeshProUGUI ammoUIText;
    [SerializeField] TextMeshProUGUI healthUIText;
    [SerializeField] TextMeshProUGUI energyUIText;
    [SerializeField] TextMeshProUGUI timerText;
    //AMMO UI
    [SerializeField] RectTransform ammoBelt;
    List<RectTransform> ammoIcons = new List<RectTransform>();
    List<Image> ammoIconImages = new List<Image>();
    [SerializeField] GameObject ammoIconPrefab;
    [SerializeField] Color32 BulletReadyColor;
    [SerializeField] Color32 BulletDepletedColor;
    [SerializeField] Color32 AllBulletsDepletedColor;
    [SerializeField] Image WeaponSpriteImage;


    private void OnEnable()
    {
        camera = transform.root.GetComponentInChildren<Camera>();
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = camera.transform.GetChild(0).GetComponent<Camera>();
        wController = transform.parent.GetComponent<WeaponController>();
        hpController = transform.parent.GetComponent<PlayerHpController>();
        energyController = transform.parent.GetComponent<PlayerEnergyController>();
        wController.onWeaponSwitch += changeWeaponUIClientRpc;
        wController.onWeaponAmmoUpdate += updateAmmoUIClientRpc;
        hpController.onHealthChange += updateHealthUIClientRpc;
        energyController.onEnergyChange += updateEnergyUIClientRpc;
        UIManager.Singleton.RegisterBinding("TimeLeft", UpdateTimer);

    }

    private void UpdateTimer(string stringOutput)
    {
        timerText.text = stringOutput;
    }

    private void OnDisable()
    {
        wController.onWeaponAmmoUpdate -= updateAmmoUIClientRpc;
        wController.onWeaponSwitch -= changeWeaponUIClientRpc;
        hpController.onHealthChange -= updateHealthUIClientRpc;
        energyController.onEnergyChange -= updateEnergyUIClientRpc;
    }
    [ClientRpc]
    void changeWeaponUIClientRpc(IWeaponable weaponable)
    {
        changeAmmoIcons(weaponable);
        changeWeaponIcon(weaponable);
    }

    private void changeWeaponIcon(IWeaponable weaponable)
    {
        WeaponSpriteImage.sprite = weaponable.getWeaponSprite();
        //Väärä ratio, korjataan se nativeen.
        WeaponSpriteImage.SetNativeSize();
    }

    private void changeAmmoIcons(IWeaponable weaponable)
    {
        if (ammoIcons.Count != weaponable.getMaxMagazineAmmo())
        {
            ammoIcons.Clear();
            ammoIconImages.Clear();
            for (int i = 0; i < ammoBelt.childCount; i++)
            {
                Destroy(ammoBelt.GetChild(i).gameObject);
            }
            int ammoCount = weaponable.getMaxMagazineAmmo();
            for (int i = 0; i < ammoCount; i++)
            {
                GameObject go = Instantiate(ammoIconPrefab, ammoBelt);
                ammoIcons.Add(go.GetComponent<RectTransform>());
                ammoIconImages.Add(go.GetComponent<Image>());

            }
            UpdateAmmoToValue(weaponable.getCurrentAmmo());
        }

    }

    private void UpdateAmmoToValue(int amount)
    {
        int depleted = amount - ammoIcons.Count;
        for (int i = 0; i < amount; i++)
        {
            ammoIconImages[i].color = BulletReadyColor;
        }
        if (depleted < 1)
        {
            depleted = Mathf.Abs(depleted);
            for (int x = ammoIcons.Count - depleted; x < ammoIcons.Count; x++)
            {
                ammoIconImages[x].color = BulletDepletedColor;
            }
        }

    }

    [ClientRpc]
    private void updateEnergyUIClientRpc(float amount)
    {
        energyUIText.text = string.Format("{0}", amount);
    }

    [ClientRpc]
    private void updateHealthUIClientRpc(float amount)
    {
        int displayAmount = (int)amount;
        string displayText = displayAmount.ToString();
        if (amount < 0)
        {
            displayText = "DEAD";
        }

        healthUIText.text = string.Format("{0}", displayText);
    }

    public void ShowUI() => transform.GetChild(0).gameObject.SetActive(true);
    public void HideUI() => transform.GetChild(0).gameObject.SetActive(false);


    [ClientRpc]
    private void updateAmmoUIClientRpc(IWeaponable weapon)
    {
        var surplusText = weapon.getSurplusAmmo().ToString();
        if (weapon.hasInfiniteAmmo())
        {
            surplusText = "∞";
        }
        ammoUIText.text = string.Format("{0}", surplusText);
        UpdateAmmoToValue(weapon.getCurrentAmmo());
    }
}
