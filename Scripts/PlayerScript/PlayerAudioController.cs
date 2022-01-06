using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class PlayerAudioController : NetworkBehaviour
{
    [SerializeField] AudioSource wallJumpAudio;
    [SerializeField] AudioSource warpJumpAudio;
    [SerializeField] AudioSource noEnergyAudio;

    public void PlayAudio(PlayerAudioType type)
    {
        PlayAudioClient(type);
        PlayAudioServerRpc(type);
    }

    [ServerRpc]
    void PlayAudioServerRpc(PlayerAudioType type) => PlayAudioClientRpc(type);
    [ClientRpc]
    void PlayAudioClientRpc(PlayerAudioType type)
    {
        if (IsOwner) return;
        PlayAudio(type);
    }

    void PlayAudioClient(PlayerAudioType type)
    {
        switch (type)
        {
            case PlayerAudioType.Footstep:
                break;
            case PlayerAudioType.Jump:
                break;
            case PlayerAudioType.Walljump:
                wallJumpAudio.Play();
                break;
            case PlayerAudioType.Warp:
                warpJumpAudio.Play();
                break;
            case PlayerAudioType.NoEnergy:
                noEnergyAudio.Play();
                break;
            default:
                break;
        }
    }
}

public enum PlayerAudioType { Footstep, Jump, Walljump, Warp, NoEnergy }
