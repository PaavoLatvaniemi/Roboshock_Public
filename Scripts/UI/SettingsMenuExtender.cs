using Assets.Scripts.PlayerScript;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenuExtender : UIElementExtender<BaseMenu>
{

    [SerializeField] Slider redSlider;
    [SerializeField] Slider greenSlider;
    [SerializeField] Slider blueSlider;
    [SerializeField] Slider alphaSlider;
    [SerializeField] Slider vertSlider;
    [SerializeField] Slider horizSlider;
    [SerializeField] Toggle horizToggler;
    [SerializeField] Toggle vertToggler;
    [SerializeField] Toggle middleDotToggler;
    [SerializeField] DynamicCrosshair dynamicCrosshair;
    public delegate void CrossHairSettingsChange();
    public static event CrossHairSettingsChange onChange;
    public override void Awake()
    {
        base.Awake();
        elementOfExtension.InitializeSettingsMenu();
        redSlider.value = PlayerGlobals.CROSSHAIR_SETTINGS.RedCrossHair;
        greenSlider.value = PlayerGlobals.CROSSHAIR_SETTINGS.GreenCrossHair;
        blueSlider.value = PlayerGlobals.CROSSHAIR_SETTINGS.BlueCrossHair;
        alphaSlider.value = PlayerGlobals.CROSSHAIR_SETTINGS.AlphaCrosshair;
        vertSlider.value = PlayerGlobals.CROSSHAIR_SETTINGS.CrossYSize;
        horizSlider.value = PlayerGlobals.CROSSHAIR_SETTINGS.CrossXSize;

    }
    public void ChangeMouseSensitivity()
    {
        if (elementOfExtension != null)
        {
            elementOfExtension.ChangeMouseSensitivity();
        }

    }

    public void SaveSettingsConfig()
    {
        elementOfExtension.SaveSettingsConfig();
        PlayerGlobals.SaveConfig();
        onChange?.Invoke();
    }
    public void goBackAMenu()
    {
        elementOfExtension.goBackAMenu();
    }
    public void ChangeColorSlider(int colorChangeType)
    {
        int value = 0;
        switch ((ColorChangeType)colorChangeType)
        {
            case ColorChangeType.Red:
                value = (int)redSlider.value;
                break;
            case ColorChangeType.Green:
                value = (int)greenSlider.value;
                break;
            case ColorChangeType.Blue:
                value = (int)blueSlider.value;
                break;
            case ColorChangeType.Alpha:
                value = (int)alphaSlider.value;
                break;

        }
        dynamicCrosshair.ChangeColor((ColorChangeType)colorChangeType, value);
    }
    public void ChangeSizeSlider(int changeableCrossHairAxis)
    {
        float value = ((ChangeableCrossHairAxis)changeableCrossHairAxis == ChangeableCrossHairAxis.Horizontal) ? vertSlider.value : horizSlider.value;
        dynamicCrosshair.ChangeLength((ChangeableCrossHairAxis)changeableCrossHairAxis, value);
    }
}
