using Assets.Scripts.PlayerScript;
using Assets.Scripts.UI;
using MLAPI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : BaseMenu
{
    [SerializeField] Transform mainMenu;
    [SerializeField] Transform allItemsTransform;
    [SerializeField] Transform inGamemenuTransform;

    protected override void OnEnable()
    {
        base.OnEnable();
        PlayerGlobals.canOpenMenus = false;
        GameModeManager.onGameModeStart += enableIngameMenus;

    }
    private void OnDisable()
    {
        GameModeManager.onGameModeStart -= enableIngameMenus;
    }
    private void enableIngameMenus()
    {
        inGamemenuTransform.gameObject.SetActive(true);
        ExitMainMenu();
    }

    // Update is called once per frame
    void Update()
    {
        GetInputForMenuToggle();
    }

    private void GetInputForMenuToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && PlayerGlobals.canOpenMenus == true)
        {
            if (allItemsTransform.gameObject.activeSelf == true)
            {
                ExitMainMenu();
            }
            else
            {
                OpenMainmenu();
            }

        }
    }

    private void OpenMainmenu()
    {
        allItemsTransform.gameObject.SetActive(true);
        OpenMenuPageIndex(0);
        PreventInput();
    }

    private void PreventInput()
    {
        Cursor.lockState = CursorLockMode.None;
        PlayerGlobals.isNotInMenu = false;
    }
    public void ExitMainMenu()
    {
        CloseMainMenu();
        Cursor.lockState = CursorLockMode.Locked;
        PlayerGlobals.isNotInMenu = true;
    }

    private void CloseMainMenu()
    {
        allItemsTransform.gameObject.SetActive(false);
    }

    public void ExitLobby()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            GameModeManager.Singleton.EndGameMode();
        }
        else
        {
            NetworkManager.Singleton.StopClient();
        }
        NetworkManager.Singleton.Shutdown();

    }
    public void GoBackToStartScreen()
    {
        SceneManager.LoadScene(2);
    }
}