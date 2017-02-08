using System.Collections;
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
        player.SetMapPos(new Vector3(1.09f,0.7f,0));
        robotPlayer.SetMapPos(new Vector3(10,0,0));
        players.Add(player);
        players.Add(robotPlayer);
        DemoUI.Ins.Player1Score.text = player.Name + "：0";
        DemoUI.Ins.Player2Score.text = robotPlayer.Name + "：0";
//        DemoUI.Ins.Image.gameObject.SetActive(true);
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
