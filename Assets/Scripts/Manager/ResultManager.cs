using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���U���g���Ǘ�����N���X
/// </summary>
public class ResultManager : MonoBehaviour
{
    // �ϐ��錾--------------------------
    // �X�R�A�̃e�L�X�g�I�u�W�F�N�g
    private List<Text> _scoreText = new List<Text>();
    // �X�R�A�̔z��
    private List<int> _score = new List<int>();
    // ����̃Q�[���̃X�R�A
    private int _nowScore = 0;
    // �n�C�X�R�A���X�V�����邩
    private bool _isHighScoreUpdate = false;
    // �X�V���ꂽ�X�R�A�̉��ɕ\������uNew�v
    private string _updateNewText = "";

    // �萔�錾---------------------
    // �X�R�A�̕ۑ���
    const string _SCORE_DATE = "ScoreDate";
    // �X�R�A�e�L�X�g��Tag
    const string _SCORE_TAG = "ScoreText";

    private void OnEnable()
    {
        // ����̃X�R�A���擾
        _nowScore = GameManager.instance._nowScore;
        // �X�R�A�X�V�t���O��������
        _isHighScoreUpdate = false;
        // �X�R�A�f�[�^�̔ԍ�
        int i = 0;
        // �L�����o�X�̎q�I�u�W�F�N�g��T������
        foreach (Transform scoreTextObject in this.gameObject.transform)
        {
            // �X�R�A��\������e�L�X�g�I�u�W�F�N�g�������^�O�ŒT��
            if (scoreTextObject.tag == _SCORE_TAG)
            {
                // �X�R�A�e�L�X�g���擾
                _scoreText.Add(scoreTextObject.GetComponent<Text>());

                // �X�R�A�z��ɕۑ�����Ă���X�R�A�����[�h
                _score.Add(PlayerPrefs.GetInt(_SCORE_DATE + i, 0));
                i++;
            }
        }
        CheckScoreUpdate();
    }

    /// <summary>
    /// �X�R�A�̕\�����X�V
    /// </summary>
    private void CheckScoreUpdate()
    {
        // ���ʂ̕ϐ�
        int rankintNumber;
        // ����ւ��p�̈ꎞ�I�ȕϐ�
        int tmp;
        // ����̃X�R�A�Ɣ�r����
        for (int i = 0; i < _score.Count; i++)
        {
            // �����X�R�A���珇�ɍ���̃X�R�A�Ɣ�ׂă\�[�g����
            if (_score[i] < _nowScore)
            {
                // �n�C�X�R�A���X�V����Ă��Ȃ�������
                if (!_isHighScoreUpdate)
                {
                    _isHighScoreUpdate = true;

                    _updateNewText = "New!!";
                }
                // �n�C�X�R�A�����ɍX�V����Ă�����
                else
                {
                    _updateNewText = "";
                }

                // �X�R�A�z����\�[�g
                tmp = _score[i];

                _score[i] = _nowScore;

                _nowScore = tmp;
            }

            // �\�����鏇��
            rankintNumber = i + 1;
            // �X�R�A�e�L�X�g���X�V
            _scoreText[i].text = rankintNumber + ".  " + _score[i].ToString("000000000   ") + _updateNewText;

            // �X�R�A�̍X�V����������
            if (_isHighScoreUpdate)
            {
                // �\�[�g�ōX�V���ꂽ�X�R�A��ۑ�
                PlayerPrefs.SetInt(_SCORE_DATE + i, _score[i]);
            }
        }
        // �X�R�A��ۑ�
        PlayerPrefs.Save();
    }

    /// <summary>
    /// �ۑ�����Ă���X�R�A���폜����
    /// </summary>
    public void OnDeleteScore()
    {
        PlayerPrefs.DeleteAll();
    }
}