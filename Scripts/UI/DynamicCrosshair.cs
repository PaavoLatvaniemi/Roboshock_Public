using Assets.Scripts.PlayerScript;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DynamicCrosshair : MonoBehaviour
{
    [SerializeField] bool inGame;
    [SerializeField] Transform middleDot;
    [SerializeField] Transform[] verticalBars;
    [SerializeField] Transform[] horizontalBars;
    float noRecoilWidth;
    float noRecoilHeight;
    Camera playerCamera;
    WeaponController playerWeaponController;
    [SerializeField] RectTransform crossHairCenterPivot;
    public List<Transform> CrossHairPieces
    {
        get
        {
            return getCrossHairPieces();
        }

    }
    private void OnValidate()
    {

        ValidateColors();
        ValidateLengths();
    }

    private void ValidateLengths()
    {
        ChangeLength(verticalBars, RectTransform.Axis.Vertical, PlayerGlobals.CROSSHAIR_SETTINGS.CrossYSize);
        ChangeLength(horizontalBars, RectTransform.Axis.Horizontal, PlayerGlobals.CROSSHAIR_SETTINGS.CrossXSize);

    }

    private void ChangeLength(Transform[] bars, RectTransform.Axis axis, float length)
    {
        for (int i = 0; i < bars.Length; i++)
        {
            bars[i].GetComponent<RectTransform>().SetSizeWithCurrentAnchors(axis, length);
        }
    }

    private void ValidateColors()
    {
        List<Image> crossHairParts = new List<Image>();
        List<Transform> pieces = getCrossHairPieces();
        Color32 newColor = new Color32(
            (byte)PlayerGlobals.CROSSHAIR_SETTINGS.RedCrossHair, (byte)PlayerGlobals.CROSSHAIR_SETTINGS.GreenCrossHair,
            (byte)PlayerGlobals.CROSSHAIR_SETTINGS.BlueCrossHair, (byte)PlayerGlobals.CROSSHAIR_SETTINGS.AlphaCrosshair);
        for (int i = 0; i < pieces.Count; i++)
        {
            crossHairParts.Add(pieces[i].GetComponent<Image>());
        }
        for (int i = 0; i < crossHairParts.Count; i++)
        {
            crossHairParts[i].color = newColor;
        }

    }

    private List<Transform> getCrossHairPieces()
    {
        List<Transform> pieces = new List<Transform>();
        if (PlayerGlobals.CROSSHAIR_SETTINGS.CrossVToggle)
        {
            pieces.AddRange(verticalBars);
            for (int i = 0; i < verticalBars.Length; i++)
            {
                verticalBars[i].gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < verticalBars.Length; i++)
            {
                verticalBars[i].gameObject.SetActive(false);
            }
        }
        if (PlayerGlobals.CROSSHAIR_SETTINGS.CrossHToggle)
        {
            pieces.AddRange(horizontalBars);
            for (int i = 0; i < horizontalBars.Length; i++)
            {
                horizontalBars[i].gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < horizontalBars.Length; i++)
            {
                horizontalBars[i].gameObject.SetActive(false);
            }
        }
        if (PlayerGlobals.CROSSHAIR_SETTINGS.CrossMToggle)
        {
            pieces.Add(middleDot);
            middleDot.gameObject.SetActive(true);
        }
        else
        {
            middleDot.gameObject.SetActive(false);
        }
        return pieces;
    }

    private void OnEnable()
    {

        if (inGame)
        {
            playerWeaponController = transform.root.GetComponent<WeaponController>();
            playerCamera = transform.root.GetComponentInChildren<CameraController>().Cam;
            noRecoilHeight = crossHairCenterPivot.sizeDelta.y;
            noRecoilWidth = crossHairCenterPivot.sizeDelta.x;
            SettingsMenuExtender.onChange += UpdateCrossHair;
        }
        UpdateCrossHair();



    }
    private void OnDisable()
    {
        SettingsMenuExtender.onChange -= UpdateCrossHair;
    }

    void Update()
    {


        if (inGame)
        {
            (float, float) recoilValues = playerWeaponController.getWeaponRecoilAsFloats();

            crossHairCenterPivot.sizeDelta = new Vector2(noRecoilWidth + (noRecoilHeight * (recoilValues.Item1 * 100f)), noRecoilHeight + (noRecoilHeight * (recoilValues.Item1 * 100f)));
        }


    }

    public void UpdateCrossHair()
    {
        getCrossHairPieces();
        ValidateColors();
        ValidateLengths();
    }
    public void ChangeLength(ChangeableCrossHairAxis changeableCrossHairAxis, float amount)
    {
        switch (changeableCrossHairAxis)
        {
            case ChangeableCrossHairAxis.Horizontal:
                PlayerGlobals.CROSSHAIR_SETTINGS.CrossXSize = amount;

                break;
            case ChangeableCrossHairAxis.Vertical:
                PlayerGlobals.CROSSHAIR_SETTINGS.CrossYSize = amount;

                break;

        }
        UpdateCrossHair();
    }
    public void ChangeColor(ColorChangeType typeColor, int colorValue)
    {
        switch (typeColor)
        {
            case ColorChangeType.Red:
                PlayerGlobals.CROSSHAIR_SETTINGS.RedCrossHair = colorValue;
                break;
            case ColorChangeType.Green:
                PlayerGlobals.CROSSHAIR_SETTINGS.GreenCrossHair = colorValue;
                break;
            case ColorChangeType.Blue:
                PlayerGlobals.CROSSHAIR_SETTINGS.BlueCrossHair = colorValue;
                break;
            case ColorChangeType.Alpha:
                PlayerGlobals.CROSSHAIR_SETTINGS.AlphaCrosshair = colorValue;
                break;
            default:
                break;
        }
        UpdateCrossHair();
    }

}
[Serializable]
public enum ChangeableCrossHairAxis
{
    Horizontal,
    Vertical
}
[Serializable]
public enum ColorChangeType
{
    Red,
    Green,
    Blue,
    Alpha
}