using System.Collections;
using DG.Tweening;
using UnityEngine;

public class BlockSprite : MonoBehaviour
{
    public SpriteRenderer Renderer;

    public int Raw;

    public int Column;

    public int Type;

    public int[,] SquireType;

    public SquareSprite[,] squares; 

    public SquareState State;

    public bool IsRemovingLine;

    public bool IsAnimating{ get { return isAnimating; } }

    private bool isAnimating = false;

    private PlayerBase player;


    public static BlockSprite CreateBlockSprite(int startRaw,int startColumn,int type,int[,] data)
    {
        GameObject go = new GameObject();
        BlockSprite bs = go.AddComponent<BlockSprite>();
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        string sprite = data.GetLength(0) + "-" + data.GetLength(1);
        sr.sortingLayerName = "Game";
        sr.sortingOrder = 2;

        bs.Renderer = sr;
        bs.Renderer.sprite = Resources.Load<Sprite>(sprite);
        bs.Raw = startRaw;
        bs.Column = startColumn;
        bs.SquireType = data;
        bs.State = SquareState.Static;
        bs.Type = type;

        go.transform.localScale = Vector3.one*0.85f;
        return bs;
    }

    public void CreateHideSquareSprite()
    {
        squares = new SquareSprite[SquireType.GetLength(0),SquireType.GetLength(1)];

        float startX = (-SquireType.GetLength(1)*GameSetting.SquareWidth/ 2f + GameSetting.SquareWidth / 2f)/transform.localScale.x;
        float startY = (SquireType.GetLength(0)*GameSetting.SquareWidth/2f - GameSetting.SquareWidth/2f)/transform.localScale.y;
        for (int r = 0; r < SquireType.GetLength(0); r++)
        {
            for (int c = 0; c < SquireType.GetLength(1); c++)
            {
                SquareSprite ss = SquareSprite.CreateSquare(SquireType[r,c],r,c);
                ss.Row = Raw - SquireType.GetLength(0) + r + 1;
                ss.Column = Column + c;
                ss.transform.SetParent(transform);
                ss.transform.localPosition =  new Vector3(startX + c * GameSetting.SquareWidth/transform.localScale.x, startY -  r * GameSetting.SquareWidth/transform.localScale.y, 0);
                ss.name = "Rect[" + r + "," + c + "]";
                ss.gameObject.layer = gameObject.layer;
                ss.State = SquareState.Hide;
                ss.gameObject.SetActive(false);
                ss.Block = this;

                player.SquareMap[ss.Row, ss.Column] = ss;
                squares[r, c] = ss;

                ss.SetPlayer(player);
            }
        }
    }

    public void SetPlayer(PlayerBase player)
    {
        this.player = player;
    }

    public bool HasSupport()
    {
        bool isSupport = false;
        int downRaw = Raw + 1;
        if (downRaw > player.SquareMap.GetLength(0) - 1)
        {
            return true;
        }

        for (int c = Column; c < Column + SquireType.GetLength(1); c++)
        {
            SquareSprite downSquare = player.SquareMap[downRaw, c];
            if (downSquare != null)
            {
                isSupport = true;
                break;
            }
        }
  
        return isSupport;
    }

    public void RemoveLine()
    {
        StartCoroutine(RemoveLineCorotine());
    }

    private IEnumerator RemoveLineCorotine()
    {
        isAnimating = true;

        int lastRaw = SquireType.GetLength(0) - 1;
        Color col = Color.white;
        col.a = 0.5f;


        for (int c = 0; c < SquireType.GetLength(1); c++)
        {
            squares[lastRaw, c].gameObject.SetActive(true);
            squares[lastRaw, c].Renderer.color = col;
            squares[lastRaw, c].transform.SetParent(player.SquareRoot);
            squares[lastRaw, c].Block = null;
        }

        int leftRawCount = SquireType.GetLength(0) - 1;

        if (leftRawCount > 0)
        {
            string sprite = leftRawCount + "-" + SquireType.GetLength(1);
            Renderer.sprite = Resources.Load<Sprite>(sprite);
            transform.localPosition += new Vector3(0, GameSetting.SquareWidth / 2, 0);


            for (int r = 0; r < SquireType.GetLength(0) - 1; r++)
            {
                for (int c = 0; c < SquireType.GetLength(1); c++)
                {
                    squares[r, c].transform.localPosition = squares[r, c].transform.localPosition - new Vector3(0f, GameSetting.SquareWidth / 2f, 0f);
                }
            }
        }
        else
        {
            Renderer.enabled = false;
        }

        yield return 0;

        for (int c = 0; c < SquireType.GetLength(1); c++)
        {
            squares[lastRaw, c].Renderer.color = Color.white;
            yield return new WaitForSeconds(0.5f);
        }
        
        for (int c = 0; c < SquireType.GetLength(1); c++)
        {
            squares[lastRaw, c].State = SquareState.Static;
        }


        if (leftRawCount > 0)
        {
            int[,] tempSquareType = new int[leftRawCount, SquireType.GetLength(1)];
            for (int r = 0; r < tempSquareType.GetLength(0); r++)
            {
                for (int c = 0; c < tempSquareType.GetLength(1); c++)
                {
                    tempSquareType[r, c] = SquireType[r, c];
                }
            }
            SquireType = tempSquareType;

            SquareSprite[,] tempSquares = new SquareSprite[leftRawCount, SquireType.GetLength(1)];

            for (int r = 0; r < tempSquares.GetLength(0); r++)
            {
                for (int c = 0; c < tempSquares.GetLength(1); c++)
                {
                    tempSquares[r, c] = squares[r, c];
                }
            }
            squares = tempSquares;
        }
        else
        {
            player.RemoveBlock(this);
            Destroy(gameObject);
        }
        Raw--;
        isAnimating = false;
    }

    public void Fall()
    {
        isAnimating = true;
        Vector3 targetPos = transform.localPosition - new Vector3(0, GameSetting.SquareWidth, 0);
        transform.DOLocalMove(targetPos, GameSetting.SquareFallTime).SetRelative(false).SetEase(Ease.Linear).OnComplete(() =>
        {
            for (int r = Raw; r > Raw - SquireType.GetLength(0); r--)
            {
                for (int c = Column; c < Column + SquireType.GetLength(1); c++)
                {
                    player.SquareMap[r, c].Row += 1;
                    player.SquareMap[r + 1, c] = player.SquareMap[r, c];
                    player.SquareMap[r, c] = null;
                }
            }
            Raw = Raw + 1;
            isAnimating = false;
        });
    }


    public void UpdateState()
    {
        //Raw = squares[0,0].Row;

        if (isAnimating)
        {
            return;
        }

        switch (State)
        {
            case SquareState.Static:
                if (!HasSupport())
                {
                    State = SquareState.Hung;
                    isAnimating = true;
                    transform.DOScale(transform.localScale, GameSetting.SquareHungTime).OnComplete(() =>
                    {
                        isAnimating = false;
                    });
                }
                break;
            case SquareState.Hung:
                State = SquareState.Fall;
                break;
            case SquareState.Fall:
                if (!HasSupport())
                {
                    Fall();
                }
                else
                {
                    State = SquareState.Static;
                } 
                break;
        }
    }

}
