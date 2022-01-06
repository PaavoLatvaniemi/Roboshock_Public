using Assets.Scripts.UI;
using TMPro;
using UnityEngine;

public class MatchJoinButtonExtender : UIElementExtender<StartMenu>
{
    int index;
    [SerializeField] TextMeshProUGUI TextMesh;
    public void Initialize(int i, string Descriptor)
    {
        index = i;
        TextMesh.text = Descriptor;
    }
    public void JoinMatch()
    {
        elementOfExtension.getMatchMaking().ConnectToServerIndex(index);
        //Lobbymenu on indeksiä 1. Eli sivu 2
        elementOfExtension.OpenMenuPageIndex(1);
        elementOfExtension.populateLobbyView();
    }
}
