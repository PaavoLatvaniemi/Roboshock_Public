using UnityEngine;
//PostFX layerin eventtejen seuraaja
public class PostFXEventAnimator : MonoBehaviour
{
    Animator anim;
    private void Start()
    {
        anim = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        BasePickUp.onPickUp += animatePickupFX;
        PlayerHpController.onClientDeath += animateDeathFX;
        PlayerRespawn.onPlayerSpawnFinish += animateSpawnFX;
    }

    private void animateSpawnFX()
    {
        anim.SetTrigger("Respawn");
    }

    private void animateDeathFX()
    {
        anim.SetTrigger("Death");
    }

    private void OnDisable()
    {
        BasePickUp.onPickUp -= animatePickupFX;
        PlayerHpController.onClientDeath -= animateDeathFX;
        PlayerRespawn.onPlayerSpawnFinish -= animateSpawnFX;
    }

    private void animatePickupFX()
    {
        anim.SetTrigger("Pickup");
    }
}
