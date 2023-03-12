using UnityEngine;

/// <summary>
/// ゲームのSEを管理するクラス
/// </summary>
public class SEManager : MonoBehaviour
{
    // 変数宣言----------------------------------
    // システム音などを管理するAudioSource
    private AudioSource _audioSource;

    // ゲームのBGM---------------------------------------
    [SerializeField,Header("タイトルのBGM")] 
    private AudioClip _titleBGM = default;
    
    [SerializeField,Header("メインゲーム中のBGM")] 
    private AudioClip _mainBGM = default;
    
    [SerializeField,Header("リザルト中のBGM")] 
    private AudioClip _resultBGM = default;

    // ゲームのSE---------------------------------------
    [SerializeField,Header("切断")] 
    private AudioClip _cuting = default;

    [SerializeField,Header("決定のSE")] 
    private AudioClip _onDecision = default;

    [SerializeField,Header("ポーズのSE")] 
    private AudioClip _onPause = default;

    [SerializeField,Header("カウントダウンのSE")] 
    private AudioClip _startCount3 = default;

    [SerializeField,Header("ゲームスタートのSE")]
    private AudioClip _startCount_Go = default;

    [SerializeField,Header("ゲーム終了のSE")]
    private AudioClip _gameTimeUp = default;

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
    /// 切断SEを鳴らす
    /// </summary>
    public void OnCut_SE()
    {
        _audioSource.PlayOneShot(_cuting);
    }
    /// <summary>
    /// 決定SEを鳴らす
    /// </summary>
    public void OnDecision_SE()
    {
        _audioSource.PlayOneShot(_onDecision);
    }
    /// <summary>
    /// ポーズSEを鳴らす
    /// </summary>
    public void OnPsuse_SE()
    {
        _audioSource.PlayOneShot(_onPause);
    }
    /// <summary>
    /// スタートカウントダウンの三秒のSEを鳴らす
    /// </summary>
    public void OnStartCount3_SE()
    {
        _audioSource.PlayOneShot(_startCount3);
    }
    /// <summary>
    /// スタートカウントダウンの最後のゲーム開始SEを鳴らす
    /// </summary>
    public void OnStartCountGo_SE()
    {
        _audioSource.PlayOneShot(_startCount_Go);
    }
    /// <summary>
    /// ゲームのタイムアップSEを鳴らす
    /// </summary>
    public void OnGameTimeUp_SE()
    {
        _audioSource.PlayOneShot(_gameTimeUp);
    }
    #endregion
}
