using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// @テスト用
/// </summary>
public class Move : MonoBehaviour
{

    private Rigidbody _rigidbody = default;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        GameManager.instance.GameStateProperty = GameManager.GameState.GameNow;
    }

    void Update()
    {
        //_rigidbody.AddForce(Vector3.back);
        
    }
}
