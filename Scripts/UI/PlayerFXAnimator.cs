using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class PlayerFXAnimator : MonoBehaviour
{
    WarpAbility warpAbility;
    [SerializeField] VisualEffect warpEffectVfx;

    float warpPower;
    float reverseEasedWarp;
    private void OnEnable()
    {
        warpAbility = GetComponent<WarpAbility>();
        warpAbility.onWarp += playWarpVfx;
    }
    private void OnDisable()
    {
        warpAbility.onWarp -= playWarpVfx;
    }
    private void playWarpVfx()
    {
        StartCoroutine(warpVfxThread());
    }

    private IEnumerator warpVfxThread()
    {
        warpEffectVfx.Play();
        float timer = 0;
        warpPower = warpAbility.WarpTime;
        while (timer < 1)
        {

            //Warpin voima on eksponentiaalinen nousu, mutta halutaan että efekti on alkuun vahva, loppuun heikko
            reverseEasedWarp = 1 - Mathf.Sin(timer * Mathf.PI * 0.5f);
            warpEffectVfx.SetFloat("WarpAmount", reverseEasedWarp);
            timer += Time.fixedDeltaTime;
            yield return null;
        }
        warpEffectVfx.SetFloat("WarpAmount", 0);
        warpEffectVfx.Stop();

    }


    private void Start()
    {

        warpEffectVfx.Stop();
    }

}
