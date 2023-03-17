using UnityEngine;

/// <summary>
/// ゲームのSEを管理するクラス
/// </summary>
public class AudioManager : MonoBehaviour
{
    // 変数宣言----------------------------------
    // システム音などを管理するAudioSource
    private AudioSource _audioSource;

    // ゲームのBGM---------------------------------------
    [SerializeField,Header("タイトルのBGM")] 
    private AudioClip _titleBGM = default;
    
    [SerializeField,Header("メインゲーム中のBGM")] 
    private AudioClip _mainBGM = default;

    [SerializeField,Header("ゲーム終盤のBGM")] 
    private AudioClip _nearLimitTimeBGM = default;
    
    [SerializeField,Header("リザルト中のBGM")] 
    private AudioClip _resultBGM = default;

    // ゲームのSE---------------------------------------
    [SerializeField,Header("切断物が投擲されたSE")] 
    private AudioClip _throwingSE = default;

    [SerializeField,Header("切断時のSE")] 
    private AudioClip _cutingSE = default;

    [SerializeField,Header("決定のSE")] 
    private AudioClip _onDecisionSE = default;

    [SerializeField,Header("ポーズのSE")] 
    private AudioClip _onPauseSE = default;

    [SerializeField,Header("カウントダウンのSE")] 
    private AudioClip _startCountDownSE = default;

    [SerializeField,Header("ゲームスタートのSE")]
    private AudioClip _gameStartSE = default;

    [SerializeField,Header("ゲーム終了のSE")]
    private AudioClip _gameTimeUpSE = default;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// ConfigManagerの設定値とaudioSourceのVolumeを揃える
    /// </summary>
    public void CheckVolume()
    {
        // ConfigurationオブジェクトがTrueのとき
        // エディタ再生時Configurationが非アクティブだとNullReferenceExceptionが出る
        if (ConfigManager.instance.gameObject.activeSelf)
        {
            // ConfigManagerの設定値をaudioSourceに反映
            if (ConfigManager.instance._masterVolume != _audioSource.volume)
            {
                _audioSource.volume = ConfigManager.instance._masterVolume;
            }
        }
    }
    #region BGMメソッド
    /// <summary>
    /// タイトルのBGMを再生する
    /// </summary>
    public void PlayTitle_BGM()
    {
        _audioSource.clip = _titleBGM;
        _audioSource.Play();
    }
    /// <summary>
    /// ゲーム中のBGMを再生する
    /// </summary>
    public void PlayGame_BGM()
    {
        _audioSource.clip = _mainBGM;
        _audioSource.Play();
    }
    /// <summary>
    /// ゲーム終盤のBGMを再生する
    /// </summary>
    public void NearLimitTime_BGM()
    {
        _audioSource.clip = _nearLimitTimeBGM;
        _audioSource.Play();
    }
    /// <summary>
    /// リザルトのBGMを再生する
    /// </summary>
    public void PlayResult_BGM()
    {
        _audioSource.clip = _resultBGM;
        _audioSource.Play();
    }
    #endregion

    #region SEメソッド
    /// <summary>
    /// 投擲SEを鳴らす
    /// </summary>
    public void Throwing_SE()
    {
        _audioSource.PlayOneShot(_throwingSE);
    }
    /// <summary>
    /// 切断SEを鳴らす
    /// </summary>
    public void OnCut_SE()
    {
        _audioSource.PlayOneShot(_cutingSE);
    }
    /// <summary>
    /// 決定SEを鳴らす
    /// </summary>
    public void OnDecision_SE()
    {
        _audioSource.PlayOneShot(_onDecisionSE);
    }
    /// <summary>
    /// ポーズSEを鳴らす
    /// </summary>
    public void OnPsuse_SE()
    {
        _audioSource.PlayOneShot(_onPauseSE);
    }
    /// <summary>
    /// スタートカウントダウンの三秒のSEを鳴らす
    /// </summary>
    public void OnStartCount3_SE()
    {
        _audioSource.PlayOneShot(_startCountDownSE);
    }
    /// <summary>
    /// スタートカウントダウンの最後のゲーム開始SEを鳴らす
    /// </summary>
    public void OnStartCountGo_SE()
    {
        _audioSource.PlayOneShot(_gameStartSE);
    }
    /// <summary>
    /// ゲームのタイムアップSEを鳴らす
    /// </summary>
    public void OnGameTimeUp_SE()
    {
        _audioSource.PlayOneShot(_gameTimeUpSE);
    }
    #endregion
}
