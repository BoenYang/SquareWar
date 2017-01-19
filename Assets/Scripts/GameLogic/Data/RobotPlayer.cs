
using UnityEngine;

public class RobotPlayer : PlayerBase
{
    public RobotPlayer()
    {
        isRobot = true;
        Name = "RobotPlayer";
    }

    protected override void OnGetScore(int addScore)
    {
        DemoUI.Ins.Player2Score.text = Name + ": " + Score;
    }
}