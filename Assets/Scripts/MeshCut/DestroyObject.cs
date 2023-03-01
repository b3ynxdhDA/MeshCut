using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    // 接地判定
    private bool _isGround = false;
    // Rayの発射地点
    private Vector3 _rayPosition = default;
    // 接地判定のRayの長さ
    private float _chechGroundDistance = 1f;
    // Rayが衝突するLayerMask
    private int _hitLayer = 3;
    // 削除のディレイ
    const float _DESTROY_DELAY = 3f;

    void Start()
    {
        _chechGroundDistance = this.transform.lossyScale.y * 0.8f;
    }

    void Update()
    {
        // メッシュの中心を取得
        _rayPosition = GetComponent<MeshRenderer>().bounds.center;

        Debug.DrawRay(_rayPosition, Vector3.down * _chechGroundDistance, Color.red);
        // オブジェクトが地面に着いたら
        if (!_isGround && Physics.Raycast(new Ray(_rayPosition, Vector3.down), _chechGroundDistance, _hitLayer))
        {
            print("ですとろい");

            _isGround = true;
            Destroy(this.gameObject, _DESTROY_DELAY);
        }
    }
}
