using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    // 接地判定
    private bool _isGround = false;
    // 接地判定のRayの長さ
    private float _chechGroundDistance = 0.5f;
    // Rayが衝突するLayerMask
    private int _hitLayer = 3;
    // 削除のディレイ
    const float _DESTROY_DELAY = 3f;

    void Start()
    {
        print(transform.position);
        _chechGroundDistance += this.transform.lossyScale.y / 2;
    }

    void Update()
    {
        Debug.DrawRay(transform.position, Vector3.down * _chechGroundDistance, Color.red);
        // オブジェクトが地面に着いたら
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), _chechGroundDistance, _hitLayer)　&& !_isGround)
        {
            _isGround = true;
            Destroy(this.gameObject, _DESTROY_DELAY);
        }
    }
}
