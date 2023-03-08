using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �X�e�[�W��
/// </summary>
public class StageManager : MonoBehaviour
{
    // �ϐ��錾--------------------------
    // �^�C�}�[
    private float _timerCount = 0;

    // �e�L�X�g�I�u�W�F�N�g---------------------------
    // �Q�[���X�^�[�g�̃J�E���g
    [SerializeField] private Text _startCountText = default;

    // �Q�[���I�[�o�[�e�L�X�g
    [SerializeField] private GameObject _gameOverText = default;

    // ���U���g�e�L�X�g
    [SerializeField] private GameObject _resultText = default;

    // �n�C�X�R�A�e�L�X�g
    [SerializeField] private Text _scoreCountText = default;

    // �^�C�}�[�e�L�X�g
    [SerializeField] private Text _timerCountText = default;

    // �萔�錾---------------------
    // 1���Ԃ̕b��
    const int _ONE_MINUTES = 60;
    // 1��̃Q�[������
    const int _GAME_TIME = 1;
    // �Q�[���I�[�o�[�e�L�X�g�̍ŏI�ʒu
    readonly Vector3 _GAMEOVER_TEXT_POSITION = Vector3.zero;

    private void Start()
    {
        // ���U���g���\����
        //_resultText.gameObject.SetActive(false);

        // �Q�[���̏�Ԃ��Q�[������
        GameManager.instance.game_State = GameManager.GameState.GameRedy;

        // �Q�[���X�^�[�g�̃J�E���g�_�E�����J�n
        StartCoroutine("CountdownCoroutine");

        // �������Ԃ�ݒ�
        _timerCount = _GAME_TIME;
    }
    private void Update()
    {
        // �n�C�X�R�A�̍X�V
        _scoreCountText.text = "" + GameManager.instance._nowScore;

        if (GameManager.instance.game_State == GameManager.GameState.GameNow)
        {
            // �^�C�}�[�̍X�V(����)
            //_timerCount += Time.deltaTime;
            //_timerCountText.text = "" + ((int)_timerCount / ONE_MINUTES).ToString("00") + " : " + ((int)_timerCount % ONE_MINUTES).ToString("00");

            // �^�C�}�[�̍X�V(����)
            _timerCount -= Time.deltaTime;
            _timerCountText.text = "" + ((int)_timerCount / _ONE_MINUTES).ToString("00") + " : " + ((int)_timerCount % _ONE_MINUTES).ToString("00");

            if(_timerCount < 0)
            {
                StartCoroutine("GameOver");
            }
        }
    }

    /// <summary>
    /// �Q�[���J�n�̂R�J�E���g�_�E���̃R���[�`��
    /// </summary>
    IEnumerator CountdownCoroutine()
    {
        _startCountText.gameObject.SetActive(true);

        _startCountText.text = "3";
        GameManager.instance._seManager.OnStartCount3_SE();
        yield return new WaitForSeconds(1f);

        _startCountText.text = "2";
        GameManager.instance._seManager.OnStartCount3_SE();
        yield return new WaitForSeconds(1f);

        _startCountText.text = "1";
        GameManager.instance._seManager.OnStartCount3_SE();
        yield return new WaitForSeconds(1f);

        _startCountText.text = "GO!";
        GameManager.instance._seManager.OnStartCountGo_SE();
        yield return new WaitForSeconds(1f);

        _startCountText.text = "";
        _startCountText.gameObject.SetActive(false);
        GameManager.instance.game_State = GameManager.GameState.GameNow;
    }

    IEnumerator GameOver()
    {
        // �Q�[���X�e�[�g��GameOver��
        GameManager.instance.game_State = GameManager.GameState.GameOver;

        _gameOverText.SetActive(true);

        yield return new WaitForSeconds(3f);

        print("result");
        GameManager.instance.game_State = GameManager.GameState.Result;
    }

    /// <summary>
    /// ���U���g��\��
    /// </summary>
    public void ResultUI()
    {
        _resultText.gameObject.SetActive(true);
    }
}
