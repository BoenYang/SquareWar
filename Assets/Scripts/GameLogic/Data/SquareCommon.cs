using System.Collections.Generic;

public enum SquareState
{
    Static = 1,
    Swap = 2,
    Fall = 3,
    Hung = 4,
    Clear = 5,
    Hide = 6,
}

public enum MoveDir
{
    Left = -1,
    Right = 1,
}

public enum RemoveDir
{
    Vertical = 1,
    Horizontal = 2,
}

public class RemoveData
{
    public int StartRow;
    public int StartColumn;
    public int Count;
    public RemoveDir Dir;

    public List<SquareSprite> RemoveList; 

    public void ConvertToList(SquareSprite[,] map)
    {
        RemoveList = new List<SquareSprite>();
        for (int i = 0; i < Count; i++)
        {
            SquareSprite squareNeedRemove = null;
            if (Dir == RemoveDir.Horizontal)
            {
                squareNeedRemove = map[StartRow, StartColumn + i];
            }
            else
            {
                squareNeedRemove = map[StartRow + i, StartColumn];
            }
            RemoveList.Add(squareNeedRemove);
        }
    }

    public override string ToString()
    {
        return string.Format("第{0}行,第{1}列，消除数量{2}", StartRow, StartColumn, Count);
    }
}