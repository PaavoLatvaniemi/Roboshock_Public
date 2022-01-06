using System.Collections;
using UnityEngine;

public class AnimatedRifleAmmoBar : MonoBehaviour
{
    [SerializeField]
    BaseWeapon weapon;
    int barDivisorByMaxAmmoCount;
    //Threadi, jossa animaatio tapahtuu whilessa
    Coroutine barVisualizationThread;
    [SerializeField]
    //Animaation eri vaiheita kuvastava liukuv‰ri
    Gradient barVisualColors;
    [SerializeField]
    MeshRenderer[] emissiveMaterialRenderers;
    [SerializeField] bool animateMaterial = false;
    [SerializeField] Renderer rend;

    private void OnEnable()
    {
        weapon.onAmmoChange += visualizeBar;
        barDivisorByMaxAmmoCount = weapon.getMaxMagazineAmmo();
    }
    private void OnDisable()
    {
        weapon.onAmmoChange -= visualizeBar;
    }

    private void visualizeBar()
    {
        if (barVisualizationThread != null)
        {
            StopCoroutine(barVisualizationThread);
        }
        barVisualizationThread = StartCoroutine(lerpForAmmobarVisualization(weapon.getCurrentAmmo()));
    }

    IEnumerator lerpForAmmobarVisualization(int ammoAmount)
    {
        float referenceLength = 0;
        float amountToBeInsertedToScale = 0;
        float newLength = 0;
        float timer = 0;
        if (!animateMaterial)
        {
            //Hankitaan reference, jotta lerp on todellisesti lineaarinen
            referenceLength = transform.localScale.z;
            //Lasketaan, ett‰ kuinka paljon yksi mitallinen on scalessa
            amountToBeInsertedToScale = 1f / barDivisorByMaxAmmoCount;
        }

        //Lasketaan yksi ammon mitta * haluttu m‰‰r‰ "ammopalkkeja"
        amountToBeInsertedToScale = amountToBeInsertedToScale * ammoAmount;

        while (transform.localScale.z != amountToBeInsertedToScale)
        {
            timer += Time.fixedDeltaTime * 5;
            newLength = Mathf.Lerp(referenceLength, amountToBeInsertedToScale, timer);
            if (animateMaterial)
            {
                rend.material.SetFloat("LiquidLevel", newLength);
            }
            else
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, newLength);
            }


            SetMaterialEmissiveColor(newLength);
            yield return null;
        }


    }

    private void SetMaterialEmissiveColor(float newLength)
    {
        for (int i = 0; i < emissiveMaterialRenderers.Length; i++)
        {
            emissiveMaterialRenderers[i].material.SetColor("_EmissionColor", barVisualColors.Evaluate(newLength) * 20f);
        }

    }
}
