using System;
using TMPro;
using UnityEngine;

public class FragCounter : MonoBehaviour
{
    PlayerInfo _playerInfo;
    [SerializeField] TextMeshProUGUI fraggerName;
    [SerializeField] TextMeshProUGUI fraggerFrags;

    public void Initialize(ref PlayerInfo info)
    {
        _playerInfo = info;
        updateInfo();

        PlayerInfo.onKillAdded += updateInfo;
    }
    private void OnDisable()
    {
        PlayerInfo.onKillAdded += updateInfo;
    }
    private void updateInfo(PlayerInfo playerInfo)
    {
        if (_playerInfo == playerInfo) updateInfo();
    }




    public void updateInfo()
    {
        fraggerName.text = _playerInfo.PlayerName;
        fraggerFrags.text = _playerInfo.PlayerKills.ToString();
    }
}
