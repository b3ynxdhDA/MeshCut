using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

//MeshCut�N���X���g��
[CustomEditor(typeof(MeshCut))]
/// <summary>
/// MeshCut�E�B���h�E�𐶐�����N���X
/// </summary>
public class MeshCutEditorWindow : EditorWindow
{
    //�ؒf����Ώۂ̃I�u�W�F�N�g
    private Object _cutObject = default;
    //�ؒf�ʂ̈ʒu
    private Vector3 _cutPosition = default;
    //�ؒf�ʂɓ\��}�e���A��
    private Material _material = default;

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
        //MeshCut meshCut = target as MeshCut;
        //�J�b�g����I�u�W�F�N�g
        EditorGUILayout.ObjectField("CutObject", _cutObject, typeof(Transform), true);
        //�ؒf�ʂ̈ʒu
        EditorGUILayout.Vector3Field("CutPosition", _cutPosition);
        //�ؒf�ʂ̃}�e���A��
        EditorGUILayout.ObjectField("Material", null, typeof(Material), true);

        EditorGUILayout.BeginHorizontal();
        //�J�b�g���s�{�^��
        if (GUILayout.Button("Cut!!", GUILayout.Width(128), GUILayout.Height(16)))
        {
            UnityEditor.EditorApplication.isPlaying = true;
            Debug.Log("�{�^����������܂�����.");
        }
        //�J�b�g���s�{�^��
        if (GUILayout.Button("Save", GUILayout.Width(128), GUILayout.Height(16)))
        {
            UnityEditor.EditorApplication.isPlaying = false;
            Debug.Log("�ۑ����܂���");
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
#endif
}