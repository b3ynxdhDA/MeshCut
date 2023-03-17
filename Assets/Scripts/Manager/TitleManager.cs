using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// タイトルを管理するクラス
/// </summary>
public class TitleManager : MonoBehaviour
{
    void Start()
    {
        // タイムスケールを直す
        Time.timeScale = 1f;

        // ゲーム起動時にタイトルBGMを再生
        GameManager.instance._audioManager.PlayTitle_BGM();

        // ゲームの状態をTitleにする
        GameManager.instance.GameStateProperty = GameManager.GameState.Title;
    }

    /// <summary>
    /// ゲームシーンに遷移するボタンが押されたら
    /// </summary>
    public void OnGameScene()
    {
        // ボタンを押したときのSEを鳴らす
        GameManager.instance._audioManager.OnDecision_SE();

        // ゲームステートをゲーム開始に遷移する
        GameManager.instance.GameStateProperty = GameManager.GameState.GameRedy;

        // ゲームシーンに遷移する
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// 設定ボタンが押されたら
    /// </summary>
    public void OnConfigButton()
    {
        // ボタンを押したときのSEを鳴らす
        GameManager.instance._audioManager.OnDecision_SE();

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
