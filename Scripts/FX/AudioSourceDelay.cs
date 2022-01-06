using UnityEngine;

public class AudioSourceDelay : MonoBehaviour
{
    [SerializeField] float delayMS;

    void Start()
    {
        Invoke("PlayAudio", delayMS / 1000f);
    }

    void PlayAudio() => GetComponent<AudioSource>().Play();
}   
