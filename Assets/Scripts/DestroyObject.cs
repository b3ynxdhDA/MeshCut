using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    // �ڒn����
    private bool _isGround = false;
    // �ڒn�����Ray�̒���
    private float _chechGroundDistance = 0.5f;
    // Ray���Փ˂���LayerMask
    private int _hitLayer = 3;
    // �폜�̃f�B���C
    const float _DESTROY_DELAY = 3f;

    void Start()
    {
        print(transform.position);
        _chechGroundDistance += this.transform.lossyScale.y / 2;
    }

    void Update()
    {
        Debug.DrawRay(transform.position, Vector3.down * _chechGroundDistance, Color.red);
        // �I�u�W�F�N�g���n�ʂɒ�������
        if (Physics.Raycast(new Ray(transform.position, Vector3.down), _chechGroundDistance, _hitLayer)�@&& !_isGround)
        {
            _isGround = true;
            Destroy(this.gameObject, _DESTROY_DELAY);
        }
    }
}
