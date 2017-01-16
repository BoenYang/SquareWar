using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PVEMode : GameModeBase
{
    public override GameMode Mode
    {
        get { return GameMode.PVE; }
    }

    public override void Init()
    {
        NormalPlayer player = new NormalPlayer();
        RobotPlayer robotPlayer = new RobotPlayer();
        List<PlayerBase> players = new List<PlayerBase>();
        robotPlayer.SetMapPos(new Vector3(10,0,0));
        players.Add(player);
        players.Add(robotPlayer);
        MapMng.Instance.SetPlayer(players);
        MapMng.Instance.InitMap(Mode);
    }

    public override IEnumerator GameLoop()
    {
        yield return 0;
    }

    public override void GamePause()
    {
    }

    public override void GameResume()
    {
    }

    public override void GameOver(GameResult result)
    {
    }
}
