using DG.Tweening;
using UnityEngine;

public class SquareSprite : MonoBehaviour
{
    public delegate void MoveEndCallBack();

    public delegate void MoveNextNullCallBack(SquareSprite square);

    public int Row;

    public int Column;

    public int Type;

    public SquareState State;

    public bool Chain;

    public bool HorizontalRemoved = false;

    public bool VerticalRemoved = false;


    private bool IsBottom()
    {
        return Row == (player.SquareMap.GetLength(0) - 1);
    }

    public bool IsAnimating
    {
        get { return isAnimating; }
    }

    private bool isAnimating = false;

    private static Sprite[] sprites;

    private Vector2 mouseDownPos;

    private Vector2 mouseUpPos;

    private SpriteRenderer renderer;

    private Color grayColor = new Color(0.3f,0.3f,0.3f,1.0f);

    private PlayerBase player;

    public static SquareSprite CreateSquare(int type, int r, int c)
    {
        GameObject go = new GameObject();
        SquareSprite ss = go.AddComponent<SquareSprite>();
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        BoxCollider2D collider = go.AddComponent<BoxCollider2D>();

        if (sprites == null)
        {
            sprites = Resources.LoadAll<Sprite>("tiles_foreground");
        }

        sr.sortingLayerName = "Game";
        sr.sortingOrder = 2;
        sr.sprite = sprites[type - 1];
    
        ss.Column = c;
        ss.Row = r;
        ss.Type = type;
        ss.renderer = sr;
        ss.State = SquareState.Static;

        collider.size = new Vector2(0.7f, 0.7f);
        return ss;
    }

    public void SetPlayer(PlayerBase player)
    {
        this.player = player;
    }

    public void OnMouseDown()
    {
        if (Row < 0)
        {
            return;
        }
        mouseDownPos = Input.mousePosition;
    }

    public void OnMouseUp()
    {
        if (Row < 0 || player.IsRobot)
        {
            return;
        }
        mouseUpPos = Input.mousePosition;
        float xDistance = mouseUpPos.x - mouseDownPos.x;

        if (Mathf.Abs(xDistance) > 30 && CanSwap())
        {
            MoveDir dir = xDistance > 0 ? MoveDir.Right : MoveDir.Left;
            player.SwapSquare(this,dir);
        }
    }

    public void MoveToPos(Vector3 pos, float time, MoveEndCallBack callBack = null)
    {
        State = SquareState.Swap;
        isAnimating = true;
        transform.DOLocalMove(pos, time).SetRelative(false).OnComplete(() =>
        {
            State = SquareState.Static;
            isAnimating = false;
            if (callBack != null)
            {
                callBack();
            }
        });
    }

    public void MarkWillRemove()
    {
        State = SquareState.Clear;
    }

    public void Remove()
    {
        isAnimating = false;
        Destroy(gameObject);
    }

    public void ShowRemoveEffect()
    {
        gameObject.SetActive(false);
        isAnimating = true;
    }

    private bool CanRemove()
    {
        return !isAnimating && State != SquareState.Fall && State != SquareState.Swap  && State != SquareState.Hung && State != SquareState.Hide ;
    }

    public bool CanVerticalRemove()
    {
        return (CanRemove() && !VerticalRemoved) ;
    }

    public bool CanHorizontalRemove()
    {
        return (CanRemove() && !HorizontalRemoved);
    }

    public bool CanSwap()
    {
        return !isAnimating && State == SquareState.Static && State != SquareState.Hide;
    }

    public void SetGray(bool gray)
    {
        if (gray)
        {
            renderer.color = renderer.color*grayColor;
        }
        else
        {
            renderer.color = Color.white;
        }
    }

    public void Fall()
    {
        isAnimating = true;
        Vector3 targetPos = transform.localPosition - new Vector3(0, GameSetting.SquareWidth, 0);
        player.SquareMap[Row + 1, Column] = this;
        player.SquareMap[Row, Column] = null;
        transform.DOLocalMove(targetPos, 0.05f).SetRelative(false).SetEase(Ease.Linear).OnComplete(() =>
        {
            Row = Row + 1;
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
            case SquareState.Swap:
                if (IsBottom())
                {
                    State = SquareState.Static;
                    Chain = false;
                }
                else
                {
                    SquareSprite under = player.SquareMap[Row + 1, Column];
                    if (under == null)
                    {
                        State = SquareState.Hung;
                    }
                    else if (under.State == SquareState.Hung)
                    {
                        Chain = under.Chain;
                    }
                    else
                    {
                        Chain = false;
                    }
                }
                break;
            case SquareState.Hung:
                State = SquareState.Fall;
                break;
            case SquareState.Fall:
                if (IsBottom())
                {
                    State = SquareState.Static;
                    Chain = false;
                }
                else
                {
                    SquareSprite under = player.SquareMap[Row + 1, Column];
                    if (under == null)
                    {
                        Chain = true;
                        Fall();
                    }
                    else
                    {
                        if (under.State == SquareState.Clear)
                        {
                            State = SquareState.Static;
                        }
                        else
                        {
                            State = under.State;
                            if (under.Chain)
                            {
                                Chain = under.Chain;
                            }
                        }
                    }
                }
                break;
            case SquareState.Hide:
                break;
        }
    }

}
