using System;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    private void OnEnable()
    {
        GameModeManager.onGameModeEnd += ResetCamera;
        GameModeManager.onGameModeStart += EndCamera;
    }

    private void ResetCamera()
    {
        gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        GameModeManager.onGameModeEnd -= ResetCamera;
        GameModeManager.onGameModeStart -= EndCamera;
    }

    private void EndCamera()
    {
        gameObject.SetActive(false);
    }
}
