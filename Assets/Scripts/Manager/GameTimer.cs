using UnityEngine;

/// <summary>
/// �Q�[�����̃^�C�}�[�N���X
/// </summary>
public class GameTimer : MonoBehaviour
{
    // �ϐ��錾--------------------------
    // �^�C�}�[
    private float _timerCount = 0;


    // �萔�錾---------------------
    // 1��̃Q�[������
    const int _GAME_TIME = 180;

    // �v���p�e�B--------------------------------------
    /// <summary>
    /// 
    /// </summary>
    public float TimerCount { get { return _timerCount; } }

    void Start()
    {
        // �������Ԃ�ݒ�
        _timerCount = _GAME_TIME;
    }

    void Update()
    {
        // �Q�[���X�e�[�g���Q�[�����̎�
        if (GameManager.instance._gameStateProperty == GameManager.GameState.GameNow)
        {
            // �^�C�}�[�̍X�V(����)
            _timerCount -= Time.deltaTime;
        }
    }
}
