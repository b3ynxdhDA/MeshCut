using UnityEngine;

/// <summary>
/// ポーズ機能を呼び出すクラス
/// 常にアクティブなオブジェクトにアタッチして
/// </summary>
public class PauseScript : MonoBehaviour
{

    // 変数宣言----------------------------------
    // ポーズしたときに表示するUI
    [SerializeField] private GameObject _pauseUI = default;

    private void Awake()
    {
        //タイムスケールの初期化
        Time.timeScale = 1f;
    }
    //ポーズ中は処理を受け付けない処理を書いてはいけない
    void Update()
    {
        // Escapeキーでポーズを呼び出す
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnPouse();
        }
    }
    /// <summary>
    /// ポーズUIの表示とゲーム状態の変更
    /// </summary>
    public void OnPouse()
    {
        // ゲームステートがゲーム中かポーズ以外ならreturnする
        if (!(GameManager.instance.game_State == GameManager.GameState.GameNow || GameManager.instance.game_State == GameManager.GameState.Pause))
        {
            return;
        }
        //ポーズUIのアクティブを切り替え
        _pauseUI.SetActive(!_pauseUI.activeSelf);


        //ポーズUIが表示されている時は停止
        if (_pauseUI.activeSelf)
        {
            // タイムスケールを0にして止める
            Time.timeScale = 0f;

            // ゲームステートをポーズに
            GameManager.instance.game_State = GameManager.GameState.Pause;

            // カーソルロックをしていたら
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                //  カーソルロックを解除
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else
        {
            // タイムスケールを1にして再開
            Time.timeScale = 1f;

            // ゲームステートをゲーム中に
            GameManager.instance.game_State = GameManager.GameState.GameNow;

            // カーソルロックをしていなかったた
            if (Cursor.lockState == CursorLockMode.None)
            {
                //  カーソルロックをする
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}
