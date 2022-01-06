using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumppadTrigger : MonoBehaviour
{
    JumpPad jumpPad;
    private void Start()
    {
        jumpPad = transform.parent.GetComponent<JumpPad>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jumpPad.TriggerJump(other);
        }
    }
}
