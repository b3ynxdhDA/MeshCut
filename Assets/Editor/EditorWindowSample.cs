using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EditorWindowSample : EditorWindow
{
    private PreviewScene _previewScene;
    //������
    private bool _isDidlnitialize;

    [MenuItem("GameObject/MeshCut")]
    static void Create()
    {
        //����
        EditorWindowSample window = GetWindow<EditorWindowSample>("MeshCut");

        // �ŏ��T�C�Y�ݒ�
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
    /// �v���r���[�V�[����K�v�ɉ����ď�����
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
        //�v���r���[�V�[�����쐬
        _previewScene = new PreviewScene("Assets/Scenes/Example.unity");
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = Vector3.one * 5;
        sphere.transform.position = Vector3.zero;
        _previewScene.AddGameObject(sphere);
        _isDidlnitialize = true;
    }
    /// <summary>
    /// �G�f�B�^�E�B���h�E���A�N�e�B�u�ɂȂ�����
    /// </summary>
    private void OnEnable()
    {
        InitializeIfNeeded();
    }

    /// <summary>
    /// �G�f�B�^�E�B���h�E����A�N�e�B�u�ɂȂ�����
    /// </summary>
    private void OnDisable()
    {
        //_previewScene.Dispose();
        _isDidlnitialize = false;
    }
}