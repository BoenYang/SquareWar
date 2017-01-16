using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase
{

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

    public string Name;

    public int[,] TypeMap;

    public SquareSprite[,] SquareMap;

    public int Score;

    private MapMng mapMng;

    private Vector3 startPos;

    private List<SquareSprite[]> squareWillInsert;

    private List<int[]> typeWillInsert; 

    private List<RemoveData> removingData = new List<RemoveData>();

    private Transform squareRoot;

    private int insertedRawCount;

    private float moveIntervalTimer;

    public bool IsRobot
    {
        get { return isRobot; }
    }

    protected bool isRobot = false;

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
        GameObject player =  new GameObject();
        player.name = Name;
        player.transform.SetParent(mapMng.gameObject.transform);
        player.transform.localPosition = Vector3.zero;
        player.transform.localScale = Vector3.one;
        startPos = player.transform.localPosition;
        squareRoot = player.transform;
        insertedRawCount = 0;
    }

    public void MoveSquare(int r,int c,MapMng.MoveDir dir)
    {
       
    }

    public void InsertRowAtIndex(int insertRowIndex, int[] rowData, SquareSprite[] squareData)
    {
        if (rowData == null || rowData.Length > TypeMap.GetLength(0) || squareData == null || squareData.Length > SquareMap.GetLength(0))
        {
            Debug.LogError("数据格式不合法");
            return;
        }

        insertedRawCount++;
        for (int r = 1; r <= insertRowIndex; r++)
        {
            for (int c = 0; c < TypeMap.GetLength(0); c++)
            {
                TypeMap[r - 1, c] = TypeMap[r, c];
                SquareMap[r - 1, c] = SquareMap[r, c];
                if (SquareMap[r, c] != null)
                {
                    SquareMap[r, c].Row -= 1;
                }
            }
        }

        //最后一行插入
        for (int i = 0; i < rowData.Length; i++)
        {
            TypeMap[insertRowIndex, i] = rowData[i];
            squareData[i].Row = insertRowIndex;
            squareData[i].SetGray(false);
            SquareMap[insertRowIndex, i] = squareData[i];
        }
    }



    public void InsertRowAtBottom()
    {
        InsertRowAtIndex(TypeMap.GetLength(0) - 1, typeWillInsert[0], squareWillInsert[0]);
    }

    public void InsertRowAtTop()
    {

    }

    public void MoveSquare(SquareSprite movingSquare, MoveDir dir)
    {
        int raw = movingSquare.Row;
        int currentColumn = movingSquare.Column;
        int targetColumn = currentColumn;
        if (dir == MoveDir.Left)
        {
            if (movingSquare.Column != 0)
            {
                targetColumn = movingSquare.Column - 1;
                SwapSquareMap(raw, currentColumn, raw, targetColumn);
            }
        }
        else
        {
            if (movingSquare.Column != TypeMap.GetLength(1))
            {
                targetColumn = movingSquare.Column + 1;
                SwapSquareMap(raw, currentColumn, raw, targetColumn);
            }
        }
    }

    private Vector3 GetPos(int r, int c)
    {
        return startPos + new Vector3(c * GameSetting.SquareWidth, -r * GameSetting.SquareWidth, 0);
    }

    private void SwapMap(int r1, int c1, int r2, int c2)
    {
        int tempType = TypeMap[r1, c1];
        TypeMap[r1, c1] = TypeMap[r2, c2];
        TypeMap[r2, c2] = tempType;

        SquareSprite tempSquare = SquareMap[r1, c1];
        SquareMap[r1, c1] = SquareMap[r2, c2];
        SquareMap[r2, c2] = tempSquare;
    }

    private void SwapSquareMap(int r1, int c1, int r2, int c2)
    {

        SquareSprite s1 = SquareMap[r1, c1];
        SquareSprite s2 = SquareMap[r2, c2];

        //如果要移动到的位置有方块在播放动画，则不能一移动
        if (s2 != null && s2.IsAnimating)
        {
            return;
        }

        //Debug.LogFormat("swap {0},{1} <--> {2},{3}", r1, c1, r2, c2);

        if (s1 != null && !s1.IsAnimating)
        {
            s1.Column = c2;
            s1.Row = r2;

            Vector3 moveToPos = GetPos(r2 + insertedRawCount, c2);
            s1.MoveToPos(moveToPos, 0.1f, () =>
            {
                SwapMap(r1, c1, r2, c2);
                //Check();
            });
        }

        if (s2 != null)
        {
            s2.Column = c1;
            s2.Row = r1;

            Vector3 moveToPos = GetPos(r1 + insertedRawCount, c1);
            s2.MoveToPos(moveToPos, 0.1f);
        }
    }

    private void MarkWillRemove(RemoveData removeData)
    {
        for (int i = 0; i < removeData.Count; i++)
        {
            if (removeData.Dir == RemoveDir.Horizontal)
            {
                SquareSprite squareNeedRemove = SquareMap[removeData.StartRow, removeData.StartColumn + i];
                squareNeedRemove.MarkWillRemove();
            }
            else
            {
                SquareSprite squareNeedRemove = SquareMap[removeData.StartRow + i, removeData.StartColumn];
                squareNeedRemove.MarkWillRemove();
            }
        }
    }

    private void Remove(RemoveData removeData)
    {
       mapMng.StartCoroutine(RemoveCorutine(removeData));
    }

    private IEnumerator RemoveCorutine(RemoveData removeData)
    {
        yield return new WaitForSeconds(GameSetting.BaseMapMoveInterval);
        for (int i = 0; i < removeData.Count; i++)
        {
            if (removeData.Dir == RemoveDir.Horizontal)
            {
                SquareSprite squareNeedRemove = SquareMap[removeData.StartRow, removeData.StartColumn + i];
                squareNeedRemove.ShowRemoveEffect();
            }
            else
            {
                SquareSprite squareNeedRemove = SquareMap[removeData.StartRow + i, removeData.StartColumn];
                squareNeedRemove.ShowRemoveEffect();
            }

            yield return new WaitForSeconds(GameSetting.BaseMapMoveInterval);
        }

        removingData.Remove(removeData);
        for (int i = 0; i < removeData.Count; i++)
        {
            if (removeData.Dir == RemoveDir.Horizontal)
            {
                SquareMap[removeData.StartRow, removeData.StartColumn + i].Remove();
                SquareMap[removeData.StartRow, removeData.StartColumn + i] = null;
                TypeMap[removeData.StartRow, removeData.StartColumn + i] = 0;
            }
            else
            {
                SquareMap[removeData.StartRow + i, removeData.StartColumn].Remove();
                SquareMap[removeData.StartRow + i, removeData.StartColumn] = null;
                TypeMap[removeData.StartRow + i, removeData.StartColumn] = 0;
            }
        }
    }

    private void CheckRemove()
    {
        //CalculateDropCount();
        CheckHorizontalRemove();
        CheckVerticalRemove();
    }

    private void CheckHorizontalRemove()
    {
        //检测水平方向消除
        for (int r = 0; r < TypeMap.GetLength(0); r++)
        {
            int firstType = 0;
            int typeCount = 0;
            for (int c = 0; c < TypeMap.GetLength(1) - 1; c++)
            {
                if (TypeMap[r, c] == 0)
                {
                    continue;
                }
                else
                {
                    firstType = TypeMap[r, c];
                    typeCount = 1;
                }

                int i = c + 1;
                for (i = c + 1; i < TypeMap.GetLength(1); i++)
                {
                    if (TypeMap[r, i] == firstType && SquareMap[r, i].CanRemove())
                    {
                        typeCount++;
                    }
                    else
                    {
                        if (typeCount >= 3)
                        {
                            RemoveData removeData = new RemoveData();
                            removeData.StartRow = r;
                            removeData.StartColumn = c;
                            removeData.Count = typeCount;
                            removeData.Dir = RemoveDir.Horizontal;
                            removingData.Add(removeData);
                            MarkWillRemove(removeData);
                            Remove(removeData);
                        }
                        // Debug.LogFormat("第{0}行，第{1}列,重复数量{2}",r,c,typeCount);
                        break;
                    }
                }

                if (typeCount >= 3 && i == TypeMap.GetLength(1) && SquareMap[r, i - 1].CanRemove())
                {
                    RemoveData removeData = new RemoveData();
                    removeData.StartRow = r;
                    removeData.StartColumn = c;
                    removeData.Count = typeCount;
                    removeData.Dir = RemoveDir.Horizontal;
                    removingData.Add(removeData);
                    MarkWillRemove(removeData);
                    Remove(removeData);
                    //Debug.LogFormat("第{0}行，第{1}列,重复数量{2}",r,c,typeCount);
                }
            }
            // Debug.LogFormat("第{0}行，第{1}列,重复数量{2}", r, mapData.GetLength(1) - 1, 1);
        }
    }

    private void CheckVerticalRemove()
    {
        //检测垂直方向消除
        for (int c = 0; c < TypeMap.GetLength(1); c++)
        {
            int firstType = 0;
            int typeCount = 0;
            for (int r = 0; r < TypeMap.GetLength(0) - 1; r++)
            {
                if (TypeMap[r, c] == 0)
                {
                    continue;
                }
                else
                {
                    firstType = TypeMap[r, c];
                    typeCount = 1;
                }

                int i = r + 1;
                for (i = r + 1; i < TypeMap.GetLength(0); i++)
                {
                    if (TypeMap[i, c] == firstType && SquareMap[i, c].CanRemove())
                    {
                        typeCount++;
                    }
                    else
                    {
                        if (typeCount >= 3)
                        {
                            RemoveData removeData = new RemoveData();
                            removeData.StartRow = r;
                            removeData.StartColumn = c;
                            removeData.Count = typeCount;
                            removeData.Dir = RemoveDir.Vertical;
                            removingData.Add(removeData);
                            MarkWillRemove(removeData);
                            Remove(removeData);
                        }
                        //Debug.LogFormat("第{0}列，第{1}行,重复数量{2}", c, r, typeCount);
                        break;
                    }
                }

                if (typeCount >= 3 && i == TypeMap.GetLength(0) && SquareMap[i - 1, c].CanRemove())
                {
                    RemoveData removeData = new RemoveData();
                    removeData.StartRow = r;
                    removeData.StartColumn = c;
                    removeData.Count = typeCount;
                    removeData.Dir = RemoveDir.Vertical;
                    removingData.Add(removeData);
                    MarkWillRemove(removeData);
                    Remove(removeData);
                    // Debug.LogFormat("第{0}行，第{1}列,重复数量{2}",r,c,typeCount);
                }
            }
            //Debug.LogFormat("第{0}列，第{1}行,重复数量{2}", c, mapData.GetLength(0), 1);
        }
    }

    private void CalculateDropCount()
    {
        for (int c = 0; c < TypeMap.GetLength(1); c++)
        {
            for (int r = 0; r < TypeMap.GetLength(0); r++)
            {
                if (SquareMap[r, c] == null || SquareMap[r, c].IsAnimating)
                {
                    continue;
                }

                SquareMap[r, c].CaluateNextNullCount(SquareMap);
                //Debug.LogFormat("{0}行,{1}列 下部空格数量为{2}",r,c,squareSpriteMap[r,c].NextNullCount);
            }
        }
    }

    private float moveDistance = 0;

    private void MoveMap()
    {
        if (removingData.Count != 0)
        {
            return;
        }

        moveIntervalTimer += Time.deltaTime;
        if (moveIntervalTimer >= GameSetting.SquareRemoveInterval)
        {
            moveIntervalTimer = 0;
            moveDistance += GameSetting.BaseMapMoveSpeed;
            Vector3 curPos = squareRoot.localPosition;
            squareRoot.localPosition = curPos + new Vector3(0, GameSetting.BaseMapMoveSpeed, 0);
            if (Mathf.Abs(moveDistance - GameSetting.SquareWidth) <= 0.000001f)
            {
                moveDistance = 0;
                InsertRowAtBottom();
            }
        }
    }

    private void MoveSquare()
    {
        for (int c = 0; c < TypeMap.GetLength(1); c++)
        {
            for (int r = TypeMap.GetLength(0) - 1; r >= 0; r--)
            {
                SquareSprite movingSquare = SquareMap[r, c];
                if (movingSquare != null && movingSquare.NextNullCount != 0 && !movingSquare.IsAnimating)
                {
                    Vector3 moveToPos = GetPos(r + movingSquare.NextNullCount + insertedRawCount, c);

                    movingSquare.MoveToNextNullPos(moveToPos, 0.1f, (s) =>
                    {
                        TypeMap[s.Row - s.NextNullCount, s.Column] = 0;
                        TypeMap[s.Row, s.Column] = s.Type;
                        SquareMap[s.Row - s.NextNullCount, s.Column] = null;
                        SquareMap[s.Row, s.Column] = s;
                        //Check();
                    });
                }
            }
        }
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


