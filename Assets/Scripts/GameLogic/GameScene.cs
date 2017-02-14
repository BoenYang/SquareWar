
using UnityEngine;

public class GameScene : MonoBehaviour
{

    public GameMode Mode = GameMode.StandAlone;

    public GameModeBase Game;

    public static GameScene Instance;

    void Awake()
    {
        Instance = this;
    }

    public void StartGame()
    {
        Instance = this;
        Game = GameModeBase.CreateGameMode(Mode);
        Game.Init();
        StartCoroutine(Game.GameLoop());
    }

    public void StopGame()
    {
        StopAllCoroutines();
    }
}
