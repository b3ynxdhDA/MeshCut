using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    // 現在のスクリーンの状態を表示するテキスト
    [SerializeField] GameObject u_ChangeScreenText = null;

    // trueならWindow,falseならFullScreen
    private bool _isFullScreen = false;

    // フルスクリーン時のスクリーンサイズ
    const int FULL_WIDHT = 1920;

    private void Start()
    {
        // 現在のスクリーンサイズがフルスクリーンサイズより小さいか
        if (Screen.width < FULL_WIDHT)
        {
            u_ChangeScreenText.GetComponent<Text>().text = "< Window >";
            _isFullScreen = false;
        }
        else
        {
            u_ChangeScreenText.GetComponent<Text>().text = "< Full Screen >";
            _isFullScreen = true;
        }
    }

    public void OnScreenChange()
    {
        // 現在のスクリーンサイズがフルスクリーンなら
        if (_isFullScreen)
        {
            _isFullScreen = true;
            Screen.SetResolution(Screen.width, Screen.height, true);
            u_ChangeScreenText.GetComponent<Text>().text = "< Full Screen >";
        }
        else
        {
            _isFullScreen = false;
            Screen.SetResolution(Screen.width, Screen.height, false);
            u_ChangeScreenText.GetComponent<Text>().text = "< Window >";
        }
    }
}
