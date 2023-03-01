using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の状態を管理するクラス
/// </summary>
public class GameManager : MonoBehaviour
{
    #region 変数

    // SEマネージャー
    public SEManager _seManager = default;

    //private GameManager _instance = null;
    public static GameManager instance { get; set; }

    // ゲームの状態の
    private GameState _game_State = GameState.Title;
    public GameState game_State { get { return _game_State; } set { _game_State = value; } }
    /// <summary>
    /// ゲームの状態
    /// </summary>
    public enum GameState
    {
        Title,
        Select,
        GameRedy,
        GameNow,
        GameOver,
        Result,
        Pause,
        Setting
    };

    // ハイスコアの変数
    public int _highScore = 0;

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
}
