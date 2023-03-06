using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �X�e�[�W��
/// </summary>
public class StageManager : MonoBehaviour
{
    private StageManager instance = null;
    public StageManager Instance { get { return instance; } set { instance = value; } }

    // �^�C�}�[
    private float _timerCount = 0;

    // �Q�[���X�^�[�g�̃J�E���g
    [SerializeField] private Text _startCountText = default;

    // ���U���g�e�L�X�g
    [SerializeField] private GameObject _resultText = default;

    // �n�C�X�R�A�e�L�X�g
    [SerializeField] private Text _scoreCountText = default;

    // �^�C�}�[�e�L�X�g
    [SerializeField] private Text _timerCountText = default;

    // �萔�錾---------------------
    // 1���Ԃ̕b��
    const int ONE_MINUTES = 60;

    private void Start()
    {
        //���U���g���\����
        //_resultText.gameObject.SetActive(false);

        // �Q�[���̏�Ԃ��Q�[������
        GameManager.instance.game_State = GameManager.GameState.GameRedy;

        // �Q�[���X�^�[�g�̃J�E���g�_�E�����J�n
        StartCoroutine("CountdownCoroutine");
    }
    private void Update()
    {
        //�n�C�X�R�A�̍X�V
        _scoreCountText.text = "" + GameManager.instance._nowScore;

        if (GameManager.instance.game_State == GameManager.GameState.GameNow)
        {
            //�^�C�}�[�̍X�V
            _timerCount += Time.deltaTime;
            _timerCountText.text = "" + ((int)_timerCount / ONE_MINUTES).ToString("00") + " : " + ((int)_timerCount % ONE_MINUTES).ToString("00");
        }
    }

    IEnumerator CountdownCoroutine()
    {
        //_imageMask.gameObject.SetActive(true);
        _startCountText.gameObject.SetActive(true);

        _startCountText.text = "3";
        GameManager.instance._seManager.OnStartCount3_SE();
        yield return new WaitForSeconds(1.0f);

        _startCountText.text = "2";
        GameManager.instance._seManager.OnStartCount3_SE();
        yield return new WaitForSeconds(1.0f);

        _startCountText.text = "1";
        GameManager.instance._seManager.OnStartCount3_SE();
        yield return new WaitForSeconds(1.0f);

        _startCountText.text = "GO!";
        GameManager.instance._seManager.OnStartCountGo_SE();
        yield return new WaitForSeconds(1.0f);

        _startCountText.text = "";
        _startCountText.gameObject.SetActive(false);
        GameManager.instance.game_State = GameManager.GameState.GameNow;
    }
    /// <summary>
    /// ���U���g��\��
    /// </summary>
    public void ResultUI()
    {
        _resultText.gameObject.SetActive(true);
    }
}
