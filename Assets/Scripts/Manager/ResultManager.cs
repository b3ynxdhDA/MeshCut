using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// リザルトを管理するクラス
/// </summary>
public class ResultManager : MonoBehaviour
{
    // 変数宣言--------------------------
    // スコアのテキストオブジェクト
    private List<Text> _scoreText = new List<Text>();
    // スコアの配列
    private List<int> _score = new List<int>();
    // 今回のゲームのスコア
    private int _nowScore = 0;
    // ハイスコアが更新があるか
    private bool _isHighScoreUpdate = false;
    // 更新されたスコアの横に表示する「New」
    private string _updateNewText = "";

    // 定数宣言---------------------
    // スコアの保存名
    const string _SCORE_DATE = "ScoreDate";
    // スコアテキストのTag
    const string _SCORE_TAG = "ScoreText";

    private void OnEnable()
    {
        // 今回のスコアを取得
        _nowScore = GameManager.instance._nowScore;
        // スコア更新フラグを初期化
        _isHighScoreUpdate = false;
        // スコアデータの番号
        int i = 0;
        // キャンバスの子オブジェクトを探索する
        foreach (Transform scoreTextObject in this.gameObject.transform)
        {
            // スコアを表示するテキストオブジェクトだけをタグで探す
            if (scoreTextObject.tag == _SCORE_TAG)
            {
                // スコアテキストを取得
                _scoreText.Add(scoreTextObject.GetComponent<Text>());

                // スコア配列に保存されているスコアをロード
                _score.Add(PlayerPrefs.GetInt(_SCORE_DATE + i, 0));
                i++;
            }
        }
        CheckScoreUpdate();
    }

    /// <summary>
    /// スコアの表示を更新
    /// </summary>
    private void CheckScoreUpdate()
    {
        // 順位の変数
        int rankintNumber;
        // 入れ替え用の一時的な変数
        int tmp;
        // 今回のスコアと比較する
        for (int i = 0; i < _score.Count; i++)
        {
            // 高いスコアから順に今回のスコアと比べてソートする
            if (_score[i] < _nowScore)
            {
                // ハイスコアが更新されていなかったら
                if (!_isHighScoreUpdate)
                {
                    _isHighScoreUpdate = true;

                    _updateNewText = "New!!";
                }
                // ハイスコアが既に更新されていたら
                else
                {
                    _updateNewText = "";
                }

                // スコア配列をソート
                tmp = _score[i];

                _score[i] = _nowScore;

                _nowScore = tmp;
            }

            // 表示する順位
            rankintNumber = i + 1;
            // スコアテキストを更新
            _scoreText[i].text = rankintNumber + ".  " + _score[i].ToString("000000000   ") + _updateNewText;

            // スコアの更新があったら
            if (_isHighScoreUpdate)
            {
                // ソートで更新されたスコアを保存
                PlayerPrefs.SetInt(_SCORE_DATE + i, _score[i]);
            }
        }
        // スコアを保存
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 保存されているスコアを削除する
    /// </summary>
    public void OnDeleteScore()
    {
        PlayerPrefs.DeleteAll();
    }
}