using UnityEngine;

/// <summary>
/// ゲーム中のタイマークラス
/// </summary>
public class GameTimer : MonoBehaviour
{
    // 変数宣言--------------------------
    // タイマー
    private float _timerCount = 0;


    // 定数宣言---------------------
    // 1回のゲーム時間
    const int _GAME_TIME = 180;

    // プロパティ--------------------------------------
    /// <summary>
    /// 
    /// </summary>
    public float TimerCount { get { return _timerCount; } }

    void Start()
    {
        // 制限時間を設定
        _timerCount = _GAME_TIME;
    }

    void Update()
    {
        // ゲームステートがゲーム中の時
        if (GameManager.instance._gameStateProperty == GameManager.GameState.GameNow)
        {
            // タイマーの更新(減少)
            _timerCount -= Time.deltaTime;
        }
    }
}
