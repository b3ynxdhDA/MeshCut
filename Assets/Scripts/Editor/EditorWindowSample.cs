using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EditorWindowSample : EditorWindow
{
    private PreviewScene _previewScene;
    //初期化
    private bool _isDidlnitialize;

    [MenuItem("GameObject/MeshCut")]
    static void Create()
    {
        //生成
        EditorWindowSample window = GetWindow<EditorWindowSample>("MeshCut");

        // 最小サイズ設定
        window.minSize = new Vector2(320, 320);
    }

    private void OnGUI()
    {
        InitializeIfNeeded();

        _previewScene.RenderTextureSize = new Vector2Int((int)position.width, (int)position.height);
        _previewScene.Render();
        GUI.DrawTexture(new Rect(0, 0, position.width, position.height), _previewScene.RenderTexture);
    }
    /// <summary>
    /// プレビューシーンを必要に応じて初期化
    /// </summary>
    private void InitializeIfNeeded()
    {

        if (_isDidlnitialize)
        {
            return;
        }
        if (_previewScene != null)
        {
            _previewScene.Dispose();
        }
        //プレビューシーンを作成
        _previewScene = new PreviewScene("Assets/Scenes/Example.unity");
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = Vector3.one * 5;
        sphere.transform.position = Vector3.zero;
        _previewScene.AddGameObject(sphere);
        _isDidlnitialize = true;
    }
    /// <summary>
    /// エディタウィンドウがアクティブになったら
    /// </summary>
    private void OnEnable()
    {
        InitializeIfNeeded();
    }

    /// <summary>
    /// エディタウィンドウが非アクティブになったら
    /// </summary>
    private void OnDisable()
    {
        //_previewScene.Dispose();
        _isDidlnitialize = false;
    }
}