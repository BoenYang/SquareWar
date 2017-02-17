using UnityEngine;

public class UIController : MonoBehaviour
{

    public GameObject GameUI;

    public GameObject MainUI;

    public Canvas canvas;

    public static UIController Ins;

    void Awake()
    {
        Ins = this;
        ShowMainUI();
        canvas = GetComponent<Canvas>();
    }

    public void ShowMainUI()
    {
        GameUI.SetActive(false);
        MainUI.SetActive(true);
    }

    public void ShowGameUI()
    {
        GameUI.SetActive(true);
        MainUI.SetActive(false);
    }

}
