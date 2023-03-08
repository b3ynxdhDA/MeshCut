using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 設定画面を管理するクラス
/// </summary>
public class ConfigManager : MonoBehaviour
{
    // 変数宣言--------------------------------------------------
    public static ConfigManager instance { get; private set; }

    // コンフィグを閉じたときに遷移するゲームステート
    private GameManager.GameState _backGameState;

    // 音量設定の変数---------------
    // SEの音量を調節するスライダー
    private Slider _masterVolumeSlider = default;

    [Header("SEの音量")]
    public float _masterVolume = 0.7f;

    // ウィンドウサイズの変数----------------------
    // 現在のスクリーンの状態を表示するテキスト
    [SerializeField] GameObject _ChangeScreenText = null;

    // 現在のスクリーンのサイズがFullScreenが
    private int _isFullScreen;

    /// <summary>
    /// 画面サイズの状態
    /// </summary>
    private enum ScreenSize
    {
        Small_1280_720,
        medium_1360_768,
        large_1600_900,
        FullScreen
    }

    // 現在のスクリーンサイズ
    private int _nowScreenSize;

    // 定数宣言-------------------------------------
    // 小スクリーン時のスクリーン幅
    const int _SMALL_WIDHT = 1280;
    // 小スクリーン時のスクリーン幅
    const int _SMALL_HEIGHT = 720;
    // 中スクリーン時のスクリーン幅
    const int _MEDIUM_WIDHT = 1360;
    // 中スクリーン時のスクリーン幅
    const int _MEDIUM_HEIGHT = 768;
    // 大スクリーン時のスクリーン幅
    const int _LARGE_WIDHT = 1600;
    // 大スクリーン時のスクリーン幅
    const int _LARGE_HEIGHT = 900;

    // 小スクリーンサイズ
    const string _SMALL_SIZE = " 1280 x 720 ";
    // 中スクリーンサイズ
    const string _MEDIUM_SIZE = " 1360 x 768 ";
    // 大スクリーンサイズ
    const string _LARGE_SIZE = " 1600 x 900 ";
    // フルスクリーンサイズ
    const string _FULL_SIZE = " Full Screen ";

    void Awake()
    {
        // Sliderコンポーネントを参照
        _masterVolumeSlider = GetComponentInChildren<Slider>();

        // Sliderのvalueを初期化
        _masterVolumeSlider.value = _masterVolume;

        // シングルトン
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
        // コンフィグを呼び出した時のゲームステートを記録
        _backGameState = GameManager.instance.game_State;
    }

    void Update()
    {
        // ボリューム値とスライダーのvalueが違ったら
        if(_masterVolume != _masterVolumeSlider.value)
        {
            _masterVolume = _masterVolumeSlider.value;
        }
    }

    /// <summary>
    /// 現在のスクリーンの大きさを確認して_isFullScreenを合わせる
    /// </summary>
    private void CheckWindowSize()
    {
        // 現在のスクリーンサイズがフルスクリーンサイズより小さいか
        if (Screen.width > _LARGE_WIDHT)
        {
            // スクリーンの状態をFull Screenとして表示
            _ChangeScreenText.GetComponent<Text>().text = _FULL_SIZE;
            _nowScreenSize = (int)ScreenSize.FullScreen;
        }
        else if (Screen.width > _MEDIUM_WIDHT)
        {
            // スクリーンの状態をWindowとして表示
            _ChangeScreenText.GetComponent<Text>().text = _LARGE_SIZE;
            _nowScreenSize = (int)ScreenSize.large_1600_900;
        }
        else if (Screen.width > _SMALL_WIDHT)
        {
            // スクリーンの状態をWindowとして表示
            _ChangeScreenText.GetComponent<Text>().text = _MEDIUM_SIZE;
            _nowScreenSize = (int)ScreenSize.medium_1360_768;
        }
        else
        {
            // スクリーンの状態をWindowとして表示
            _ChangeScreenText.GetComponent<Text>().text = _SMALL_SIZE;
            _nowScreenSize = (int)ScreenSize.Small_1280_720;
        }
    }

    /// <summary>
    /// スクリーンサイズの右矢印ボタンが押されたら
    /// </summary>
    public void ScreenSizeUp()
    {
        // 現在のスクリーンサイズを1段階大きくする
        _nowScreenSize++;
        // _nowScreenSizeがScreenSizeの範囲より大きくなったら
        if(_nowScreenSize > (int)ScreenSize.FullScreen)
        {
            // 1番小さいサイズに直す
            _nowScreenSize = (int)ScreenSize.Small_1280_720;
        }
        ScreenChange();
    }

    /// <summary>
    /// スクリーンサイズの左矢印ボタンが押されたら
    /// </summary>
    public void ScreenSizeDown()
    {
        // 現在のスクリーンサイズを1段階小さくする
        _nowScreenSize--;
        // 
        // _nowScreenSizeがScreenSizeの範囲より小さくなったら
        if (_nowScreenSize < (int)ScreenSize.Small_1280_720)
        {
            // 1番大きいサイズに直す
            _nowScreenSize = (int)ScreenSize.FullScreen;
        }
        ScreenChange();
    }

    /// <summary>
    /// スクリーンの大きさとテキストを切り替える
    /// </summary>
    public void ScreenChange()
    {
        // スクリーンのサイズとテキストを再設定
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
    /// 設定画面から戻るボタン
    /// </summary>
    private void OnBack()
    {
        // ゲームステートを呼び出す前の状態に戻す
        GameManager.instance.game_State = _backGameState;
        this.gameObject.SetActive(false);
    }
}
