using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// �I�u�W�F�N�g�̔��˒n�_�𓮂����N���X
/// </summary>
public class GeneratPointMove : MonoBehaviour
{
    // ���˒n�_���ړ�����ʉߒn�_�̃��X�g
    private List<Vector3> _relayPoint = new List<Vector3>();

    [SerializeField, Header("�ʉߒn�_��1�����鎞��")]
    private float _motionTime = 10f;

    void Start()
    {
        // �q�I�u�W�F�N�g�̒ʉߒn�_�����X�g�Ɋi�[
        foreach (Transform childObject in this.gameObject.transform)
        {
            _relayPoint.Add(childObject.transform.position);
        }

        // ���X�g��z��ɕϊ�����DOPath�œ�����
        this.transform.DOPath(_relayPoint.ToArray(), _motionTime).OnComplete(MoveLoop);
    }

    private void MoveLoop()
    {
        this.transform.DOPath(_relayPoint.ToArray(), _motionTime).OnComplete(MoveLoop);
    }
}
