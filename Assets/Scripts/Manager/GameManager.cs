using UnityEngine;

/// <summary>
/// ゲームの状態を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    // 変数宣言-----------------------------------------
    [SerializeField,Header("設定のUICanvas")]
    private GameObject _configCanvas = default;

    [HideInInspector]// SEマネージャー
    public AudioManager _audioManager = default;


    // GameManager内のゲームステート
    private GameState _gameState = GameState.Title;

    /// <summary>
    /// ゲームの状態
    /// Title:タイトル
    /// GameRedy:ゲーム開始前
    /// GameNow:ゲーム中
    /// GameOver:死亡後
    /// Result:リザルト
    /// Pause:ポーズ
    /// Config:設定
    /// </summary>
    public enum GameState
    {
        Title,     
        GameRedy,  
        GameNow,   
        GameOver,  
        Result,    
        Pause,     
        Config    
    };

    // 制限時間の変数
    private float _timerCount = 0;
    // タイムオーバーしたか
    private bool _isTimeOver = false;
    // 制限時間が残り僅かか
    private bool _isNearTimeLimit = false;

    // 定数宣言---------------------
    // 1回のゲーム時間
    const int _GAME_TIME = 90;
    // 最終局面の時間
    const float _NEAR_LIMIT_TIME = 30f;

    // プロパティ--------------------------------------
    /// <summary>
    /// 他のクラスから参照されるゲームステートのプロパティ
    /// </summary>
    public GameState GameStateProperty { get; set; } = GameState.Title;

    /// <summary>
    /// ゲームマネージャーインスタンスプロパティ
    /// </summary>
    public static GameManager instance { get; set; }

    /// <summary>
    /// スコアのプロパティ
    /// </summary>
    public int NowScore { get; set; } = 0;
    
    /// <summary>
    /// ゲームの制限時間のプロパティ
    /// </summary>
    public float TimerCount { get { return _timerCount; } }
    
    /// <summary>
    /// タイムオーバーのプロパティ
    /// </summary>
    public bool IsTimeOver { get { return _isTimeOver; } }

    /// <summary>
    /// 制限時間が残り僅かかのプロパティ
    /// </summary>
    public bool IsNearTimeLimit{ get { return _isNearTimeLimit; } }


    private void Awake()
    {
        // SEマネージャーを外部から参照しやすく
        _audioManager = transform.GetComponent<AudioManager>();

        // GameManagerをシングルトンにする
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        // ゲームステートが切り替わった瞬間
        if(_gameState != GameStateProperty)
        {
            // 切り替え後のゲームステートでそれぞれ処理する
            switch (GameStateProperty)
            {
                case GameState.Title:
                    _gameState = GameStateProperty;

                    ChangeStateToTitle();
                    break;
                case GameState.GameRedy:
                    _gameState = GameStateProperty;

                    ChangeStateToGameRedy();
                    break;
                case GameState.GameNow:
                    _gameState = GameStateProperty;

                    ChangeStateToGameNow();
                    break;
                case GameState.GameOver:
                    _gameState = GameStateProperty;

                    ChangeStateToGameOver();
                    break;
                case GameState.Pause:
                    _gameState = GameStateProperty;

                    ChangeStateToPause();
                    break;
                case GameState.Config:
                    _gameState = GameStateProperty;

                    ChangeStateToConfig();
                    break;
                case GameState.Result:
                    _gameState = GameStateProperty;

                    ChangeStateToResult();
                    break;
            }
        }
        // audioSourseにConfigの設定値を反映させる
        _audioManager.CheckVolume();

        // 
        GameTimer();
    }
    /// <summary>
    /// ゲームのタイマーを減らすメソッド
    /// </summary>
    private void GameTimer()
    {
        // ゲームステートがゲーム中の時以外は処理しない
        if (_gameState != GameManager.GameState.GameNow)
        {
            return;
        }
        // タイマーの更新(減少)
        _timerCount -= Time.deltaTime;

        // 制限時間が0より小さいなら
        if (!_isTimeOver && _timerCount < 0)
        {
            _isTimeOver = true;
        }

        // 制限時間が_NEAR_LIMIT_TIME以下か
        if (!_isNearTimeLimit && _timerCount <= _NEAR_LIMIT_TIME)
        {
            _isNearTimeLimit = true;

            // BGMを変える
            _audioManager.NearLimitTime_BGM();
        }
    }
    
    /// <summary>
    /// コンフィグキャンバスを表示
    /// </summary>
    public void CallConfigUI()
    {
        _configCanvas.SetActive(true);
        GameStateProperty = GameState.Config;
    }

    /// <summary>
    /// ゲームの終了
    /// </summary>
    public void OnExit()
    {
#if UNITY_EDITOR
        //エディターの時は再生をやめる
        UnityEditor.EditorApplication.isPlaying = false;
#else
            //アプリケーションを終了する
            Application.Quit();
#endif
    }

    #region ゲームステート切り替え時の処理メソッド
    /// <summary>
    /// ゲームステートがTitleになったとき
    /// </summary>
    private void ChangeStateToTitle()
    {
        // マウスカーソルを表示する
        Cursor.visible = true;
    }
    /// <summary>
    /// ゲームステートがGameRedyになったとき
    /// </summary>
    private void ChangeStateToGameRedy()
    {
        // スコアを初期化
        NowScore = 0;

        // 制限時間を初期化
        _timerCount = _GAME_TIME;

        // 初期化
        _isTimeOver = false;

        // 初期化
        _isNearTimeLimit = false;

        // カーソルをゲーム画面内でのみ動かせるようにする
        Cursor.lockState = CursorLockMode.Confined;

        // マウスカーソルを非表示にする
        Cursor.visible = false;

        // メインBGMを再生
        _audioManager.PlayGame_BGM();
    }
    /// <summary>
    /// ゲームステートがGameNowになったとき
    /// </summary>
    private void ChangeStateToGameNow()
    {
        // マウスカーソルを非表示にする
        Cursor.visible = false;
    }
    /// <summary>
    /// ゲームステートがGameOverになったとき
    /// </summary>
    private void ChangeStateToGameOver()
    {

    }
    /// <summary>
    /// ゲームステートがPauseになったとき
    /// </summary>
    private void ChangeStateToPause()
    {
        // マウスカーソルを表示する
        Cursor.visible = true;
    }
    /// <summary>
    /// ゲームステートがConfigになったとき
    /// </summary>
    private void ChangeStateToConfig()
    {

    }
    /// <summary>
    /// ゲームステートがResultになったとき
    /// </summary>
    private void ChangeStateToResult()
    {
        // マウスカーソルを表示する
        Cursor.visible = true;
        // カーソルロックを解除する
        Cursor.lockState = CursorLockMode.None;
        // リザルトBGMを再生
        _audioManager.PlayResult_BGM();
    }

    #endregion
}
