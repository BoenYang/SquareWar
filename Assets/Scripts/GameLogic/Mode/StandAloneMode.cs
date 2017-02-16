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
        LocalPlayer = player = PlayerBase.CreatePlayer(PlayerBase.PlayerType.Normal,MapMng.Instance.transform);
        player.SetMapPos(new Vector3(0, 0.7f, 0));

        player.OnChain += OnPlayerChain;
        player.OnGetScore += OnPlayerGetScore;
        player.OnGameOver += GameOver;

        Players = new List<PlayerBase>();
        Players.Add(player);

        GameUI.Ins.Init(Mode);
        MapMng.Instance.SetPlayer(Players);
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

    public override void GameOver()
    {
        GameUI.Ins.ShowResultView(Players);
    }

    public override void RestartGame()
    {
        GameScene.Instance.StopGame();
        MapMng.Instance.ClearAllPlayer();
        GameScene.Instance.StartGame();
        GameUI.Ins.CloseResultView();
    }

    private void OnPlayerGetScore(int addScore)
    {
        GameUI.Ins.Player1Score.text = player.Name + ": " + player.Score;
    }

    private void OnPlayerChain(int chain)
    {
        if (chain > 1)
        {
            GameUI.Ins.Player1Chain.gameObject.SetActive(true);
            GameUI.Ins.Player1Chain.text = "连击+" + chain;
        }
        else
        {
            GameUI.Ins.Player1Chain.gameObject.SetActive(false);
        }
    }
}
