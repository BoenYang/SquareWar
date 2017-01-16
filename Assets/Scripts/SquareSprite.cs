using DG.Tweening;
using UnityEngine;

public class SquareSprite : MonoBehaviour
{
    public delegate void MoveEndCallBack();

    public delegate void MoveNextNullCallBack(SquareSprite square);

    public int Row;

    public int Column;

    public int Type;

    public int NextNullCount
    {
        get { return nextNullCount; }
    }

    private int nextNullCount;

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
        collider.size = new Vector2(0.7f, 0.7f);
        ss.Column = c;
        ss.Row = r;
        ss.Type = type;
        ss.renderer = sr;
        return ss;
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
        if (Row < 0)
        {
            return;
        }
        mouseUpPos = Input.mousePosition;
        float xDistance = mouseUpPos.x - mouseDownPos.x;

        if (Mathf.Abs(xDistance) > 30 && !isAnimating)
        {
            MapMng.MoveDir dir = xDistance > 0 ? MapMng.MoveDir.Right : MapMng.MoveDir.Left;
            MapMng.Instance.MoveSquare(this, dir);
        }
    }

    public void MoveToPos(Vector3 pos, float time, MoveEndCallBack callBack = null)
    {
        isAnimating = true;
        transform.DOLocalMove(pos, time).SetRelative(false).OnComplete(() =>
        {
            isAnimating = false;
            if (callBack != null)
            {
                callBack();
            }
        });
    }

    public void MoveToNextNullPos(Vector3 pos,float time, MoveNextNullCallBack callBack)
    {
        Row = Row + NextNullCount;
     
        isAnimating = true;
        transform.DOLocalMove(pos, time).SetRelative(false).OnComplete(() =>
        {
            isAnimating = false;
            if (callBack != null)
            {
                callBack(this);
                ResetNextNullCount();
            }
        });
    }

    public void MarkWillRemove()
    {
        isAnimating = true;
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

    public void ResetNextNullCount()
    {
        nextNullCount = 0;
    }

    public void CaluateNextNullCount(SquareSprite[,] mapData)
    {
        int nullCount = 0;
        for (int i = Row + 1; i < mapData.GetLength(0); i++)
        {
            if (mapData[i, Column] == null)
            {
                nullCount++;
            }
        }
        nextNullCount = nullCount;
    }

    public bool CanRemove()
    {
        return !IsAnimating && NextNullCount == 0;
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
}
