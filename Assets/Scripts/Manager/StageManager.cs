using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ステージの
/// </summary>
public class StageManager : MonoBehaviour
{
    // 変数宣言--------------------------
    // タイマー
    private float _timerCount = 0;

    // テキストオブジェクト---------------------------
    // ゲームスタートのカウント
    [SerializeField] private Text _startCountText = default;

    // ゲームオーバーテキスト
    [SerializeField] private GameObject _gameOverText = default;

    // リザルトテキスト
    [SerializeField] private GameObject _resultText = default;

    // ハイスコアテキスト
    [SerializeField] private Text _scoreCountText = default;

    // タイマーテキスト
    [SerializeField] private Text _timerCountText = default;

    // 定数宣言---------------------
    // 1分間の秒数
    const int _ONE_MINUTES = 60;
    // 1回のゲーム時間
    const int _GAME_TIME = 1;
    // ゲームオーバーテキストの最終位置
    readonly Vector3 _GAMEOVER_TEXT_POSITION = Vector3.zero;

    private void Start()
    {
        // リザルトを非表示に
        //_resultText.gameObject.SetActive(false);

        // ゲームの状態をゲーム中に
        GameManager.instance.game_State = GameManager.GameState.GameRedy;

        // ゲームスタートのカウントダウンを開始
        StartCoroutine("CountdownCoroutine");

        // 制限時間を設定
        _timerCount = _GAME_TIME;
    }
    private void Update()
    {
        // ハイスコアの更新
        _scoreCountText.text = "" + GameManager.instance._nowScore;

        if (GameManager.instance.game_State == GameManager.GameState.GameNow)
        {
            // タイマーの更新(増加)
            //_timerCount += Time.deltaTime;
            //_timerCountText.text = "" + ((int)_timerCount / ONE_MINUTES).ToString("00") + " : " + ((int)_timerCount % ONE_MINUTES).ToString("00");

            // タイマーの更新(減少)
            _timerCount -= Time.deltaTime;
            _timerCountText.text = "" + ((int)_timerCount / _ONE_MINUTES).ToString("00") + " : " + ((int)_timerCount % _ONE_MINUTES).ToString("00");

            if(_timerCount < 0)
            {
                StartCoroutine("GameOver");
            }
        }
    }

    /// <summary>
    /// ゲーム開始の３カウントダウンのコルーチン
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
        // ゲームステートをGameOverに
        GameManager.instance.game_State = GameManager.GameState.GameOver;

        _gameOverText.SetActive(true);

        yield return new WaitForSeconds(3f);

        print("result");
        GameManager.instance.game_State = GameManager.GameState.Result;
    }

    /// <summary>
    /// リザルトを表示
    /// </summary>
    public void ResultUI()
    {
        _resultText.gameObject.SetActive(true);
    }
}
