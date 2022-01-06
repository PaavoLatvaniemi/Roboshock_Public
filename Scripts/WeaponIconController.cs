using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponIconController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI inputText;
    [SerializeField] Image weaponImage;
    public void Initialize(string inputIndex, Sprite weaponSprite)
    {
        weaponImage.sprite = weaponSprite;
        //Aspect ratio voi olla p‰in persett‰. set native size
        weaponImage.SetNativeSize();
        //Numeroinputin numero lis‰t‰‰n t‰ss‰.
        inputText.text = inputIndex;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
