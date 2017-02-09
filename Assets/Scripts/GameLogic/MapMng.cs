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
            if (ins == null)
            {
                GameObject go = new GameObject("MapMng");
                ins = go.AddComponent<MapMng>();
            }
            return ins;
        }
    }

    public List<PlayerBase> Players;

    void Awake()
    {
        ins = this;
    }

    public int[,] RandomGenerateMapData()
    {
        int[,] mapData = new int[,]
        {
            { 0,0,0,0,0,0},
            { 0,0,0,0,0,0},
            { 0,0,0,0,0,0},
            { 0,0,0,0,0,0},
            { 0,0,0,0,0,0},
            { 0,0,0,0,0,0},
            { 0,0,0,0,0,0},
            { 0,1,0,2,0,3},
            { 0,1,0,1,4,2},
            { 1,2,1,1,5,1},
            { 5,3,2,3,1,2},
            { 1,2,3,4,5,3},
        };
        return mapData;
    }

    public int[] RandomGenerateRaw()
    {
        int[] willInsertRaw = new int[GameSetting.ColumnCount];
        for (int i = 0; i < GameSetting.ColumnCount; i++)
        {
            int type = Random.Range(1, 6);
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
            Players[i].InitPlayerMap(this,mapData);
            Players[i].AddWillInsertRaw(willInsertRaw);
        }
        StartCoroutine(MapUpdate());
    }

    private IEnumerator MapUpdate()
    {
        while (true)
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

            yield return new WaitForEndOfFrame();

        }
    }

    private bool IsGameOver()
    {
        bool gameOver = false;
        for (int i = 0; i < Players.Count; i++)
        {
            if (Players[i].CheckGameOver())
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

    void OnGUI()
    {
        GUILayout.Space(20);

        if (GUILayout.Button("测试左边加板", GUILayout.Width(100)))
        {
            Players[0].InsertBlockAtTopLeft(new int[,] { { 1, 2, 3, 4 }, { 4, 2, 3, 1 } },7);
        }

        if (GUILayout.Button("测试右边加板", GUILayout.Width(100)))
        {
            Players[0].InsertBlockAtTopRight(new int[,] { { 1, 2, 3, 4 } }, 7);
        }
    }
}
