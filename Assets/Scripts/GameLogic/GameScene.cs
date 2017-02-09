
using UnityEngine;

public class GameScene : MonoBehaviour
{

    public GameMode Mode = GameMode.StandAlone;

    private GameModeBase gameMode;

    public static GameScene Instance;

    void Start()
    {
        Instance = this;
        gameMode = GameModeBase.CreateGameMode(Mode);
        gameMode.Init();
        StartCoroutine(gameMode.GameLoop());
    }
}
