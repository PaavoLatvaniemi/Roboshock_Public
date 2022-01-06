using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class DestroyTimer : NetworkBehaviour
{
    [SerializeField] float destroyTime;

    private void OnEnable()
    {
        if (!IsServer) return;
        Destroy(gameObject, destroyTime);
    }
}
