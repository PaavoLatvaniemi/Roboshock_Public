using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradientImage : MonoBehaviour
{
    public UnityEngine.UI.RawImage img;

    public Color32 color1;

    public Color32 color2;
    private Texture2D backgroundTexture;

    void OnEnable()
    {

        backgroundTexture = new Texture2D(2, 1);
        backgroundTexture.wrapMode = TextureWrapMode.Clamp;
        backgroundTexture.filterMode = FilterMode.Bilinear;
        SetColor(color1, color2);
        backgroundTexture.Apply();
    }

    public void SetColor(Color32 color1, Color32 color2)
    {
        backgroundTexture.SetPixels32(new Color32[] { color1, color2 });
        backgroundTexture.Apply();
        img.texture = backgroundTexture;
    }
}
