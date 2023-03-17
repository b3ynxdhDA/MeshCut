using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ステージ上のUIを管理するクラス
/// </summary>
public class UIManager : MonoBehaviour
{
    // 変数宣言--------------------------
    // タイマーから残り時間を取得
    private float _timerCount = 0;
    // 制限時間が少ないか
    private bool _isNearTimeLimit = false;

    // テキストオブジェクト---------------------------
    // ゲームオーバーテキスト
    [SerializeField] private GameObject _gameOverText = default;

    // リザルトテキスト
    [SerializeField] private GameObject _resultUI = default;

    // ゲームスタートのカウント
    [SerializeField] private Text _startCountText = default;

    // ハイスコアテキスト
    [SerializeField] private Text _scoreCountText = default;

    // タイマーテキスト
    [SerializeField] private Text _timerCountText = default;

    // 定数宣言---------------------
    // 1分間の秒数
    const int _ONE_MINUTES = 60;
    // 通常のタイムスケール
    const float _DEFAULT_TIMESCALE = 1f;
    // ゲームオーバーテキストの移動後のY座標
    const float _GAMEOVER_TEXT_POSITION_Y = 0;

    private void Start()
    {
        // タイムスケールを初期化する
        Time.timeScale = _DEFAULT_TIMESCALE;

        // ゲームオーバーテキストを初期化
        _gameOverText.SetActive(false);

        // ゲームの状態をゲーム中に
        GameManager.instance.GameStateProperty = GameManager.GameState.GameRedy;

        // ゲームスタートのカウントダウンを開始
        StartCoroutine("CountdownCoroutine");
    }
    private void Update()
    {
        // ゲームステートがゲーム中の時以外は処理しない
        if (GameManager.instance.GameStateProperty != GameManager.GameState.GameNow)
        {
            return;
        }
        // ハイスコアの表示をUIに反映
        _scoreCountText.text = "" + GameManager.instance.NowScore;

        // タイマーの更新(減少)
        _timerCount = GameManager.instance.TimerCount;

        // タイマーの残り時間をUIに反映
        _timerCountText.text = "" + ((int)_timerCount / _ONE_MINUTES).ToString("00") + " : " + ((int)_timerCount % _ONE_MINUTES).ToString("00");

        // 制限時間が少なくなったら
        if (GameManager.instance.IsNearTimeLimit && !_isNearTimeLimit)
        {
            _timerCountText.color = Color.red;
        }

        // 制限時間が0より小さくなったら
        if (GameManager.instance.IsTimeOver)
        {
            StartCoroutine("GameOver");
        }

    }

    /// <summary>
    /// ゲーム開始の３カウントダウンのコルーチン
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
        GameManager.instance.GameStateProperty = GameManager.GameState.GameNow;
    }

    /// <summary>
    /// ゲームオーバーしてからリザルトまでの処理
    /// </summary>
    /// <returns></returns>
    IEnumerator GameOver()
    {
        // タイムスケールを初期化する
        Time.timeScale = _DEFAULT_TIMESCALE;

        // タイムアップのSEを鳴らす
        GameManager.instance._audioManager.OnGameTimeUp_SE();

        // ゲームステートをGameOverに
        GameManager.instance.GameStateProperty = GameManager.GameState.GameOver;

        // ゲームオーバーテキストを表示
        _gameOverText.SetActive(true);

        // ゲームオーバーテキストのポジションが0より大きい間
        while (_GAMEOVER_TEXT_POSITION_Y < _gameOverText.transform.localPosition.y)
        {
            // ゲームオーバーテキストのポジションを下げる
            _gameOverText.transform.localPosition +=  Vector3.down * 10;
            yield return new WaitForSeconds(0.001f);
        }

        yield return new WaitForSeconds(2f);

        // ゲームステートをResultに
        GameManager.instance.GameStateProperty = GameManager.GameState.Result;
        
        _resultUI.SetActive(true);
    }
}
