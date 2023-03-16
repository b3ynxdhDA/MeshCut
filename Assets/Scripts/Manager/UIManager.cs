using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �X�e�[�W���UI���Ǘ�����N���X
/// </summary>
public class UIManager : MonoBehaviour
{
    // �ϐ��錾--------------------------
    // �^�C�}�[����c�莞�Ԃ��擾
    private float _timerCount = 0;

    // �e�L�X�g�I�u�W�F�N�g---------------------------
    // �Q�[���X�^�[�g�̃J�E���g
    [SerializeField] private Text _startCountText = default;

    // �Q�[���I�[�o�[�e�L�X�g
    [SerializeField] private GameObject _gameOverText = default;

    // ���U���g�e�L�X�g
    [SerializeField] private GameObject _resultUI = default;

    // �n�C�X�R�A�e�L�X�g
    [SerializeField] private Text _scoreCountText = default;

    // �^�C�}�[�e�L�X�g
    [SerializeField] private Text _timerCountText = default;

    // �萔�錾---------------------
    // 1���Ԃ̕b��
    const int _ONE_MINUTES = 60;
    // �ʏ�̃^�C���X�P�[��
    const float _DEFAULT_TIMESCALE = 1f;
    // �Q�[���I�[�o�[�e�L�X�g�̈ړ����Y���W
    const float _GAMEOVER_TEXT_POSITION_Y = 0;

    private void Start()
    {
        // �^�C���X�P�[��������������
        Time.timeScale = _DEFAULT_TIMESCALE;

        // �Q�[���I�[�o�[�e�L�X�g��������
        _gameOverText.SetActive(false);

        // �Q�[���̏�Ԃ��Q�[������
        GameManager.instance._gameStateProperty = GameManager.GameState.GameRedy;

        // �Q�[���X�^�[�g�̃J�E���g�_�E�����J�n
        StartCoroutine("CountdownCoroutine");
    }
    private void Update()
    {
        // �n�C�X�R�A�̕\����UI�ɔ��f
        _scoreCountText.text = "" + GameManager.instance._nowScore;

        // �Q�[���X�e�[�g���Q�[�����̎�
        if (GameManager.instance._gameStateProperty == GameManager.GameState.GameNow)
        {
            // �^�C�}�[�̍X�V(����)
            _timerCount = GetComponent<GameTimer>().TimerCount;
            // �^�C�}�[�̎c�莞�Ԃ�UI�ɔ��f
            _timerCountText.text = "" + ((int)_timerCount / _ONE_MINUTES).ToString("00") + " : " + ((int)_timerCount % _ONE_MINUTES).ToString("00");

            // �������Ԃ�0��菬�����Ȃ�����
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
        GameManager.instance._audioManager.OnStartCount3_SE();
        yield return new WaitForSeconds(1f);

        _startCountText.text = "2";
        GameManager.instance._audioManager.OnStartCount3_SE();
        yield return new WaitForSeconds(1f);

        _startCountText.text = "1";
        GameManager.instance._audioManager.OnStartCount3_SE();
        yield return new WaitForSeconds(1f);

        _startCountText.text = "GO!";
        GameManager.instance._audioManager.OnStartCountGo_SE();
        yield return new WaitForSeconds(1f);

        _startCountText.text = "";
        _startCountText.gameObject.SetActive(false);
        GameManager.instance._gameStateProperty = GameManager.GameState.GameNow;
    }

    /// <summary>
    /// �Q�[���I�[�o�[���Ă��烊�U���g�܂ł̏���
    /// </summary>
    /// <returns></returns>
    IEnumerator GameOver()
    {
        // �^�C���X�P�[��������������
        Time.timeScale = _DEFAULT_TIMESCALE;

        // �^�C���A�b�v��SE��炷
        GameManager.instance._audioManager.OnGameTimeUp_SE();

        // �Q�[���X�e�[�g��GameOver��
        GameManager.instance._gameStateProperty = GameManager.GameState.GameOver;

        // �Q�[���I�[�o�[�e�L�X�g��\��
        _gameOverText.SetActive(true);

        // �Q�[���I�[�o�[�e�L�X�g�̃|�W�V������0���傫����
        while (_GAMEOVER_TEXT_POSITION_Y < _gameOverText.transform.localPosition.y)
        {
            // �Q�[���I�[�o�[�e�L�X�g�̃|�W�V������������
            _gameOverText.transform.localPosition +=  Vector3.down * 10;
            yield return new WaitForSeconds(0.001f);
        }

        yield return new WaitForSeconds(2f);

        // �Q�[���X�e�[�g��Result��
        GameManager.instance._gameStateProperty = GameManager.GameState.Result;
        
        _resultUI.SetActive(true);
    }
}
