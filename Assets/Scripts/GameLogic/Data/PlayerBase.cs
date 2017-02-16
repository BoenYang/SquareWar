using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerBase : MonoBehaviour
{

    public enum PlayerType
    {
        Normal  = 1,
        Robot   = 2,
        PVP     = 3        
    }

    public delegate void GetScoreDelegate(int addScore);

    public delegate void ChainDelegate(int chainCount);

    public delegate void GameOverDelegate();

    //得分事件回调
    public event GetScoreDelegate OnGetScore = null;

    //连消事件回调
    public event ChainDelegate OnChain = null;

    //游戏结束事件回调
    public event GameOverDelegate OnGameOver = null;

    //玩家名称
    public string Name = "";

    //分数
    public int Score = 0;

    //方块地图容器
    public SquareSprite[,] SquareMap = null;

    //游戏总时间
    public float TotalGameTime = 0;

    //方块更节点
    [System.NonSerialized]
    public Transform SquareRoot;

    //是否是机器人判断
    public bool IsRobot { get { return isRobot; } }

    protected bool isRobot = false;

    //缓存行数目，简化代码
    protected int row;

    //缓存列数目，简化代码
    protected int column;

    //已经插入的行数量，插入之后会导致位置的坐标计算不正确，正确计算应该是 insertedRawcout + 当前行
    private int insertedRawCount = 0;

    //移动间隔计时器
    private float moveIntervalTimer;

    //当前地图移动间隔
    public float currentMapMoveInterval;

    //移动间隔减少计时器 大于 GameSetting.SpeedAddInterval 时 currentMapMoveInterval = currentMapMoveInterval -  GameSetting.MoveIntervalSubStep
    private float moveIntervalSubTimer;

    //是否游戏结束
    private bool gameOver = false;

    //0行0列的方块位置
    private Vector3 startPos;

    //地图偏移
    private Vector3 mapOffset = Vector3.zero;

    //底部待插入的方块行
    private List<SquareSprite[]> squareWillInsert = new List<SquareSprite[]>();

    //正在移除的方块数据
    private List<RemoveData> removingDataList = new List<RemoveData>();

    //障碍方块容器
    private List<BlockSprite> blocks = new List<BlockSprite>(); 

    //背景容器，用于保存背景指针，便于在插入时移除第0行，插入最后一行
    private List<SpriteRenderer[]> backgroundMap = new List<SpriteRenderer[]>();

    //移除的背景的SpriteRenderer缓存，用于减少背景GameObject创建次数
    private List<SpriteRenderer> backgroundCache = new List<SpriteRenderer>();


    public static PlayerBase CreatePlayer(PlayerType playerType,Transform root)
    {
        GameObject playerGo = new GameObject(playerType.ToString());

        PlayerBase player = null;

        switch (playerType)
        {
            case PlayerType.Normal:
                player = playerGo.AddComponent<NormalPlayer>();
                break;
            case PlayerType.Robot:
                player = playerGo.AddComponent<RobotPlayer>();
                break;
            case PlayerType.PVP:
                player = playerGo.AddComponent<PVPPlayer>();
                break;
        }

        playerGo.transform.SetParent(root);
        playerGo.transform.localPosition = Vector3.zero;
        playerGo.transform.localScale = Vector3.one;
        playerGo.layer = root.gameObject.layer;

        return player;
    }

    //初始化
    public virtual void InitPlayerMap(int[,] map)
    {
        currentMapMoveInterval = GameSetting.BaseMapMoveInterval;

        row = map.GetLength(0);
        column = map.GetLength(1);
        startPos = new Vector3(-column * GameSetting.SquareWidth / 2f + GameSetting.SquareWidth / 2, row * GameSetting.SquareWidth / 2 - GameSetting.SquareWidth / 2, 0);

        SquareMap = new SquareSprite[row, column];
        backgroundMap = new List<SpriteRenderer[]>();

        SquareRoot = transform;

        transform.localPosition = mapOffset;

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

    //初始化地图遮罩
    public void InitMapMask()
    {
        Sprite sprite = Resources.Load<Sprite>("fk1");

        GameObject mask = new GameObject(Name + "MapMask");
        mask.gameObject.layer = gameObject.layer;
        mask.transform.SetParent(transform.parent);
        mask.transform.localPosition = mapOffset;

        SpriteRenderer sr = mask.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.material = Resources.Load<Material>("Materials/SpriteStencilMask");
        sr.sortingLayerName = "Game";
        sr.sortingOrder = 1;

        float mapWidth = GameSetting.SquareWidth * column * 100;
        float mapHeight = GameSetting.SquareWidth * row * 100;

        //计算遮罩的Sprite 缩放宽，高
        float xScale = mapWidth / sprite.rect.size.x;
        float yScale = mapHeight / sprite.rect.size.y;

        mask.transform.localScale = new Vector3(xScale, yScale, 0);
    }

    #region 插入行算法

    //是否需要插入行
    public bool NeedAddNewRaw()
    {
        return squareWillInsert.Count == 0;
    }

    /// <summary>
    /// 在指定行处插入行
    /// </summary>
    /// <param name="insertRowIndex">插入位置索引</param>
    /// <param name="squareData">插入方块指针</param>
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

        //移除第9行背景加入到缓存
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

    //在底部插入行
    public void InsertRowAtBottom()
    {
        //检查是否有待插入行
        if (squareWillInsert.Count == 0)
        {
            return;
        }

        //检查是否到顶，到顶插入则游戏结束
        if (IsReachTop())
        {
            GameOver();
            return;
        }

        
        insertedRawCount++;
        InsertRowAtIndex(row - 1,squareWillInsert[0]);

        //插入行之后障碍方块向上移动，行号-1
        for (int i = 0; i < blocks.Count; i++)
        {
            blocks[i].Raw--;
        }

        squareWillInsert.RemoveAt(0);
    }

    //增加待插入行
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

    //创建方块背景
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

            sr.material = Resources.Load<Material>("Materials/SpriteWithStencil");

            sprite.layer = SquareRoot.gameObject.layer;
            sprite.transform.SetParent(SquareRoot);
            sprite.transform.localScale = Vector3.one * 0.8f;
        }

        sprite.transform.localPosition = GetPos(r, c);
        sr.sprite = bg;
        return sr;
    }

    /// <summary>
    /// 在左上方插入障碍方块
    /// </summary>
    /// <param name="data">方块类型数组</param>
    /// <param name="type">障碍方块类型</param>
    public void InsertBlockAtTopLeft(int[,] data,int type)
    {

        int insertRaw = data.GetLength(0) - 1;
        int insertColumn = 0;
        int dataColumnCount = data.GetLength(1);

        for (int c = 0; c < dataColumnCount; c++)
        {
            for (int r = 0; r <= insertRaw; r++)
            {
                if (SquareMap[r, c] != null)
                {
                    GameOver();
                    return;
                }
            }
        }

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


    /// <summary>
    /// 在右上方插入障碍方块
    /// </summary>
    /// <param name="data">方块类型数组</param>
    /// <param name="type">障碍方块类型</param>
    public void InsertBlockAtTopRight(int[,] data,int type)
    {

        int insertRaw = data.GetLength(0) - 1;
        int insertColumn = SquareMap.GetLength(1) - data.GetLength(1);
        int dataColumnCount = data.GetLength(1);

        for (int c = insertColumn; c < column; c++)
        {
            for (int r = 0; r <= insertRaw; r++)
            {
                if (SquareMap[r, c] != null)
                {
                    GameOver();
                    return;
                }
            }
        }

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

    /// <summary>
    /// 移除方块
    /// </summary>
    /// <param name="block"></param>
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
            s2.MoveToPos(moveToPos, GameSetting.SquareSwapTime, () =>
            {
            });
        }
    }

    #endregion

    #region 消除核心算法

    //当前帧移除的方块列表，用于计算分数
    private List<SquareSprite> removingSquareList = new List<SquareSprite>();

    //连消数
    private int chainCount = 1;

    //连消计时器
    private float chainTimer;

    //连消间隔
    private float chainInterval;

    private void CheckRemove()
    {
        removingSquareList.Clear();
        //removingDataList.Clear();
        CheckHorizontalRemove();
        CheckVerticalRemove();
        CalculateScore();
        UpdateChainTimer();
    }

    /// <summary>
    /// 消除核心算法
    /// </summary>
    private void CheckRemove2()
    {
        removingSquareList.Clear();
        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < column; c++)
            {
                SquareSprite square = SquareMap[r, c];
                if (square != null && square.CanSwap())
                {
                    if (square.RightSameTypeSquareCount >= 3)
                    {
                        RemoveData removeData = new RemoveData();
                        removeData.StartRow = r;
                        removeData.StartColumn = c;
                        removeData.Count = square.RightSameTypeSquareCount;
                        removeData.Dir = RemoveDir.Horizontal;
                        RemoveConnectedBlock(removeData);
                        Remove(removeData);
                    }
                }
            }
          
        }

        for (int c = 0; c < column; c++)
        {
            for (int r = 0; r < row; r++)
            {
                SquareSprite square = SquareMap[r, c];
                if (square != null && square.CanSwap())
                {
                    if (square.UnderSameTypeSquareCount >= 3)
                    {

                        RemoveData removeData = new RemoveData();
                        removeData.StartRow = r;
                        removeData.StartColumn = c;
                        removeData.Count = square.UnderSameTypeSquareCount;
                        removeData.Dir = RemoveDir.Vertical;
                        RemoveConnectedBlock(removeData);
                        Remove(removeData);
                    }
                }
            }
        }

        CalculateScore();
        UpdateChainTimer();
    }

    //更新连消计时器
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

    //计算分数
    private void CalculateScore()
    {
        if (removingSquareList.Count > 0)
        {
            int scoreGain = (removingSquareList.Count - 1);
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

    //检查横向消除
    private void CheckHorizontalRemove()
    {
        //检测水平方向消除
        for (int r = 0; r < row; r++)
        {
            //搜索开始的第一个方块
            int firstType = 0;
            //同类型方块数目
            int typeCount = 0;
            for (int c = 0; c < column - 1; c++)
            {
                //从左到右，横向遍历不为空的方块
                if (SquareMap[r, c] == null || !SquareMap[r,c].CanHorizontalRemove())
                {
                    continue;
                }
                else
                {
                    firstType = SquareMap[r, c].Type;
                    typeCount = 1;
                }

                //从不为空的方块后一个方块开始搜索
                int i = c + 1;
                for (i = c + 1; i < column; i++)
                {
                    SquareSprite checkingSuqare = SquareMap[r, i];
                    //方块跟开始查找的第一个方块类型相同，并且横向可以被移除，则增加同类型方块数目
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

    //检查纵向消除
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

    //移除与消除方块相连接的障碍方块
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

            //获取上下左右的方块
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

    /// <summary>
    /// 检查方块是否是在障碍方块之中
    /// </summary>
    /// <param name="squareInBlock">检查的方块</param>
    private void RemoveBlock(SquareSprite squareInBlock)
    {
        if (squareInBlock != null && 
            squareInBlock.State == SquareState.Hide && 
            squareInBlock.Block != null &&  
            !squareInBlock.Block.IsAnimating)
        {
            //障碍方块消除一行
            squareInBlock.Block.RemoveLine();
        }
    }

    private void Remove(RemoveData removeData)
    {
        removingDataList.Add(removeData);

        //移除方块指针存储到List列表中，防止移除过程中增加行引起方块的列和行变化导致根据行,列获取的方块不正确
        removeData.ConvertToList(SquareMap);
        //标记方块为移除，避免重复检测
        MarkWillRemove(removeData);

        //移除方块协程
        StartCoroutine(RemoveCorutine(removeData));
    }

    /// <summary>
    /// 设置要移除的方块状态为clear
    /// </summary>
    /// <param name="removeData"></param>
    private void MarkWillRemove(RemoveData removeData)
    {
        bool chain = false;

        for (int i = 0; i < removeData.RemoveList.Count; i++)
        {
            SquareSprite squareNeedRemove = removeData.RemoveList[i];
            squareNeedRemove.MarkWillRemove();
            if (squareNeedRemove.Chain)
            {
                chain = true;
            }
            if (!removingSquareList.Contains(squareNeedRemove))
            {
                removingSquareList.Add(squareNeedRemove);
            }
        }

        //计算连消
        if (chain)
        {
            chainCount++;
            chainInterval = removeData.Count * 2;
            chainTimer = 0f;
            if (OnChain != null)
            {
                OnChain(chainCount);
            }
        }
    }

    //移除协程
    private IEnumerator RemoveCorutine(RemoveData removeData)
    {
        //在协程中一个一个移除
        yield return new WaitForSeconds(GameSetting.RemoveSquareDelay);

        //播放移除方块特效
        for (int i = 0; i < removeData.RemoveList.Count; i++)
        {
            SquareSprite squareNeedRemove = removeData.RemoveList[i];
            if (squareNeedRemove != null)
            {
                squareNeedRemove.ShowRemoveEffect();
            }
            //yield return new WaitForSeconds(GameSetting.SquareRemoveInterval);
        }

        yield return new WaitForSeconds(GameSetting.SquareRemoveInterval);

        //销毁移除的方块，并在地图上将对应位置置空
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
        removingDataList.Remove(removeData);
    }

    /// <summary>
    /// 统一每个方块的下方和右方有多少个相连接的类型同的方块，用于消除算法，包括其自身
    /// </summary>
    private void UpdateSquareStatistic()
    {
        //从下到上，从右到做遍历
        for (int r = row - 1; r >= 0; r--)
        {
            for (int c = column - 1; c >= 0; c--)
            {
                SquareSprite square = SquareMap[r, c];
                if (square != null)
                {
                    if (square.CanSwap())   //可交换
                    {
                        if (r < row - 1) 
                        {
                            SquareSprite under = SquareMap[r + 1, c];
                            //类型相等且可交换
                            if (under != null && square.Type == under.Type && under.CanSwap())
                            {
                                square.UnderSameTypeSquareCount = under.UnderSameTypeSquareCount + 1;
                            }
                            else
                            {
                                square.UnderSameTypeSquareCount = 1;
                            }
                        }
                        else //最下方的方块，相同数目方块始终为1
                        {
                            square.UnderSameTypeSquareCount = 1;
                        }

                        if (c < column - 1)
                        {
                            SquareSprite right = SquareMap[r, c + 1];
                            //类型相等且可交换
                            if (right != null && square.Type == right.Type && right.CanSwap())
                            {
                                square.RightSameTypeSquareCount = right.RightSameTypeSquareCount + 1;
                            }
                            else
                            {
                                square.RightSameTypeSquareCount = 1;
                            }
                        }
                        else  //最右方的方块，相同数目方块始终为1
                        {
                            square.RightSameTypeSquareCount = 1;
                        }
                    }
                    else //不可交换则全置1
                    {
                        square.RightSameTypeSquareCount = 1;
                        square.UnderSameTypeSquareCount = 1;
                    }
                }
             
            }
        }
    }

    #endregion

    #region Update更新逻辑

    //移动的距离
    private float moveDistance = 0;

    private float currentMoveIntervalRecord;

    //更新所有方块和障碍方块的状态
    protected virtual void UpdateState()
    {
        for (int r = row - 1; r >= 0; r--)
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

    //地图移动
    protected virtual void MoveMap()
    {
        if (removingDataList.Count != 0)
        {
            return;
        }

        TotalGameTime += Time.deltaTime;
        moveIntervalTimer += Time.deltaTime;
        moveIntervalSubTimer += Time.deltaTime;

        //游戏时间到达加速间隔时间，移动间隔减少进行加速
        if (moveIntervalSubTimer > GameSetting.SpeedAddInterval)
        {
            moveIntervalSubTimer = 0;
            //速度封顶
            if (currentMapMoveInterval - GameSetting.MoveIntervalSubStep > GameSetting.MinSquareMoveInterval)
            {
                currentMapMoveInterval -= GameSetting.MoveIntervalSubStep;
            }
        }

        //每隔一段时间移动一次
        if (moveIntervalTimer >= currentMapMoveInterval)
        {
            //方块到顶且超过大于2分之一个方块个时候结束
            if (IsReachTop() && moveDistance > GameSetting.SquareWidth / 2f)
            {
                GameOver();
            }


            moveIntervalTimer = 0;
            moveDistance += GameSetting.BaseMapMoveSpeed;
            Vector3 curPos = SquareRoot.localPosition;
            SquareRoot.localPosition = curPos + new Vector3(0, GameSetting.BaseMapMoveSpeed, 0);

            //每移动一个方块距离后增加一行
            if (Mathf.Abs(moveDistance - GameSetting.SquareWidth) <= 0.000001f)
            {
                moveDistance = 0;
                InsertRowAtBottom();
            }
        }
    }

    //更新逻辑
    public virtual void PlayerUpdate()
    {
        UpdateSquareStatistic();
        CheckRemove2();
        UpdateState();
        MoveMap();
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    //是否到达顶部
    public bool IsReachTop()
    {
        //第0行有任何方块不为空则到达顶部
        for (int c = 0; c < column; c++)
        {
            if (SquareMap[0, c] != null)
            {
                return true;
            }
        }
        return false;
    }

    public void StartAddSpeed()
    {
        currentMoveIntervalRecord = currentMapMoveInterval;
        currentMapMoveInterval = GameSetting.MinSquareMoveInterval;
    }

    public void StopAddSpeed()
    {
        currentMapMoveInterval = currentMoveIntervalRecord;
    }

    #endregion

    public void SetMapPos(Vector3 pos)
    {
        this.mapOffset = pos;
    }

    private Vector3 GetPos(int r, int c)
    {
        return startPos + new Vector3(c * GameSetting.SquareWidth, -r * GameSetting.SquareWidth, 0);
    }

    private void GameOver()
    {
        gameOver = true;
        if (OnGameOver != null)
        {
            OnGameOver();
        }
    }
}



