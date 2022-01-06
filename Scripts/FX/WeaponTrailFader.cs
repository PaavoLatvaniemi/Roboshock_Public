using System.Collections;
using UnityEngine;
//Skripti, jonka avulla aseiden aiheuttamat linerendererit häivytetään pois
public class WeaponTrailFader : MonoBehaviour
{
    LineRenderer trail;
    private void OnEnable()
    {
        trail = GetComponent<LineRenderer>();
        StartCoroutine(FadeTrailEffect());
    }
    private IEnumerator FadeTrailEffect()
    {

        float fadingTrailTimer = 0;

        var colorGradient = trail.colorGradient;

        var alphaKeys = colorGradient.alphaKeys;

        while (fadingTrailTimer < 0.7f)
        {

            fadingTrailTimer += Time.fixedDeltaTime;
            //Alkupää
            alphaKeys[0].alpha -= 0.02f;
            //Loppupää
            alphaKeys[1].alpha -= 0.01f;

            colorGradient.alphaKeys = alphaKeys;

            trail.colorGradient = colorGradient;

            yield return null;
        }
        Destroy(trail.gameObject);

    }
}
