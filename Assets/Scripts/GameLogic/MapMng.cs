using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMng : MonoBehaviour
{
    private static MapMng ins;
    
    public static MapMng Instance
    {
        get
        {
            return ins;
        }
    }

    public List<PlayerBase> Players = null;

    void Awake()
    {
        ins = this;
        Players = null;
    }

    public int[,] RandomGenerateMapData()
    {
        int[,] mapData = new int[,]
        {
            { 0,0,0,0,0,0},
            { 0,0,0,0,0,0},
            { 0,2,0,0,0,0},
            { 0,1,0,0,0,0},
            { 0,1,0,0,0,0},
            { 0,2,0,0,0,0},
            { 0,2,0,0,0,0},
            { 0,1,0,2,0,3},
            { 0,1,0,1,4,2},
            { 1,2,1,6,5,1},
            { 5,6,2,3,6,2},
            { 1,2,3,4,5,3},
        };
        return mapData;
    }

    public int[] RandomGenerateRaw()
    {
        int[] willInsertRaw = new int[GameSetting.ColumnCount];
        for (int i = 0; i < GameSetting.ColumnCount; i++)
        {
            int type = Random.Range(1, GameSetting.SquareTypeCount);
            willInsertRaw[i] = type;
        }
        return willInsertRaw;
    }

    public void SetPlayer(List<PlayerBase> playList)
    {
        Players = playList;
    }

    public void InitMap(GameMode mode)
    {
        int[,] mapData = RandomGenerateMapData();
        int[] willInsertRaw = RandomGenerateRaw();

        for (int i = 0; i < Players.Count; i++)
        {
            Players[i].InitPlayerMap(mapData);
            Players[i].InitMapMask();
            Players[i].AddWillInsertRaw(willInsertRaw);
        }
        StartCoroutine(MapUpdate());
    }

    public void ClearAllPlayer()
    {
        StopAllCoroutines();

        Players = null;

        for (int i = transform.childCount - 1;i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private IEnumerator MapUpdate()
    {
        while (true)
        {
            if (Players != null)
            {
                if (IsGameOver())
                {
                    yield break;
                }

                if (NeedAddNewRow())
                {
                    int[] willInsertRaw = RandomGenerateRaw();
                    for (int i = 0; i < Players.Count; i++)
                    {
                        Players[i].AddWillInsertRaw(willInsertRaw);
                    }
                }

                for (int i = 0; i < Players.Count; i++)
                {
                    Players[i].PlayerUpdate();
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private bool IsGameOver()
    {
        bool gameOver = false;
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].IsGameOver())
            {
                gameOver = true;
                break;
            }
        }
        return gameOver;
    }

    private bool NeedAddNewRow()
    {
        bool need = false;
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].NeedAddNewRaw())
            {
                need = true;
                break;
            }
        }
        return need;
    }

}
