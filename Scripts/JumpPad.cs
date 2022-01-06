using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] float jumpPadForce = 5f;

    internal void TriggerJump(Collider other)
    {
        //Transformdirection jotta voidaan sallia vaikka sivuttaiset "jump" padit
        other.GetComponent<PlayerController>().addForceToCharacterController(transform.TransformDirection(Vector3.up) * jumpPadForce);
        other.GetComponent<PlayerAudioController>().PlayAudio(PlayerAudioType.Walljump);
    }
}
