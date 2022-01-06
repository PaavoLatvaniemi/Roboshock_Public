using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullFlasherIndicator : MonoBehaviour
{
    Animator anim;
    private void OnEnable()
    {
        PlayerInfo.onKill += showSkull;
    }

    private void showSkull(string killName, int killCount)
    {
        anim.SetTrigger("Kill");
    }

    private void OnDisable()
    {
        PlayerInfo.onKill -= showSkull;
    }
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

}
