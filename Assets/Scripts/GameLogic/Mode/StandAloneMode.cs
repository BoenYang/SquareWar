using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandAloneMode : GameModeBase
{
    public override GameMode Mode
    {
        get { return GameMode.StandAlone; }
    }

    public override void Init()
    {
        NormalPlayer player = new NormalPlayer();
        player.SetMapPos(new Vector3(1.09f, 0.7f, 0));
        players = new List<PlayerBase>();
        players.Add(player);
        DemoUI.Ins.Image.gameObject.SetActive(false);
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
