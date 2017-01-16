using System.Collections.Generic;

public class PlayerBase
{
    public string Name;

    public int[,] TypeMap;

    private MapMng mapMng;

    public SquareSprite[,] SquareMap;

    public List<SquareSprite[]> SquareWillInsert;

    public float MapMoveSpeed;

    public float MapMoveInterval;

    public bool IsRobot
    {
        get { return isRobot; }
    }

    protected bool isRobot = false;

    protected PlayerBase()
    {

    }

    public void SetMapdata(int[,] map)
    {
        TypeMap = new int[map.GetLength(0),map.GetLength(1)];
        for (int r = 0; r < map.GetLength(0); r++)
        {
            for (int c = 0; c < map.GetLength(1); c++)
            {
                TypeMap[r, c] = map[r, c];
            }
        }
    }

    public void InitMap(MapMng mapMng)
    {
        this.mapMng = mapMng;
    }

    public void MoveSquare(int r,int c,MapMng.MoveDir dir)
    {
        mapMng.MoveSquare(SquareMap[r,c],dir);
    }

}

public class NormalPlayer : PlayerBase
{

    public NormalPlayer()
    {
        Name = "LocalPlayer";
        isRobot = false;
    }
}


public class RobotPlayer : NormalPlayer
{
    public RobotPlayer()
    {
        isRobot = true;
        Name = "RobotPlayer";
    }
}

public class PVPPlayer : PlayerBase
{
    public bool IsLocal
    {
        get { return isLocal; }
    }

    protected bool isLocal = false;

    public PVPPlayer(bool isLocal)
    {
        this.isLocal = isLocal;
        this.isRobot = false;
    }
}


