using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioDirector : MonoBehaviour
{
    [SerializeField] AudioSource[] sources;
    public void PlaySound(int index)
    {
        sources[index].Play();
    }
}
