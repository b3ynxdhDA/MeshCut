using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ステージの
/// </summary>
public class StageManager : MonoBehaviour
{
    private StageManager instance = null;
    public StageManager Instance { get { return instance; } set { instance = value; } }

    // タイマー
    private float _timerCount = 0;

    // ゲームスタートのカウント
    [SerializeField] private Text _startCountText = default;

    // リザルトテキスト
    [SerializeField] private GameObject _resultText = default;

    // ハイスコアテキスト
    [SerializeField] private Text _scoreCountText = default;

    // タイマーテキスト
    [SerializeField] private Text _timerCountText = default;

    // 定数宣言---------------------
    // 1分間の秒数
    const int ONE_MINUTES = 60;

    private void Start()
    {
        //リザルトを非表示に
        //_resultText.gameObject.SetActive(false);

        // ゲームの状態をゲーム中に
        GameManager.instance.game_State = GameManager.GameState.GameRedy;

        // ゲームスタートのカウントダウンを開始
        StartCoroutine("CountdownCoroutine");
    }
    private void Update()
    {
        //ハイスコアの更新
        _scoreCountText.text = "" + GameManager.instance._nowScore;

        if (GameManager.instance.game_State == GameManager.GameState.GameNow)
        {
            //タイマーの更新
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
    /// リザルトを表示
    /// </summary>
    public void ResultUI()
    {
        _resultText.gameObject.SetActive(true);
    }
}
