using UnityEngine;
using UnityEngine.UI;

public class DemoUI : MonoBehaviour
{
    public Text Player1Score;

    public Text Player2Score;

    public Text Chain;

    public RawImage Image;

    public static DemoUI Ins;

    void Awake()
    {
        Ins = this;
    }
}
