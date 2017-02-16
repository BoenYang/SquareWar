using System;
using System.Reflection;
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
        win.position = new Rect(200, 200, winWidth, winHeight);
        win.titleContent = new GUIContent("MapData");
        win.Init();
        ((EditorWindow)win).Show();
    }

    public void Init()
    {
        currentSelectPlayerIndex = 0;
    }

    [MenuItem("Tools/Test")]
    public static void SetCameraToZero()
    {
        //        if (SceneView.lastActiveSceneView != null)
        //        {
        //            SceneView.lastActiveSceneView.camera.transform.position = Vector3.zero;
        //            SceneView.lastActiveSceneView.Repaint();
        //        }

        MonoScript ms = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/GameLogic/Data/BlockSprite.cs");//如果路径分隔符是//将无法正确读取文件
        Debug.Log(ms == null);

        GameObject go = Selection.activeGameObject;

    }

    private void AddScriptComponentUncheckedUndoable(GameObject go, MonoScript monoScript)
    {
        Type t = typeof(UnityEditorInternal.InternalEditorUtility);
        MethodInfo methodInfo = t.GetMethod("AddScriptComponentUncheckedUndoable", BindingFlags.NonPublic | BindingFlags.Static);
        methodInfo.Invoke(null, new object[] { go, monoScript });
    }

    [MenuItem("Tools/PrintLocalPosition")]
    public static void PrintLocalPosition()
    {
        GameObject go = Selection.activeGameObject;
        Debug.Log(go.transform.localPosition);
    }

    void OnGUI()
    {
        if (Application.isPlaying)
        {
            if (MapMng.Instance != null && MapMng.Instance.Players != null)
            {
                DrawPlayerPopup();

                DrawSquareSpriteData();
            }
        }
        Repaint();
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

    private void DrawSquareSpriteData()
    {
        SquareSprite[,] squareMap = MapMng.Instance.Players[currentSelectPlayerIndex].SquareMap;

        if (squareMap != null)
        {
            Vector2 startPos = new Vector2(100, startY);

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

            startPos = new Vector2(300, startY);

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
                        string s = squareMap[r, c].Type.ToString();
                        GUI.Label(new Rect(pos, size), new GUIContent(s));
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
                        string s = ((int)squareMap[r, c].State) + "";
                        GUI.Label(new Rect(pos, size), new GUIContent(s));
                    }
                }
            }

            startPos = new Vector2(700, startY);

            width = 25;
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
                        string s = ((int)squareMap[r, c].RightSameTypeSquareCount) + "-" + squareMap[r,c].UnderSameTypeSquareCount;
                        GUI.Label(new Rect(pos, size), new GUIContent(s));
                    }
                }
            }
        }
    }
}
