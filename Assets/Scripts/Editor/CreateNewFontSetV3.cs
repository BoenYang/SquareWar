using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;

public class CreateNewFontSetV3 : EditorWindow {

    public int _width;
    public int _height;
    public int _size;
    public int _startIndex;
    public int _advance;
    public Font fontSetting;

    public Texture2D origialTexture;

    [MenuItem("字体生成/Custom Font V3")]

    public static void Open()
    {
        EditorWindow.GetWindow(typeof(CreateNewFontSetV3));
    }

    void OnGUI()
    {
        _width = EditorGUILayout.IntField("width", _width);
        _height = EditorGUILayout.IntField("height", _height);
        _size = EditorGUILayout.IntField("size", _size);
        _startIndex = EditorGUILayout.IntField("startIndex", _startIndex);
        _advance = EditorGUILayout.IntField("advance", _advance);
        origialTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", origialTexture, typeof(Texture2D), true);
        fontSetting = (Font)EditorGUILayout.ObjectField("Font setting", fontSetting, typeof(Font), true);
        EditorGUILayout.LabelField("width:填入每个字体宽度");
        EditorGUILayout.LabelField("height:填入每个字体高度");
        EditorGUILayout.LabelField("size:字体的总数量");
        EditorGUILayout.LabelField("startIndex:填入起始ASCII码值，例如第一个字体是0则填入48");




        if (GUILayout.Button("开始导入"))
        {
            string fontPath = AssetDatabase.GetAssetPath(fontSetting);
            string texPath = AssetDatabase.GetAssetPath(origialTexture);
            Font font = (Font)AssetDatabase.LoadAssetAtPath(fontPath, typeof(Font));

            if (font == null)
            {
                Debug.Log("wrong path");
                return;
            }
            Debug.Log(texPath);
            Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath(texPath, typeof(Texture2D));
            tex.alphaIsTransparency = true;

            string PathNamePrefix = fontPath;
            PathNamePrefix = PathNamePrefix.Remove(PathNamePrefix.LastIndexOf('.'));

            string MatPathName = PathNamePrefix + "mat.mat";

            Material mat = (Material)AssetDatabase.LoadAssetAtPath(MatPathName, typeof(Material));
            if (mat == null)
            {
                mat = new Material(Shader.Find("UI/Unlit/Transparent"));
                mat.SetTexture("_MainTex", tex);
                AssetDatabase.CreateAsset(mat, MatPathName);
            }
            else
            {
                mat = new Material(Shader.Find("UI/Unlit/Transparent"));
                mat.SetTexture("_MainTex", tex);
            }

            float textureWidth = tex.width;
            float textureheight = tex.height;

            List<CharacterInfo> _infoList = new List<CharacterInfo>();
            for (int i = 0; i < _size; ++i)
            {
                CharacterInfo info = new CharacterInfo();
                info.index = _startIndex + i;
                
                //info.uv.x = _width * i / textureWidth;
               // info.uv.x = (float)i / _size;
                Debug.Log("i:" + i + "; size: " + _size + " i/_size; " + (float)i / _size);

               // info.uv.y = 0;
                //info.uv.width = _width / textureWidth;
               // info.uv.width = (float)1 / _size;
               // info.uv.height = 1.0f;
                //Debug.Log(_width + " " + textureWidth + " " + info.uv.x);
                info.uvBottomLeft = new Vector2((float)i / _size, 0.0f);
                info.uvBottomRight = new Vector2(((float)i/_size) +(float)1/_size , 0.0f);
                info.uvTopLeft = new Vector2((float)i/_size, 1.0f);
                info.uvTopRight = new Vector2(((float)i / _size) + (float)1 / _size, 1.0f);

                info.minX = 0;
                info.minY = -(int)_height / 2;
                info.maxX = (int)_width;
                info.maxY = (int)_height / 2;

                info.advance = _advance;
                _infoList.Add(info);
            }

            font.characterInfo = _infoList.ToArray();
            font.material = mat;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

    }


}
