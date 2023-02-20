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

    // ���_�̌������Ǘ�����A�z�z��
    private Dictionary<int, (int, int)> newVertexDirection = new Dictionary<int, (int, int)>(101);

    // @
    private FragmentList fragmentList = new FragmentList();

    //@
    private RoopFragmentCollection roopFragmentCollection = new RoopFragmentCollection();

    // ���ʑ��̒��_
    private List<Vector3> _frontVertices = new List<Vector3>();
    // ��둤�̒��_
    private List<Vector3> _backVertices = new List<Vector3>();
    // ���ʑ��̖@��
    private List<Vector3> _frontNormals = new List<Vector3>();
    // ��둤�̖@��
    private List<Vector3> _backNormals = new List<Vector3>();
    // ���ʑ���UV
    private List<Vector2> _frontUVs = new List<Vector2>();
    // ��둤��UV
    private List<Vector2> _backUVs = new List<Vector2>();

    // ���ʑ��̃T�u���b�V�����
    private List<List<int>> _frontSubmeshIndices = new List<List<int>>();
    // ��둤�̃T�u���b�V�����
    private List<List<int>> _backSubmeshIndices = new List<List<int>>();

    /// <summary>
    /// gameObject��ؒf����2��Mesh�ɂ��ĕԂ��܂�.1�ڂ�Mesh���ؒf�ʂ̖@���ɑ΂��ĕ\��, 2�ڂ������ł�
    /// ���x���؂�悤�ȃI�u�W�F�N�g�ł����_���������Ȃ��悤�ɏ��������Ă���ق�, �ȒP�ȕ��̂Ȃ�ؒf�ʂ�D�����킹�邱�Ƃ��ł��܂�
    /// </summary>
    /// <param name="targetMesh">�ؒf����Mesh</param>
    /// <param name="targetTransform">�ؒf����Mesh��Transform</param>
    /// <param name="planeAnchorPoint">�ؒf�ʏ�̃��[���h��ԏ�ł�1�_</param>
    /// <param name="planeNormalDirection">�ؒf�ʂ̃��[���h��ԏ�ł̖@��</param>
    /// <param name="makeCutSurface">�ؒf���Mesh��D�����킹�邩�ǂ���</param>
    /// <param name="addNewMeshIndices">�V����subMesh����邩(�ؒf�ʂɐV�����}�e���A�������蓖�Ă�ꍇ��true�A���ɐؒf�ʂ̃}�e���A����Renderer�ɂ��Ă���ꍇ��false)</param>
    /// <returns>plane�ɑ΂��Đ��ʑ���Mesh����둤��Mesh</returns>
    public (Mesh frontside, Mesh backside) CutMesh(Mesh targetMesh, Transform targetTransform, Vector3 planeAnchorPoint, Vector3 planeNormalDirection, bool makeCutSurface = true, bool addNewMeshIndices = false)
    {
        // �ؒf�ʂ̖@���x�N�g�����[����������
        if(planeNormalDirection == Vector3.zero)
        {
            Debug.LogError("the normal vector magnitude is zero!");

            Mesh empty = new Mesh();
            empty.vertices = new Vector3[] { };
            return (null, null);
        }

        #region ������
            // Mesh���擾
            _targetMesh = targetMesh;
            // for����_targetMesh����ĂԂ̂͏d���Ȃ�̂Ŕz��Ɋi�[����for���ł͂�������n��(Mesh.vertices�Ȃǂ͎Q�Ƃł͂Ȃ�����R�s�[��Ԃ��Ă�炵��)
            _targetVertices = _targetMesh.vertices;
            _targetNormals = _targetMesh.normals;
            _targetUVs = _targetMesh.uv;

            // ���_�̒���
            int verticesLangth = _targetVertices.Length;
            _makeCutSurface = makeCutSurface;

            _trackedArray = new int[verticesLangth];
            _isFront = new bool[verticesLangth];
            newVertexDirection.Clear();
            roopFragmentCollection.Clear();
            fragmentList.Clear();

            _frontVertices.Clear();
            _frontNormals.Clear();
            _frontUVs.Clear();
            _frontSubmeshIndices.Clear();

            _backVertices.Clear();
            _backNormals.Clear();
            _backUVs.Clear();
            _backSubmeshIndices.Clear();

            // localScale�ɍ��킹��Plane�ɓ����normal�␳��������
            Vector3 scale = targetTransform.localScale;
            _planeNormal = Vector3.Scale(scale, targetTransform.InverseTransformDirection(planeAnchorPoint)).normalized;
        #endregion

        #region �ŏ��ɒ��_�̏�񂾂�����͂���
        // ���[�J����ԏ�ł̐ؒf�ʂ̍��W
        Vector3 anchor = targetTransform.transform.InverseTransformPoint(planeAnchorPoint);
        // �ؒf�ʂ̖@����anchor�̓���
        _planeValue = Vector3.Dot(_planeNormal, anchor);
        {
            float planeNormal_x = _planeNormal.x;
            float planeNormal_y = _planeNormal.y;
            float planeNormal_z = _planeNormal.z;

            float anchor_x = anchor.x;
            float anchor_y = anchor.y;
            float anchor_z = anchor.z;

            int frontCont = 0;
            int backCont = 0;
            for (int i = 0; i < _targetVertices.Length; i++)
            {
                Vector3 targetVerticesPos = _targetVertices[i];
                // plane�̕\���ɂ��邩�����ɂ��邩�𔻒�
                if(_isFront[i] == (planeNormal_x * (targetVerticesPos.x - anchor_x) + 
                                   planeNormal_y *(targetVerticesPos.y - anchor_y) + 
                                    planeNormal_z * (targetVerticesPos.z - anchor_z)) > 0)
                {
                    // ���_����ǉ����Ă���
                    _frontVertices.Add(targetVerticesPos);
                    _frontNormals.Add(_targetNormals[i]);
                    _frontUVs.Add(_targetUVs[i]);
                    // ���Ƃ�Mesh��n�Ԗڂ̒��_���V����Mesh�ŉ��ԖڂɂȂ邩���L�^(�ʂ�\��Ƃ��Ɏg��)
                    _trackedArray[i] = frontCont++;
                }
                else
                {
                    // ���_����ǉ�
                    _backVertices.Add(targetVerticesPos);
                    _backNormals.Add(_targetNormals[i]);
                    _backUVs.Add(_targetUVs[i]);
                    // ���Ƃ�Mesh��n�Ԗڂ̒��_���V����Mesh�ŉ��ԖڂɂȂ邩���L�^
                    _trackedArray[i] = backCont++;
                }
            }
            // ���_���S���Б��Ɋ�����ꍇ�͏I��
            if(frontCont == 0 || backCont == 0)
            {
                return (null, null);
            }
        }
        #endregion

        #region �O�p�`�̃|���S������ǉ�
        // submesh�̔ԍ�
        int submeshCount = _targetMesh.subMeshCount;

        for(int sub = 0; sub < submeshCount; sub++)
        {
            // �|���S�����̔z��(indices:index�̕����`)
            int[] indices = _targetMesh.GetIndices(sub);

            // �|���S�����`�����钸�_�̔ԍ�������int�̔z��(submesh���Ƃɒǉ�)
            List<int> frontIndices = new List<int>();
            _frontSubmeshIndices.Add(frontIndices);
            List<int> backIndices = new List<int>();
            _backSubmeshIndices.Add(backIndices);

            // �|���S���̏��͒��_3��1�Z�b�g�Ȃ̂�3��΂��Ń��[�v
            for(int i = 0; i < indices.Length; i += 3)
            {
                int peak1;
                int peak2;
                int peak3;
                peak1 = indices[i];
                peak2 = indices[i + 1];
                peak3 = indices[i + 2];

                // �\�ߌv�Z���Ă��������ʂ������Ă���(�����Ōv�Z����Ɠ������_�ɂ������ĉ���������v�Z�����邱�ƂɂȂ邩��)
                bool side1 = _isFront[peak1];
                bool side2 = _isFront[peak2];
                bool side3 = _isFront[peak3];

                // 3�Ƃ��������ɂ��鎞�͂��̂܂�
                if(side1 && side2 && side3)
                {
                    // indices�͐ؒf�O��Mesh�̒��_�ԍ��������Ă���̂�_trackedArry���o�R���邱�ƂŐؒf���Mesh�ł̔ԍ��ɕς��Ă���
                    frontIndices.Add(_trackedArray[peak1]);
                    frontIndices.Add(_trackedArray[peak2]);
                    frontIndices.Add(_trackedArray[peak3]);
                }
                else if(!side1 && !side2 && !side3)
                {
                    frontIndices.Add(_trackedArray[peak1]);
                    frontIndices.Add(_trackedArray[peak2]);
                    frontIndices.Add(_trackedArray[peak3]);
                }
                else
                {
                    // �O�p�|���S�����`������e�_�Ŗʂɑ΂���\�����قȂ�ꍇ, �܂�ؒf�ʂƏd�Ȃ��Ă��镽�ʂ͕�������
                    Sepalate(new bool[3] { side1, side2, side3 }, new int[3] { peak1, peak2, peak3 }, sub);
                }
            }
        }
        #endregion

        #region �ؒf���ꂽ�|���S���͂����ł��ꂼ��̃��b�V���ɒǉ������
        fragmentList.MakeTriangle();

        if (makeCutSurface)
        {
            if (addNewMeshIndices)
            {
                // submesh��������̂Ń��X�g��ǉ�
                _frontSubmeshIndices.Add(new List<int>());
                _backSubmeshIndices.Add(new List<int>());
            }
            roopFragmentCollection.MakeCutSurface(_frontSubmeshIndices.Count - 1, targetTransform);
        }
        #endregion

        #region 2��Mesh��V�K�ɍ���Ă��ꂼ��ɏ���ǉ����ďo��
        Mesh frontMesh = new Mesh();
        frontMesh.name = "Split Mesh front";

        frontMesh.vertices = _frontVertices.ToArray();
        frontMesh.normals = _frontNormals.ToArray();
        frontMesh.uv = _frontUVs.ToArray();

        frontMesh.subMeshCount = _frontSubmeshIndices.Count;
        for(int i = 0; i < _frontSubmeshIndices.Count; i++)
        {
            // ���b�V���̍쐬
            frontMesh.SetIndices(_frontSubmeshIndices[i].ToArray(), MeshTopology.Triangles, i, false);
        }

        Mesh backMesh = new Mesh();
        backMesh.name = "Splite Mesh back";
        backMesh.vertices = _backVertices.ToArray();
        backMesh.normals = _backNormals.ToArray();
        backMesh.uv = _backUVs.ToArray();

        backMesh.subMeshCount = _backSubmeshIndices.Count;
        for(int i = 0; i < _backSubmeshIndices.Count; i++)
        {
            // ���b�V���̍쐬
            backMesh.SetIndices(_backSubmeshIndices[i].ToArray(), MeshTopology.Triangles, i, false);
        }

        return (frontMesh, backMesh);
        #endregion
    }

    /// <summary>
    /// Mesh��ؒf���܂�. 
    /// 1�ڂ�GameObject���@���̌����Ă�������ŐV����Instantiate��������, 1�ڂ�GameObject���@���Ɣ��Ε����œ��͂������̂�Ԃ��܂�
    /// </summary>
    /// <param name="targetGameObject">�ؒf�����GameObject</param>
    /// <param name="planeAnchorPoint">�ؒf���ʏ�̂ǂ���1�_(���[���h���W)</param>
    /// <param name="planeNormalDirection">�ؒf���ʂ̖@��(���[���h���W)</param>
    /// <param name="makeCutSurface">�ؒf�ʂ���邩�ǂ���</param>
    /// <param name="cutSurfaceMaterial">�ؒf�ʂɊ��蓖�Ă�}�e���A��(null�̏ꍇ�͓K���ȃ}�e���A�������蓖�Ă�)</param>
    /// <returns></returns>
    public (GameObject copy_normaliside,GameObject original_anitiNormalside)CutObject(GameObject targetGameObject, Vector3 planeAnchorPoint,Vector3 planeNormalDirection,bool makeCutSurface,Material cutSurfaceMaterial = null)
    {
        if (!targetGameObject.GetComponent<MeshFilter>())
        {
            Debug.LogError("�����̃I�u�W�F�N�g�ɂ�MeshFilter���A�^�b�`���Ă�������");
            return (null, null);
        }
        else if (!targetGameObject.GetComponent<MeshRenderer>())
        {
            Debug.LogError("�����̃I�u�W�F�N�g�ɂ�Meshrenderer���A�^�b�`���Ă�������");
            return (null, null);
        }

        Mesh targetMesh = targetGameObject.GetComponent<MeshFilter>().mesh;
        Transform targetTransform = targetGameObject.transform;
        // �ؒf�ʂɃ}�e���A�������蓖�Ă��Ă��邩�ǂ���
        bool addNewMaterial;

        MeshRenderer renderer = targetGameObject.GetComponent<MeshRenderer>();
        // material�ɃA�N�Z�X����Ƃ��̏u�Ԃ�material�̌ʂ̃C���X�^���X������ă}�e���A������(instance)�����Ă��܂��̂œǂݍ��݂�sharedMaterial�ōs��
        Material[] materials = renderer.sharedMaterials;
        if(makeCutSurface && cutSurfaceMaterial != null)
        {
            // ���łɐؒf�ʂɃ}�e���A�����ǉ�����Ă���Ƃ��͂�����g���̂Œǉ����Ȃ�
            if (materials[materials.Length - 1]?.name == cutSurfaceMaterial.name)
            {
                addNewMaterial = false;
            }
            else
            {
                addNewMaterial = true;
            }
        }
        else
        {
            addNewMaterial = false;
        }

        (Mesh fragMeah, Mesh originMesh) = CutMesh(targetMesh, targetTransform, planeAnchorPoint, planeNormalDirection, makeCutSurface, addNewMaterial);

        if(originMesh == null || fragMeah == null)
        {
            return(null, null);
        }

        if (addNewMaterial)
        {
            int materialLength = materials.Length;
            Material[] newMaterials = new Material[materialLength + 1];
            materials.CopyTo(newMaterials, 0);
            newMaterials[materialLength] = cutSurfaceMaterial;

            renderer.sharedMaterials = newMaterials;
        }

        targetGameObject.GetComponent<MeshFilter>().mesh = originMesh;

        Transform originTransform = targetTransform.transform;
        GameObject fragment = Instantiate(targetGameObject, originTransform.position, originTransform.rotation, originTransform.parent);
        fragment.transform.parent = null;
        fragment.GetComponent<MeshFilter>().mesh = fragMeah;
        fragment.GetComponent<MeshRenderer>().sharedMaterials = targetGameObject.GetComponent<MeshRenderer>().sharedMaterials;
        
        if(targetGameObject.GetComponent<MeshCollider>())
        {
            // ���_��1�_�ɏd�Ȃ��Ă���ꍇ�̓G���[���o��̂ŁA
            targetGameObject.GetComponent<MeshCollider>().sharedMesh = originMesh;
            fragment.GetComponent<MeshCollider>().sharedMesh = fragMeah;
        }
        return (fragment, targetGameObject);
    }

    /// <summary>
    /// �|���S����ؒf
    /// �|���S���͐ؒf�ʂ̕\���Ɨ����ɕ��������.
    /// ���̂Ƃ��O�p�|���S����\�ʂ��猩��, �Ȃ����ؒf�ʂ̕\���ɂ��钸�_�����ɗ���悤�Ɍ���,
    /// �O�p�`�̍����̕ӂ��`������_��front0,back0, �E���ɂ���ӂ����_��f1,b1�Ƃ���.(f�͕\���ɂ���_��b�͗���)(���_��3�Ȃ̂Ŕ�肪���݂���)
    /// �����Ń|���S���̌��������߂Ă����ƌ�X�ƂĂ��֗�
    /// �ȍ~�����ɂ�����̂�0,�E���ɂ�����̂�1�����Ĉ���
    /// (�Ђ���Ƃ���Ǝ��ۂ̌����͋t��������Ȃ�����vertexIndices�Ɠ����܂����ŏo�͂��Ă�̂ŋt�ł����͂Ȃ�)
    /// </summary>
    /// <param name="sides"></param>
    /// <param name="vertexIndices"></param>
    /// <param name="submesh"></param>
    private static void Sepalate(bool[] sides, int[] vertexIndices, int submesh)
    {
        // ���_��index�ԍ����i�[����̂Ɏg�p
        // �O�p�`�̍����ɂ���ӂ����_
        int frontLeft = 0;
        int backLeft = 0;
        // �O�p�`�̉E���ɂ���ӂ����_
        int frontRight = 0;
        int backRight = 0;

        // �ǂ��炪�ɒ��_��2���邩
        bool twoPointsInFrontSide;

        // �|���S���̌����𑵂���
        if(sides[0])
        {
            if (sides[1])
            {
                frontLeft = vertexIndices[1];
                frontRight = vertexIndices[0];
                backLeft = backRight = vertexIndices[2];
                twoPointsInFrontSide = true;
            }
        }
    }
}
