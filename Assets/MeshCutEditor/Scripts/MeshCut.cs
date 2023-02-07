using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCut : MonoBehaviour
{
    public class MeshCutSide
    {
        // �O�p�`�̒��_���X�g
        public List<Vector3> vertices = new List<Vector3>();

        // �O�p�`�̖@��
        public List<Vector3> normals = new List<Vector3>();

        // �e�N�X�`���̍��W
        public List<Vector2> uvs = new List<Vector2>();

        // �I�u�W�F�N�g�̎O�p�`
        public List<int> triangles = new List<int>();

        // �T�u�C���f�b�N�X�̃��X�g�z��
        public List<List<int>> subIndices = new List<List<int>>();

        /// <summary>
        /// ���X�g�̒��g���폜
        /// </summary>
        public void ClearAll()
        {
            vertices.Clear();
            normals.Clear();
            uvs.Clear();
            triangles.Clear();
            subIndices.Clear();
        }

        /// <summary>
        /// �ŏ��̎O�p�`�Ƃ���3���_��ǉ�
        /// �� ���_���͌��̃��b�V������R�s�[����
        /// </summary>
        /// <param name="peak1">���_1</param>
        /// <param name="peak2">���_2</param>
        /// <param name="peak3">���_3</param>
        /// <param name="submesh">�Ώۂ̃T�u���V��</param>
        public void AddTriangle(int peak1, int peak2, int peak3, int submesh)
        {
            // �O�p�`�̃C���f�b�N�X���� 1,2,3,4....

            // ���_�z��̃J�E���g�B�����ǉ�����Ă������߁A�x�[�X�ƂȂ�index���`����B
            // �� AddTriangle���Ă΂�邽�тɒ��_���͑����Ă����B
            int base_index = vertices.Count;

            // �ΏۃT�u���b�V���̃C���f�b�N�X�ɒǉ����Ă���
            subIndices[submesh].Add(base_index + PEAK_ZERO);
            subIndices[submesh].Add(base_index + PEAK_ONE);
            subIndices[submesh].Add(base_index + PEAK_TWO);

            // �O�p�`�S�̒��_��ݒ�
            triangles.Add(base_index + PEAK_ZERO);
            triangles.Add(base_index + PEAK_ONE);
            triangles.Add(base_index + PEAK_TWO);

            // �ΏۃI�u�W�F�N�g�̒��_�z�񂩂璸�_�����擾���ݒ肷��
            vertices.Add(victim_mesh.vertices[peak1]);
            vertices.Add(victim_mesh.vertices[peak2]);
            vertices.Add(victim_mesh.vertices[peak3]);

            // ���l�ɁA�ΏۃI�u�W�F�N�g�̖@���z�񂩂�@�����擾���ݒ肷��
            normals.Add(victim_mesh.normals[peak1]);
            normals.Add(victim_mesh.normals[peak2]);
            normals.Add(victim_mesh.normals[peak3]);

            // ���l�ɁAUV���B
            uvs.Add(victim_mesh.uv[peak1]);
            uvs.Add(victim_mesh.uv[peak2]);
            uvs.Add(victim_mesh.uv[peak3]);
        }

        /// <summary>
        /// �O�p�`��ǉ�����
        /// �� �I�[�o�[���[�h���Ă��鑼���\�b�h�Ƃ͈قȂ�A�����̒l�Œ��_�i�|���S���j��ǉ�����
        /// </summary>
        /// <param name="points3">�g���C�A���O�����`������3���_</param>
        /// <param name="normals3">3���_�̖@��</param>
        /// <param name="uvs3">3���_��UV</param>
        /// <param name="faceNormal">�|���S���̖@��</param>
        /// <param name="submesh">�T�u���b�V��ID</param>
        public void AddNewTriangle(Vector3[] points3, Vector3[] normals3, Vector2[] uvs3, Vector3 faceNormal, int submesh)
        {
            // ������3���_����@�����v�Z
            Vector3 calculated_normal = Vector3.Cross((points3[PEAK_ONE] - points3[PEAK_ZERO]).normalized, (points3[PEAK_TWO] - points3[PEAK_ZERO]).normalized);

            int peak1 = 0;
            int peak2 = 1;
            int peak3 = 2;

            // �����Ŏw�肳�ꂽ�@���Ƌt�������ꍇ�̓C���f�b�N�X�̏��Ԃ��t���ɂ���i�܂�ʂ𗠕Ԃ��j
            if (Vector3.Dot(calculated_normal, faceNormal) < 0)
            {
                peak1 = 2;
                peak2 = 1;
                peak3 = 0;
            }

            // ���_�z��̃J�E���g�B�����ǉ�����Ă������߁A�x�[�X�ƂȂ�index���`����B
            int base_index = vertices.Count;

            subIndices[submesh].Add(base_index + PEAK_ZERO);
            subIndices[submesh].Add(base_index + PEAK_ONE);
            subIndices[submesh].Add(base_index + PEAK_TWO);

            triangles.Add(base_index + PEAK_ZERO);
            triangles.Add(base_index + PEAK_ONE);
            triangles.Add(base_index + PEAK_TWO);

            vertices.Add(points3[peak1]);
            vertices.Add(points3[peak2]);
            vertices.Add(points3[peak3]);

            normals.Add(normals3[peak1]);
            normals.Add(normals3[peak2]);
            normals.Add(normals3[peak3]);

            uvs.Add(uvs3[peak1]);
            uvs.Add(uvs3[peak2]);
            uvs.Add(uvs3[peak3]);
        }

    }

    // ���_�̏���
    const int PEAK_ZERO = 0;
    const int PEAK_ONE = 1;
    const int PEAK_TWO = 2;

    //�J�b�g�ʕ���

    //
    private MeshCutSide left_side = new MeshCutSide();
    private MeshCutSide right_side = new MeshCutSide();

    private Plane blade;
    private Mesh _victim_mesh;
    private static Mesh victim_mesh{ get; set; }

    // �L���b�s���O�������������_
    private List<Vector3> new_vertices = new List<Vector3>();

    /// <summary>
    /// �i�w�肳�ꂽ�uvictim�v���J�b�g����B�u���[�h�i���ʁj�ƃ}�e���A������ؒf�����s����j
    /// </summary>
    /// <param name="victim">�J�b�g����I�u�W�F�N�g</param>
    /// <param name="anchorPoint">�ؒf�ʂ̈ʒu</param>
    /// <param name="normalDirection">�ؒf�ʂ̖@��</param>
    /// <param name="capMaterial">�ؒf�ʂ̃}�e���A��</param>
    public GameObject[] Cut(GameObject victim, Vector3 anchorPoint, Vector3 normalDirection, Material capMaterial)
    {
        // victim���瑊�ΓI�ȕ��ʁi�u���[�h�j���Z�b�g
        // ��̓I�ɂ́A�ΏۃI�u�W�F�N�g�̃��[�J�����W�ł̕��ʂ̖@���ƈʒu���畽�ʂ𐶐�����
        blade = new Plane(
            // ���[�J�����W�ɕϊ�
            victim.transform.InverseTransformDirection(-normalDirection),
            victim.transform.InverseTransformPoint(anchorPoint)
        );
        // �Ώۂ̃��b�V�����擾
        victim_mesh = victim.GetComponent<MeshFilter>().mesh;

        // �V�������_�S
        new_vertices.Clear();

        // ���ʂ�荶�̒��_�S�iMeshCutSide�j
        left_side.ClearAll();

        //���ʂ��E�̒��_�S�iMeshCutSide�j
        right_side.ClearAll();

        // �����ł́u3�v�͎O�p�`�̒��_��
        bool[] sides = new bool[3];
        int[] indices;
        int peak1, peak2, peak3;

        // �T�u���b�V���̐��������[�v
        for (int sub = 0; sub < victim_mesh.subMeshCount; sub++)
        {
            // �T�u���b�V���̃C���f�b�N�X�����擾
            indices = victim_mesh.GetIndices(sub);

            // List<List<int>>�^�̃��X�g�ɃT�u���b�V������̃C���f�b�N�X���X�g��ǉ�
            left_side.subIndices.Add(new List<int>());  // ��
            right_side.subIndices.Add(new List<int>()); // �E

            // �T�u���b�V���̃C���f�b�N�X�������[�v
            for (int i = 0; i < indices.Length; i += 3)
            {
                // peak1 ���� peak3�̃C���f�b�N�X���擾�B
                peak1 = indices[i + PEAK_ZERO];
                peak2 = indices[i + PEAK_ONE];
                peak3 = indices[i + PEAK_TWO];

                // ���ꂼ��]�����̃��b�V���̒��_���A�ؒf�ʂ̍��E�ǂ���ɂ��邩�𒲂ׂ�B
                // `GetSide` ���\�b�h�ɂ��bool�𓾂���B
                sides[PEAK_ZERO] = blade.GetSide(victim_mesh.vertices[peak1]);
                sides[PEAK_ONE] = blade.GetSide(victim_mesh.vertices[peak2]);
                sides[PEAK_TWO] = blade.GetSide(victim_mesh.vertices[peak3]);

                // ���_�O�ƒ��_�P����ђ��_�Q���ǂ�����������ɂ���ꍇ�̓J�b�g���Ȃ�
                if (sides[PEAK_ZERO] == sides[PEAK_ONE] && sides[PEAK_ZERO] == sides[PEAK_TWO])
                {
                    if (sides[PEAK_ZERO])
                    {
                      // GetSide���\�b�h�Ń|�W�e�B�u�itrue�j�̏ꍇ�͍����ɂ���
                        left_side.AddTriangle(peak1, peak2, peak3, sub);
                    }
                    else
                    {
                        right_side.AddTriangle(peak1, peak2, peak3, sub);
                    }
                }
                else
                {
                  // �ǂ��炩�̓_�����ʂ̔��Α��ɂ���ꍇ�̓J�b�g�����s����
                    Cut_this_Face(sub, sides, peak1, peak2, peak3);
                }
            }
        }

        // �ݒ肳��Ă���}�e���A�����X�g���擾
        List<Material> mats = new List<Material>(victim.GetComponent<MeshRenderer>().sharedMaterials);

        // �擾�����}�e���A�����X�g�̍Ō�̃}�e���A�����A�J�b�g�ʂ̃}�e���A���łȂ��ꍇ
        if (mats[mats.Count - 1].name != capMaterial.name)
        {
          // �J�b�g�ʗp�̃C���f�b�N�X�z���ǉ�
            left_side.subIndices.Add(new List<int>());
            right_side.subIndices.Add(new List<int>());

            // �}�e���A�����X�g�ɁA�J�b�g�ʗp�}�e���A����ǉ�
            mats.Add(capMaterial);
        }

        // �J�b�g�J�n
        Capping();

        // �����̃��b�V���𐶐�
        // MeshCutSide�N���X�̃����o����e�l���R�s�[
        Mesh left_HalfMesh = new Mesh();
        left_HalfMesh.name = "Split Mesh Left";
        left_HalfMesh.vertices = left_side.vertices.ToArray();
        left_HalfMesh.triangles = left_side.triangles.ToArray();
        left_HalfMesh.normals = left_side.normals.ToArray();
        left_HalfMesh.uv = left_side.uvs.ToArray();

        left_HalfMesh.subMeshCount = left_side.subIndices.Count;
        for (int i = 0; i < left_side.subIndices.Count; i++)
        {
            left_HalfMesh.SetIndices(left_side.subIndices[i].ToArray(), MeshTopology.Triangles, i);
        }

        // �E���̃��b�V���𐶐�
        Mesh right_HalfMesh = new Mesh();
        right_HalfMesh.name = "Split Mesh Right";
        right_HalfMesh.vertices = right_side.vertices.ToArray();
        right_HalfMesh.triangles = right_side.triangles.ToArray();
        right_HalfMesh.normals = right_side.normals.ToArray();
        right_HalfMesh.uv = right_side.uvs.ToArray();

        right_HalfMesh.subMeshCount = right_side.subIndices.Count;
        for (int i = 0; i < right_side.subIndices.Count; i++)
        {
            right_HalfMesh.SetIndices(right_side.subIndices[i].ToArray(), MeshTopology.Triangles, i);
        }

        // �Q�[���I�u�W�F�N�g�����蓖�Ă�

        // ���̃I�u�W�F�N�g�������̃I�u�W�F�N�g��
        victim.name = "left side";
        victim.GetComponent<MeshFilter>().mesh = left_HalfMesh;
        if (!victim.GetComponent<MeshCollider>())
        {
            // ���̃I�u�W�F�N�g��MeshCollider�ȊO��������
            // �����̃R���C�_�[���폜����MeshCollider���A�^�b�`����
            Destroy(victim.GetComponent<Collider>());
            victim.AddComponent<MeshCollider>().convex = true;
        }
        victim.GetComponent<MeshCollider>().sharedMesh = left_HalfMesh;
        // ���A�Z�b�g�ɂ��|���S���팸
        victim.AddComponent<OptimizeMesh>()._quality = 0.15f;
        victim.GetComponent<OptimizeMesh>().DecimateMesh();

        // �����̃I�u�W�F�N�g�͂��̂܂܃R�s�[
        GameObject leftSideObj = victim;

        

        // �E���̃I�u�W�F�N�g�͐V�K�쐬���ăR���|�[�l���g���w��
        GameObject rightSideObj = new GameObject("right side", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider), typeof(OptimizeMesh));//
        rightSideObj.transform.position = victim.transform.position;
        rightSideObj.transform.rotation = victim.transform.rotation;
        rightSideObj.transform.localScale = victim.transform.localScale;
        rightSideObj.GetComponent<MeshFilter>().mesh = right_HalfMesh;
        // �E���̃I�u�W�F�N�g�̃R���C�_�[�𒲐�
        rightSideObj.GetComponent<MeshCollider>().sharedMesh = right_HalfMesh;
        rightSideObj.GetComponent<MeshCollider>().convex = true;
        // ���A�Z�b�g�ɂ��|���S���팸
        victim.AddComponent<OptimizeMesh>()._quality = 0.15f;
        victim.GetComponent<OptimizeMesh>().DecimateMesh();
        // ���̃I�u�W�F�N�g��Rigidbody�����Ă��邩
        if (victim.GetComponent<Rigidbody>())
        {
            rightSideObj.AddComponent<Rigidbody>();
        }

        // �V�K���������}�e���A�����X�g�����ꂼ��̃I�u�W�F�N�g�ɓK�p����
        leftSideObj.GetComponent<MeshRenderer>().materials = mats.ToArray();
        rightSideObj.GetComponent<MeshRenderer>().materials = mats.ToArray();

        // ���E��GameObject�̔z���Ԃ�
        return new GameObject[] { leftSideObj, rightSideObj };
    }

    /// <summary>
    /// �J�b�g�����s����B�������A���ۂ̃��b�V���̑���ł͂Ȃ��A�����܂Œ��_�̐U�蕪���A���O�����Ƃ��Ă̎��s
    /// </summary>
    /// <param name="submesh">�T�u���b�V���̃C���f�b�N�X</param>
    /// <param name="sides">�]������3���_�̍��E���</param>
    /// <param name="index1">���_1</param>
    /// <param name="index2">���_2</param>
    /// <param name="index3">���_3</param>
    private void Cut_this_Face(int submesh, bool[] sides, int index1, int index2, int index3)
    {
        // ���E���ꂼ��̏���ێ����邽�߂̔z��S
        Vector3[] leftPoints = new Vector3[2];
        Vector3[] leftNormals = new Vector3[2];
        Vector2[] leftUvs = new Vector2[2];
        Vector3[] rightPoints = new Vector3[2];
        Vector3[] rightNormals = new Vector3[2];
        Vector2[] rightUvs = new Vector2[2];

        //���ɐݒ肳��Ă��邩
        bool didset_left = false;
        //�E�ɐݒ肳��Ă��邩
        bool didset_right = false;

        // 3���_���J��Ԃ�
        // �������e�Ƃ��ẮA���E�𔻒肵�āA���E�̔z���3���_��U�蕪���鏈�����s���Ă���
        int p = index1;
        for (int side = 0; side < 3; side++)
        {
            switch (side)
            {
                case PEAK_ZERO:
                    p = index1;
                    break;
                case PEAK_ONE:
                    p = index2;
                    break;
                case PEAK_TWO:
                    p = index3;
                    break;
            }

            // sides[side]��true�A�܂荶���̏ꍇ
            if (sides[side])
            {
                // ���łɍ����̒��_���ݒ肳��Ă��邩�i3���_�����E�ɐU�蕪�����邽�߁A�K�����E�ǂ��炩��2�̒��_�������ƂɂȂ�j
                if (!didset_left)
                {
                    didset_left = true;

                    // ������0,1�Ƃ��ɓ����l�ɂ��Ă���̂́A����������
                    // leftPoints[0,1]�̒l���g���ĕ����_�����߂鏈�������Ă��邽�߁B
                    // �܂�A�A�N�Z�X�����\��������

                    // ���_�̐ݒ�
                    leftPoints[PEAK_ZERO] = victim_mesh.vertices[p];
                    leftPoints[PEAK_ONE] = leftPoints[PEAK_ZERO];
                    // UV�̐ݒ�
                    leftUvs[PEAK_ZERO] = victim_mesh.uv[p];
                    leftUvs[PEAK_ONE] = leftUvs[PEAK_ZERO];
                    // �@���̐ݒ�
                    leftNormals[PEAK_ZERO] = victim_mesh.normals[p];
                    leftNormals[PEAK_ONE] = leftNormals[PEAK_ZERO];
                }
                else
                {
                    // 2���_�ڂ̏ꍇ��2�Ԗڂɒ��ڒ��_����ݒ肷��
                    leftPoints[PEAK_ONE] = victim_mesh.vertices[p];
                    leftUvs[PEAK_ONE] = victim_mesh.uv[p];
                    leftNormals[PEAK_ONE] = victim_mesh.normals[p];
                }
            }
            else
            {
                // ���Ɠ��l�̑�����E�ɂ��s��
                if (!didset_right)
                {
                    didset_right = true;

                    // ���_�̐ݒ�
                    rightPoints[PEAK_ZERO] = victim_mesh.vertices[p];
                    rightPoints[PEAK_ONE] = rightPoints[PEAK_ZERO];
                    // UV�̐ݒ�
                    rightUvs[PEAK_ZERO] = victim_mesh.uv[p];
                    rightUvs[PEAK_ONE] = rightUvs[PEAK_ZERO];
                    // �@���̐ݒ�
                    rightNormals[PEAK_ZERO] = victim_mesh.normals[p];
                    rightNormals[PEAK_ONE] = rightNormals[PEAK_ZERO];
                }
                else
                {
                    // 2�Ԗڂɒ��ڒ��_����ݒ肷��
                    rightPoints[PEAK_ONE] = victim_mesh.vertices[p];
                    rightUvs[PEAK_ONE] = victim_mesh.uv[p];
                    rightNormals[PEAK_ONE] = victim_mesh.normals[p];
                }
            }
        }

        // �������ꂽ�_�̔䗦�v�Z�̂��߂̋���
        float normalizedDistance = 0f;

        // ����
        float distance = 0f;

        // ---------------------------
        // �����̏���

        // ��`�����ʂƌ�������_��T���B
        // �܂�A���ʂɂ���ĕ��������_��T���B
        // ���̓_���N�_�ɁA�E�̓_�Ɍ��������C���΂��A���̕����_��T��B
        blade.Raycast(new Ray(leftPoints[PEAK_ZERO], (rightPoints[PEAK_ZERO] - leftPoints[PEAK_ZERO]).normalized), out distance);
        Debug.DrawRay(leftPoints[PEAK_ZERO], (rightPoints[PEAK_ZERO] - leftPoints[PEAK_ZERO]).normalized * 10f, Color.red, 100,false);

        // �������������_���A���_�Ԃ̋����Ŋ��邱�ƂŁA�����_�̍��E�̊������Z�o����
        normalizedDistance = distance / (rightPoints[PEAK_ZERO] - leftPoints[PEAK_ZERO]).magnitude;

        // �J�b�g��̐V���_�ɑ΂��鏈���B�t���O�����g�V�F�[�_�ł̕⊮�Ɠ������A���������ʒu�ɉ����ēK�؂ɕ⊮�����l��ݒ肷��
        Vector3 newVertex1 = Vector3.Lerp(leftPoints[PEAK_ZERO], rightPoints[PEAK_ZERO], normalizedDistance);
        Vector2 newUv1 = Vector2.Lerp(leftUvs[PEAK_ZERO], rightUvs[PEAK_ZERO], normalizedDistance);
        Vector3 newNormal1 = Vector3.Lerp(leftNormals[PEAK_ZERO], rightNormals[PEAK_ZERO], normalizedDistance);

        // �V���_�S�ɐV�������_��ǉ�
        new_vertices.Add(newVertex1);


        // ---------------------------
        // �E���̏���

        blade.Raycast(new Ray(leftPoints[PEAK_ONE], (rightPoints[PEAK_ONE] - leftPoints[PEAK_ONE]).normalized), out distance);

        normalizedDistance = distance / (rightPoints[PEAK_ONE] - leftPoints[PEAK_ONE]).magnitude;
        Vector3 newVertex2 = Vector3.Lerp(leftPoints[PEAK_ONE], rightPoints[PEAK_ONE], normalizedDistance);
        Vector2 newUv2 = Vector2.Lerp(leftUvs[PEAK_ONE], rightUvs[PEAK_ONE], normalizedDistance);
        Vector3 newNormal2 = Vector3.Lerp(leftNormals[PEAK_ONE], rightNormals[PEAK_ONE], normalizedDistance);

        // �V���_�S�ɐV�������_��ǉ�
        new_vertices.Add(newVertex2);


        // �v�Z���ꂽ�V�������_���g���āA�V�g���C�A���O�������E�Ƃ��ɒǉ�����
        // memo: �ǂ���������Ă��A���E�ǂ��炩��1�̎O�p�`�ɂȂ�C�����邯�ǁA�k�ގO�p�`�I�Ȋ����łƂɂ���2���ǉ����Ă��銴�����낤���H
        left_side.AddNewTriangle(
            new Vector3[] { leftPoints[PEAK_ZERO], newVertex1, newVertex2 },
            new Vector3[] { leftNormals[PEAK_ZERO], newNormal1, newNormal2 },
            new Vector2[] { leftUvs[PEAK_ZERO], newUv1, newUv2 },
            newNormal1,
            submesh
        );

        left_side.AddNewTriangle(
            new Vector3[] { leftPoints[PEAK_ZERO], leftPoints[PEAK_ONE], newVertex2 },
            new Vector3[] { leftNormals[PEAK_ZERO], leftNormals[PEAK_ONE], newNormal2 },
            new Vector2[] { leftUvs[PEAK_ZERO], leftUvs[PEAK_ONE], newUv2 },
            newNormal2,
            submesh
        );

        right_side.AddNewTriangle(
            new Vector3[] { rightPoints[PEAK_ZERO], newVertex1, newVertex2 },
            new Vector3[] { rightNormals[PEAK_ZERO], newNormal1, newNormal2 },
            new Vector2[] { rightUvs[PEAK_ZERO], newUv1, newUv2 },
            newNormal1,
            submesh
        );

        right_side.AddNewTriangle(
            new Vector3[] { rightPoints[PEAK_ZERO], rightPoints[PEAK_ONE], newVertex2 },
            new Vector3[] { rightNormals[PEAK_ZERO], rightNormals[PEAK_ONE], newNormal2 },
            new Vector2[] { rightUvs[PEAK_ZERO], rightUvs[PEAK_ONE], newUv2 },
            newNormal2,
            submesh
        );
    }

    private List<Vector3> capVertTracker = new List<Vector3>();
    private List<Vector3> capVertpolygon = new List<Vector3>();

    /// <summary>
    /// �J�b�g�����s
    /// </summary>
    private void Capping()
    {
        // �J�b�g�p���_�ǐՃ��X�g
        // ��̓I�ɂ͐V���_�S���ɑ΂��钲�����s���B���̉ߒ��Œ����ς݂��}�[�N����ړI�ŗ��p����B
        capVertTracker.Clear();

        // �V���������������_���������[�v���遁�S�V���_�ɑ΂��ă|���S�����`�����邽�ߒ������s��
        // ��̓I�ɂ́A�J�b�g�ʂ��\������|���S�����`�����邽�߁A�J�b�g���ɏd���������_��ԗ����āu�ʁv���`�����钸�_�𒲍�����
        for (int i = 0; i < new_vertices.Count; i++)
        {
            // �Ώے��_�����łɒ����ς݂̃}�[�N����āi�ǐՔz��Ɋ܂܂�āj������X�L�b�v
            if (capVertTracker.Contains(new_vertices[i]))
            {
                continue;
            }

            // �J�b�g�p�|���S���z����N���A
            capVertpolygon.Clear();

            // �������_�Ǝ��̒��_���|���S���z��ɕێ�����
            capVertpolygon.Add(new_vertices[i + PEAK_ZERO]);
            capVertpolygon.Add(new_vertices[i + PEAK_ONE]);

            // �ǐՔz��Ɏ��g�Ǝ��̒��_��ǉ�����i�����ς݂̃}�[�N������j
            capVertTracker.Add(new_vertices[i + PEAK_ZERO]);
            capVertTracker.Add(new_vertices[i + PEAK_ONE]);

            // �d�����_���Ȃ��Ȃ�܂Ń��[�v����������
            bool isDone = false;
            while (!isDone)
            {
                isDone = true;

                // �V���_�S�����[�v���A�u�ʁv���\������v���ƂȂ钸�_�����ׂĒ��o����B���o���I���܂Ń��[�v���J��Ԃ�
                // 2���_���Ƃɒ������s�����߁A���[�v��2�P�ʂł����߂�
                for (int k = 0; k < new_vertices.Count; k += 2)
                { // go through the pairs
                  // �y�A�ƂȂ钸�_��T��
                  // �����ł̃y�A�Ƃ́A�����g���C�A���O�����琶�������V���_�̃y�A�B
                  // �g���C�A���O������͕K��2���_����������邽�߁A�����T���B
                  // �܂��A�S�|���S���ɑ΂��ĕ����_�𐶐����Ă��邽�߁A�قڕK���A�܂����������ʒu�ɑ��݂���A�ʃg���C�A���O���̕������_�����݂���͂��ł���B
                    if (new_vertices[k] == capVertpolygon[capVertpolygon.Count - PEAK_ONE] && !capVertTracker.Contains(new_vertices[k + PEAK_ONE]))
                    {   // if so add the other
                        // �y�A�̒��_�����������炻����|���S���z��ɒǉ����A
                        // �����σ}�[�N�����āA���̃��[�v�����ɉ�
                        isDone = false;
                        capVertpolygon.Add(new_vertices[k + PEAK_ONE]);
                        capVertTracker.Add(new_vertices[k + PEAK_ONE]);
                    }
                    else if (new_vertices[k + PEAK_ONE] == capVertpolygon[capVertpolygon.Count - PEAK_ONE] && !capVertTracker.Contains(new_vertices[k]))
                    {   // if so add the other
                        isDone = false;
                        capVertpolygon.Add(new_vertices[k]);
                        capVertTracker.Add(new_vertices[k]);
                    }
                }
            }

            // �����������_�S�����ɁA�|���S�����`������
            FillCap(capVertpolygon);
        }
    }

    /// <summary>
    /// �J�b�g�ʂ𖄂߂�H
    /// </summary>
    /// <param name="vertices">�|���S�����`�����钸�_���X�g</param>
    private void FillCap(List<Vector3> vertices)
    {
        // �J�b�g���ʂ̒��S�_���v�Z����
        Vector3 center = Vector3.zero;

        // �����œn���ꂽ���_�ʒu�����ׂč��v����
        foreach (Vector3 point in vertices)
        {
            center += point;
        }

        // ����𒸓_���̍��v�Ŋ���A���S�Ƃ���
        center = center / vertices.Count;

        // �J�b�g���ʂ��x�[�X�ɂ���upward
        Vector3 upward = Vector3.zero;

        // �J�b�g���ʂ̖@���𗘗p���āA�u��v���������߂�
        // ��̓I�ɂ́A���ʂ̍�������Ƃ��ė��p����
        upward.x = blade.normal.y;
        upward.y = -blade.normal.x;
        upward.z = blade.normal.z;

        // �@���Ɓu������v����A�������Z�o
        Vector3 left = Vector3.Cross(blade.normal, upward);

        Vector3 displacement = Vector3.zero;
        Vector3 newUV1 = Vector3.zero;
        Vector3 newUV2 = Vector3.zero;

        // �����ŗ^����ꂽ���_�����[�v����
        for (int i = 0; i < vertices.Count; i++)
        {
            // �v�Z�ŋ��߂����S�_����A�e���_�ւ̕����x�N�g��
            displacement = vertices[i] - center;

            // �V�K��������|���S����UV���W�����߂�B
            // displacement�����S����̃x�N�g���̂��߁AUV�I�Ȓ��S�ł���0.5���x�[�X�ɁA���ς��g����UV�̍ŏI�I�Ȉʒu�𓾂�
            newUV1 = Vector3.zero;
            newUV1.x = 0.5f + Vector3.Dot(displacement, left);
            newUV1.y = 0.5f + Vector3.Dot(displacement, upward);
            newUV1.z = 0.5f + Vector3.Dot(displacement, blade.normal);

            // ���̒��_�B�������A�Ō�̒��_�̎��͍ŏ��̒��_�𗘗p���邽�߁A�኱�g���b�L�[�Ȏw����@�����Ă���i% vertices.Count�j
            displacement = vertices[(i + 1) % vertices.Count] - center;

            newUV2 = Vector3.zero;
            newUV2.x = 0.5f + Vector3.Dot(displacement, left);
            newUV2.y = 0.5f + Vector3.Dot(displacement, upward);
            newUV2.z = 0.5f + Vector3.Dot(displacement, blade.normal);

            // uvs.Add(new Vector2(relativePosition.x, relativePosition.y));
            // normals.Add(blade.normal);

            // �����̃|���S���Ƃ��āA���߂�UV�𗘗p���ăg���C�A���O����ǉ�
            left_side.AddNewTriangle(
                new Vector3[]{
                        vertices[i],
                        vertices[(i + 1) % vertices.Count],
                        center
                },
                new Vector3[]{
                        -blade.normal,
                        -blade.normal,
                        -blade.normal
                },
                new Vector2[]{
                        newUV1,
                        newUV2,
                        new Vector2(0.5f, 0.5f)
                },
                -blade.normal,
                left_side.subIndices.Count - 1 // �J�b�g�ʁB�Ō�̃T�u���b�V���Ƃ��ăg���C�A���O����ǉ�
            );

            // �E���̃g���C�A���O���B��{�͍����Ɠ��������A�@�������t�����B
            right_side.AddNewTriangle(
                new Vector3[]{
                        vertices[i],
                        vertices[(i + 1) % vertices.Count],
                        center
                },
                new Vector3[]{
                        blade.normal,
                        blade.normal,
                        blade.normal
                },
                new Vector2[]{
                        newUV1,
                        newUV2,
                        new Vector2(0.5f, 0.5f)
                },
                blade.normal,
                right_side.subIndices.Count - 1 // �J�b�g�ʁB�Ō�̃T�u���b�V���Ƃ��ăg���C�A���O����ǉ�
            );
        }
    }
}
