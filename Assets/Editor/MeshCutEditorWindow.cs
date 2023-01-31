using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// MeshCutウィンドウを生成するクラス
/// </summary>
public class MeshCutEditorWindow : EditorWindow
{
    private Object _cutObject = default;

    private Vector3 _cutPosition = default;

    [MenuItem("GameObject/MeshCut")]
    static void Create()
    {
        //生成
        MeshCutEditorWindow window = GetWindow<MeshCutEditorWindow>("MeshCut");

        // 最小サイズ設定
        window.minSize = new Vector2(320, 320);
    }

    private void OnGUI()
    {
        //カットするオブジェクト
        _cutObject = EditorGUILayout.ObjectField("CutObject", null, typeof(Transform), true);

        EditorGUILayout.Vector3Field("CutPosition", _cutPosition);

        //カット実行ボタン
        if (GUILayout.Button("Cut!!", GUILayout.Width(64), GUILayout.Height(16)))
        {
            Debug.Log("ボタンが押されましたよ.");
        }
    }

    /// <summary>
    /// エディタウィンドウがアクティブになったら
    /// </summary>
    private void OnEnable()
    {

    }

    /// <summary>
    /// エディタウィンドウが非アクティブになったら
    /// </summary>
    private void OnDisable()
    {

    }
}