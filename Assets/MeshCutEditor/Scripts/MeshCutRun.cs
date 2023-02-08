using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MeshCut���\�b�h�����s���邽�߂̃N���X
/// </summary>
public class MeshCutRun : MonoBehaviour
{
    //MeshCut�N���X
    private MeshCut _meshCut = default;
    //�J�b�g����I�u�W�F�N�g
    private GameObject _tergetObject = default;
    //�ؒf�ʂɓ\��}�e���A��
    [SerializeField] Material _material = default;
    //Ray�������������W�̔z��
    private List<Vector3> _hitPositions = new List<Vector3>();
    //Ray��Hit���
    private RaycastHit hit;
    //Ray���I�u�W�F�N�g��ؒf����
    private bool isCutting = false;
    //���X�g�̍Ō���𒲐�����
    const int LIST_END = -1;
Vector3 normal = default;

    void Start()
    {
        _meshCut = new MeshCut();
    }
    void Update()
    {
        
        //�ؒf�ʂ𐶐�����Ray
        Ray bladeRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(bladeRay.origin, bladeRay.direction * 50f, Color.green);
        if (Input.GetMouseButton(0) && Physics.Raycast(bladeRay, out hit) && hit.transform.GetComponent<MeshFilter>())
        {
            _tergetObject = hit.transform.gameObject;
            _hitPositions.Add(hit.point);
            isCutting = true;
        }
        else if (Input.GetMouseButton(0) && isCutting)
        {
            //Cut�ɓn���ؒf�ʂ̒��S
            Vector3 center = CalculationNormal(_hitPositions[0], _hitPositions[_hitPositions.Count + LIST_END]).center;
            //Cut�ɓn���@���x�N�g��
            normal = CalculationNormal(_hitPositions[0], _hitPositions[_hitPositions.Count + LIST_END]).normal;
            _meshCut.Cut(_tergetObject, center, normal, _material);
            print(normal);
            //���X�g�̏�����
            _hitPositions.Clear();
            isCutting = false;
        }
        Debug.DrawRay(_tergetObject.transform.position, normal * 20f, Color.blue);
        Debug.DrawRay(centerRay.origin, centerRay.direction * 10, Color.black);
    }
    Ray centerRay;
    /// <summary>
    /// �@���x�N�g�����v�Z���郁�\�b�h
    /// </summary>
    /// <param name="startPos">�n�_�̍��W</param>
    /// <param name="endPos">�I�_�̍��W</param>
    /// <returns>�@���x�N�g��</returns>
    private (Vector3 normal, Vector3 center) CalculationNormal(Vector3 startPos, Vector3 endPos)
    {
        // 2�l�����邽�߂̕ϐ�
        const int TWO = 2;
        // 3�l�����邽�߂̕ϐ�
        const int THREE = 3;
        // ���_�ƂȂ�J�����̃|�W�V����
        Vector3 cameraPos = Camera.main.transform.position;

        // �ӂ̒��������߂�
        Vector3 centerPosDirection = (startPos + endPos) / TWO;
        // �I�u�W�F�N�g�̔��Α�����ӂ̒�����Ray���o��
        centerRay = new Ray(cameraPos, (centerPosDirection - cameraPos).normalized);
        // �I�u�W�F�N�g�̔��Α��̒��_
        Vector3 otherSidePos = default;
        if(Physics.Raycast(centerRay, out hit, 20f))
        {
            otherSidePos = hit.point;
        }

        //�@���x�N�g���v�Z
        //���_����n�_�̕���
        Vector3 startDirection = (startPos - otherSidePos);
        //���_����I�_�̕���
        Vector3 endDirection = (endPos - otherSidePos);

        //�ؒf�ʂ̒��S���v�Z
        Vector3 centerPos = (startPos + endPos + otherSidePos) / THREE;

        print("�X�^�[�g" + startPos);
        print("�G���h" + endPos);
        print("���_" + otherSidePos);

        print("���S" + centerPos);

        return (Vector3.Cross(startDirection, endDirection), centerPos);
    }
}
