using UnityEditor;
using UnityEngine;

public class MapDataEditor : EditorWindow
{

    private int width = 20;

    private int startY = 30;

    private static float winHeight = 300;

    private static float winWidth = 800;

    private int currentSelectPlayerIndex = 0;

    private string[] playersName;

    [MenuItem("Tools/ShowMapData")]
    public static void Show()
    {
        MapDataEditor win = EditorWindow.GetWindow<MapDataEditor>();
        win.position = new Rect(200,200, winWidth, winHeight);
        win.titleContent = new GUIContent("MapData");
        ((EditorWindow) win).Show();
    }

    [MenuItem("Tools/Test")]
    public static void SetCameraToZero()
    {
        Debug.Log(SceneView.lastActiveSceneView == null);
        if (SceneView.lastActiveSceneView != null)
        {
            Debug.Log(SceneView.lastActiveSceneView.camera.transform.position);
            SceneView.lastActiveSceneView.camera.transform.position = Vector3.zero;
            SceneView.lastActiveSceneView.Repaint();
        }
    }

    void OnGUI()
    {
        if (Application.isPlaying)
        {
            if (MapMng.Instance != null)
            {
                DrawPlayerPopup();
               
                DrawMapData();

                DrawSquareSpriteData();

                Repaint();
            }
        }
    }

    private void DrawPlayerPopup()
    {
        playersName = new string[MapMng.Instance.Players.Count];
        for (int i = 0; i < MapMng.Instance.Players.Count; i++)
        {
            playersName[i] = MapMng.Instance.Players[i].Name;
        }
        currentSelectPlayerIndex = EditorGUI.Popup(new Rect(100, 10, 100, 50), currentSelectPlayerIndex, playersName);
    }

    private void DrawMapData()
    {
        int[,] mapData = MapMng.Instance.Players[currentSelectPlayerIndex].TypeMap;

        if (mapData != null)
        {
            Vector2 startPos = new Vector2(100, startY);

            for (int r = 0; r < mapData.GetLength(0); r++)
            {
                for (int c = 0; c < mapData.GetLength(1); c++)
                {
                    Vector2 pos = startPos + new Vector2(c * width, r * width);
                    Vector2 size = Vector2.one * width;
                    GUI.Label(new Rect(pos, size), new GUIContent("" + mapData[r, c]));
                }
            }
        }
    }

    private void DrawSquareSpriteData()
    {
        SquareSprite[,] squareMap = MapMng.Instance.Players[currentSelectPlayerIndex].SquareMap;

        if (squareMap != null)
        {
            Vector2 startPos = new Vector2(300, startY);

            for (int r = 0; r < squareMap.GetLength(0); r++)
            {
                for (int c = 0; c < squareMap.GetLength(1); c++)
                {
                    Vector2 pos = startPos + new Vector2(c * width, r * width);
                    Vector2 size = Vector2.one * width;

                    if (squareMap[r, c] == null)
                    {
                        GUI.Label(new Rect(pos, size), new GUIContent("-"));
                    }
                    else
                    {
                        GUI.Label(new Rect(pos, size), new GUIContent("" + squareMap[r, c].Type));
                    }
                }
            }

            startPos = new Vector2(500, startY);

            for (int r = 0; r < squareMap.GetLength(0); r++)
            {
                for (int c = 0; c < squareMap.GetLength(1); c++)
                {
                    Vector2 pos = startPos + new Vector2(c * width, r * width);
                    Vector2 size = Vector2.one * width;

                    if (squareMap[r, c] == null)
                    {
                        GUI.Label(new Rect(pos, size), new GUIContent("-"));
                    }
                    else
                    {
                        string s = squareMap[r, c].CanRemove() ? "T" : "F";
                        GUI.Label(new Rect(pos, size), new GUIContent(s));
                    }
                }
            }

            startPos = new Vector2(700, startY);

            for (int r = 0; r < squareMap.GetLength(0); r++)
            {
                for (int c = 0; c < squareMap.GetLength(1); c++)
                {
                    Vector2 pos = startPos + new Vector2(c * width, r * width);
                    Vector2 size = Vector2.one * width;

                    if (squareMap[r, c] == null)
                    {
                        GUI.Label(new Rect(pos, size), new GUIContent("-"));
                    }
                    else
                    {
                        string s = squareMap[r, c].NextNullCount + "";
                        GUI.Label(new Rect(pos, size), new GUIContent(s));
                    }
                }
            }
        }
    }
}
