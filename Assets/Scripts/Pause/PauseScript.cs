using UnityEngine;

//ポーズ機能を呼び出すクラス
//常にアクティブなオブジェクトにアタッチして
public class PauseScript : MonoBehaviour
{
    //ポーズしたときに表示するUI
    [SerializeField] private GameObject _pauseUI = default;

    private void Awake()
    {
        //タイムスケールの初期化
        Time.timeScale = 1f;
    }
    //ポーズ中は処理を受け付けない処理を書いてはいけない
    void Update()
    {
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
        if (!(GameManager.instance.game_State == GameManager.GameState.GameNow || GameManager.instance.game_State == GameManager.GameState.Pause))
        {
            return;
        }
        //ポーズUIのアクティブを切り替え
        _pauseUI.SetActive(!_pauseUI.activeSelf);

        //ポーズUIが表示されている時は停止
        if (_pauseUI.activeSelf)
        {
            Time.timeScale = 0f;
            GameManager.instance.game_State = GameManager.GameState.Pause;
        }
        else
        {
            Time.timeScale = 1f;
            GameManager.instance.game_State = GameManager.GameState.GameNow;
        }
    }
}
