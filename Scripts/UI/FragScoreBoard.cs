using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FragScoreBoard : MonoBehaviour
{
    [SerializeField] Transform fragBoard;
    [SerializeField] Transform fragContainer;
    [SerializeField] GameObject fragCounter;
    [SerializeField] VerticalLayoutGroup verticalLayoutGroup;
    Dictionary<PlayerInfo, FragCounter> placements = new Dictionary<PlayerInfo, FragCounter>();
    // Start is called before the first frame update
    void Start()
    {
        GameModeManager.onGameModeEnd += ResetScoreboard;
        GameModeManager.onNewHotJoin += updateOnHotJoin;
        PlayerManager.onPlayerDisconnection += updateHotJoinByDisconnection;
        EnableScoreboard();
        toggleBoard(false);
    }

    private void updateHotJoinByDisconnection(ulong objectID, ulong networkID)
    {
        SetPlacements();
    }

    private void OnDisable()
    {
        GameModeManager.onGameModeEnd -= ResetScoreboard;
        GameModeManager.onNewHotJoin -= updateOnHotJoin;
    }

    private void updateOnHotJoin(ulong ID)
    {
        SetPlacements();
    }

    void toggleBoard(bool state)
    {
        fragBoard.gameObject.SetActive(state);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            toggleBoard(true);
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            toggleBoard(false);
        }
    }
    void SetPlacements()
    {
        //Jos tämä on hotjoin, täytyy myös deletoida vanha scoreboard ja generoida uusi, annetaan olla näin, ihan ei paras performance mutta ei myöskään megaoperaatio
        for (int i = 0; i < fragContainer.childCount; i++)
        {
            Destroy(fragContainer.GetChild(i).gameObject);
            placements.Clear();
        }
        List<PlayerInfo> playersByDescending = PlayerManager.Singleton.AllPlayerInfos
            .OrderByDescending(o => o.PlayerKills).ToList();
        for (int i = 0; i < playersByDescending.Count; i++)
        {
            GameObject go = Instantiate(fragCounter, fragContainer);
            FragCounter copyfragCounter = go.GetComponent<FragCounter>();
            PlayerInfo playerInfo = playersByDescending[i];
            copyfragCounter.Initialize(ref playerInfo);
            placements.Add(playersByDescending[i], copyfragCounter);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(fragContainer.GetComponent<RectTransform>());

    }



    public void EnableScoreboard()
    {
        SetPlacements();
    }

    public void ResetScoreboard()
    {
        placements.Clear();
    }

}
