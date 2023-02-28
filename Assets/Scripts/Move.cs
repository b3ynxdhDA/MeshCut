using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{

    private Rigidbody _rigidbody = default;
    void Start()
    {
        print("Žn‚Ü‚è");
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        _rigidbody.AddForce(Vector3.one);
        
    }
}
