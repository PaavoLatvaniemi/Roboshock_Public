using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadTextController : MonoBehaviour
{
    Animator anim;
    WeaponController WeaponController;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        WeaponController = transform.root.GetComponent<WeaponController>();
    }
    private void OnEnable()
    {
        WeaponController.onReload += animateText;
        WeaponController.onReloadEnd += animateTextOut;
    }
    private void OnDisable()
    {
        WeaponController.onReload -= animateText;
        WeaponController.onReloadEnd -= animateTextOut;
    }

    private void animateText()
    {
        anim.SetTrigger("ReloadIn");
    }
    private void animateTextOut()
    {
        anim.SetTrigger("ReloadOut");
    }
}
