using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlidingTiledTexture : MonoBehaviour
{
    Image rend;
    [SerializeField] float scrollSpeedX = 0.5f;
    [SerializeField] float scrollSpeedY = 0.5f;
    [SerializeField] float tiling = 1;
    Vector2 offSet = new Vector2();
    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Image>();
        //Tarvitaan instanssi, ei muuteta kaikkia spritejä
        rend.material = new Material(rend.material);
    }

    // Update is called once per frame
    void Update()
    {
        offSet += new Vector2(scrollSpeedX, scrollSpeedY) * Time.deltaTime;

        rend.material.SetTextureScale("_MainTex", new Vector2(tiling, tiling));
        rend.material.SetTextureOffset("_MainTex", offSet);
    }
}
