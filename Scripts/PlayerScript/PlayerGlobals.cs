using Assets.Scripts.Config;
using System;
using UnityEngine;

namespace Assets.Scripts.PlayerScript
{
    static class PlayerGlobals
    {

        public static bool isNotInMenu = true;
        public static bool canOpenMenus = true;
        public static CrossHairSettings CROSSHAIR_SETTINGS = new CrossHairSettings();
        public static float MouseSensitivity = 1.75f;
        public static string PlayerName;
        [RuntimeInitializeOnLoadMethod(loadType: RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeMethodLoad()
        {
            InitializeGlobals();
        }

        private static void InitializeGlobals()
        {
            MouseSensitivity = float.Parse(ConfigManager.getConfigKeyValue("MouseSensitivity"));
            PlayerName = ConfigManager.getConfigKeyValue("PlayerName");


        }
        public static void SaveConfig()
        {
            ConfigManager.saveConfigKeyValue("MouseSensitivity", MouseSensitivity.ToString());
            ConfigManager.saveConfigKeyValue("PlayerName", PlayerName);
            ConfigManager.saveConfigKeyValue("RedCrossHair", CROSSHAIR_SETTINGS.RedCrossHair.ToString());
            ConfigManager.saveConfigKeyValue("GreenCrossHair", CROSSHAIR_SETTINGS.GreenCrossHair.ToString());
            ConfigManager.saveConfigKeyValue("BlueCrossHair", CROSSHAIR_SETTINGS.BlueCrossHair.ToString());
            ConfigManager.saveConfigKeyValue("AlphaCrossHair", CROSSHAIR_SETTINGS.AlphaCrosshair.ToString());
            ConfigManager.saveConfigKeyValue("CrossXSize", CROSSHAIR_SETTINGS.CrossXSize.ToString());
            ConfigManager.saveConfigKeyValue("CrossYSize", CROSSHAIR_SETTINGS.CrossYSize.ToString());
            ConfigManager.saveConfigKeyValue("CrossVToggle", CROSSHAIR_SETTINGS.CrossVToggle.ToString());
            ConfigManager.saveConfigKeyValue("CrossHToggle", CROSSHAIR_SETTINGS.CrossHToggle.ToString());
            ConfigManager.saveConfigKeyValue("CrossMToggle", CROSSHAIR_SETTINGS.CrossMToggle.ToString());
            ConfigManager.serializeConfigValues();
        }

    }
    class CrossHairSettings
    {
        int redColor;
        int greenColor;
        int blueColor;
        int alphaColor;
        float verticalBarLength;
        bool verticalBarsOn;
        float horizontalBarLength;
        bool horizontalBarsOn;
        bool middleDotOn;
        public CrossHairSettings()
        {
            RedCrossHair = int.Parse(ConfigManager.getConfigKeyValue("RedCrossHair"));
            GreenCrossHair = int.Parse(ConfigManager.getConfigKeyValue("GreenCrossHair"));
            BlueCrossHair = int.Parse(ConfigManager.getConfigKeyValue("BlueCrossHair"));
            AlphaCrosshair = int.Parse(ConfigManager.getConfigKeyValue("AlphaCrossHair"));
            CrossXSize = float.Parse(ConfigManager.getConfigKeyValue("CrossXSize"));
            CrossYSize = float.Parse(ConfigManager.getConfigKeyValue("CrossYSize"));
            CrossVToggle = Convert.ToBoolean((ConfigManager.getConfigKeyValue("CrossVToggle")));
            CrossHToggle = Convert.ToBoolean(ConfigManager.getConfigKeyValue("CrossHToggle"));
            CrossMToggle = Convert.ToBoolean(ConfigManager.getConfigKeyValue("CrossMToggle"));

        }
        public int RedCrossHair { get => redColor; set => redColor = value; }
        public int GreenCrossHair { get => greenColor; set => greenColor = value; }
        public int BlueCrossHair { get => blueColor; set => blueColor = value; }
        public float CrossYSize { get => verticalBarLength; set => verticalBarLength = value; }
        public bool CrossVToggle { get => verticalBarsOn; set => verticalBarsOn = value; }
        public bool CrossHToggle { get => horizontalBarsOn; set => horizontalBarsOn = value; }
        public bool CrossMToggle { get => middleDotOn; set => middleDotOn = value; }
        public int AlphaCrosshair { get => alphaColor; set => alphaColor = value; }
        public float CrossXSize { get => horizontalBarLength; set => horizontalBarLength = value; }
    }
}
