
using System.Collections;

public class PVPMode : GameModeBase
{
    public override GameMode Mode
    {
        get { return GameMode.PVP; }
    }

    public override void Init()
    {
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
