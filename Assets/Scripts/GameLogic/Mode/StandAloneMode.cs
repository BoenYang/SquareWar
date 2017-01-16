using System.Collections;
using System.Collections.Generic;

public class StandAloneMode : GameModeBase
{
    public override GameMode Mode
    {
        get { return GameMode.StandAlone; }
    }

    public override void Init()
    {
        NormalPlayer player = new NormalPlayer();
        List<PlayerBase> players = new List<PlayerBase>();
        players.Add(player);
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
