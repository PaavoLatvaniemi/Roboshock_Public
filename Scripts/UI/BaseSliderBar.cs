using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class BaseSliderBar : MonoBehaviour
    {
        Coroutine slidingBarThread;
        [SerializeField] Slider slider;

        IEnumerator startBarVisualChange(float newValue)
        {
            float referenceValue = slider.value;
            float timer = 0;
            while (timer < 0.45f)
            {
                slider.value = Mathf.Lerp(referenceValue, newValue, timer / 0.45f);
                timer += Time.fixedDeltaTime;
                yield return null;
            }
        }
        protected void changeBarValue(float newValue)
        {
            if (slidingBarThread != null) StopCoroutine(slidingBarThread);
            slidingBarThread = StartCoroutine(startBarVisualChange(newValue));
        }
    }
}