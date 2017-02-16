using System.Collections.Generic;
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

    public BlockSprite Block;

    public SpriteRenderer Renderer;

    public int UnderSameTypeSquareCount;

    public int RightSameTypeSquareCount;
    
    private static Dictionary<int,string> effectDict = new Dictionary<int, string>()
    {
        { 1,"green_effect"},
        { 2,"purple_effect"},
        { 3,"orange_effect"},
        { 4,"yellow_effect"},
        { 5,"blue_effect"},
        { 6,"red_effect"},
        { 7,"brown_effect"},
    }; 

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

    private Color grayColor = new Color(0.3f,0.3f,0.3f,1.0f);

    private PlayerBase player;


    public static SquareSprite CreateSquare(int type, int r, int c)
    {
        GameObject go = new GameObject();
        SquareSprite ss = go.AddComponent<SquareSprite>();
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        BoxCollider2D collider = go.AddComponent<BoxCollider2D>();


        sr.sortingLayerName = "Game";
        sr.sortingOrder = 3;
        sr.sprite = Resources.Load<Sprite>("sg" + type);

        sr.material = Resources.Load<Material>("Materials/SpriteWithStencil");

        ss.Column = c;
        ss.Row = r;
        ss.Type = type;
        ss.Renderer = sr;
        ss.State = SquareState.Static;

        collider.size = new Vector2(0.7f, 0.7f);

        go.transform.localScale = Vector3.one*0.8f;

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
        if (State != SquareState.Clear)
        {
            State = SquareState.Clear;
            transform.DOScale(Vector3.one, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
    }

    public void Remove()
    {
        if (Row - 1 > 0)
        {
            SquareSprite above = player.SquareMap[Row - 1, Column];
            if (above != null)
            {
                above.Chain = true;
            }
        }
        isAnimating = false;
        Destroy(gameObject);
    }

    public void ShowRemoveEffect()
    {
        GameObject effectObj = Resources.Load<GameObject>("Effect/" + effectDict[Type]);
        GameObject effect = Instantiate(effectObj, gameObject.transform.position, Quaternion.identity) as GameObject;

        Destroy(effect,1.0f);
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
            Renderer.color = Renderer.color*grayColor;
        }
        else
        {
            Renderer.color = Color.white;
        }
    }

    private void Fall()
    {
        isAnimating = true;
        player.SquareMap[Row + 1, Column] = this;
        player.SquareMap[Row, Column] = null;
        Vector3 targetPos = transform.localPosition - new Vector3(0, GameSetting.SquareWidth, 0);
        transform.DOLocalMove(targetPos, GameSetting.SquareFallTime).SetRelative(false).SetEase(Ease.Linear).OnComplete(() =>
        {
            Row = Row + 1;
            isAnimating = false;
        });
    }

    private void Hung()
    {
        isAnimating = true;
        transform.DOScale(transform.localScale, GameSetting.SquareHungTime).OnComplete(() =>
        {
            isAnimating = false;
        });
    }

    private void Landing()
    {
        //transform.DOScaleY(transform.localScale.x*0.6f, 0.1f).SetLoops(2, LoopType.Yoyo);
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

                        if (Row > 0)
                        {
                            SquareSprite above = player.SquareMap[Row - 1, Column];
                            if (above != null && above.CanSwap())
                            {
                                above.State = SquareState.Hung;
                            }
                        }

                        Hung();
                    }
                    else if (under.State == SquareState.Hung)
                    {
                        Chain = under.Chain;
                        State = SquareState.Hung;
                        Hung();
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
                    Landing();
                }
                else
                {
                    SquareSprite under = player.SquareMap[Row + 1, Column];
                    if (under == null)
                    {
                        Fall();
                    }
                    else if (under.State == SquareState.Clear)
                    {
                        State = SquareState.Static;
                    }
                    else
                    {
                        if (under.State != SquareState.Hide)
                        {
                            State = under.State;
                            if (under.Chain)
                            {
                                Chain = under.Chain;
                            }
                            if (State == SquareState.Static)
                            {
                                Landing();
                            }
                        }
                        else
                        {
                          
                            State = SquareState.Static;
                        }
                    }
                }
                break;
            case SquareState.Hide:
                break;
        }
    }

}

