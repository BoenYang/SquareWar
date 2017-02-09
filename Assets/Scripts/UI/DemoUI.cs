using UnityEngine;
using UnityEngine.UI;

public class DemoUI : MonoBehaviour
{
    public Text Player1Score;

    public Text Player2Score;

    public Text Player1Chain;

    public Text Player2Chain;

    public GameObject Player2View;

    public static DemoUI Ins;

    void Awake()
    {
        Ins = this;
    }
}
