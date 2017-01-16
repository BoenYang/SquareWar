using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMng : MonoBehaviour
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

    private static MapMng ins;
    
    public static MapMng Instance
    {
        get
        {
            if (ins == null)
            {
                GameObject go = new GameObject("MapMng");
                ins = go.AddComponent<MapMng>();
            }
            return ins;
        }
    }

    public int RowCount = 12;

    public int ColumnCount = 6;

    public float Width = 50;

    public int[,] mapData;

    public SquareSprite[,] squareSpriteMap;

    public float MoveSpeed = 5f;

    public float MoveInterval = 0.2f;

    public float SquareRemoveInterval = 0.5f;

    public List<PlayerBase> Players;

    private Vector3 startPos;

    private Transform trCache;

    private float moveIntervalTimer = 0;

    private int insertedRawCount = 0;

    private int[] willInsertRaw;

    private SquareSprite[] willInsertSquare;

    private bool moveStop;

    private List<RemoveData> removingData = new List<RemoveData>();

    private bool gameOver = false;

    void Awake()
    {
        ins = this;
        startPos = new Vector3(-ColumnCount * Width / 2f + Width / 2, RowCount * Width / 2 - Width / 2, 0);
        trCache = transform;
    }

    public void RandomGenerateMapData()
    {
        mapData = new int[,]
        {
            { 0,0,0,0,0,0},
            { 0,0,0,0,0,0},
            { 0,0,0,0,0,0},
            { 0,0,0,0,0,0},
            { 0,0,0,0,0,0},
            { 0,0,0,0,0,0},
            { 0,1,0,2,0,3},
            { 0,1,0,1,4,2},
            { 2,2,3,1,5,1},
            { 5,3,4,2,1,2},
            { 1,2,3,4,5,3},
        };

        squareSpriteMap = new SquareSprite[RowCount,ColumnCount];
    }

    public void SetPlayer(List<PlayerBase> playList)
    {
        Players = playList;
    }

    public void InitMap(GameMode mode)
    {
        RandomGenerateMapData();
    
        for (int r = 0; r < RowCount; r++)
        {
            for (int c = 0; c < ColumnCount; c++)
            {
                if (mapData[r, c] != 0)
                {
                    Vector3 pos = GetPos(r,c);
                    SquareSprite ss = SquareSprite.CreateSquare(mapData[r, c],r,c);
                    ss.transform.SetParent(trCache);
                    ss.transform.localPosition = pos;
                    ss.transform.localScale = Vector3.one * 0.9f;
                    ss.name = "Rect[" + r + "," + c + "]";
                    squareSpriteMap[r, c] = ss;
                    squareSpriteMap[r, c].gameObject.layer = trCache.gameObject.layer;
                }
                else
                {
                    squareSpriteMap[r, c] = null;
                }
            }
        }
        CreateAInsertRaw();
        StartCoroutine(MapUpdate());
    }

    public void SetMapData(int[,] data)
    {
        mapData = data;
    }

    private void CreateAInsertRaw()
    {
        willInsertRaw = new int[ColumnCount];
        willInsertSquare = new SquareSprite[ColumnCount];
        for (int i = 0; i < ColumnCount; i++)
        {
            Vector3 pos = GetPos(RowCount + insertedRawCount, i);
            int type = Random.Range(1,5);
            willInsertRaw[i] = type;
            willInsertSquare[i] = SquareSprite.CreateSquare(type, -1, i);
            willInsertSquare[i].transform.SetParent(trCache);
            willInsertSquare[i].transform.localPosition = pos;
            willInsertSquare[i].transform.localScale = Vector3.one * 0.9f;
            willInsertSquare[i].name = "Rect[" + 0 + "," + i + "]";
            willInsertSquare[i].SetGray(true);
            willInsertSquare[i].gameObject.layer = trCache.gameObject.layer;
        }
    }

    //在index位置插入一行，将行号小于index的所有行向上移动一行，第0行的数据被丢弃
    public void InsertRowAtIndex(int insertRowIndex,int[] rowData,SquareSprite[] squareData)
    {
        if (rowData == null || rowData.Length > ColumnCount || squareData == null || squareData.Length > ColumnCount)
        {
            Debug.LogError("数据格式不合法");
            return;
        }

        insertedRawCount++;
        for (int r = 1; r <= insertRowIndex; r++)
        {
            for (int c = 0; c < ColumnCount; c++)
            {
                mapData[r - 1, c] = mapData[r, c];
                squareSpriteMap[r - 1, c] = squareSpriteMap[r, c];
                if (squareSpriteMap[r, c] != null)
                {
                    squareSpriteMap[r, c].Row -= 1;
                }
            }
        }

        for (int i = 0; i < rowData.Length; i++)
        {
            mapData[insertRowIndex, i] = rowData[i];
            squareData[i].Row = insertRowIndex;
            squareData[i].SetGray(false);
            squareSpriteMap[insertRowIndex, i] = squareData[i];
        }
    }

    public void InsertRowAtBottom()
    {
        gameOver = false;
        for (int i = 0; i < ColumnCount; i++)
        {
            if (mapData[0, i] != 0)
            {
                gameOver = true;
                return;
            }
        }
        InsertRowAtIndex(RowCount - 1, willInsertRaw, willInsertSquare);
    }

    public void InsertRowAtTop()
    {

    }

    public void MoveSquare(SquareSprite movingSquare , MoveDir dir)
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
            if (movingSquare.Column != ColumnCount)
            {
                targetColumn = movingSquare.Column + 1;
                SwapSquareMap(raw, currentColumn, raw, targetColumn);
            }
        }
    }

    private Vector3 GetPos(int r, int c)
    {
        return startPos + new Vector3(c * Width, -r * Width, 0);
    }

    private void SwapMap(int r1,int c1,int r2,int c2)
    {
        int tempType = mapData[r1, c1];
        mapData[r1, c1] = mapData[r2, c2];
        mapData[r2, c2] = tempType;

        SquareSprite tempSquare = squareSpriteMap[r1, c1];
        squareSpriteMap[r1, c1] = squareSpriteMap[r2, c2];
        squareSpriteMap[r2, c2] = tempSquare;
    }

    private void SwapSquareMap(int r1, int c1, int r2, int c2)
    {
        
        SquareSprite s1 = squareSpriteMap[r1, c1];
        SquareSprite s2 = squareSpriteMap[r2, c2];

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
            s1.MoveToPos(moveToPos,0.1f, () =>
            {
                SwapMap(r1,c1,r2,c2);
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
                SquareSprite squareNeedRemove = squareSpriteMap[removeData.StartRow, removeData.StartColumn + i];
                squareNeedRemove.MarkWillRemove();
            }
            else
            {
                SquareSprite squareNeedRemove = squareSpriteMap[removeData.StartRow + i, removeData.StartColumn];
                squareNeedRemove.MarkWillRemove();
            }
        }
    }

    private void Remove(RemoveData removeData)
    {
        StartCoroutine(RemoveCorutine(removeData));
    }

    private IEnumerator RemoveCorutine(RemoveData removeData)
    {
        yield return new WaitForSeconds(SquareRemoveInterval);
        for (int i = 0; i < removeData.Count; i++)
        {
            if (removeData.Dir == RemoveDir.Horizontal)
            {
                SquareSprite squareNeedRemove = squareSpriteMap[removeData.StartRow, removeData.StartColumn + i];
                squareNeedRemove.ShowRemoveEffect();
            }
            else
            {
                SquareSprite squareNeedRemove = squareSpriteMap[removeData.StartRow + i, removeData.StartColumn];
                squareNeedRemove.ShowRemoveEffect();
            }

            yield return new WaitForSeconds(SquareRemoveInterval);
        }

        removingData.Remove(removeData);
        for (int i = 0; i < removeData.Count; i++)
        {
            if (removeData.Dir == RemoveDir.Horizontal)
            {
                squareSpriteMap[removeData.StartRow, removeData.StartColumn + i].Remove();
                squareSpriteMap[removeData.StartRow, removeData.StartColumn + i] = null;
                mapData[removeData.StartRow, removeData.StartColumn + i] = 0;
            }
            else
            {
                squareSpriteMap[removeData.StartRow + i, removeData.StartColumn].Remove();
                squareSpriteMap[removeData.StartRow + i, removeData.StartColumn] = null;
                mapData[removeData.StartRow + i, removeData.StartColumn] = 0;
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
        for (int r = 0; r < RowCount; r++)
        {
            int firstType = 0;
            int typeCount = 0;
            for (int c = 0; c < ColumnCount - 1; c++)
            {
                if (mapData[r, c] == 0)
                {
                    continue;
                }
                else
                {
                    firstType = mapData[r, c];
                    typeCount = 1;
                }

                int i = c + 1;
                for (i = c + 1; i < ColumnCount; i++)
                {
                    if (mapData[r, i] == firstType && squareSpriteMap[r, i].CanRemove())
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

                if (typeCount >= 3 && i == mapData.GetLength(1) && squareSpriteMap[r, i - 1].CanRemove())
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
        for (int c = 0; c < ColumnCount; c++)
        {
            int firstType = 0;
            int typeCount = 0;
            for (int r = 0; r < RowCount - 1; r++)
            {
                if (mapData[r, c] == 0)
                {
                    continue;
                }
                else
                {
                    firstType = mapData[r, c];
                    typeCount = 1;
                }

                int i = r + 1;
                for (i = r + 1; i < mapData.GetLength(0); i++)
                {
                    if (mapData[i, c] == firstType && squareSpriteMap[i, c].CanRemove())
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

                if (typeCount >= 3 && i == mapData.GetLength(0) && squareSpriteMap[i - 1, c].CanRemove())
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
        for (int c = 0; c < ColumnCount; c++)
        {
            for (int r = 0; r < RowCount; r++)
            {
                if (squareSpriteMap[r,c] == null || squareSpriteMap[r, c].IsAnimating)
                {
                    continue;
                }

                squareSpriteMap[r, c].CaluateNextNullCount(squareSpriteMap);
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
        if (moveIntervalTimer >= MoveInterval)
        {
            moveIntervalTimer = 0;
            moveDistance += MoveSpeed;
            Vector3 curPos = trCache.localPosition;
            trCache.localPosition = curPos + new Vector3(0, MoveSpeed, 0);
            if (Mathf.Abs(moveDistance - Width) <= 0.000001f)
            {
                moveDistance = 0;
                InsertRowAtBottom();
                if (!gameOver)
                {
                    CreateAInsertRaw();
                }
            }
        }
    }

    private void MoveSquare()
    {
        for (int c = 0; c < ColumnCount; c++)
        {
            for (int r = RowCount - 1; r >=0 ; r--)
            {
                SquareSprite movingSquare = squareSpriteMap[r, c];
                if (movingSquare != null && movingSquare.NextNullCount != 0 && !movingSquare.IsAnimating)
                {
                    Vector3 moveToPos = GetPos(r + movingSquare.NextNullCount + insertedRawCount, c);
                
                    movingSquare.MoveToNextNullPos(moveToPos, 0.1f, (SquareSprite s) =>
                    {
                        squareSpriteMap[s.Row - s.NextNullCount, s.Column] = null;
                        mapData[s.Row - s.NextNullCount, s.Column] = 0;
                        mapData[s.Row, s.Column] = s.Type;
                        squareSpriteMap[s.Row, s.Column] = s;
                        //Check();
                    });
                }
            }
        }
    }

    private IEnumerator MapUpdate()
    {
        while (true)
        {
            if (gameOver)
            {
                yield break;
            }

            CalculateDropCount();
            MoveSquare();
            CheckRemove();
            MoveMap();
            yield return new WaitForEndOfFrame();
        }
    }

    public void OnDrawGizmos()
    {
        Vector3 drawStartPos = new Vector3(transform.position.x - ColumnCount * Width / 2f , transform.position.y - RowCount * Width / 2f - insertedRawCount * Width, transform.position.z);

        for (int i = 0; i <= RowCount; i++)
        {
            Vector3 p1 = drawStartPos + new Vector3(0, i * Width, 0);
            Vector3 p2 = drawStartPos + new Vector3(ColumnCount * Width, i * Width, 0);
            Gizmos.DrawLine(p1, p2);
        }

        for (int i = 0; i <= ColumnCount; i++)
        {
            Vector3 p1 = drawStartPos + new Vector3(i * Width, 0, 0);
            Vector3 p2 = drawStartPos + new Vector3(i * Width, RowCount * Width, 0);
            Gizmos.DrawLine(p1, p2);
        }
    }

}
