using MLAPI;
using UnityEngine;

public class HitMarker : NetworkBehaviour
{
    Animator anim;
    private void OnEnable()
    {
        anim = GetComponent<Animator>();
        PlayerHpController.onDamageNotify += PlayDamage;
    }
    private void OnDisable()
    {
        PlayerHpController.onDamageNotify -= PlayDamage;
    }


    private void PlayDamage(ulong healthChanger)
    {
        if (healthChanger == ConnectPlayer.localPlayerNetworkObject.NetworkObjectId)
        {
            PlayHitMarker();
        }

    }

    private void PlayHitMarker()
    {
        anim.SetTrigger("Hit");
    }
}
