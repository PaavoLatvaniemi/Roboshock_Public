using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class GrenadeImpactVfxAnimator : MonoBehaviour
{
    [SerializeField] GameObject _vfxParent;
    private void OnTriggerEnter(Collider other)
    {
        GameObject go = Instantiate(_vfxParent, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

}
