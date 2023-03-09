using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �X�e�[�W�₻��UI���Ǘ�����N���X
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
    [SerializeField] private GameObject _resultUI = default;

    // �n�C�X�R�A�e�L�X�g
    [SerializeField] private Text _scoreCountText = default;

    // �^�C�}�[�e�L�X�g
    [SerializeField] private Text _timerCountText = default;

    // �萔�錾---------------------
    // 1���Ԃ̕b��
    const int _ONE_MINUTES = 60;
    // 1��̃Q�[������
    const int _GAME_TIME = 1;

    private void Start()
    {
        // �Q�[���̏�Ԃ��Q�[������
        GameManager.instance.game_State = GameManager.GameState.GameRedy;

        // �Q�[���X�^�[�g�̃J�E���g�_�E�����J�n
        StartCoroutine("CountdownCoroutine");

        // �������Ԃ�ݒ�
        _timerCount = _GAME_TIME;
    }
    private void Update()
    {
        // �n�C�X�R�A�̕\�����X�V
        _scoreCountText.text = "" + GameManager.instance._nowScore;

        // 
        if (GameManager.instance.game_State == GameManager.GameState.GameNow)
        {
            // �^�C�}�[�̍X�V(����)
            //_timerCount += Time.deltaTime;
            //_timerCountText.text = "" + ((int)_timerCount / ONE_MINUTES).ToString("00") + " : " + ((int)_timerCount % ONE_MINUTES).ToString("00");

            // �^�C�}�[�̍X�V(����)
            _timerCount -= Time.deltaTime;
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

    /// <summary>
    /// �Q�[���I�[�o�[���Ă��烊�U���g�܂ł̏���
    /// </summary>
    /// <returns></returns>
    IEnumerator GameOver()
    {
        // �Q�[���X�e�[�g��GameOver��
        GameManager.instance.game_State = GameManager.GameState.GameOver;

        _gameOverText.SetActive(true);
        //@�Q�[���I�[�o�[�e�L�X�g���~��Ă���A�j���[�V�������Đ�

        yield return new WaitForSeconds(3f);

        // �Q�[���X�e�[�g��Result��
        GameManager.instance.game_State = GameManager.GameState.Result;
        
        _resultUI.SetActive(true);
    }
}
