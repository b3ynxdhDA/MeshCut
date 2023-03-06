using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������x���ӎ��������b�V���ؒf�N���X
/// </summary>
public class NewMeshCut : MonoBehaviour
{
    // �ؒf�Ώۂ̃��b�V��
    static Mesh _targetMesh;

    // �Q�Ɠn���h�~�̂��ߔz��ł��炩���ߐ錾
    // @����3�͂߂�����厖�ł��ꏑ���Ȃ���10�{���炢�d���Ȃ�(for�����Ŏg������Q�Ɠn�����Ƃ�΂�)
    // �ؒf�Ώۂ̃��b�V���̒��_(�|���S��)
    static Vector3[] _targetVertices;
    // �ؒf�Ώۂ̃��b�V���̖@��
    static Vector3[] _targetNormals;
    // �ؒf�Ώۂ̃��b�V����UV
    static Vector2[] _targetUVs;   

    //���ʂ̕�������n�Er=h(n�͖@��,r�͈ʒu�x�N�g��,h��const(=_planeValue))
    static Vector3 _planeNormal; //n�̕���
    static float _planeValue;    //r�̕���

    //���_���ؒf�ʂɑ΂��ĕ\�ɂ��邩���ɂ��邩
    static bool[] _isFront;
    //�ؒf���Mesh�ł̐ؒf�O�̒��_�̔ԍ���ǐՂ���z��
    static int[] _trackedArray;

    // �ؒf�ʂ𐶐��ς݂�
    static bool _isMakeCutSurface;

    // ���_�̌������Ǘ�����A�z�z��
    static Dictionary<int, (int, int)> newVertexDic = new Dictionary<int, (int, int)>(101);

    // FragmentList�N���X�̎Q��
    static FragmentList fragmentList = new FragmentList();

    // RoopFragmentCallection�N���X�̎Q��
    static RoopFragmentCollection roopCollection = new RoopFragmentCollection();

    // ���ʑ��̒��_
    static List<Vector3> _frontVertices = new List<Vector3>();
    // ��둤�̒��_
    static List<Vector3> _backVertices = new List<Vector3>();
    // ���ʑ��̖@��
    static List<Vector3> _frontNormals = new List<Vector3>();
    // ��둤�̖@��
    static List<Vector3> _backNormals = new List<Vector3>();
    // ���ʑ���UV
    static List<Vector2> _frontUVs = new List<Vector2>();
    // ��둤��UV
    static List<Vector2> _backUVs = new List<Vector2>();

    // ���ʑ��̃T�u���b�V�����
    static List<List<int>> _frontSubmeshIndices = new List<List<int>>();
    // ��둤�̃T�u���b�V�����
    static List<List<int>> _backSubmeshIndices = new List<List<int>>();

    /// <summary>
    /// NewMeshCut�N���X���ŌĂяo����郁�\�b�h
    /// <para>���b�V����ؒf����2��Mesh�ɂ��ĕԂ��܂�</para>
    /// <para>���x���؂�悤�ȃI�u�W�F�N�g�ł����_���������Ȃ��悤�ɏ��������Ă���ق�, �ȒP�ȕ��̂Ȃ�ؒf�ʂ�D�����킹�邱�Ƃ��ł��܂�</para>
    /// </summary>
    /// <param name="targetMesh">�ؒf����Mesh</param>
    /// <param name="targetTransform">�ؒf����Mesh��Transform</param>
    /// <param name="planeAnchorPoint">�ؒf�ʏ�̃��[���h��ԏ�ł�1�_</param>
    /// <param name="planeNormalDirection">�ؒf�ʂ̃��[���h��ԏ�ł̖@��</param>
    /// <param name="makeCutSurface">�ؒf���Mesh��D�����킹�邩�ۂ�</param>
    /// <param name="addNewMeshIndices">�V����subMesh����邩(�ؒf�ʂɐV�����}�e���A�������蓖�Ă�ꍇ�ɂ�true, ���łɐؒf�ʂ̃}�e���A����Renderer�ɂ��Ă�ꍇ��false)</param>
    /// <returns>frontside���ؒf�ʂ̖@���ɑ΂��ĕ\��, backside�������ł�</returns>
    public static (Mesh frontside, Mesh backside) CutMesh(Mesh targetMesh, Transform targetTransform, Vector3 planeAnchorPoint, Vector3 planeNormalDirection, bool makeCutSurface = true, bool addNewMeshIndices = false)
    {
        // �ؒf�ʂ̖@���x�N�g�����[����������
        if (planeNormalDirection == Vector3.zero)
        {
            Debug.LogError("�@���x�N�g���̑傫���̓[���ł�!");

            Mesh empty = new Mesh();
            empty.vertices = new Vector3[] { };
            return (null, null);
        }


        #region ������
        //Mesh���擾
        _targetMesh = targetMesh;
        //for����_targetMesh����ĂԂ͔̂��ɏd���Ȃ�̂ł����Ŕz��Ɋi�[����for���ł͂�������n��(Mesh.vertices�Ȃǂ͎Q�Ƃł͂Ȃ��Ė���R�s�[�������̂�Ԃ��Ă�炵��)
        _targetVertices = _targetMesh.vertices;
        _targetNormals = _targetMesh.normals;
        _targetUVs = _targetMesh.uv;


        // ���_�̒���
        int verticesLength = _targetVertices.Length;
        _isMakeCutSurface = makeCutSurface;

        _trackedArray = new int[verticesLength];
        _isFront = new bool[verticesLength];
        newVertexDic.Clear();
        roopCollection.Clear();
        fragmentList.Clear();

        _frontVertices.Clear();
        _frontNormals.Clear();
        _frontUVs.Clear();
        _frontSubmeshIndices.Clear();

        _backVertices.Clear();
        _backNormals.Clear();
        _backUVs.Clear();
        _backSubmeshIndices.Clear();

        //localscale�ɍ��킹��Plane�ɓ����normal�ɕ␳��������
        Vector3 scale = targetTransform.localScale;
        _planeNormal = Vector3.Scale(scale, targetTransform.InverseTransformDirection(planeNormalDirection)).normalized;
        #endregion

        #region �ŏ��ɒ��_�̏�񂾂�����͂��Ă���

        // ���[�J����ԏ�ł̐ؒf�ʂ̍��W
        Vector3 anchor = targetTransform.transform.InverseTransformPoint(planeAnchorPoint);
        // �ؒf�ʂ̖@����anchor�̓���
        _planeValue = Vector3.Dot(_planeNormal, anchor);
        {
            float planeNormal_x = _planeNormal.x;
            float planeNormal_y = _planeNormal.y;
            float planeNormal_z = _planeNormal.z;

            float anchor_x = anchor.x;
            float ancy = anchor.y;
            float anchor_z = anchor.z;

            int frontCount = 0;
            int backCount = 0;
            for (int i = 0; i < _targetVertices.Length; i++)
            {
                Vector3 pos = _targetVertices[i];
                // ���_��plane�̕\���ɂ��邩�����ɂ��邩�𔻒�
                if (_isFront[i] = (planeNormal_x * (pos.x - anchor_x) +
                    planeNormal_y * (pos.y - ancy) +
                    planeNormal_z * (pos.z - anchor_z)) > 0)
                {
                    //���_����ǉ����Ă���
                    _frontVertices.Add(pos);
                    _frontNormals.Add(_targetNormals[i]);
                    _frontUVs.Add(_targetUVs[i]);
                    //���Ƃ�Mesh��n�Ԗڂ̒��_���V����Mesh�ŉ��ԖڂɂȂ�̂����L�^(�ʂ�\��Ƃ��Ɏg��)
                    _trackedArray[i] = frontCount++;
                }
                else
                {
                    // ���_����ǉ�
                    _backVertices.Add(pos);
                    _backNormals.Add(_targetNormals[i]);
                    _backUVs.Add(_targetUVs[i]);
                    // ���Ƃ�Mesh��n�Ԗڂ̒��_���V����Mesh�ŉ��ԖڂɂȂ邩���L�^
                    _trackedArray[i] = backCount++;
                }
            }

            // �Б��ɑS��������ꍇ�͂����ŏI��
            if (frontCount == 0 || backCount == 0)
            {
                return (null, null);
            }
        }
        #endregion

        #region �O�p�`�̃|���S������ǉ�

        // submesh�̔ԍ�
        int submeshCount = _targetMesh.subMeshCount;

        for (int sub = 0; sub < submeshCount; sub++)
        {
            // �|���S�����̔z��(indices:index�̕����`)
            int[] indices = _targetMesh.GetIndices(sub);

            //�|���S�����`�����钸�_�̔ԍ�������int�̔z�������Ă���.(submesh���Ƃɒǉ�)
            List<int> frontIndices = new List<int>();
            _frontSubmeshIndices.Add(frontIndices);
            List<int> backIndices = new List<int>();
            _backSubmeshIndices.Add(backIndices);

            //�|���S���̏��͒��_3��1�Z�b�g�Ȃ̂�3��΂��Ń��[�v
            for (int i = 0; i < indices.Length; i += 3)
            {
                int peak1;
                int peak2;
                int peak3;
                peak1 = indices[i];
                peak2 = indices[i + 1];
                peak3 = indices[i + 2];


                //�\�ߌv�Z���Ă��������ʂ������Ă���(�����Ōv�Z����Ɠ������_�ɂ������ĉ���������v�Z�����邱�ƂɂȂ邩��)
                // �O�p�`�̒��_���ؒf�ʂ̕\���ǂ��瑤�ɂ��邩
                bool side1 = _isFront[peak1];
                bool side2 = _isFront[peak2];
                bool side3 = _isFront[peak3];


                //3�Ƃ��\��, 3�Ƃ������̂Ƃ��͂��̂܂܏o��
                if (side1 && side2 && side3)
                {
                    //indices�͐ؒf�O��Mesh�̒��_�ԍ��������Ă���̂�_trackedArray��ʂ����ƂŐؒf���Mesh�ł̔ԍ��ɕς��Ă���
                    frontIndices.Add(_trackedArray[peak1]);
                    frontIndices.Add(_trackedArray[peak2]);
                    frontIndices.Add(_trackedArray[peak3]);
                }
                else if (!side1 && !side2 && !side3)
                {
                    backIndices.Add(_trackedArray[peak1]);
                    backIndices.Add(_trackedArray[peak2]);
                    backIndices.Add(_trackedArray[peak3]);
                }
                else
                {
                    //�O�p�|���S�����`������e�_�Ŗʂɑ΂���\�����قȂ�ꍇ, �܂�ؒf�ʂƏd�Ȃ��Ă��镽�ʂ͕�������.
                    Sepalate(new bool[3] { side1, side2, side3 }, new int[3] { peak1, peak2, peak3 }, sub);
                }

            }

        }
        #endregion

        // �ؒf���ꂽ�|���S���͂����ł��ꂼ���Mesh�ɒǉ������
        fragmentList.MakeTriangle();

        if (makeCutSurface)
        {
            if (addNewMeshIndices)
            {
                //submesh��������̂Ń��X�g�ǉ�
                _frontSubmeshIndices.Add(new List<int>());
                _backSubmeshIndices.Add(new List<int>());
            }
            roopCollection.MakeCutSurface(_frontSubmeshIndices.Count - 1, targetTransform);
        }

        #region 2��Mesh��V�K�ɍ���Ă��ꂼ��ɏ���ǉ����ďo��

        // �ؒf�ʂ̕\���ɂ��郁�b�V��
        Mesh frontMesh = new Mesh();
        frontMesh.name = "Split Mesh front";

        frontMesh.vertices = _frontVertices.ToArray();
        frontMesh.normals = _frontNormals.ToArray();
        frontMesh.uv = _frontUVs.ToArray();

        // �T�u���b�V���̃J�E���g��i�߂�
        frontMesh.subMeshCount = _frontSubmeshIndices.Count;
        for (int i = 0; i < _frontSubmeshIndices.Count; i++)
        {
            // ���b�V���̐���
            frontMesh.SetIndices(_frontSubmeshIndices[i].ToArray(), MeshTopology.Triangles, i, false);
        }

        // �ؒf�ʂ̗����ɂ��郁�b�V��
        Mesh backMesh = new Mesh();
        backMesh.name = "Split Mesh back";
        backMesh.vertices = _backVertices.ToArray();
        backMesh.normals = _backNormals.ToArray();
        backMesh.uv = _backUVs.ToArray();

        // �T�u���b�V���̃J�E���g��i�߂�
        backMesh.subMeshCount = _backSubmeshIndices.Count;
        for (int i = 0; i < _backSubmeshIndices.Count; i++)
        {
            // ���b�V������
            backMesh.SetIndices(_backSubmeshIndices[i].ToArray(), MeshTopology.Triangles, i, false);
        }
        #endregion

        return (frontMesh, backMesh);
    }

    /// <summary>
    /// �O�̃N���X����Ăяo�����\�b�h
    /// GameObject��ؒf���ĂQ��GameObject��Ԃ�
    /// </summary>
    /// <param name="targetGameObject">�ؒf�����GameObject</param>
    /// <param name="planeAnchorPoint">�ؒf���ʏ�̂ǂ���1�_(���[���h���W)</param>
    /// <param name="planeNormalDirection">�ؒf���ʂ̖@��(���[���h���W)</param>
    /// <param name="makeCutSurface">�ؒf�ʂ���邩�ǂ���</param>
    /// <param name="cutSurfaceMaterial">�ؒf�ʂɊ��蓖�Ă�}�e���A��(null�̏ꍇ�͓K���ȃ}�e���A�������蓖�Ă�)</param>
    /// <returns>copy_normalside���@���̌����Ă�������ŐV����Instantiate��������,original_anitiNormalside���@���Ɣ��Ε����œ��͂�������</returns>
    public static (GameObject copy_normalside, GameObject original_anitiNormalside) CutGameObject(GameObject targetGameObject, Vector3 planeAnchorPoint, Vector3 planeNormalDirection, bool makeCutSurface = true, Material cutSurfaceMaterial = null)
    {
        // �ؒf�Ώۂ�MeshFilter���A�^�b�`����Ă��邩
        if (!targetGameObject.GetComponent<MeshFilter>())
        {
            Debug.LogError("�����̃I�u�W�F�N�g�ɂ�MeshFilter���A�^�b�`����!");
            return (null, null);
        }
        // �ؒf�Ώۂ�MeshRenderer���A�^�b�`����Ă��邩
        else if (!targetGameObject.GetComponent<MeshRenderer>())
        {
            Debug.LogError("�����̃I�u�W�F�N�g�ɂ�Meshrenderer���A�^�b�`����!");
            return (null, null);
        }

        Mesh mesh = targetGameObject.GetComponent<MeshFilter>().mesh;
        Transform transform = targetGameObject.transform;
        // �ؒf�ʂɃ}�e���A�������蓖�Ă��Ă��邩�ǂ���
        bool addNewMaterial;

        MeshRenderer renderer = targetGameObject.GetComponent<MeshRenderer>();
        //material�ɃA�N�Z�X����Ƃ��̏u�Ԃ�material�̌ʂ̃C���X�^���X������ă}�e���A������(instance)�����Ă��܂��̂œǂݍ��݂�sharedMaterial�ōs��
        Material[] materials = renderer.sharedMaterials;
        // �ؒf�ʂ𖄂߂邩 && �ؒf�ʂɐV�����}�e���A����ݒ肷�邩
        if (makeCutSurface && cutSurfaceMaterial != null)
        {
            // ���łɐؒf�}�e���A�����ǉ�����Ă���Ƃ��͂�����g���̂Œǉ����Ȃ�
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

        (Mesh fragMesh, Mesh originMesh) = CutMesh(mesh, transform, planeAnchorPoint, planeNormalDirection, makeCutSurface, addNewMaterial);

        // ���Âꂩ�̃��b�V���������ł��Ă��Ȃ����
        if (originMesh == null || fragMesh == null)
        {
            return (null, null);

        }
        // �V�����}�e���A����ǉ����邩
        if (addNewMaterial)
        {
            int matLength = materials.Length;
            Material[] newMats = new Material[matLength + 1];
            materials.CopyTo(newMats, 0);
            newMats[matLength] = cutSurfaceMaterial;


            renderer.sharedMaterials = newMats;
        }

        // ���̃I�u�W�F�N�g�̃��b�V����ؒf�ʂɑ΂��ė����̃��b�V���ɕς���
        targetGameObject.GetComponent<MeshFilter>().mesh = originMesh;
        // �؂ꂽ�I�u�W�F�N�g���n�ʂɒ����Ə�����X�N���v�g�����Ă��Ȃ�������
        if (targetGameObject.GetComponent<DestroyObject>() == null)
        {
            targetGameObject.AddComponent<DestroyObject>();
        }
        
        // �ؒf�ʂɑ΂��ĕ\���̃��b�V���̃I�u�W�F�N�g�𐶐�
        Transform originTransform = targetGameObject.transform;
        GameObject fragment = Instantiate(targetGameObject, originTransform.position, originTransform.rotation, originTransform.parent);
        fragment.transform.parent = null;
        fragment.GetComponent<MeshFilter>().mesh = fragMesh;
        fragment.GetComponent<MeshRenderer>().sharedMaterials = targetGameObject.GetComponent<MeshRenderer>().sharedMaterials;
        // �ؒf�Ώۂ������Ă�����velocity�𑵂���
        if (fragment.GetComponent<Rigidbody>())
        {
            fragment.GetComponent<Rigidbody>().velocity = targetGameObject.GetComponent<Rigidbody>().velocity;
        }

        // �ؒf�Ώۂ̃I�u�W�F�N�g�ɃR���C�_�[�����Ă��邩
        if (targetGameObject.GetComponent<Collider>())
        {
            // �ؒf�Ώۂ̃I�u�W�F�N�g��MeshCollider�����Ă��邩
            if (targetGameObject.GetComponent<MeshCollider>())
            {
                // ���_��1�_�ɏd�Ȃ��Ă���ꍇ�ɂ̓G���[���o��̂�, ���������ꍇ��mesh.RecalculateBounds�̂��Ƃ�mesh.bounds.size.magnitude<0.00001�Ȃǂŏ����������đΏ����Ă�������
                targetGameObject.GetComponent<MeshCollider>().sharedMesh = originMesh;
                fragment.GetComponent<MeshCollider>().sharedMesh = fragMesh;
            }
            // MeshCollider�ȊO��Collider�̏ꍇ��
            else
            {
                // ���ɂ���R���C�_�[���폜����
                Destroy(targetGameObject.GetComponent<Collider>());
                Destroy(fragment.GetComponent<Collider>());
                // MeshCollider�����b�V�������ăA�^�b�`
                targetGameObject.AddComponent<MeshCollider>().sharedMesh = originMesh;
                fragment.AddComponent<MeshCollider>().sharedMesh = fragMesh;
                // MeshCollider��convex��True�ɂ���
                targetGameObject.GetComponent<MeshCollider>().convex = true;
                fragment.GetComponent<MeshCollider>().convex = true;
            }
        }


        return (fragment, targetGameObject);
    }

    /// <summary>
    /// �|���S���𕪗�����
    /// �|���S���͐ؒf�ʂ̕\���Ɨ����ɕ��������.
    /// ���̂Ƃ��O�p�|���S����\�ʂ��猩��, �Ȃ����ؒf�ʂ̕\���ɂ��钸�_�����ɗ���悤�Ɍ���,
    /// �O�p�`�̕ӂ��`�����钸�_��3�Ȃ̂Ŕ�肪���݂���
    /// �����Ń|���S���̌��������߂Ă����ƌ�X�ƂĂ��֗�
    /// �ȍ~�����ɂ�����̂�0,�E���ɂ�����̂�1�����Ĉ���
    /// (�Ђ���Ƃ���Ǝ��ۂ̌����͋t��������Ȃ�����vertexIndices�Ɠ����܂����ŏo�͂��Ă�̂ŋt�ł����͂Ȃ�)
    /// </summary>
    /// <param name="sides"></param>
    /// <param name="vertexIndices">�O�p�`�̒��_�̔ԍ�</param>
    /// <param name="submesh">�T�u���b�V���̔ԍ�</param>
    private static void Sepalate(bool[] sides, int[] vertexIndices, int submesh)
    {
        #region ���_��index�ԍ����i�[����
        // �O�p�`�̍����ɂ���ӂ����_
        int frontLeft = 0;
        int backLeft = 0;
        // �O�p�`�̉E���ɂ���ӂ����_
        int frontRight = 0;
        int backRight = 0;

        //�ǂ��炪�ɒ��_��2���邩
        bool twoPointsInFrontSide;

        //�|���S���̌����𑵂���
        if (sides[0])
        {
            if (sides[1])
            {
                frontLeft = vertexIndices[1];
                frontRight = vertexIndices[0];
                backLeft = backRight = vertexIndices[2];
                twoPointsInFrontSide = true;
            }
            else
            {
                if (sides[2])
                {
                    frontLeft = vertexIndices[0];
                    frontRight = vertexIndices[2];
                    backLeft = backRight = vertexIndices[1];
                    twoPointsInFrontSide = true;
                }
                else
                {
                    frontLeft = frontRight = vertexIndices[0];
                    backLeft = vertexIndices[1];
                    backRight = vertexIndices[2];
                    twoPointsInFrontSide = false;
                }
            }
        }
        else
        {
            if (sides[1])
            {
                if (sides[2])
                {
                    frontLeft = vertexIndices[2];
                    frontRight = vertexIndices[1];
                    backLeft = backRight = vertexIndices[0];
                    twoPointsInFrontSide = true;
                }
                else
                {
                    frontLeft = frontRight = vertexIndices[1];
                    backLeft = vertexIndices[2];
                    backRight = vertexIndices[0];
                    twoPointsInFrontSide = false;
                }
            }
            else
            {
                frontLeft = frontRight = vertexIndices[2];
                backLeft = vertexIndices[0];
                backRight = vertexIndices[1];
                twoPointsInFrontSide = false;
            }
        }
        #endregion

        #region �ؒf�O�̃|���S���̒��_�̍��W���擾(��2�͏d������)
        Vector3 frontPoint0 = default;
        Vector3 frontPoint1 = default;
        Vector3 backPoint0 = default;
        Vector3 backPoint1 = default;
        if (twoPointsInFrontSide)
        {
            frontPoint0 = _targetVertices[frontLeft];
            frontPoint1 = _targetVertices[frontRight];
            backPoint0 = backPoint1 = _targetVertices[backLeft];
        }
        else
        {
            frontPoint0 = frontPoint1 = _targetVertices[frontLeft];
            backPoint0 = _targetVertices[backLeft];
            backPoint1 = _targetVertices[backRight];
        }
        #endregion

        // �V�������_�̍��W
        // �����p�����[�^���x�N�g��[backPoint0 - frontPoint0]�����{������ؒf���ʂɓ��B���邩�͈ȉ��̎��ŕ\�����
        // ���ʂ̎�: dot(r ,n)=A ,A�͒萔,n�͖@��, 
        // ����    r =frontPoint0+k*(backPoint0 - frontPoint0), (0 �� k �� 1)
        // �����, �V�����ł��钸�_��2�̒��_�����Ή��ɓ������Ăł���̂����Ӗ����Ă���
        float dividingParameter0 = (_planeValue - Vector3.Dot(_planeNormal, frontPoint0)) / (Vector3.Dot(_planeNormal, backPoint0 - frontPoint0));
        // Lerp�Őؒf�ɂ���Ă��܂��V�������_�̍��W�𐶐�
        Vector3 newVertexPos0 = Vector3.Lerp(frontPoint0, backPoint0, dividingParameter0);

        float dividingParameter1 = (_planeValue - Vector3.Dot(_planeNormal, frontPoint1)) / (Vector3.Dot(_planeNormal, backPoint1 - frontPoint1));
        Vector3 newVertexPos1 = Vector3.Lerp(frontPoint1, backPoint1, dividingParameter1);

        // �V�������_�̐���, �����ł�Normal��UV�͌v�Z�����ォ��v�Z�ł���悤�ɒ��_��index(_trackedArray[frontLeft], _trackedArray[backLeft],)�Ɠ����_�̏��(dividingParameter0)�������Ă���
        NewVertex vertex0 = new NewVertex(_trackedArray[frontLeft], _trackedArray[backLeft], dividingParameter0, newVertexPos0);
        NewVertex vertex1 = new NewVertex(_trackedArray[frontRight], _trackedArray[backRight], dividingParameter1, newVertexPos1);

        //�ؒf�łł����(���ꂪ�����|���S���͌�Ō������Ē��_���̑�����}����)
        Vector3 cutLine = (newVertexPos1 - newVertexPos0).normalized;
        int KEY_CUTLINE = MakeIntFromVector3_ErrorCut(cutLine);//Vector3���Ə������d�����Ȃ̂�int�ɂ��Ă���, ���łɊۂߌ덷��؂藎�Ƃ�

        //�ؒf�����܂�Fragment�N���X���烁�\�b�h���Ăяo��
        Fragment fragment = new Fragment(vertex0, vertex1, twoPointsInFrontSide, KEY_CUTLINE, submesh);
        //List�ɒǉ�����List�̒��œ��ꕽ�ʂ�Fragment�͌����Ƃ�����
        fragmentList.Add(fragment, KEY_CUTLINE);
    }

    #region �N���X

    /// <summary>
    /// 
    /// </summary>
    class RoopFragment
    {
        public RoopFragment next; //�E�ׂ̂��
        public Vector3 rightPosition;//�E���̍��W(�����̍��W�͍��ׂ̂�������Ă�)
        public RoopFragment(Vector3 _rightPosition)
        {
            next = null;
            rightPosition = _rightPosition;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    class RooP
    {
        // ���[�v�̍��[
        public RoopFragment start = default;
        // ���[�v�̉E��
        public RoopFragment end = default;
        // ���[�v�̍��[��Position
        public Vector3 startPos = default;
        // ���[�v�̉E����Position
        public Vector3 endPos = default;
        // �܂܂�钸�_��(���[�v������܂ł͒��_��-1�̒l�ɂȂ�)
        public int verticesCount;
        // Position�̘a.�����count�Ŋ���Ɛ}�`�̒��_��������
        public Vector3 sum_rightPosition;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_left"></param>
        /// <param name="_right"></param>
        /// <param name="_startPos"></param>
        /// <param name="_endPos"></param>
        /// <param name="rightPos"></param>
        public RooP(RoopFragment _left, RoopFragment _right, Vector3 _startPos, Vector3 _endPos, Vector3 rightPos)
        {
            start = _left;
            end = _right;
            startPos = _startPos;
            endPos = _endPos;
            verticesCount = 1;
            sum_rightPosition = rightPos;
        }
    }
    /// <summary>
    /// �ؒf�ʂɏd�Ȃ��Ă��钸�_�Ɋւ��鏈��
    /// </summary>
    public class RoopFragmentCollection
    {
        Dictionary<Vector3, RooP> leftPointDic = new Dictionary<Vector3, RooP>();
        Dictionary<Vector3, RooP> rightPointDic = new Dictionary<Vector3, RooP>();

        /// <summary>
        /// �ؒf�ӂ�ǉ����Ă����ėׂ荇���ӂ����łɂ���΂�������.�ŏI�I�Ƀ��[�v�����
        /// </summary>
        /// <param name="leftVertexPos">�ؒf�ӂ̍��̓_��Position</param>
        /// <param name="rightVertexPos">�ؒf�ӂ̉E�̓_��Position</param>
        public void Add(Vector3 leftVertexPos, Vector3 rightVertexPos)
        {
            // �V�������[�v�ӂ����
            RoopFragment target = new RoopFragment(rightVertexPos);


            RooP roop1 = null;
            bool find1 = default;
            // �����̍���Ƃ������̂͑���̉E��Ȃ̂ŉE��disctionary�̒��Ɋ���leftVertexPos��key�������Ă��Ȃ����`�F�b�N
            if (find1 = rightPointDic.ContainsKey(leftVertexPos))
            {
                roop1 = rightPointDic[leftVertexPos];

                //roop�̉E�[(�I�[)�̉E(next)��target����������
                roop1.end.next = target;
                //roop�̉E�[��target�ɕς���(roop�͍��[�ƉE�[�̏�񂾂��������Ă���)
                roop1.end = target;
                roop1.endPos = rightVertexPos;

                //roop1�����X�g����O��(���ƂŉE��List�̎����̉E��index�̏ꏊ�Ɉڂ�����)
                rightPointDic.Remove(leftVertexPos);
            }

            RooP roop2 = null;
            bool find2 = default;

            // �����̉E��Ƃ������̂͑���̍���Ȃ̂ō���disctionary�̒��Ɋ���rightVertexPos��key�������Ă��Ȃ����`�F�b�N
            if (find2 = leftPointDic.ContainsKey(rightVertexPos))
            {
                roop2 = leftPointDic[rightVertexPos];
                // roop1==roop2�̂Ƃ�, roop�����������̂�return
                if (roop1 == roop2)
                {
                    roop1.verticesCount++;
                    roop1.sum_rightPosition += rightVertexPos;
                    return;
                }

                // target�̉E��roop2�̍��[(�n�[)����������
                target.next = roop2.start;
                // roop2�̍��[��target�ɕύX
                roop2.start = target;
                roop2.startPos = leftVertexPos;

                // roop2�����X�g����O��
                leftPointDic.Remove(rightVertexPos);
            }

             // 
            if (find1)
            {
                // 2��roop�����������Ƃ� 
                if (find2)
                {
                    //roop1+target+roop2�̏��łȂ����Ă���͂��Ȃ̂�roop1��roop2�̏���ǉ�����
                    roop1.end = roop2.end;//�I�[��ύX
                    roop1.endPos = roop2.endPos;
                    roop1.verticesCount += roop2.verticesCount + 1;//+1��target�̕�
                    roop1.sum_rightPosition += roop2.sum_rightPosition + rightVertexPos;
                    var key = roop2.endPos;
                    //dictionary�ɓ���Ă����Ȃ��ƍŌ�ɖʂ�\��Ȃ��̂œK���ɓ˂�����ł���.
                    rightPointDic[key] = roop1;
                }
                //�����̍����roop�̉E�肪���������Ƃ�, �E��dictionary�̎����̉E��Position�̏ꏊ��roop������
                else
                {
                    roop1.verticesCount++;
                    roop1.sum_rightPosition += rightVertexPos;
                    if (leftPointDic.ContainsKey(leftVertexPos))
                    {
                        //����dictionary�ɒǉ�����Ă���ꍇ��return (�ςȃ|���S���\���ɂȂ��Ă���Ƃ����Ȃ�)
                        return;
                    }
                    rightPointDic.Add(rightVertexPos, roop1);
                }
            }
            else
            {
                if (find2)
                {
                    roop2.verticesCount++;
                    roop2.sum_rightPosition += rightVertexPos;
                    if (leftPointDic.ContainsKey(leftVertexPos))
                    {
                        //����dictionary�ɒǉ�����Ă���ꍇ��return (�ςȃ|���S���\���ɂȂ��Ă���Ƃ����Ȃ�)
                        return;
                    }
                    leftPointDic.Add(leftVertexPos, roop2);
                }
                //�ǂ��ɂ��������Ȃ������Ƃ�, roop���쐬, �ǉ�
                else
                {
                    RooP newRoop = new RooP(target, target, leftVertexPos, rightVertexPos, rightVertexPos);
                    rightPointDic.Add(rightVertexPos, newRoop);
                    leftPointDic.Add(leftVertexPos, newRoop);
                }
            }
        }
        /// <summary>
        /// �ؒf�ʂ�����
        /// </summary>
        /// <param name="submesh"></param>
        /// <param name="targetTransform"></param>
        public void MakeCutSurface(int submesh, Transform targetTransform)
        {
            Vector3 scale = targetTransform.localScale;
            // ���[���h���W�̏�������I�u�W�F�N�g���W�ɕϊ�
            Vector3 world_Up = Vector3.Scale(scale, targetTransform.InverseTransformDirection(Vector3.up)).normalized;//���[���h���W�̏�������I�u�W�F�N�g���W�ɕϊ�
            // ���[���h���W�̉E�������I�u�W�F�N�g���W�ɕϊ�
            Vector3 world_Right = Vector3.Scale(scale, targetTransform.InverseTransformDirection(Vector3.right)).normalized;//���[���h���W�̉E�������I�u�W�F�N�g���W�ɕϊ�


            // �I�u�W�F�N�g��ԏ�ł�UV��U��
            Vector3 uVector = default;
            // �I�u�W�F�N�g��ԏ�ł�UV��V��
            Vector3 vVector = default;
            //U���͐ؒf�ʂ̖@����Y���Ƃ̊O��
            uVector = Vector3.Cross(world_Up, _planeNormal);
            //�ؒf�ʂ̖@����Z�������̂Ƃ���uVector���[���x�N�g���ɂȂ�̂ŏꍇ����
            uVector = (uVector.sqrMagnitude != 0) ? uVector.normalized : world_Right;

            //V����U���Ɛؒf���ʂ̃m�[�}���Ƃ̊O��
            vVector = Vector3.Cross(_planeNormal, uVector).normalized;
            //v���̕��������[���h���W������ɑ�����.
            if (Vector3.Dot(vVector, world_Up) < 0)
            {
                vVector *= -1;
            }

            // u���̊e�l
            float u_min = default;
            float u_max = default;
            float u_range = default;
            // v���̊e�l
            float v_min = default;
            float v_max = default;
            float v_range = default;

            foreach (RooP roop in leftPointDic.Values)
            {
                {
                    u_min = u_max = Vector3.Dot(uVector, roop.startPos);
                    v_min = v_max = Vector3.Dot(vVector, roop.startPos);
                    RoopFragment fragment = roop.start;

                    int count = 0;
                    do
                    {
                        float u_value = Vector3.Dot(uVector, fragment.rightPosition);
                        u_min = Mathf.Min(u_min, u_value);
                        u_max = Mathf.Max(u_max, u_value);

                        float v_value = Vector3.Dot(vVector, fragment.rightPosition);
                        v_min = Mathf.Min(v_min, v_value);
                        v_max = Mathf.Max(v_max, v_value);


                        // �������[�v��������鏈��
                        if (count > 1000)
                        {
                            Debug.LogError("Something is wrong?");
                            break;
                        }
                        count++;

                    }
                    while ((fragment = fragment.next) != null);

                    u_range = u_max - u_min;
                    v_range = v_max - v_min;

                }

                // roopFragment��next�����ǂ��Ă������Ƃ�roop������ł���

                // �ؒf�ʂ̒��S�ɒ��_��ǉ����Ē��_�ԍ���Ԃ�
                MakeVertex(roop.sum_rightPosition / roop.verticesCount, out int center_f, out int center_b);

                RoopFragment nowFragment = roop.start;

                //���[�v�̎n�[�̒��_��ǉ����Ē��_�ԍ���Ԃ�
                MakeVertex(nowFragment.rightPosition, out int first_f, out int first_b);
                int previous_f = first_f;
                int previous_b = first_b;

                //�I�[�ɒB����܂Ń��[�v
                while (nowFragment.next != null)
                {
                    nowFragment = nowFragment.next;

                    //�V�������_��ǉ����Ē��_�ԍ���Ԃ�
                    MakeVertex(nowFragment.rightPosition, out int index_f, out int index_b);

                    _frontSubmeshIndices[submesh].Add(center_f);
                    _frontSubmeshIndices[submesh].Add(index_f);
                    _frontSubmeshIndices[submesh].Add(previous_f);

                    _backSubmeshIndices[submesh].Add(center_b);
                    _backSubmeshIndices[submesh].Add(previous_b);
                    _backSubmeshIndices[submesh].Add(index_b);

                    previous_f = index_f;
                    previous_b = index_b;
                }
                _frontSubmeshIndices[submesh].Add(center_f);
                _frontSubmeshIndices[submesh].Add(first_f);
                _frontSubmeshIndices[submesh].Add(previous_f);

                _backSubmeshIndices[submesh].Add(center_b);
                _backSubmeshIndices[submesh].Add(previous_b);
                _backSubmeshIndices[submesh].Add(first_b);
            }

            void MakeVertex(Vector3 vertexPos, out int findex, out int bindex)
            {
                findex = _frontVertices.Count;
                bindex = _backVertices.Count;
                Vector2 uv = default;
                //position��UV�ɕϊ�
                {
                    float uValue = Vector3.Dot(uVector, vertexPos);
                    float normalizedU = (uValue - u_min) / u_range;
                    float vValue = Vector3.Dot(vVector, vertexPos);
                    float normalizedV = (vValue - v_min) / v_range;

                    uv = new Vector2(normalizedU, normalizedV);
                }
                _frontVertices.Add(vertexPos);
                _frontNormals.Add(-_planeNormal);
                _frontUVs.Add(uv);

                _backVertices.Add(vertexPos);
                _backNormals.Add(_planeNormal);
                //UV�����E���]����
                _backUVs.Add(new Vector2(1 - uv.x, uv.y));
            }
        }
        /// <summary>
        /// Dictionary���ꊇ��Clear���郁�\�b�h
        /// </summary>
        public void Clear()
        {
            leftPointDic.Clear();
            rightPointDic.Clear();
        }
    }
    /// <summary>
    /// �ؒf�ʂ��ׂ����_�Ɋւ���N���X
    /// </summary>
    public class Fragment
    {
        // �ؒf�ʂƏd�Ȃ��Ă���V�������_
        public NewVertex vertex0 = default;
        public NewVertex vertex1 = default;
        public int KEY_CUTLINE;
        // submesh�ԍ�(�ǂ̃}�e���A���𓖂Ă邩)
        public int submesh;
        // �|���S����4��(3��)�̒��_�̏��
        public Point firstPoint_f = default;
        public Point lastPoint_f = default;
        public Point firstPoint_b = default;
        public Point lastPoint_b = default;
        // front��,back���̒��_��
        public int count_f;
        public int count_b;

        /// <summary>
        /// �ؒf�Ђ̐ؒf���
        /// </summary>
        /// <param name="_vertex0"></param>
        /// <param name="_vertex1"></param>
        /// <param name="_twoPointsInFrontSide">�ؒf�ʂ̂ǂ��瑤�ɒ��_��2���邩</param>
        /// <param name="_KEY_CUTLINE"></param>
        /// <param name="_submesh">�T�u���b�V��</param>
        public Fragment(NewVertex _vertex0, NewVertex _vertex1, bool _twoPointsInFrontSide, int _KEY_CUTLINE, int _submesh)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            KEY_CUTLINE = _KEY_CUTLINE;
            submesh = _submesh;

            // �ؒf�ʂ̕\���ɒ��_��2����ꍇ
            if (_twoPointsInFrontSide)
            {
                firstPoint_f = new Point(_vertex0.frontsideindex_of_frontMesh);
                lastPoint_f = new Point(_vertex1.frontsideindex_of_frontMesh);
                firstPoint_f.next = lastPoint_f;
                firstPoint_b = new Point(vertex0.backsideindex_of_backMash);
                lastPoint_b = firstPoint_b;
                count_f = 2;
                count_b = 1;
            }
            else
            {
                firstPoint_f = new Point(_vertex0.frontsideindex_of_frontMesh);
                lastPoint_f = firstPoint_f;
                firstPoint_b = new Point(vertex0.backsideindex_of_backMash);
                lastPoint_b = new Point(vertex1.backsideindex_of_backMash);
                firstPoint_b.next = lastPoint_b;
                count_f = 1;
                count_b = 2;
            }
        }
        /// <summary>
        /// �T�u���b�V���̎O�p�`��ǉ����郁�\�b�h
        /// </summary>
        public void AddTriangle()
        {
            //Vertex�̒��ŐV�����������ꂽ���_��o�^���Ă��̔ԍ�������Ԃ��Ă���
            (int frontindex0, int backindex0) = vertex0.GetIndex();
            (int frontindex1, int backindex1) = vertex1.GetIndex();

            Point point = firstPoint_f;
            int preIndex = point.index;

            int count = count_f;
            int halfcount = count_f / 2;
            for (int i = 0; i < halfcount; i++)
            {
                point = point.next;
                int index = point.index;
                _frontSubmeshIndices[submesh].Add(index);
                _frontSubmeshIndices[submesh].Add(preIndex);
                _frontSubmeshIndices[submesh].Add(frontindex0);
                preIndex = index;
            }
            _frontSubmeshIndices[submesh].Add(preIndex);
            _frontSubmeshIndices[submesh].Add(frontindex0);
            _frontSubmeshIndices[submesh].Add(frontindex1);
            int elseCount = count_f - halfcount - 1;
            for (int i = 0; i < elseCount; i++)
            {
                point = point.next;
                int index = point.index;
                _frontSubmeshIndices[submesh].Add(index);
                _frontSubmeshIndices[submesh].Add(preIndex);
                _frontSubmeshIndices[submesh].Add(frontindex1);
                preIndex = index;
            }


            point = firstPoint_b;
            preIndex = point.index;
            count = count_b;
            halfcount = count_b / 2;

            for (int i = 0; i < halfcount; i++)
            {
                point = point.next;
                int index = point.index;
                _backSubmeshIndices[submesh].Add(index);
                _backSubmeshIndices[submesh].Add(backindex0);
                _backSubmeshIndices[submesh].Add(preIndex);
                preIndex = index;
            }
            _backSubmeshIndices[submesh].Add(preIndex);
            _backSubmeshIndices[submesh].Add(backindex1);
            _backSubmeshIndices[submesh].Add(backindex0);
            elseCount = count_b - halfcount - 1;
            for (int i = 0; i < elseCount; i++)
            {
                point = point.next;
                int index = point.index;
                _backSubmeshIndices[submesh].Add(index);
                _backSubmeshIndices[submesh].Add(backindex1);
                _backSubmeshIndices[submesh].Add(preIndex);
                preIndex = index;
            }

            if (_isMakeCutSurface)
            {
                //�ؒf���ʂ��`�����鏀��
                roopCollection.Add(vertex0.position, vertex1.position);
            }
        }
    }

    /// <summary>
    /// �V�������_��Normal��UV�͍Ō�ɐ�������̂�, 
    /// ���Ƃ��Ƃ��钸�_���ǂ̔�ō����邩��dividingParameter�������Ă���
    /// </summary>
    public class NewVertex
    {
        // frontVertices,frontNormals,frontUVs�ł̒��_�̔ԍ�
        // (frontsideindex_of_frontMesh��backsideindex_of_backMash�łł���ӂ̊ԂɐV�������_���ł���)
        public int frontsideindex_of_frontMesh;
        public int backsideindex_of_backMash;
        // �V�������_��(frontsideindex_of_frontMesh��backsideindex_of_backMash�łł���ӂɑ΂���)�����_
        public float dividingParameter;
        public int KEY_VERTEX;
        public Vector3 position;

        /// <summary>
        /// �V�������_��ݒ�
        /// </summary>
        /// <param name="front"></param>
        /// <param name="back"></param>
        /// <param name="parameter"></param>
        /// <param name="vertexPosition"></param>
        public NewVertex(int front, int back, float parameter, Vector3 vertexPosition)
        {
            frontsideindex_of_frontMesh = front;
            backsideindex_of_backMash = back;
            KEY_VERTEX = (front << 16) | back;
            dividingParameter = parameter;
            position = vertexPosition;
        }

        /// <summary>
        /// �\���̒��_�����擾
        /// </summary>
        /// <returns></returns>
        public (int frontindex, int backindex) GetIndex()
        {
            //�@����UV�̏��͂����Ő�������
            Vector3 frontNormal, backNormal;
            Vector2 frontUV, backUV;

            frontNormal = _frontNormals[frontsideindex_of_frontMesh];
            frontUV = _frontUVs[frontsideindex_of_frontMesh];

            backNormal = _backNormals[backsideindex_of_backMash];
            backUV = _backUVs[backsideindex_of_backMash];



            Vector3 newNormal = Vector3.Lerp(frontNormal, backNormal, dividingParameter);
            Vector2 newUV = Vector2.Lerp(frontUV, backUV, dividingParameter);

            int findex, bindex;
            (int, int) trackNumPair;
            //����2�̓_�̊Ԃɐ�������钸�_��1�ɂ܂Ƃ߂����̂�Dictionary���g��
            if (newVertexDic.TryGetValue(KEY_VERTEX, out trackNumPair))
            {
                //�V�������_���\����Mesh�ŉ��Ԗڂ�
                findex = trackNumPair.Item1;
                bindex = trackNumPair.Item2;
            }
            else
            {

                findex = _frontVertices.Count;
                _frontVertices.Add(position);
                _frontNormals.Add(newNormal);
                _frontUVs.Add(newUV);

                bindex = _backVertices.Count;
                _backVertices.Add(position);
                _backNormals.Add(newNormal);
                _backUVs.Add(newUV);

                newVertexDic.Add(KEY_VERTEX, (findex, bindex));
            }
            return (findex, bindex);
        }
    }
    /// <summary>
    /// Fragment�N���X�Ŏg���钸�_���̃|�C���^
    /// </summary>
    public class Point
    {
        public Point next;
        public int index;
        public Point(int _index)
        {
            index = _index;
            next = null;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class FragmentList
    {

        // �����ؒf�ӂ�������Fragment�����X�g�ɂ܂Ƃ߂�
        Dictionary<int, List<Fragment>> cutLineDictionary = new Dictionary<int, List<Fragment>>();

        /// <summary>
        /// �ؒf�Ђ�ǉ����ėׂ荇���ؒf�Ђ͂������Ă��܂�
        /// </summary>
        /// <param name="fragment">�ǉ�����ؒf��</param>
        /// <param name="KEY_CUTLINE">�ؒf���ꂽ�ӂ̌�����int�ɕϊ���������</param>
        public void Add(Fragment fragment, int KEY_CUTLINE)
        {
            List<Fragment> flist;

            //�e�ؒf�ӂ�1�߂̃|���S���ɂ��Ă͐V����key-value��ǉ�
            if (!cutLineDictionary.TryGetValue(KEY_CUTLINE, out flist))
            {
                flist = new List<Fragment>();
                cutLineDictionary.Add(KEY_CUTLINE, flist);
            }


            bool connect = false;
            //�i�[����Ă���Fragment���炭����������T��
            for (int i = flist.Count - 1; i >= 0; i--)
            {
                Fragment compareFragment = flist[i];
                //�����ؒf�ӂ��������f
                if (fragment.KEY_CUTLINE == compareFragment.KEY_CUTLINE)
                {
                    Fragment left, right;
                    //fragment��compareFragment�ɉE�����炭�����ꍇ
                    if (fragment.vertex0.KEY_VERTEX == compareFragment.vertex1.KEY_VERTEX)
                    {
                        right = fragment;
                        left = compareFragment;
                    }
                    //�������炭�����ꍇ
                    else if (fragment.vertex1.KEY_VERTEX == compareFragment.vertex0.KEY_VERTEX)
                    {
                        left = fragment;
                        right = compareFragment;
                    }
                    else
                    {
                        //�ǂ����ł��Ȃ��Ƃ��͎��̃��[�v��
                        continue;
                    }

                    //Point�N���X�̂Ȃ����킹. 
                    //firstPoint.next��null�Ƃ������Ƃ͒��_��1���������Ă��Ȃ�. 
                    //�܂����̒��_��left��lastPoint�Ƃ��Ԃ��Ă���̂Œ��_�������邱�Ƃ͂Ȃ�
                    //(left.lastPoint_f��right.lastPoint_f�͓����_���������ʂ̃C���X�^���X�Ȃ̂�next��null�̂Ƃ��ɓ���ւ���ƃ��[�v���r�؂�Ă��܂�)
                    if ((left.lastPoint_f.next = right.firstPoint_f.next) != null)
                    {
                        left.lastPoint_f = right.lastPoint_f;
                        left.count_f += right.count_f - 1;
                    }
                    if ((left.lastPoint_b.next = right.firstPoint_b.next) != null)
                    {
                        left.lastPoint_b = right.lastPoint_b;
                        left.count_b += right.count_b - 1;
                    }

                    //�������s��
                    //Fragment�����L���Ȃ�悤�ɒ��_����ς���
                    left.vertex1 = right.vertex1;
                    right.vertex0 = left.vertex0;

                    //connect��true�ɂȂ��Ă���Ƃ������Ƃ�2��Fragment�̂������ɐV��������͂܂���3��1�ɂȂ����Ƃ�������
                    //connect==true�̂Ƃ�, right��left��List�ɂ��łɓo�^����Ă��Ȃ̂łǂ������������Ă��
                    if (connect)
                    {
                        flist.Remove(right);

                        break;
                    }

                    flist[i] = left;
                    fragment = left;
                    connect = true;
                }
            }

            if (!connect)
            {
                flist.Add(fragment);
            }
        }
        /// <summary>
        /// �ؒf���ꂽ�|���S�������ꂼ���Mesh�ɒǉ�
        /// </summary>
        public void MakeTriangle()
        {
            int sum = 0;
            foreach (List<Fragment> list in cutLineDictionary.Values)
            {
                foreach (Fragment f in list)
                {
                    f.AddTriangle();
                    sum++;
                }
            }
        }
        /// <summary>
        /// MeshCut�N���X�ŘA�z�z���
        /// </summary>
        public void Clear()
        {
            cutLineDictionary.Clear();
        }
    }

    const int filter = 0x000003FF;
    //�ۂߌ덷�𗎂Ƃ����߂ɂ���߂̔{�����������Ă���
    const int amp = 1 << 10;
    /// <summary>
    /// Vector3����ۂߌ덷�𗎂Ƃ���int�ɕϊ�
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static int MakeIntFromVector3_ErrorCut(Vector3 vec)
    {
        int cutLineX = ((int)(vec.x * amp) & filter) << 20;
        int cutLineY = ((int)(vec.y * amp) & filter) << 10;
        int cutLineZ = ((int)(vec.z * amp) & filter);

        return cutLineX | cutLineY | cutLineZ;
    }
    #endregion
}
