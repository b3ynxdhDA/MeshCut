using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ؒf�I�u�W�F�N�g�������_���ɐ������v���C���[�Ɍ����Ĕ�΂�
/// </summary>
public class Generater : MonoBehaviour
{
    [SerializeField, Header("��������I�u�W�F�N�g�̃v���n�u")]
    private GameObject[] _generatedObjectPrefab;

    // �I�u�W�F�N�g�𐶐�����ʒu�̃Q�[���I�u�W�F�N�g
    private GameObject[] _generatPoint = new GameObject[1];
    // ���ˊp�x�̍ŏ�
    const float _ANGLE_MIN = 10;
    // ���ˊp�x�̍ŏ�
    const float _ANGLE_MAX = 71;
    // �v���C���[�̈ʒu
    readonly Vector3 _PLAYER_POSITION = new Vector3(0f, 2f, 0f);


    void Start()
    {
        int childObjectCount = 0;
        foreach(Transform childObject in this.gameObject.transform)
        {
            _generatPoint[childObjectCount] = childObject.gameObject;
            childObjectCount++;
        }

        Throwing();
    }

    void Update()
    {
        
    }

    /// <summary>
    /// �I�u�W�F�N�g���ˏo����
    /// </summary>
    private void Throwing()
    {
        GameObject generatedObject = Instantiate(_generatedObjectPrefab[0], _generatPoint[0].transform.position, Quaternion.identity);

        // �ˏo�p�x�������_���Ɍ��߂�
        float throwAngle = Random.Range(_ANGLE_MIN, _ANGLE_MAX);

        // �ˏo���x���Z�o
        Vector3 velocity = CalculateVelocity(_generatPoint[0].transform.position, _PLAYER_POSITION, throwAngle);

        print(velocity);
        // �ˏo
        Rigidbody rid = generatedObject.GetComponent<Rigidbody>();
        rid.AddForce(velocity * rid.mass, ForceMode.Impulse);
    }

    /// <summary>
    /// �ˏo���x�̌v�Z
    /// </summary>
    /// <param name="targetPos">�ˏo�J�n���W</param>
    /// <param name="throwPos">�W�I�̍��W</param>
    /// <param name="angle">�ˏo���x</param>
    /// <returns></returns>
    private Vector3 CalculateVelocity(Vector3 targetPos, Vector3 throwPos, float angle)
    {
        // �ˏo�p�����W�A���ɕϊ�
        float rad = angle * Mathf.PI / 180;
        print("rad" + rad);
        // ���������̋���x
        float horizontalDistance = Vector3.Distance(new Vector3(targetPos.x,0, targetPos.z), new Vector3(throwPos.x,0, throwPos.z));
        print("horiDis" + horizontalDistance);

        // ���������̋���y
        float verticalDistance = targetPos.y - throwPos.y;
        print("verDis" + verticalDistance);

        // �Ε����˂̌����������x�ɂ��ĉ���
        float speed = Mathf.Sqrt(-Physics.gravity.y * Mathf.Pow(horizontalDistance, 2) / (2 * Mathf.Pow(Mathf.Cos(rad), 2) * (horizontalDistance * Mathf.Tan(rad) + verticalDistance)));
        print(speed);

        if (float.IsNaN(speed))
        {     
            print("isnan");
            // �����𖞂����������Z�o�ł��Ȃ����Vector3.zero��Ԃ�
            return Vector3.zero;
        }
        else
        {
            return (new Vector3(throwPos.x - targetPos.x, horizontalDistance * Mathf.Tan(rad), throwPos.z - targetPos.z).normalized * speed);
        }
    }
}
