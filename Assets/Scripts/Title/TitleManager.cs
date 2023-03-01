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

    void FixedUpdate()
    {
        // いずれかのボタンが押されたらゲームに進む
        if (Input.anyKey)
        {
            GameManager.instance.game_State = GameManager.GameState.GameRedy;
            SceneManager.LoadScene("GameScene");
        }
    }
}
