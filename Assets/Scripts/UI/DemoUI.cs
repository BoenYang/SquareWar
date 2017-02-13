using UnityEngine;
using UnityEngine.UI;

public class DemoUI : MonoBehaviour
{
    public Text Player1Score;

    public Text Player2Score;

    public Text Player1Chain;

    public Text Player2Chain;

    public GameObject Player1View;

    public GameObject Player2View;

    public GameObject Player1ScoreContainer;

    public GameObject Player2ScoreContainer;

    public static DemoUI Ins;

    private Vector3 player1ViewInitPos;

    void Awake()
    {
        Ins = this;

        player1ViewInitPos = Player1View.transform.localPosition;
    }

    public void Init(GameMode mode)
    {

        Player1Score.text = "0";
        Player2Score.text = "0";

        Player1Chain.text = "";
        Player2Chain.text = "";

        if (mode == GameMode.StandAlone)
        {
            Player1ScoreContainer.SetActive(true);
            Player2ScoreContainer.SetActive(false);

            Player1View.SetActive(true);
            Player2View.SetActive(false);

            Vector3 pos = player1ViewInitPos;
            pos.x = 0;
            Player1View.transform.localPosition = pos;


        } else if (mode == GameMode.PVE)
        {
            Player1View.SetActive(true);
            Player2View.SetActive(true);

            Player1View.transform.localPosition = player1ViewInitPos;

            Player1ScoreContainer.SetActive(true);
            Player2ScoreContainer.SetActive(true);
        }
    }

}
