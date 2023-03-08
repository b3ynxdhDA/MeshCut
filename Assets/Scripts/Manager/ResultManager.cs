using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// リザルトを管理するクラス
/// </summary>
public class ResultManager : MonoBehaviour
{
    // 変数宣言--------------------------
    // スコアの配列
    private int[] _score = new int[5];
    // スコアの保存名の配列
    private string[] _save = { "Score0", "Score1", "Score2", "Score3", "Score4" };
    // 今回のゲームのスコア
    private int _nowScore = GameManager.instance._nowScore;



    void Start()
    {
        // 保存されているスコアをロードする
        for (int i = 0; i < _score.Length; i++)
        {
            _score[i] = PlayerPrefs.GetInt(_save[i], 0);
        }
    }

    void Update()
    {
        int tmp;
        // 今回のスコアと比較する
        for (int i = 0; i < _score.Length; i++)
        {
            // 高いスコアから順に今回のスコアと比べる
            if(_score[i] < _nowScore)
            {
                // @途中
                tmp = _score[i];
                _score[i] = _nowScore;
            }
        }
    }
}
