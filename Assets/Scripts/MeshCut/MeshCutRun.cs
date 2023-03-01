using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �J�������烁�b�V���ؒf������Ray���΂��N���X
/// </summary>
public class MeshCutRun : MonoBehaviour
{
    #region MeshCut�֌W�̕ϐ�
    // �ؒf�ł���LayerMask
    const int _cutLayerMask = 1 << 8;
    [SerializeField,Header("�ؒf�ʂ𖄂߂�")]
    private bool _isCutSurfaceFill = false;
    [SerializeField,Header("�ؒf�ʂ̃}�e���A��(null�Ȃ�I�u�W�F�N�g�̃}�e���A�����疄�߂�)")]
    private Material _CutSurfaceMaterial = default;
    //�J�b�g����I�u�W�F�N�g
    private GameObject _tergetObject = default;
    // �@���̌����Ă�������ŐV����Instantiate�����I�u�W�F�N�g
    private GameObject _frontObject = default;
    // �@���̔��Ε����ŕό`�����I���W�i���̃I�u�W�F�N�g
    private GameObject _backObject = default;
    //Ray�������������W�̔z��
    private List<Vector3> _hitPositions = new List<Vector3>();
    //Ray��Hit���
    private RaycastHit _hit;
    //Ray���I�u�W�F�N�g��ؒf����
    private bool _isCutting = false;
    //���X�g�̍Ō���𒲐�����
    const int _LIST_END = -1;
    // centerPos����邽�߂�Ray�̒���
    const float _CENTER_RAY_RANGE = 20f;
    #endregion

    #region DestroyObject�֌W�̕ϐ�
    // �ڒn�����Ray�̒���
    private float _chechGroundDistance = 0.5f;
    // Ray���Փ˂���LayerMask
    // �ڒn�������郌�C���[
    const int _hitGroundLayer = 1 << 9;
    // �폜�̃f�B���C
    const float _DESTROY_DELAY = 3f;
    #endregion

    void Update()
    {
        //�ؒf�ʂ𐶐�����Ray
        Ray bladeRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(bladeRay.origin, bladeRay.direction * 50f, Color.green);
        // ���N���b�N��������Ă��邩��Ray���I�u�W�F�N�g�������Ă��邩
        if (Input.GetMouseButton(0) && Physics.Raycast(bladeRay, out _hit, _cutLayerMask))
        {
            print(_cutLayerMask);
            print(_hit.transform.gameObject.layer);
            _tergetObject = _hit.transform.gameObject;
            _hitPositions.Add(_hit.point);
            _isCutting = true;
        }
        // �ؒfRay���I�u�W�F�N�g��ʉ߂�����
        else if (Input.GetMouseButton(0) && _isCutting)
        {
            (Vector3 center, Vector3 normal) = CalculationNormal(_hitPositions[0], _hitPositions[_hitPositions.Count + _LIST_END]);
            //_meshCut.Cut(_tergetObject, center, normal, _material);
            (_backObject, _frontObject) = NewMeshCut.CutGameObject(_tergetObject, center, normal, _isCutSurfaceFill, _CutSurfaceMaterial);

            // �ؒf�Ώۂ�rigidbody���t���Ă�����
            if (_tergetObject.GetComponent<Rigidbody>())
            {
                // �ؒf�����I�u�W�F�N�g�̊Ԃ��J����
                _frontObject.GetComponent<Rigidbody>().AddForce(-normal);
                _backObject.GetComponent<Rigidbody>().AddForce(normal);
            }
            
            //���X�g�̏�����
            _hitPositions.Clear();
            _isCutting = false;
        }
    }
    /// <summary>
    /// �@���x�N�g�����v�Z���郁�\�b�h
    /// </summary>
    /// <param name="startPos">�n�_�̍��W</param>
    /// <param name="endPos">�I�_�̍��W</param>
    /// <returns>�ؒf�ʂ̒��S�Ɛؒf�ʂ̖@���x�N�g��</returns>
    private (Vector3 center, Vector3 normal) CalculationNormal(Vector3 startPos, Vector3 endPos)
    {

        // ���_�ƂȂ�J�����̃|�W�V����
        Vector3 cameraPos = Camera.main.transform.position;

        // �ʂ̒��S�̌v�Z
        // ��(startPos-endPos)�̒��������߂�
        Vector3 centerPosDirection = (startPos + endPos) / 2;
        // �J��������ӂ̒�����Ray���o��
        Ray centerRay = new Ray(cameraPos, (centerPosDirection - cameraPos).normalized);
        // �����̒��_
        Vector3 otherSidePos = default;
        if(Physics.Raycast(centerRay, out _hit, _CENTER_RAY_RANGE))
        {
            otherSidePos = _hit.point;
        }
        // �ؒf�ʂ̎O�_�̒��S���v�Z
        Vector3 centerPos = (startPos + endPos + otherSidePos) / 3;


        // �@���x�N�g���v�Z
        // ���_����n�_�̕���
        Vector3 startDirection = (startPos - centerPos);
        // ���_����I�_�̕���
        Vector3 endDirection = (endPos - centerPos);
        // �ʂ̖@��
        Vector3 normalDirection = Vector3.Cross(startDirection, endDirection);

        return (centerPos, normalDirection);
    }
    /// <summary>
    /// �ؒf��̃I�u�W�F�N�g���ڒn������폜���郁�\�b�h
    /// </summary>
    private void DestroyObject(GameObject destroyObj)
    {
        // �ڒn����
        bool _isGround = false;

        // �I�u�W�F�N�g�̓������ڒn�����Ray�𒷂�����
        _chechGroundDistance += this.transform.lossyScale.y / 2;

        int i = 0;

        while(!_isGround)
        {
            print("�ڒn�T��");
            Debug.DrawRay(transform.position, Vector3.down * _chechGroundDistance, Color.red);
            // �I�u�W�F�N�g���n�ʂɒ�������
            if (!_isGround && Physics.Raycast(new Ray(transform.position, Vector3.down), _chechGroundDistance, _hitGroundLayer))
            {
                _isGround = true;
                Destroy(destroyObj, _DESTROY_DELAY);
            }

            i++;
            if(i > 100)
            {
                break;
            }
            
        }
    }
}