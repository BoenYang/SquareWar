using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandAloneMode : GameModeBase
{

    private PlayerBase player;

    public override GameMode Mode
    {
        get { return GameMode.StandAlone; }
    }

    public override void Init()
    {
        player = new NormalPlayer();
        player.SetMapPos(new Vector3(1.09f, 0.7f, 0));
        player.OnChain += OnPlayerChain;
        player.OnGetScore += OnPlayerGetScore;


        players = new List<PlayerBase>();
        players.Add(player);
        DemoUI.Ins.Player2View.SetActive(false);
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

    private void OnPlayerGetScore(int addScore)
    {
        DemoUI.Ins.Player1Score.text = player.Name + ": " + player.Score;
    }

    private void OnPlayerChain(int chain)
    {
        if (chain > 1)
        {
            DemoUI.Ins.Player1Chain.gameObject.SetActive(true);
            DemoUI.Ins.Player1Chain.text = "连击+" + chain;
        }
        else
        {
            DemoUI.Ins.Player1Chain.gameObject.SetActive(false);
        }
    }
}
