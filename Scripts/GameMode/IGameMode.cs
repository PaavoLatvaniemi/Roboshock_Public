using UnityEngine;

public interface IGameMode
{
    public string GameModeName { get; }
    public GameObject GameModeGO { get; }
    public void StartGame();
    public void EndGame(PlayerInfo winner);
}
