public class NormalPlayer : PlayerBase
{
    public override void InitPlayerMap(int[,] map)
    {
        base.InitPlayerMap(map);

        Name = "LocalPlayer";
        isRobot = false;
    }
}