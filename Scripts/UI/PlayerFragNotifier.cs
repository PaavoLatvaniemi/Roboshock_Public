using TMPro;
using UnityEngine;

public class PlayerFragNotifier : MonoBehaviour
{
    [SerializeField] GameObject fragAnimatorObjectParent;
    private void OnEnable()
    {
        PlayerInfo.onKill += logKill;
    }
    private void OnDisable()
    {
        PlayerInfo.onKill -= logKill;
    }
    private void logKill(string playerInfo, int killCount)
    {
        GameObject go = Instantiate(fragAnimatorObjectParent, transform);
        go.SetActive(true);
        go.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>().text
            = "You fragged " + playerInfo + "!";
    }
}
