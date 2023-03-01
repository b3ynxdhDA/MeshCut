using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲームのSEを管理するクラス
/// </summary>
public class SEManager : MonoBehaviour
{
    private AudioSource _audioSource;

    //決定のSE
    [SerializeField] private AudioClip _onDecision = default;

    //カーソル移動のSE
    [SerializeField] private AudioClip _moveCorsor = default;

    //カウントダウンのSE
    [SerializeField] private AudioClip _startCount3 = default;

    //ゲームスタートのSE
    [SerializeField] private AudioClip _startCount_Go = default;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 決定SEを鳴らす
    /// </summary>
    public void OnDecision_SE()
    {
        _audioSource.PlayOneShot(_onDecision);
    }
    /// <summary>
    /// カーソル移動SEを鳴らす
    /// </summary>
    public void OnMoveCorsor_SE()
    {
        _audioSource.PlayOneShot(_moveCorsor);
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
}
