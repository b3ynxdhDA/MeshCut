using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    // �ڒn����
    private bool _isGround = false;
    // Ray�̔��˒n�_
    private Vector3 _rayPosition = default;
    // �ڒn�����Ray�̒���
    private float _chechGroundDistance = 1f;
    // Ray���Փ˂���LayerMask
    private int _hitLayer = 3;
    // �폜�̃f�B���C
    const float _DESTROY_DELAY = 3f;

    void Start()
    {
        _chechGroundDistance = this.transform.lossyScale.y * 0.8f;
    }

    void Update()
    {
        // ���b�V���̒��S���擾
        _rayPosition = GetComponent<MeshRenderer>().bounds.center;

        Debug.DrawRay(_rayPosition, Vector3.down * _chechGroundDistance, Color.red);
        // �I�u�W�F�N�g���n�ʂɒ�������
        if (!_isGround && Physics.Raycast(new Ray(_rayPosition, Vector3.down), _chechGroundDistance, _hitLayer))
        {
            print("�ł��Ƃ낢");

            _isGround = true;
            Destroy(this.gameObject, _DESTROY_DELAY);
        }
    }
}
