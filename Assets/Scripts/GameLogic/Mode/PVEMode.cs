using System.Collections;

public class PVEMode : GameModeBase
{
    public override GameMode Mode
    {
        get { return GameMode.PVE; }
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
