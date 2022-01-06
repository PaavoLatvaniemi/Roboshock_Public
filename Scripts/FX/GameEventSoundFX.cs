using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEventSoundFX : MonoBehaviour
{
    [SerializeField] AudioSource deathEventAudio;
    [SerializeField] AudioSource respawnEventAudio;
    [SerializeField] AudioSource MainMenuMusicAudio;

    private void OnEnable()
    {
        PlayerHpController.onClientDeath += DeathEventSound;
        GameModeManager.onGameModeStart += StopMainMenuMusic;
        GameModeManager.onGameModeEnd += PlayMainMenuMusic;
    }
    private void OnDisable()
    {
        PlayerHpController.onClientDeath -= DeathEventSound;
        GameModeManager.onGameModeStart -= StopMainMenuMusic;
        GameModeManager.onGameModeEnd -= PlayMainMenuMusic;
    }

    private void DeathEventSound()
    {
        deathEventAudio.Play();
        respawnEventAudio.PlayDelayed(2.2f);
    }

    private void PlayMainMenuMusic() => MainMenuMusicAudio.Play();
    private void StopMainMenuMusic()
    {
        StartCoroutine(FadeOutMusic(MainMenuMusicAudio.volume, 0, 2f));
    }

    IEnumerator FadeOutMusic(float startVolume, float endVolume, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float normalizedTime = t / duration;
            MainMenuMusicAudio.volume = Mathf.Lerp(startVolume, endVolume, normalizedTime);

            yield return null;
        }
        MainMenuMusicAudio.Stop();
        MainMenuMusicAudio.volume = startVolume;
    }
}
