using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ステージやそのUIを管理するクラス
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
    [SerializeField] private GameObject _resultUI = default;

    // ハイスコアテキスト
    [SerializeField] private Text _scoreCountText = default;

    // タイマーテキスト
    [SerializeField] private Text _timerCountText = default;

    // 定数宣言---------------------
    // 1分間の秒数
    const int _ONE_MINUTES = 60;
    // 1回のゲーム時間
    const int _GAME_TIME = 1;

    private void Start()
    {
        // ゲームの状態をゲーム中に
        GameManager.instance.game_State = GameManager.GameState.GameRedy;

        // ゲームスタートのカウントダウンを開始
        StartCoroutine("CountdownCoroutine");

        // 制限時間を設定
        _timerCount = _GAME_TIME;
    }
    private void Update()
    {
        // ハイスコアの表示を更新
        _scoreCountText.text = "" + GameManager.instance._nowScore;

        // 
        if (GameManager.instance.game_State == GameManager.GameState.GameNow)
        {
            // タイマーの更新(増加)
            //_timerCount += Time.deltaTime;
            //_timerCountText.text = "" + ((int)_timerCount / ONE_MINUTES).ToString("00") + " : " + ((int)_timerCount % ONE_MINUTES).ToString("00");

            // タイマーの更新(減少)
            _timerCount -= Time.deltaTime;
            _timerCountText.text = "" + ((int)_timerCount / _ONE_MINUTES).ToString("00") + " : " + ((int)_timerCount % _ONE_MINUTES).ToString("00");

            // 制限時間が0より小さくなったら
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

    /// <summary>
    /// ゲームオーバーしてからリザルトまでの処理
    /// </summary>
    /// <returns></returns>
    IEnumerator GameOver()
    {
        // ゲームステートをGameOverに
        GameManager.instance.game_State = GameManager.GameState.GameOver;

        _gameOverText.SetActive(true);
        //@ゲームオーバーテキストが降りてくるアニメーションを再生

        yield return new WaitForSeconds(3f);

        // ゲームステートをResultに
        GameManager.instance.game_State = GameManager.GameState.Result;
        
        _resultUI.SetActive(true);
    }
}
