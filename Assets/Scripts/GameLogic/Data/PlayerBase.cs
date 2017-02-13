using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBase
{

    public delegate void GetScoreDelegate(int addScore);

    public delegate void ChainDelegate(int chainCount);

    public string Name = "";

    public int Score = 0;

    public SquareSprite[,] SquareMap = null;

    public bool IsRobot { get { return isRobot; } }

    public Transform SquareRoot;

    public event GetScoreDelegate OnGetScore = null;

    public event ChainDelegate OnChain = null;

    protected bool isRobot = false;

    protected int row;

    protected int column;

    private int insertedRawCount = 0;

    private float moveIntervalTimer;

    private bool gameOver = false;

    private MapMng mapMng;

    private Vector3 startPos;

    private Vector3 mapOffset = Vector3.zero;

    private List<SquareSprite[]> squareWillInsert = new List<SquareSprite[]>();

    private List<RemoveData> removingData = new List<RemoveData>();

    private List<BlockSprite> blocks = new List<BlockSprite>(); 

    private List<SpriteRenderer[]> backgroundMap = new List<SpriteRenderer[]>();

    private List<SpriteRenderer> backgroundCache = new List<SpriteRenderer>(); 

    public virtual void InitPlayerMap(MapMng mapMng, int[,] map)
    {
        this.mapMng = mapMng;

        row = map.GetLength(0);
        column = map.GetLength(1);
        SquareMap = new SquareSprite[row, column];

        backgroundMap = new List<SpriteRenderer[]>();

        GameObject player =  new GameObject();
        player.name = Name;
        player.transform.SetParent(mapMng.gameObject.transform);
        player.transform.localPosition = mapOffset;
        player.transform.localScale = Vector3.one;
        player.gameObject.layer = mapMng.gameObject.layer;

        startPos = new Vector3(-column*GameSetting.SquareWidth/2f + GameSetting.SquareWidth/2, row*GameSetting.SquareWidth/2 - GameSetting.SquareWidth/2, 0);
        SquareRoot = player.transform;
        insertedRawCount = 0;

        for (int r = 0; r < row; r++)
        {
            SpriteRenderer[] rowBackground = new SpriteRenderer[column];
            backgroundMap.Add(rowBackground);

            for (int c = 0; c < column; c++)
            {
                if (map[r, c] != 0)
                {
                    Vector3 pos = GetPos(r, c);
                    SquareSprite ss = SquareSprite.CreateSquare(map[r, c], r, c);
                    ss.transform.SetParent(SquareRoot);
                    ss.transform.localPosition = pos;
                    ss.name = "Rect[" + r + "," + c + "]";
                    ss.SetPlayer(this);
                    SquareMap[r, c] = ss;
                    SquareMap[r, c].gameObject.layer = SquareRoot.gameObject.layer;
                }
                else
                {
                    SquareMap[r, c] = null;
                }

                SpriteRenderer sr = CreateBackground(r, c);
                rowBackground[c] = sr;
            }
        }
        OnChain(chainCount);
    }

    public void SetMapPos(Vector3 pos)
    {
        this.mapOffset = pos;
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
        //从第1行开始往上移动
        for (int r = 1; r <= insertRowIndex; r++)
        {
            for (int c = 0; c < column; c++)
            {
                SquareMap[r - 1, c] = SquareMap[r, c];
                if (SquareMap[r, c] != null)
                {
                    SquareMap[r, c].Row -= 1;
                }
            }
        }

        SpriteRenderer[] removeBackground = backgroundMap[0];
        backgroundMap.RemoveAt(0);

        for (int i = 0; i < removeBackground.Length; i++)
        {
            removeBackground[i].gameObject.SetActive(false);
            backgroundCache.Add(removeBackground[i]);
        }


        //最后一行插入
        for (int i = 0; i < squareData.Length; i++)
        {
            if (squareData[i] != null)
            {
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

        if (IsReachTop())
        {
            gameOver = true;
            return;
        }

        insertedRawCount++;
        InsertRowAtIndex(row - 1,squareWillInsert[0]);

        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].Raw--;
        }

        squareWillInsert.RemoveAt(0);
    }

    public void AddWillInsertRaw(int[] insertRawData)
    {
        if (insertRawData == null || insertRawData.Length != SquareMap.GetLength(1))
        {
            Debug.LogError("数据格式不合法");
            return;
        }

        SpriteRenderer[] rowBackground = new SpriteRenderer[insertRawData.Length];
        SquareSprite[] insertRawSquare = new SquareSprite[SquareMap.GetLength(1)];

        for (int i = 0; i < insertRawData.Length; i++)
        {
            Vector3 pos = GetPos(row + squareWillInsert.Count + insertedRawCount, i);
            insertRawSquare[i] = SquareSprite.CreateSquare(insertRawData[i], -1, i);
            insertRawSquare[i].transform.SetParent(SquareRoot);
            insertRawSquare[i].transform.localPosition = pos;
            insertRawSquare[i].name = "Rect[" + 0 + "," + i + "]";
            insertRawSquare[i].gameObject.layer = SquareRoot.gameObject.layer;
            insertRawSquare[i].SetGray(true);
            insertRawSquare[i].SetPlayer(this);

            SpriteRenderer sr = CreateBackground(row + squareWillInsert.Count + insertedRawCount,i);
            rowBackground[i] = sr;
        }

        backgroundMap.Add(rowBackground);
        squareWillInsert.Add(insertRawSquare);
    }

    private SpriteRenderer CreateBackground(int r,int c)
    {
        Sprite bg = Resources.Load<Sprite>("fk" + (((r + c) % 2) + 1));
        GameObject sprite = null;
        SpriteRenderer sr = null;

        if (backgroundCache.Count > 0)
        {
            sprite = backgroundCache[0].gameObject;
            sprite.SetActive(true);
            sr = backgroundCache[0];
            backgroundCache.RemoveAt(0);
        }
        else
        {
            sprite = new GameObject("sprite");
            sr = sprite.AddComponent<SpriteRenderer>();

            sr.sortingLayerName = "Game";
            sr.sortingOrder = 2;

            //sr.material = Resources.Load<Material>("Materials/SpriteWithStencil");

            sprite.layer = SquareRoot.gameObject.layer;
            sprite.transform.SetParent(SquareRoot);
            sprite.transform.localScale = Vector3.one * 0.8f;
        }

        sprite.transform.localPosition = GetPos(r, c);
        sr.sprite = bg;
        return sr;
    }

    public void InsertBlockAtTopLeft(int[,] data,int type)
    {
        int insertRaw = data.GetLength(0) - 1;
        int insertColumn = 0;
        int dataColumnCount = data.GetLength(1);
        Vector3 pos = GetPos(insertRaw + insertedRawCount, insertColumn) + new Vector3( (dataColumnCount - 1) * GameSetting.SquareWidth/2f, insertRaw * GameSetting.SquareWidth/2f,0);
        BlockSprite bs = BlockSprite.CreateBlockSprite(insertRaw, insertColumn, type, data);
        bs.transform.SetParent(SquareRoot);
        bs.transform.localPosition = pos;
        bs.name = "Block[" + insertRaw + "," + insertColumn + "]";
        bs.gameObject.layer = SquareRoot.gameObject.layer;

        bs.SetPlayer(this);
        bs.CreateHideSquareSprite();
        blocks.Add(bs);
    }

    public void InsertBlockAtTopRight(int[,] data,int type)
    {
        int insertRaw = data.GetLength(0) - 1;
        int insertColumn = SquareMap.GetLength(1) - data.GetLength(1);
        int dataColumnCount = data.GetLength(1);
        Vector3 pos = GetPos(insertRaw  + insertedRawCount, insertColumn) + new Vector3((dataColumnCount - 1) * GameSetting.SquareWidth / 2f, insertRaw * GameSetting.SquareWidth / 2f, 0);
        BlockSprite bs = BlockSprite.CreateBlockSprite(insertRaw, insertColumn, type, data);
        bs.transform.SetParent(SquareRoot);
        bs.transform.localPosition = pos;
        bs.name = "Block[" + insertRaw + "," + insertColumn + "]";
        bs.gameObject.layer = SquareRoot.gameObject.layer;

        bs.SetPlayer(this);
        bs.CreateHideSquareSprite();
        blocks.Add(bs);
    }

    public void RemoveBlock(BlockSprite block)
    {
        blocks.Remove(block);
    }

    #endregion

    #region 方块移动

    public void SwapSquare(SquareSprite movingSquare, MoveDir dir)
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
            if (movingSquare.Column != column)
            {
                targetColumn = movingSquare.Column + 1;
                SwapSquareMap(raw, currentColumn, raw, targetColumn);
            }
        }
    }

    private void SwapSquareMap(int r1, int c1, int r2, int c2)
    {

        SquareSprite s1 = SquareMap[r1, c1];
        SquareSprite s2 = SquareMap[r2, c2];

        //如果要移动到的位置有方块在播放动画，则不能移动
        if (s2 != null && !s2.CanSwap())
        {
            return;
        }

        //Debug.LogFormat("swap {0},{1} <--> {2},{3}", r1, c1, r2, c2);

        if (s1 != null && !s1.IsAnimating)
        {
            s1.Column = c2;
            s1.Row = r2;
            SquareMap[r1, c1] = s2;
            SquareMap[r2, c2] = s1;

            Vector3 moveToPos =  GetPos(r2 + insertedRawCount, c2);
            s1.MoveToPos(moveToPos, GameSetting.SquareSwapTime, () =>
            {
                
            });
        }

        if (s2 != null)
        {
            s2.Column = c1;
            s2.Row = r1;

            Vector3 moveToPos = GetPos(r1 + insertedRawCount, c1);
            s2.MoveToPos(moveToPos, GameSetting.SquareSwapTime);
        }
    }

    #endregion

    #region 消除核心算法

    private List<SquareSprite> removeList = new List<SquareSprite>();

    private int chainCount = 1;

    private float chainTimer;

    private float chainInterval;

    private void CheckRemove()
    {
        removeList.Clear();
        removingData.Clear();
        CheckHorizontalRemove();
        CheckVerticalRemove();
        CalculateScore();
        UpdateChainTimer();
    }

    private void UpdateChainTimer()
    {
        if (chainCount > 1)
        {
            chainTimer += Time.deltaTime;
            if (chainTimer > chainInterval)
            {
                chainCount = 1;
                chainTimer = 0f;
                if (OnChain != null)
                {
                    OnChain(chainCount);
                }
            }
        }
    }

    private void CalculateScore()
    {
        if (removeList.Count > 0)
        {
            int scoreGain = (removeList.Count - 1);
            if (chainCount > 1)
            {
                scoreGain += 6*(chainCount - 1);
            }
            Score += scoreGain;
            if (OnGetScore != null)
            {
                OnGetScore(scoreGain);
            }
        }
       
    }

    private void CheckHorizontalRemove()
    {
        //检测水平方向消除
        for (int r = 0; r < row; r++)
        {
            int firstType = 0;
            int typeCount = 0;
            for (int c = 0; c < column - 1; c++)
            {
                if (SquareMap[r, c] == null || !SquareMap[r,c].CanHorizontalRemove())
                {
                    continue;
                }
                else
                {
                    firstType = SquareMap[r, c].Type;
                    typeCount = 1;
                }

                int i = c + 1;
                for (i = c + 1; i < column; i++)
                {
                    SquareSprite checkingSuqare = SquareMap[r, i];
                    if (checkingSuqare != null && checkingSuqare.Type == firstType && checkingSuqare.CanHorizontalRemove())
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
                            RemoveConnectedBlock(removeData);
                            Remove(removeData);
                        }
                        // Debug.LogFormat("第{0}行，第{1}列,重复数量{2}",r,c,typeCount);
                        break;
                    }
                }

                SquareSprite lastSquare = SquareMap[r, i - 1];
                if (typeCount >= 3 && i == column && lastSquare != null && lastSquare.CanHorizontalRemove())
                {
                    RemoveData removeData = new RemoveData();
                    removeData.StartRow = r;
                    removeData.StartColumn = c;
                    removeData.Count = typeCount;
                    removeData.Dir = RemoveDir.Horizontal;
                    RemoveConnectedBlock(removeData);
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
            for (int r = 0; r < row - 1; r++)
            {
                if (SquareMap[r, c] == null || !SquareMap[r, c].CanVerticalRemove())
                {
                    continue;
                }
                else
                {
                    firstType = SquareMap[r, c].Type;
                    typeCount = 1;
                }

                int i = r + 1;
                for (i = r + 1; i < row; i++)
                {
                    SquareSprite checkingSuqare = SquareMap[i, c];
                    if (checkingSuqare != null && checkingSuqare.Type == firstType && checkingSuqare.CanVerticalRemove())
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
                            Remove(removeData);
                            RemoveConnectedBlock(removeData);
                        }
                        //Debug.LogFormat("第{0}列，第{1}行,重复数量{2}", c, r, typeCount);
                        break;
                    }
                }

                SquareSprite lastSquare = SquareMap[i - 1, c];
                if (typeCount >= 3 && i == row && lastSquare != null && lastSquare.CanVerticalRemove())
                {
                    RemoveData removeData = new RemoveData();
                    removeData.StartRow = r;
                    removeData.StartColumn = c;
                    removeData.Count = typeCount;
                    removeData.Dir = RemoveDir.Vertical;
                    Remove(removeData);
                    RemoveConnectedBlock(removeData);
                    // Debug.LogFormat("第{0}行，第{1}列,重复数量{2}",r,c,typeCount);
                }
            }
            //Debug.LogFormat("第{0}列，第{1}行,重复数量{2}", c, mapData.GetLength(0), 1);
        }
    }

    private void MarkWillRemove(RemoveData removeData)
    {
        bool chain = false;
        for (int i = 0; i < removeData.Count; i++)
        {
            if (removeData.Dir == RemoveDir.Horizontal)
            {
                SquareSprite squareNeedRemove = SquareMap[removeData.StartRow, removeData.StartColumn + i];
                squareNeedRemove.HorizontalRemoved = true;
                squareNeedRemove.MarkWillRemove();
                if (squareNeedRemove.Chain)
                {
                    chain = true;
                }
                if (!squareNeedRemove.VerticalRemoved)
                {
                    removeList.Add(squareNeedRemove);
                }
            }
            else
            {
               
                SquareSprite squareNeedRemove = SquareMap[removeData.StartRow + i, removeData.StartColumn];
                squareNeedRemove.VerticalRemoved =  true;
                squareNeedRemove.MarkWillRemove();
                if (squareNeedRemove.Chain)
                {
                    chain = true;
                }
                if (!squareNeedRemove.HorizontalRemoved)
                {
                    removeList.Add(squareNeedRemove);
                }
            }
        }

        if (chain)
        {
            chainCount++;
            chainInterval = removeData.Count* GameSetting.SquareRemoveInterval;
            chainTimer = 0f;
            if (OnChain != null)
            {
                OnChain(chainCount);
            }
        }
    }

    private void RemoveConnectedBlock(RemoveData removeData)
    {
        for (int i = 0; i < removeData.Count; i++)
        {

            SquareSprite left = null;
            SquareSprite right = null;
            SquareSprite above = null;
            SquareSprite under = null;
            SquareSprite squareNeedRemove = null;
            if (removeData.Dir == RemoveDir.Horizontal)
            {
                squareNeedRemove = SquareMap[removeData.StartRow, removeData.StartColumn + i];
            }
            else
            {
                squareNeedRemove = SquareMap[removeData.StartRow + i, removeData.StartColumn];
            }

            if (squareNeedRemove != null)
            {
                if (squareNeedRemove.Column > 0)
                {
                    left = SquareMap[squareNeedRemove.Row, squareNeedRemove.Column - 1];
                }

                if (squareNeedRemove.Column < column - 1)
                {
                    right = SquareMap[squareNeedRemove.Row, squareNeedRemove.Column + 1];
                }

                if (squareNeedRemove.Row > 0)
                {
                    above = SquareMap[squareNeedRemove.Row - 1, squareNeedRemove.Column];
                }

                if (squareNeedRemove.Row < row - 1)
                {
                    under = SquareMap[squareNeedRemove.Row + 1, squareNeedRemove.Column];
                }
                
                RemoveBlock(left);
                RemoveBlock(right);
                RemoveBlock(above);
                RemoveBlock(under);
            }
        }
    }

    private void RemoveBlock(SquareSprite squareInBlock)
    {
        if (squareInBlock != null && squareInBlock.State == SquareState.Hide && squareInBlock.Block != null &&
            !squareInBlock.Block.IsAnimating)
        {
            squareInBlock.Block.RemoveLine();
        }
    }

    private void Remove(RemoveData removeData)
    {
        removeData.ConvertToList(SquareMap);
        removingData.Add(removeData);
        MarkWillRemove(removeData);
        mapMng.StartCoroutine(RemoveCorutine(removeData));
    }

    private IEnumerator RemoveCorutine(RemoveData removeData)
    {
        //在协程中一个一个移除
        yield return new WaitForSeconds(GameSetting.SquareRemoveInterval);
        for (int i = 0; i < removeData.RemoveList.Count; i++)
        {
            SquareSprite squareNeedRemove = removeData.RemoveList[i];
            if (squareNeedRemove != null)
            {
                squareNeedRemove.ShowRemoveEffect();
            }
            yield return new WaitForSeconds(GameSetting.SquareRemoveInterval);
        }

        for (int i = 0; i < removeData.RemoveList.Count; i++)
        {
            SquareSprite squareNeedRemove = removeData.RemoveList[i];

            if (squareNeedRemove != null)
            {
                squareNeedRemove.Remove();
                SquareMap[squareNeedRemove.Row, squareNeedRemove.Column] = null;
            }
        }

        //消除完毕之后全部清除
        removingData.Remove(removeData);
    }

    #endregion

    #region Update更新逻辑

    private float moveDistance = 0;

    protected virtual void MoveMap()
    {
        if (removingData.Count != 0)
        {
            return;
        }

        moveIntervalTimer += Time.deltaTime;
        if (moveIntervalTimer >= GameSetting.BaseMapMoveInterval)
        {

            if (moveDistance > GameSetting.SquareWidth)
            {
                if (IsReachTop())
                {
                    gameOver = true;
                }
            }

            moveIntervalTimer = 0;
            moveDistance += GameSetting.BaseMapMoveSpeed;
            Vector3 curPos = SquareRoot.localPosition;
            SquareRoot.localPosition = curPos + new Vector3(0, GameSetting.BaseMapMoveSpeed, 0);
            if (Mathf.Abs(moveDistance - GameSetting.SquareWidth) <= 0.000001f)
            {
                moveDistance = 0;
                InsertRowAtBottom();
            }
        }
    }

    protected virtual void UpdateState()
    {
        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                if (SquareMap[r, c] != null)
                {
                    SquareMap[r, c].UpdateState();
                }
            }
        }
        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].UpdateState();
        }
    }

    public virtual void PlayerUpdate()
    {
        UpdateState();
        CheckRemove();
        MoveMap();
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    public bool IsReachTop()
    {
        for (int c = 0; c < column; c++)
        {
            if (SquareMap[0, c] != null)
            {
                return true;
            }
        }
        return false;
    }

    #endregion


}



