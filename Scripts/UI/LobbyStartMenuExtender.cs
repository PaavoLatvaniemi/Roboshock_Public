using Assets.Scripts.UI;
/// <summary>
/// T‰m‰ skripti antaa mahdollisuuden lis‰t‰ ylemm‰n tason (StartMenu.cs) metodeja buttoneihin, ym ilman ett‰
/// Prefab modessa t‰m‰n objektin kanvaksen t‰ytyy sis‰llytt‰‰ kyseinen asia.
/// </summary>
public class LobbyStartMenuExtender : UIElementExtender<StartMenu>
{
    public void returnUpMethod()
    {
        elementOfExtension.DisconnectFromGame();
        elementOfExtension.goBackAMenu();
    }
    public void startGame()
    {
        elementOfExtension.StartGame();
    }
}
