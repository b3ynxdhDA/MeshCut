using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EditorWindowSample : EditorWindow
{
    [MenuItem("Editor/Sample")]

    private void Create()
    {
        //生成
        EditorWindowSample window = GetWindow<EditorWindowSample>("サンプル");

        // 最小サイズ設定
        window.minSize = new Vector2(320, 320);
    }

    void Update()
    {
        
    }
}
