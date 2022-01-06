using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KillFeedBox : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI killerInfo;
    [SerializeField] TextMeshProUGUI killedInfo;
    [SerializeField] Image weaponImage;
    [SerializeField] Color32 friendlyKillColor;
    [SerializeField] Outline outline;
    Animator animator;
    private void OnEnable()
    {
        animator = GetComponent<Animator>();

    }
    public void SetupKillUIElement(string killer, string killed, Sprite weaponSprite = null)
    {
        killerInfo.text = killer;
        killedInfo.text = killed;
        if (weaponSprite != null)
        {
            weaponImage.sprite = weaponSprite;
            //Kuvat on vähän eri kokoisia, joten aspect ratio menee ihan vituiksi. Pakotetaan preserve
            weaponImage.preserveAspect = true;
        }

    }

    internal void SetupKillAsOwn()
    {
        outline.effectColor = friendlyKillColor;
    }
    public void EndKill()
    {
        animator.SetTrigger("Out");
    }
    public void destroyAfterAnimation()
    {
        Destroy(gameObject);
    }
}
