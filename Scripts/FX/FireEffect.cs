using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class FireEffect : MonoBehaviour
{
    [SerializeField] bool alwaysOn;
    [SerializeField] float timeOn = 2;
    [SerializeField] float timeOff = 2;
    [SerializeField] VisualEffect[] fireEffects;
    [SerializeField] Light[] fireLights;
    [SerializeField] float lightFadeInTime = 1;
    [SerializeField] float lightFadeOutTime = 1;
    [Header("Audio Clips")]
    [SerializeField] AudioClip FireStart;
    [SerializeField] AudioClip FireLoop;
    [SerializeField] AudioClip FireEnd;
    AudioSource audioSource;
    float initLightIntensity;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        foreach (Transform child in transform) child.gameObject.SetActive(true);
        foreach (VisualEffect fx in fireEffects) fx.Stop();
        initLightIntensity = fireLights[0].intensity;
        StartFireLoop();
    }

    public void StartFireLoop()
    {
        EnableFire();
    }

    void EnableFire()
    {
        foreach (VisualEffect fx in fireEffects) fx.Play();
        StartCoroutine(playFireSound());
        if (fireLights.Length > 0)
        {
            foreach (Light light in fireLights)
            {
                light.intensity = 0;
                light.enabled = true;
            }
            StartCoroutine(FadeLight(0, initLightIntensity, lightFadeOutTime));
        }
        if (!alwaysOn)
            Invoke("DisableFire", timeOn);
    }

    void DisableFire()
    {
        foreach (VisualEffect fx in fireEffects) fx.Stop();
        audioSource.Stop();
        audioSource.PlayOneShot(FireEnd);
        if (fireLights.Length > 0)
        {
            StartCoroutine(FadeLight(initLightIntensity, 0, lightFadeOutTime));
        }
        Invoke("EnableFire", timeOff);
    }

    IEnumerator playFireSound()
    {
        audioSource.PlayOneShot(FireStart);
        yield return new WaitForSeconds(FireStart.length);
        audioSource.clip = FireLoop;
        audioSource.Play();
    }

    IEnumerator FadeLight(float startIntensity, float endIntensity, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float normalizedTime = t / duration;
            foreach (Light light in fireLights)
                light.intensity = Mathf.Lerp(startIntensity, endIntensity, normalizedTime);

            yield return null;
        }
        foreach (Light light in fireLights) light.intensity = endIntensity;
    }
}
