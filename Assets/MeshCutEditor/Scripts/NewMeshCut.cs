using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewMeshCut : MonoBehaviour
{
    // @�ϐ��錾��static����private��
    // �ؒf�Ώۂ̃��b�V��
    private Mesh _targetMesh = default;

    // �Q�Ɠn���h�~�̂��ߔz��ł��炩���ߐ錾
    // �ؒf�Ώۂ̃��b�V���̒��_(�|���S��)
    private Vector3[] _targetVertices = default;
    // �ؒf�Ώۂ̃��b�V���̖@��
    private Vector3[] _targetNormals = default;
    // �ؒf�Ώۂ̃��b�V����UV
    private Vector2[] _targetUVs = default;

    //���ʂ̕�������n�Er=h(n�͖@��,r�͈ʒu�x�N�g��,h��const(=_planeValue))
    private Vector3 _planeNormal = default;  //n�̕���
    private float _planeValue = default;  //r�̕���

    // ���_���ؒf�ʂɑ΂��ĕ\�ɂ��邩���ɂ��邩
    private bool[] _isFront = default;
    // �ؒf��̃��b�V���ł̐ؒf�O�̒��_�̔ԍ�
    private int[] _trackedArray = default;

    // �ؒf�ʂ𐶐��ς݂�
    private bool _makeCutSurface = default;

    // 
    private Dictionary<int, (int, int)> newVertexDic = new Dictionary<int, (int, int)>(101);

}
