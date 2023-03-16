using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// UIのボタンによるシーン遷移などを呼び出すクラス
/// </summary>
public class PauseButton : MonoBehaviour
{
    //EventSystemのFirstSelectedに
    //呼び出した時に選択状態にするボタンオブジェクトをアタッチして

    /// <summary>
    /// タイトルボタンが押されたら
    /// </summary>
    public void OnTitleButton()
    {
        // ボタンを押したときのSEを鳴らす
        GameManager.instance._audioManager.OnDecision_SE();

        // タイトルシーンに戻る
        SceneManager.LoadScene("TitleScene");
    }
    /// <summary>
    /// リトライボタンが押されたら
    /// </summary>
    public void OnRetryButton()
    {
        // ボタンを押したときのSEを鳴らす
        GameManager.instance._audioManager.OnDecision_SE();

        // 現在のシーンを再び呼び出す
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
        // ボタンを押したときのSEを鳴らす
        GameManager.instance._audioManager.OnDecision_SE();

        // ゲームマネージャーの終了メソッドを呼び出す
        GameManager.instance.OnExit();
    }
}
