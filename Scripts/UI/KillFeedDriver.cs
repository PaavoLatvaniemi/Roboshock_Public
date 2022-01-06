using System.Collections;
using UnityEngine;

public class KillFeedDriver : MonoBehaviour
{
    //Wrapataan saadut infot uuteen classiin.

    [SerializeField] GameObject killFeedUIElement;
    [SerializeField] Sprite[] killSprites;
    private void OnEnable()
    {
        PlayerHpController.onPlayerDeath += addKillFeedKill;
    }
    private void OnDisable()
    {
        PlayerHpController.onPlayerDeath -= addKillFeedKill;
    }
    //Kill weapon 5 = self kill ikoni
    private void addKillFeedKill(PlayerInfo KilledPlayerInfo, PlayerInfo KillerPlayerInfo, int killWeapon = 5)
    {
        KillFeedKill killFeedKill = new KillFeedKill(KillerPlayerInfo, KilledPlayerInfo, killWeapon);
        AddKillToCanvas(killFeedKill);
    }

    private void AddKillToCanvas(KillFeedKill killFeedKill)
    {
        GameObject cloneKillFeedElement = Instantiate(killFeedUIElement, transform);
        string killed = killFeedKill.Killed.PlayerName.ToString();
        string killer = (killFeedKill.Killer != null) ? killFeedKill.Killer.PlayerName.ToString() : killFeedKill.Killed.PlayerName.ToString();
        cloneKillFeedElement.GetComponent<KillFeedBox>().SetupKillUIElement(killer, killed, getKillWeapon(killFeedKill.KillWeapon));
        if (!killFeedKill.Killed.IsLocalPlayer)
        {
            if (killFeedKill.Killer != null)
            {
                if (killFeedKill.Killer.IsLocalPlayer)
                    cloneKillFeedElement.GetComponent<KillFeedBox>().SetupKillAsOwn();
            }

        }
        StartCoroutine(WaitForKillFeedEnd(cloneKillFeedElement.GetComponent<KillFeedBox>()));

    }

    private IEnumerator WaitForKillFeedEnd(KillFeedBox killFeed)
    {
        yield return new WaitForSeconds(4.5f);
        killFeed.EndKill();
    }

    Sprite getKillWeapon(int index)
    {
        return killSprites[index];
    }
}
public class KillFeedKill
{
    PlayerInfo _killer;
    PlayerInfo _killed;
    int _killWeapon;
    public PlayerInfo Killed { get => _killed; set => _killed = value; }
    public PlayerInfo Killer { get => _killer; set => _killer = value; }
    public int KillWeapon { get => _killWeapon; set => _killWeapon = value; }

    //Asetta ei välttämättä ole
    public KillFeedKill(PlayerInfo killer, PlayerInfo killed, int killWeapon = 0)
    {
        Killer = killer;
        Killed = killed;
        KillWeapon = killWeapon;
    }
}