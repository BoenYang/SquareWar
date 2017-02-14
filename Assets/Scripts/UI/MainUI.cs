using UnityEngine;

public class MainUI : MonoBehaviour
{

    public void OnStandloneClick()
    {
        UIController.Ins.ShowGameUI();
        GameScene.Instance.Mode = GameMode.StandAlone;
        GameScene.Instance.StartGame();
    }

    public void OnPVEClick()
    {

        UIController.Ins.ShowGameUI();
        GameScene.Instance.Mode = GameMode.PVE;
        GameScene.Instance.StartGame();
    }   

}
