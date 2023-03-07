using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// タイトルを管理するクラス
/// </summary>
public class TitleManager : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 1f;
        // ゲームの状態をTitleにする
        GameManager.instance.game_State = GameManager.GameState.Title;
    }

    /// <summary>
    /// ゲームシーンに遷移するボタンが押されたら
    /// </summary>
    public void OnGameScene()
    {
        GameManager.instance.game_State = GameManager.GameState.GameRedy;
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// 設定ボタンが押されたら
    /// </summary>
    public void OnConfigButton()
    {
        // ゲームマネージャーの終了メソッドを呼び出す
        GameManager.instance.CallConfigUI();
    }

    /// <summary>
    /// 終了ボタンが押されたら
    /// </summary>
    public void OnExitButton()
    {
        // ゲームマネージャーの終了メソッドを呼び出す
        GameManager.instance.OnExit();
    }
}
