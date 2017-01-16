using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
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
            { 0,1,0,2,0,3},
            { 0,1,0,1,4,2},
            { 2,2,3,1,5,1},
            { 5,3,4,2,1,2},
            { 1,2,3,4,5,3},
        };
        return mapData;
    }

    public int[] RandomGenerateRaw()
    {
        int[] willInsertRaw = new int[GameSetting.ColumnCount];
        for (int i = 0; i < GameSetting.ColumnCount; i++)
        {
            int type = Random.Range(1, 5);
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
            Players[i].SetMapdata(mapData);
            Players[i].InitMap(this);
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
                Players[i].UpdateMap();
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

    public void OnDrawGizmos()
    {
  
    }

}
