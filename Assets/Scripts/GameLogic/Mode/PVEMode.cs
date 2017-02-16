using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PVEMode : GameModeBase
{
    public override GameMode Mode
    {
        get { return GameMode.PVE; }
    }

    private PlayerBase player;

    private PlayerBase robotPlayer;

    public override void Init()
    {
        LocalPlayer = player = PlayerBase.CreatePlayer(PlayerBase.PlayerType.Normal, MapMng.Instance.transform);
        robotPlayer = PlayerBase.CreatePlayer(PlayerBase.PlayerType.Robot, MapMng.Instance.transform);

        player.OnGetScore += OnPlayerGetScore;
        player.OnChain += OnPlayerChain;
        player.OnGameOver += GameOver;

        robotPlayer.OnGetScore += OnRobotGetScore;
        robotPlayer.OnChain += OnRobotChain;
        robotPlayer.OnGameOver += GameOver;

        player.SetMapPos(new Vector3(1.09f,0.7f,0));
        robotPlayer.SetMapPos(new Vector3(10,0,0));
        Players.Add(player);
        Players.Add(robotPlayer);

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
        GameUI.Ins.Player1Score.text = player.Score.ToString();
        Random.seed = DateTime.Now.Millisecond;
        AddBlock(robotPlayer, addScore);
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

    private void OnRobotGetScore(int addScore)
    {
        GameUI.Ins.Player2Score.text = robotPlayer.Score.ToString();
        Random.seed = DateTime.Now.Millisecond;
        AddBlock(player,addScore);
    }

    private void AddBlock(PlayerBase player,int addScore)
    {
        List<int[,]> blockDatas = GetDropData(addScore);
        for (int i = 0; i < blockDatas.Count; i++)
        {
            GameScene.Instance.StartCoroutine(AddBlockDelay(1 + i, player, blockDatas[i]));
        }
    }

    private void OnRobotChain(int chain)
    {
        if (chain > 1)
        {
            GameUI.Ins.Player2Chain.gameObject.SetActive(true);
            GameUI.Ins.Player2Chain.text = "连击+" + chain;
        }
        else
        {
            GameUI.Ins.Player2Chain.gameObject.SetActive(false);
        }
    }

    private IEnumerator AddBlockDelay(int delay,PlayerBase player,int[,] blockData)
    {
        yield return new WaitForSeconds(delay);

        int pos = Random.Range(0, 2);

        if (pos == 0)
        {
            player.InsertBlockAtTopLeft(blockData,1);
        }
        else
        {
            player.InsertBlockAtTopRight(blockData,1);
        }
    }

    private List<int[,]> GetDropData(int score)
    {
        List<int[,]> drops = new List<int[,]>();
        switch (score)
        {
           case 3:
           case 4:
           case 5:
           case 6:
                drops.Add(GenerateBlock(1,score));
                break;
           case 7:
                drops.Add(GenerateBlock(1,4));
                drops.Add(GenerateBlock(1,3));
                break;
           case 8:
                drops.Add(GenerateBlock(1, 4));
                drops.Add(GenerateBlock(1, 4));
                break;
           case 9:
                drops.Add(GenerateBlock(1, 5));
                drops.Add(GenerateBlock(1, 4));
                break;
           case 10:
                drops.Add(GenerateBlock(1, 5));
                drops.Add(GenerateBlock(1, 5));
                break;
           case 12:
                drops.Add(GenerateBlock(2, 6));
                break;
           case 18:
                drops.Add(GenerateBlock(3, 6));
                break;
        }
        return drops;
    }

    private int[,] GenerateBlock(int row,int column)
    {
        int[,] data = new int[row,column];
        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                data[r, c] = Random.Range(1,5);
            }
        }
        return data;
    }
}
