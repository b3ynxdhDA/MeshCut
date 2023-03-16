using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �ؒf�I�u�W�F�N�g�������_���ɐ������v���C���[�Ɍ����Ĕ�΂�
/// </summary>
public class Generater : MonoBehaviour
{
    // �ϐ��錾----------------------------------
    [SerializeField, Header("�v���C���[�̈ʒu")]
    private Transform _playerTransform;
    // ��������I�u�W�F�N�g�̃v���n�u
    private GameObject[] _generatedObjectPrefab;
    // �I�u�W�F�N�g�𐶐�����ʒu�̃Q�[���I�u�W�F�N�g�̃��X�g
    private List<GameObject> _generatPoint = new List<GameObject>();
    // �����C���^�[�o��
    float _throwInterval = default;

    // �萔�錾---------------------
    // ���ˊp�x�̍ŏ�
    const float _ANGLE_MIN = 30;
    // ���ˊp�x�̍ŏ�
    const float _ANGLE_MAX = 61;
    // �����Ԋu�̍ŏ�
    const int _THROW_INTERVAL_MIN = 1;
    // �����Ԋu�̍ő�
    const int _THROW_INTERVAL_MAX = 4;

    void Start()
    {
        // Generater�̎q�I�u�W�F�N�g��List�Ɋi�[
        foreach(Transform childObject in this.gameObject.transform)
        {
            _generatPoint.Add(childObject.gameObject);
        }

        // CutTarget�t�H���_����v���n�u���擾
        _generatedObjectPrefab = Resources.LoadAll<GameObject>("CutTarget");

        // �����̊Ԋu�������_���ɐݒ肷��
        _throwInterval = Random.Range(_THROW_INTERVAL_MIN, _THROW_INTERVAL_MAX);

    }

    void Update()
    {
        // �Q�[���̏�Ԃ��Q�[�����ȊO�Ȃ珈�����Ȃ�
        if (GameManager.instance._gameStateProperty != GameManager.GameState.GameNow)
        {
            return;
        }

        // �C���^�[�o����1�b�Â��炷
        _throwInterval -= Time.deltaTime;
        // �C���^�[�o����0��菬�����Ȃ��
        if(_throwInterval < 0)
        {
            RandomThrow();
            // �����̊Ԋu�������_���ɐݒ肷��
            _throwInterval = Random.Range(_THROW_INTERVAL_MIN, _THROW_INTERVAL_MAX);
        }


        // @�X�y�[�X�Ń����_������
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RandomThrow();
        }
    }

    /// <summary>
    /// �����_���ȃI�u�W�F�N�g�������_���ȃ|�W�V�������瓊������
    /// </summary>
    private void RandomThrow()
    {
        // �����_���ȓ�������I�u�W�F�N�g�v���n�u�̈���
        int randomThrowObjectArgument = Random.Range(0, _generatedObjectPrefab.Length);
        // �����_���ȓ����ʒu
        int randomThrowPositionArgument = Random.Range(0, _generatPoint.Count);
        // �����_���ȃI�u�W�F�N�g�������_���ȃ|�W�V�������瓊������
        Throwing(_generatedObjectPrefab[randomThrowObjectArgument], _generatPoint[randomThrowPositionArgument].transform.position);
    }
    /// <summary>
    /// �I�u�W�F�N�g���ˏo����
    /// </summary>
    private void Throwing(GameObject throwObject, Vector3 throwPosition)
    {
        // ��������I�u�W�F�N�g���쐬����
        GameObject generatedObject = Instantiate(throwObject, throwPosition, Quaternion.identity);

        // �ˏo�p�x�������_���Ɍ��߂�
        float throwAngle = Random.Range(_ANGLE_MIN, _ANGLE_MAX);

        // �ˏo���x���Z�o
        Vector3 velocity = CalculateVelocity(throwPosition, _playerTransform.position, throwAngle);

        // AddForce�Ŏˏo����
        Rigidbody rid = generatedObject.GetComponent<Rigidbody>();
        rid.AddForce(velocity * rid.mass, ForceMode.Impulse);

        // ���������Ƃ���SE��炷
        GameManager.instance._audioManager.Throwing_SE();
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

        // ���������̋���x
        float horizontalDistance = Vector3.Distance(new Vector3(targetPos.x, 0, targetPos.z), new Vector3(throwPos.x, 0, throwPos.z));

        // ���������̋���y
        float verticalDistance = targetPos.y - throwPos.y;

        // �Ε����˂̌����������x�ɂ��ĉ���
        float speed = Mathf.Sqrt(-Physics.gravity.y * Mathf.Pow(horizontalDistance, 2) / (2 * Mathf.Pow(Mathf.Cos(rad), 2) * (horizontalDistance * Mathf.Tan(rad) + verticalDistance)));

        // �����𖞂����������Z�o�ł��Ȃ����Vector3.zero��Ԃ�
        if (float.IsNaN(speed))
        {
            return Vector3.zero;
        }
        else
        {
            return (new Vector3(throwPos.x - targetPos.x, horizontalDistance * Mathf.Tan(rad), throwPos.z - targetPos.z).normalized * speed);
        }
    }
}
