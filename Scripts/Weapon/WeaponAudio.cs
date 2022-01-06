using MLAPI;
using MLAPI.Messaging;
using UnityEngine;

public class WeaponAudio : NetworkBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] bool playOneShotAudio = true;
    [Header("Audio Clips")]
    [SerializeField] AudioClip FireAudioClip;
    [SerializeField] AudioClip SpecialFireAudioClip;
    [SerializeField] AudioClip ReloadAudioClip;
    [SerializeField] AudioClip EquipAudioClip;

    BaseWeapon weapon;

    private void Start()
    {
        weapon = GetComponent<BaseWeapon>();
        weapon.onFire += PlayFireAudio;
        weapon.onSpecialFire += PlaySpecialFireAudio;
        weapon.onReload += PlayReloadAudio;
        weapon.onEquip += PlayEquipAudio;
    }

    private void OnDestroy()
    {
        weapon.onFire -= PlayFireAudio;
        weapon.onSpecialFire -= PlaySpecialFireAudio;
        weapon.onReload -= PlayReloadAudio;
        weapon.onEquip -= PlayEquipAudio;
    }

    public void PlayFireAudio()
    {
        if (FireAudioClip == null) return;

        if (playOneShotAudio)
            audioSource.PlayOneShot(FireAudioClip);
        else
        {
            audioSource.clip = FireAudioClip;
            audioSource.Play();
        }
        PlayFireAudioServerRpc();
    }
    [ServerRpc] void PlayFireAudioServerRpc() => PlayFireAudioClientRpc();
    [ClientRpc]
    void PlayFireAudioClientRpc()
    {
        if (IsOwner) return;

        if (playOneShotAudio)
            audioSource.PlayOneShot(FireAudioClip);
        else
        {
            audioSource.clip = FireAudioClip;
            audioSource.Play();
        }
    }

    public void PlaySpecialFireAudio()
    {
        if (SpecialFireAudioClip == null) return;
        audioSource.PlayOneShot(SpecialFireAudioClip);
        PlaySpecialFireAudioServerRpc();
    }
    [ServerRpc] void PlaySpecialFireAudioServerRpc() => PlaySpecialFireAudioClientRpc();
    [ClientRpc]
    void PlaySpecialFireAudioClientRpc()
    {
        if (IsOwner) return;

        audioSource.PlayOneShot(SpecialFireAudioClip);
    }

    public void PlayReloadAudio()
    {
        if (ReloadAudioClip == null) return;
        audioSource.PlayOneShot(ReloadAudioClip);
        PlayReloadAudioServerRpc();
    }
    [ServerRpc] void PlayReloadAudioServerRpc() => PlayReloadAudioClientRpc();
    [ClientRpc]
    void PlayReloadAudioClientRpc()
    {
        if (IsOwner) return;

        audioSource.PlayOneShot(ReloadAudioClip);
    }

    public void PlayEquipAudio()
    {
        if (EquipAudioClip == null) return;
        audioSource.PlayOneShot(EquipAudioClip);
    }
}
