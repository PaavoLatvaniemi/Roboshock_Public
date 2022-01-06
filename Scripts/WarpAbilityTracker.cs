using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarpAbilityTracker : MonoBehaviour
{
    WarpAbility warpAbility;
    float maxProgress;
    [SerializeField] Slider cooldownOverlaySlider;
    [SerializeField] TextMeshProUGUI textComponent;

    private void OnEnable()
    {
        warpAbility = transform.root.GetComponent<WarpAbility>();
        maxProgress = warpAbility.WarpCooldown;
        warpAbility.onWarp += enableText;
        warpAbility.onWarpEnd += disableText;
        WarpAbility.onWarpCDProgress += updateProgress;
    }
    private void OnDisable()
    {
        warpAbility.onWarp -= enableText;
        warpAbility.onWarpEnd -= disableText;
        WarpAbility.onWarpCDProgress -= updateProgress;
    }

    private void disableText()
    {
        textComponent.gameObject.SetActive(false);
    }

    private void enableText()
    {
        textComponent.gameObject.SetActive(true);
    }

    private void updateProgress(float progress)
    {

        float newProgress = progress / maxProgress;
        cooldownOverlaySlider.value = newProgress;
        string textFormatted = progress.ToString("0.00");
        textComponent.text = textFormatted;
        if (textFormatted == "0,00")
        {
            disableText();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
