using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// MeshCut�E�B���h�E�𐶐�����N���X
/// </summary>
public class MeshCutEditorWindow : EditorWindow
{
    private Object _cutObject = default;

    private Vector3 _cutPosition = default;

    [MenuItem("GameObject/MeshCut")]
    static void Create()
    {
        //����
        MeshCutEditorWindow window = GetWindow<MeshCutEditorWindow>("MeshCut");

        // �ŏ��T�C�Y�ݒ�
        window.minSize = new Vector2(320, 320);
    }

    private void OnGUI()
    {
        //�J�b�g����I�u�W�F�N�g
        _cutObject = EditorGUILayout.ObjectField("CutObject", null, typeof(Transform), true);

        EditorGUILayout.Vector3Field("CutPosition", _cutPosition);

        //�J�b�g���s�{�^��
        if (GUILayout.Button("Cut!!", GUILayout.Width(64), GUILayout.Height(16)))
        {
            Debug.Log("�{�^����������܂�����.");
        }
    }

    /// <summary>
    /// �G�f�B�^�E�B���h�E���A�N�e�B�u�ɂȂ�����
    /// </summary>
    private void OnEnable()
    {

    }

    /// <summary>
    /// �G�f�B�^�E�B���h�E����A�N�e�B�u�ɂȂ�����
    /// </summary>
    private void OnDisable()
    {

    }
}