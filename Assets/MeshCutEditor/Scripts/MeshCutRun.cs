using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    //���ϒl����邽�߂�
    const int TWO = 2;

    void Start()
    {
        _meshCut = new MeshCut();
    }
    void Update()
    {
        //�ؒf�ʂ𐶐�����Ray
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButton(0) && Physics.Raycast(ray, out hit))
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
            Vector3 normal = CalculationNormal(_hitPositions[0], _hitPositions[_hitPositions.Count + LIST_END]).normal;
            _meshCut.Cut(_tergetObject, center, normal, _material);
            //���X�g�̏�����
            _hitPositions.Clear();
            isCutting = false;
        }
    }

    /// <summary>
    /// �@���x�N�g�����v�Z���郁�\�b�h
    /// </summary>
    /// <param name="startPos">�n�_�̍��W</param>
    /// <param name="endPos">�I�_�̍��W</param>
    /// <returns>�@���x�N�g��</returns>
    private (Vector3 normal, Vector3 center) CalculationNormal(Vector3 startPos, Vector3 endPos)
    {
        //�ؒf�ʂ̒��S���v�Z
        Vector3 centerPos = (startPos + endPos) / TWO;


        //�@���x�N�g���v�Z
        //���_�ƂȂ�J�����̃|�W�V����
        Vector3 cameraPos = Camera.main.transform.position;
        //���_����n�_�̕���
        Vector3 startDirection = (startPos - cameraPos);
        //���_����I�_�̕���
        Vector3 endDirection = (endPos - cameraPos);

        return (Vector3.Cross(startDirection, endDirection), centerPos);
    }
}
