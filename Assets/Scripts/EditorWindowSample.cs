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
        //����
        EditorWindowSample window = GetWindow<EditorWindowSample>("�T���v��");

        // �ŏ��T�C�Y�ݒ�
        window.minSize = new Vector2(320, 320);
    }

    void Update()
    {
        
    }
}
