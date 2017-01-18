
using UnityEngine;

public class GameScene : MonoBehaviour
{

    public GameMode Mode = GameMode.StandAlone;

    private GameModeBase gameMode;

    void Start()
    {
        gameMode = GameModeBase.CreateGameMode(Mode);
        gameMode.Init();
        StartCoroutine(gameMode.GameLoop());
    }
}
