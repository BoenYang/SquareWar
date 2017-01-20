
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
    Left = 1,
    Right = 2,
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

    public override string ToString()
    {
        return string.Format("第{0}行,第{1}列，消除数量{2}", StartRow, StartColumn, Count);
    }
}