using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �ݒ��ʂ��Ǘ�����N���X
/// </summary>
public class ConfigManager : MonoBehaviour
{
    // �ϐ��錾--------------------------------------------------
    public static ConfigManager instance { get; private set; }

    // �R���t�B�O������Ƃ��ɑJ�ڂ���Q�[���X�e�[�g
    private GameManager.GameState _backGameState;

    // ���ʐݒ�̕ϐ�---------------
    // SE�̉��ʂ𒲐߂���X���C�_�[
    private Slider _masterVolumeSlider = default;

    [Header("SE�̉���")]
    public float _masterVolume = 0.7f;

    // �E�B���h�E�T�C�Y�̕ϐ�----------------------
    // ���݂̃X�N���[���̏�Ԃ�\������e�L�X�g
    [SerializeField] GameObject _ChangeScreenText = null;

    // ���݂̃X�N���[���̃T�C�Y��FullScreen��
    private int _isFullScreen;

    /// <summary>
    /// ��ʃT�C�Y�̏��
    /// </summary>
    private enum ScreenSize
    {
        Small_1280_720,
        medium_1360_768,
        large_1600_900,
        FullScreen
    }

    // ���݂̃X�N���[���T�C�Y
    private int _nowScreenSize;

    // �萔�錾-------------------------------------
    // ���X�N���[�����̃X�N���[����
    const int _SMALL_WIDHT = 1280;
    // ���X�N���[�����̃X�N���[����
    const int _SMALL_HEIGHT = 720;
    // ���X�N���[�����̃X�N���[����
    const int _MEDIUM_WIDHT = 1360;
    // ���X�N���[�����̃X�N���[����
    const int _MEDIUM_HEIGHT = 768;
    // ��X�N���[�����̃X�N���[����
    const int _LARGE_WIDHT = 1600;
    // ��X�N���[�����̃X�N���[����
    const int _LARGE_HEIGHT = 900;

    // ���X�N���[���T�C�Y
    const string _SMALL_SIZE = " 1280 x 720 ";
    // ���X�N���[���T�C�Y
    const string _MEDIUM_SIZE = " 1360 x 768 ";
    // ��X�N���[���T�C�Y
    const string _LARGE_SIZE = " 1600 x 900 ";
    // �t���X�N���[���T�C�Y
    const string _FULL_SIZE = " Full Screen ";

    void Awake()
    {
        // Slider�R���|�[�l���g���Q��
        _masterVolumeSlider = GetComponentInChildren<Slider>();

        // Slider��value��������
        _masterVolumeSlider.value = _masterVolume;

        // �V���O���g��
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }

        CheckWindowSize();

        this.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // �R���t�B�O���Ăяo�������̃Q�[���X�e�[�g���L�^
        _backGameState = GameManager.instance.game_State;
    }

    void Update()
    {
        // �{�����[���l�ƃX���C�_�[��value���������
        if(_masterVolume != _masterVolumeSlider.value)
        {
            _masterVolume = _masterVolumeSlider.value;
        }
    }

    /// <summary>
    /// ���݂̃X�N���[���̑傫�����m�F����_isFullScreen�����킹��
    /// </summary>
    private void CheckWindowSize()
    {
        // ���݂̃X�N���[���T�C�Y���t���X�N���[���T�C�Y��菬������
        if (Screen.width > _LARGE_WIDHT)
        {
            // �X�N���[���̏�Ԃ�Full Screen�Ƃ��ĕ\��
            _ChangeScreenText.GetComponent<Text>().text = _FULL_SIZE;
            _nowScreenSize = (int)ScreenSize.FullScreen;
        }
        else if (Screen.width > _MEDIUM_WIDHT)
        {
            // �X�N���[���̏�Ԃ�Window�Ƃ��ĕ\��
            _ChangeScreenText.GetComponent<Text>().text = _LARGE_SIZE;
            _nowScreenSize = (int)ScreenSize.large_1600_900;
        }
        else if (Screen.width > _SMALL_WIDHT)
        {
            // �X�N���[���̏�Ԃ�Window�Ƃ��ĕ\��
            _ChangeScreenText.GetComponent<Text>().text = _MEDIUM_SIZE;
            _nowScreenSize = (int)ScreenSize.medium_1360_768;
        }
        else
        {
            // �X�N���[���̏�Ԃ�Window�Ƃ��ĕ\��
            _ChangeScreenText.GetComponent<Text>().text = _SMALL_SIZE;
            _nowScreenSize = (int)ScreenSize.Small_1280_720;
        }
    }

    /// <summary>
    /// �X�N���[���T�C�Y�̉E���{�^���������ꂽ��
    /// </summary>
    public void ScreenSizeUp()
    {
        // ���݂̃X�N���[���T�C�Y��1�i�K�傫������
        _nowScreenSize++;
        // _nowScreenSize��ScreenSize�͈̔͂��傫���Ȃ�����
        if(_nowScreenSize > (int)ScreenSize.FullScreen)
        {
            // 1�ԏ������T�C�Y�ɒ���
            _nowScreenSize = (int)ScreenSize.Small_1280_720;
        }
        ScreenChange();
    }

    /// <summary>
    /// �X�N���[���T�C�Y�̍����{�^���������ꂽ��
    /// </summary>
    public void ScreenSizeDown()
    {
        // ���݂̃X�N���[���T�C�Y��1�i�K����������
        _nowScreenSize--;
        // 
        // _nowScreenSize��ScreenSize�͈̔͂�菬�����Ȃ�����
        if (_nowScreenSize < (int)ScreenSize.Small_1280_720)
        {
            // 1�ԑ傫���T�C�Y�ɒ���
            _nowScreenSize = (int)ScreenSize.FullScreen;
        }
        ScreenChange();
    }

    /// <summary>
    /// �X�N���[���̑傫���ƃe�L�X�g��؂�ւ���
    /// </summary>
    public void ScreenChange()
    {
        // �X�N���[���̃T�C�Y�ƃe�L�X�g���Đݒ�
        switch (_nowScreenSize)
        {
            case (int)ScreenSize.Small_1280_720:
                Screen.SetResolution(_SMALL_WIDHT, _SMALL_HEIGHT, false);
                _ChangeScreenText.GetComponent<Text>().text = _SMALL_SIZE;
                break;
            case (int)ScreenSize.medium_1360_768:
                Screen.SetResolution(_MEDIUM_WIDHT, _MEDIUM_HEIGHT, false);
                _ChangeScreenText.GetComponent<Text>().text = _MEDIUM_SIZE;
                break;
            case (int)ScreenSize.large_1600_900:
                Screen.SetResolution(_LARGE_WIDHT, _LARGE_HEIGHT, false);
                _ChangeScreenText.GetComponent<Text>().text = _LARGE_SIZE;
                break;
            case (int)ScreenSize.FullScreen:
                Screen.SetResolution(Screen.width, Screen.height, true);
                _ChangeScreenText.GetComponent<Text>().text = _FULL_SIZE;
                break;
        }
    }

    /// <summary>
    /// �ݒ��ʂ���߂�{�^��
    /// </summary>
    private void OnBack()
    {
        // �Q�[���X�e�[�g���Ăяo���O�̏�Ԃɖ߂�
        GameManager.instance.game_State = _backGameState;
        this.gameObject.SetActive(false);
    }
}
