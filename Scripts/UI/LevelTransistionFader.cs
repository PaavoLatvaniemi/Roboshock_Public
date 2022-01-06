using Assets.Scripts.Network;
using MLAPI;
using UnityEngine;

public class LevelTransistionFader : MonoBehaviour
{
    [SerializeField] Animator anim;

    private void OnEnable()
    {
        GameModeManager.onGameIsStarting += StartLoad;
    }
    private void OnDisable()
    {
        GameModeManager.onGameIsStarting -= StartLoad;

    }


    void endFade()
    {
        anim.SetTrigger("FadeIn");
    }
    public void StartLoad()
    {
        anim.SetTrigger("FadeOut");
    }
    public void InvokeChange()
    {
        endFade();
        if (NetworkManager.Singleton.IsServer)
        {

            NetworkMatchmaking.Singleton.StartGameModeServerRpc();
        }
    }
}
