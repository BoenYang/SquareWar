using NUnit.Framework;

public class GameModeBaseTest {

	[Test]
	public void GameModeCreateTest()
	{
        Assert.NotNull(GameModeBase.CreateGameMode(GameMode.PVE));

        Assert.NotNull(GameModeBase.CreateGameMode(GameMode.PVP));

        Assert.NotNull(GameModeBase.CreateGameMode(GameMode.StandAlone));
    }
}
