using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase
{
    public string Name = "";

    public int Score = 0;

    public int[,] TypeMap = null;

    public SquareSprite[,] SquareMap = null;

    public bool IsRobot { get { return isRobot; } }

    protected bool isRobot = false;

    private MapMng mapMng;

    private Vector3 startPos;

    private List<SquareSprite[]> squareWillInsert;

    private List<SquareSprite.RemoveData> removingData = new List<SquareSprite.RemoveData>();

    private Transform squareRoot;

    private Vector3 pos = Vector3.zero;

    private int insertedRawCount = 0;

    private int raw;

    private int column;

    private float moveIntervalTimer;

    private bool gameOver = false;

    public void SetMapdata(int[,] map)
    {
        raw = map.GetLength(0);
        column = map.GetLength(1);
        TypeMap = new int[raw,column];
        SquareMap = new SquareSprite[raw, column];

        for (int r = 0; r < raw; r++)
        {
            for (int c = 0; c < column; c++)
            {
                TypeMap[r, c] = map[r, c];
            }
        }
    }

    public void InitMap(MapMng mapMng)
    {
        Debug.Log("init player " + Name + " map");
        this.mapMng = mapMng;
        this.squareWillInsert = new List<SquareSprite[]>();

        GameObject player =  new GameObject();
        player.name = Name;
        player.transform.SetParent(mapMng.gameObject.transform);
        player.transform.localPosition = this.pos;
        player.transform.localScale = Vector3.one;
        player.gameObject.layer = mapMng.gameObject.layer;

        startPos = new Vector3(-column*GameSetting.SquareWidth/2f + GameSetting.SquareWidth/2, raw*GameSetting.SquareWidth/2 - GameSetting.SquareWidth/2, 0);
        squareRoot = player.transform;
        insertedRawCount = 0;

        for (int r = 0; r < raw; r++)
        {
            for (int c = 0; c < column; c++)
            {
                if (TypeMap[r, c] != 0)
                {
                    Vector3 pos = GetPos(r, c);
                    SquareSprite ss = SquareSprite.CreateSquare(TypeMap[r, c], r, c);
                    ss.transform.SetParent(squareRoot);
                    ss.transform.localPosition = pos;
                    ss.transform.localScale = Vector3.one * 0.9f;
                    ss.name = "Rect[" + r + "," + c + "]";
                    ss.SetPlayer(this);
                    SquareMap[r, c] = ss;
                    SquareMap[r, c].gameObject.layer = squareRoot.gameObject.layer;
                }
                else
                {
                    SquareMap[r, c] = null;
                }
            }
        }
    }

    public void SetMapPos(Vector3 pos)
    {
        this.pos = pos;
    }


    private Vector3 GetPos(int r, int c)
    {
        return startPos + new Vector3(c * GameSetting.SquareWidth, -r * GameSetting.SquareWidth, 0);
    }

    #region 插入行算法

    public bool NeedAddNewRaw()
    {
        return squareWillInsert.Count == 0;
    }

    public void InsertRowAtIndex(int insertRowIndex,SquareSprite[] squareData)
    {
        if (squareData == null || squareData.Length > SquareMap.GetLength(0))
        {
            Debug.LogError("数据格式不合法");
            return;
        }


        for (int r = 1; r <= insertRowIndex; r++)
        {
            for (int c = 0; c < column; c++)
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
        for (int i = 0; i < squareData.Length; i++)
        {
            if (squareData[i] != null)
            {
                TypeMap[insertRowIndex, i] = squareData[i].Type;
                squareData[i].Row = insertRowIndex;
                squareData[i].SetGray(false);
            }
            SquareMap[insertRowIndex, i] = squareData[i];
        }
    }

    public void InsertRowAtBottom()
    {
        if (squareWillInsert.Count == 0)
        {
            return;
        }

        insertedRawCount++;
        Debug.Log(Name + "inserted raw count " + insertedRawCount);
        for (int c = 0; c < TypeMap.GetLength(1); c++)
        {
            if(TypeMap[0,c] != 0)
            {
                gameOver = true;
                return;
            }
        }

        InsertRowAtIndex(TypeMap.GetLength(0) - 1,squareWillInsert[0]);
        squareWillInsert.RemoveAt(0);
    }

    public void AddWillInsertRaw(int[] insertRawData)
    {
        if (insertRawData == null || insertRawData.Length != SquareMap.GetLength(1))
        {
            Debug.LogError("数据格式不合法");
            return;
        }
    
        SquareSprite[] insertRawSquare = new SquareSprite[SquareMap.GetLength(1)];
        for (int i = 0; i < insertRawData.Length; i++)
        {
            Vector3 pos = GetPos(raw + insertedRawCount, i);
            insertRawSquare[i] = SquareSprite.CreateSquare(insertRawData[i], -1, i);
            insertRawSquare[i].transform.SetParent(squareRoot);
            insertRawSquare[i].transform.localPosition = pos;
            insertRawSquare[i].transform.localScale = Vector3.one * 0.9f;
            insertRawSquare[i].name = "Rect[" + 0 + "," + i + "]";
            insertRawSquare[i].SetGray(true);
            insertRawSquare[i].SetPlayer(this);
            insertRawSquare[i].gameObject.layer = squareRoot.gameObject.layer;
        }
        squareWillInsert.Add(insertRawSquare);
    }

    public void InsertRowAtTop()
    {

    }

    #endregion

    #region 方块移动

    public void MoveSquare(SquareSprite movingSquare, SquareSprite.MoveDir dir)
    {
        int raw = movingSquare.Row;
        int currentColumn = movingSquare.Column;
        int targetColumn = currentColumn;
        if (dir == SquareSprite.MoveDir.Left)
        {
            if (movingSquare.Column != 0)
            {
                targetColumn = movingSquare.Column - 1;
                SwapSquareMap(raw, currentColumn, raw, targetColumn);
            }
        }
        else
        {
            if (movingSquare.Column != column)
            {
                targetColumn = movingSquare.Column + 1;
                SwapSquareMap(raw, currentColumn, raw, targetColumn);
            }
        }
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

    #endregion

    #region 消除核心算法

    private void MarkWillRemove(SquareSprite.RemoveData removeData)
    {
        for (int i = 0; i < removeData.Count; i++)
        {
            if (removeData.Dir == SquareSprite.RemoveDir.Horizontal)
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

    private void Remove(SquareSprite.RemoveData removeData)
    {
        mapMng.StartCoroutine(RemoveCorutine(removeData));
    }

    private IEnumerator RemoveCorutine(SquareSprite.RemoveData removeData)
    {
        yield return new WaitForSeconds(GameSetting.BaseMapMoveInterval);
        for (int i = 0; i < removeData.Count; i++)
        {
            if (removeData.Dir == SquareSprite.RemoveDir.Horizontal)
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
            if (removeData.Dir == SquareSprite.RemoveDir.Horizontal)
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
        CheckHorizontalRemove();
        CheckVerticalRemove();
    }

    private void CheckHorizontalRemove()
    {
        //检测水平方向消除
        for (int r = 0; r < raw; r++)
        {
            int firstType = 0;
            int typeCount = 0;
            for (int c = 0; c < column - 1; c++)
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
                for (i = c + 1; i < column; i++)
                {
                    if (TypeMap[r, i] == firstType && SquareMap[r, i].CanRemove())
                    {
                        typeCount++;
                    }
                    else
                    {
                        if (typeCount >= 3)
                        {
                            SquareSprite.RemoveData removeData = new SquareSprite.RemoveData();
                            removeData.StartRow = r;
                            removeData.StartColumn = c;
                            removeData.Count = typeCount;
                            removeData.Dir = SquareSprite.RemoveDir.Horizontal;
                            removingData.Add(removeData);
                            MarkWillRemove(removeData);
                            Remove(removeData);
                        }
                        // Debug.LogFormat("第{0}行，第{1}列,重复数量{2}",r,c,typeCount);
                        break;
                    }
                }

                if (typeCount >= 3 && i == column && SquareMap[r, i - 1].CanRemove())
                {
                    SquareSprite.RemoveData removeData = new SquareSprite.RemoveData();
                    removeData.StartRow = r;
                    removeData.StartColumn = c;
                    removeData.Count = typeCount;
                    removeData.Dir = SquareSprite.RemoveDir.Horizontal;
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
        for (int c = 0; c < column; c++)
        {
            int firstType = 0;
            int typeCount = 0;
            for (int r = 0; r < raw - 1; r++)
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
                for (i = r + 1; i < raw; i++)
                {
                    if (TypeMap[i, c] == firstType && SquareMap[i, c].CanRemove())
                    {
                        typeCount++;
                    }
                    else
                    {
                        if (typeCount >= 3)
                        {
                            SquareSprite.RemoveData removeData = new SquareSprite.RemoveData();
                            removeData.StartRow = r;
                            removeData.StartColumn = c;
                            removeData.Count = typeCount;
                            removeData.Dir = SquareSprite.RemoveDir.Vertical;
                            removingData.Add(removeData);
                            MarkWillRemove(removeData);
                            Remove(removeData);
                        }
                        //Debug.LogFormat("第{0}列，第{1}行,重复数量{2}", c, r, typeCount);
                        break;
                    }
                }

                if (typeCount >= 3 && i == raw && SquareMap[i - 1, c].CanRemove())
                {
                    SquareSprite.RemoveData removeData = new SquareSprite.RemoveData();
                    removeData.StartRow = r;
                    removeData.StartColumn = c;
                    removeData.Count = typeCount;
                    removeData.Dir = SquareSprite.RemoveDir.Vertical;
                    removingData.Add(removeData);
                    MarkWillRemove(removeData);
                    Remove(removeData);
                    // Debug.LogFormat("第{0}行，第{1}列,重复数量{2}",r,c,typeCount);
                }
            }
            //Debug.LogFormat("第{0}列，第{1}行,重复数量{2}", c, mapData.GetLength(0), 1);
        }
    }

    #endregion

    #region Update更新逻辑

    private float moveDistance = 0;

    private void CalculateDropCount()
    {
        for (int c = 0; c < column; c++)
        {
            for (int r = 0; r < raw; r++)
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

    protected void MoveMap()
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

    private void DropSquare()
    {
        for (int c = 0; c < column; c++)
        {
            for (int r = raw - 1; r >= 0; r--)
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

    public void UpdateMap()
    {
        CalculateDropCount();
        DropSquare();
        CheckRemove();
        MoveMap();
    }

    public bool CheckGameOver()
    {
        return gameOver;
    }

    #endregion

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


