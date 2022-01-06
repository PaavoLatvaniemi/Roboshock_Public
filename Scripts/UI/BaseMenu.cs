using Assets.Scripts.Config;
using Assets.Scripts.PlayerScript;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class BaseMenu : MonoBehaviour
    {
        [SerializeField]
        protected List<MenuPage> menuPages = new List<MenuPage>();
        protected MenuPage activeMenu;
        [SerializeField]
        protected Slider sensSlider;
        [SerializeField]
        TextMeshProUGUI sensSliderText;
        public void OpenMenuPageIndex(int index)
        {
            activeMenu.CloseThisMenu();
            menuPages[index].OpenMenu(activeMenu, true);
            activeMenu = menuPages[index];

        }
        public void goBackAMenu()
        {
            activeMenu = activeMenu.GoBackToPreviousMenu();
        }
        protected virtual void OnEnable()
        {
            activeMenu = menuPages[0];

        }
        public void InitializeSettingsMenu()
        {
            sensSlider.value = PlayerGlobals.MouseSensitivity;
        }
        public void ChangeMouseSensitivity()
        {

            sensSliderText.text = sensSlider.value.ToString("0.00");
        }

        public void SaveSettingsConfig()
        {
            PlayerGlobals.MouseSensitivity = sensSlider.value;
            ConfigManager.saveConfigKeyValue("MouseSensitivity", PlayerGlobals.MouseSensitivity.ToString());
        }
    }
}

