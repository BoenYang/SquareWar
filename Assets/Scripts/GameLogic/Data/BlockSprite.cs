using DG.Tweening;
using UnityEngine;

public class BlockSprite : MonoBehaviour
{
    public SpriteRenderer Renderer;

    public int Raw;

    public int Column;

    public int Type;

    public int[,] SquireType;

    public SquareState State;

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

        go.transform.localScale = Vector3.one*0.9f;
        return bs;
    }

    public void CreateHideSquareSprite()
    {
        float startX = -SquireType.GetLength(1)*GameSetting.SquareWidth/2f + GameSetting.SquareWidth/2f;
        for (int r = Raw; r < Raw + SquireType.GetLength(0); r++)
        {
            for (int c = Column; c < Column + SquireType.GetLength(1); c++)
            {
                SquareSprite ss = SquareSprite.CreateSquare(SquireType[r,c],r,c);
                ss.Row = r;
                ss.Column = c;
                ss.transform.SetParent(transform);
                ss.transform.localPosition =  new Vector3(startX + c * GameSetting.SquareWidth,0,0);
                ss.transform.localScale = Vector3.one * 0.9f;
                ss.name = "Rect[" + r + "," + c + "]";
                ss.gameObject.layer = gameObject.layer;
                ss.State = SquareState.Hide;
                ss.gameObject.SetActive(false);
                player.SquareMap[r, c] = ss;
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
            if (downSquare != null && (downSquare.State == SquareState.Static || downSquare.State == SquareState.Hide))
            {
                isSupport = true;
                break;
            }
        }
  
        return isSupport;
    }

    public void RemoveALine()
    {

    }

    public void Fall()
    {
        isAnimating = true;
        Vector3 targetPos = transform.localPosition - new Vector3(0, GameSetting.SquareWidth, 0);
        transform.DOLocalMove(targetPos, 0.05f).SetRelative(false).SetEase(Ease.Linear).OnComplete(() =>
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
                    transform.DOScale(Vector3.one, 0.1f).OnComplete(() =>
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
