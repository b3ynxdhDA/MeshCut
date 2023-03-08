using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲームのSEを管理するクラス
/// </summary>
public class SEManager : MonoBehaviour
{
    // 変数宣言----------------------------------
    // システム音などを管理するAudioSource
    private AudioSource _audioSource;

    // ゲームの全体で使われる可能性があるSE--------------------
    [SerializeField,Header("決定のSE")] 
    private AudioClip _onDecision = default;

    [SerializeField,Header("カウントダウンのSE")] 
    private AudioClip _startCount3 = default;

    [SerializeField,Header("ゲームスタートのSE")]
    private AudioClip _startCount_Go = default;

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

    #region SEメソッド
    /// <summary>
    /// 決定SEを鳴らす
    /// </summary>
    public void OnDecision_SE()
    {
        _audioSource.PlayOneShot(_onDecision);
    }
    /// <summary>
    /// 切断SEを鳴らす
    /// </summary>
    public void OnCut_SE()
    {
        
    }
    /// <summary>
    /// ポーズSEを鳴らす
    /// </summary>
    public void OnPsuse_SE()
    {
        
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
    #endregion
}
