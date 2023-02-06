using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

//MeshCutクラスを拡張
[CustomEditor(typeof(MeshCut))]
/// <summary>
/// MeshCutウィンドウを生成するクラス
/// </summary>
public class MeshCutEditorWindow : EditorWindow
{
    //切断する対象のオブジェクト
    private Object _cutObject = default;
    //切断面の位置
    private Vector3 _cutPosition = default;
    //切断面に貼るマテリアル
    private Material _material = default;

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
        //MeshCut meshCut = target as MeshCut;
        //カットするオブジェクト
        EditorGUILayout.ObjectField("CutObject", _cutObject, typeof(Transform), true);
        //切断面の位置
        EditorGUILayout.Vector3Field("CutPosition", _cutPosition);
        //切断面のマテリアル
        EditorGUILayout.ObjectField("Material", null, typeof(Material), true);

        EditorGUILayout.BeginHorizontal();
        //カット実行ボタン
        if (GUILayout.Button("Cut!!", GUILayout.Width(128), GUILayout.Height(16)))
        {
            UnityEditor.EditorApplication.isPlaying = true;
            Debug.Log("ボタンが押されましたよ.");
        }
        //カット実行ボタン
        if (GUILayout.Button("Save", GUILayout.Width(128), GUILayout.Height(16)))
        {
            UnityEditor.EditorApplication.isPlaying = false;
            Debug.Log("保存しました");
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
#endif
}