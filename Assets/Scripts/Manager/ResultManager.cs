using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���U���g���Ǘ�����N���X
/// </summary>
public class ResultManager : MonoBehaviour
{
    // �ϐ��錾--------------------------
    // �X�R�A�̔z��
    private int[] _score = new int[5];
    // �X�R�A�̕ۑ����̔z��
    private string[] _save = { "Score0", "Score1", "Score2", "Score3", "Score4" };
    // ����̃Q�[���̃X�R�A
    private int _nowScore = GameManager.instance._nowScore;



    void Start()
    {
        // �ۑ�����Ă���X�R�A�����[�h����
        for (int i = 0; i < _score.Length; i++)
        {
            _score[i] = PlayerPrefs.GetInt(_save[i], 0);
        }
    }

    void Update()
    {
        int tmp;
        // ����̃X�R�A�Ɣ�r����
        for (int i = 0; i < _score.Length; i++)
        {
            // �����X�R�A���珇�ɍ���̃X�R�A�Ɣ�ׂ�
            if(_score[i] < _nowScore)
            {
                // @�r��
                tmp = _score[i];
                _score[i] = _nowScore;
            }
        }
    }
}
