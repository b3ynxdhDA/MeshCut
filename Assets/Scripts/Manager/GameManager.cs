using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の状態を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    #region 変数

    [SerializeField,Header("設定のUICanvas")]
    private GameObject _configCanvas = default;

    [HideInInspector]// SEマネージャー
    public SEManager _seManager = default;

    // ゲームマネージャーインスタンス
    public static GameManager instance { get; set; }

    // ゲームの状態の
    private GameState _game_State = GameState.Title;
    public GameState game_State { get { return _game_State; } set { _game_State = value; } }
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

    [HideInInspector]// スコアの変数
    public int _nowScore = 0;

    #endregion
    private void Awake()
    {
        // SEマネージャーを外部から参照しやすく
        _seManager = transform.GetComponent<SEManager>();

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

    /// <summary>
    /// コンフィグキャンバスを表示
    /// </summary>
    public void CallConfigUI()
    {
        _game_State = GameState.Config;
        _configCanvas.SetActive(true);
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
}
