using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// オブジェクトの発射地点を動かすクラス
/// </summary>
public class GeneratPointMove : MonoBehaviour
{
    // 発射地点が移動する通過地点のリスト
    private List<Vector3> _relayPoint = new List<Vector3>();

    [SerializeField, Header("通過地点を1周する時間")]
    private float _motionTime = 10f;

    void Start()
    {
        // 子オブジェクトの通過地点をリストに格納
        foreach (Transform childObject in this.gameObject.transform)
        {
            _relayPoint.Add(childObject.transform.position);
        }

        // リストを配列に変換してDOPathで動かす
        this.transform.DOPath(_relayPoint.ToArray(), _motionTime).OnComplete(MoveLoop);
    }

    private void MoveLoop()
    {
        this.transform.DOPath(_relayPoint.ToArray(), _motionTime).OnComplete(MoveLoop);
    }
}
