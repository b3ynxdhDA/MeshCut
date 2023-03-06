using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �v���C���[�̖��O���
/// </summary>
namespace Player
{
    /// <summary>
    /// �J�������烁�b�V���ؒf������Ray���΂��N���X
    /// </summary>
    public class MeshCutController : MonoBehaviour
    {
        #region �ϐ��錾
        [SerializeField, Header("�ؒf�ʂ𖄂߂�")]
        private bool _isCutSurfaceFill = false;
        [SerializeField, Header("�ؒf�ʂ̃}�e���A��(null�Ȃ�I�u�W�F�N�g�̃}�e���A�����疄�߂�)")]
        private Material _CutSurfaceMaterial = default;
        //�J�b�g����I�u�W�F�N�g
        private GameObject _tergetObject = default;
        // Ray�������������W�̔z��
        private List<Vector3> _hitPositions = new List<Vector3>();
        // Ray��Hit���
        private RaycastHit _hit;
        // �ؒfRay�̒���
        private float _cutRayDistance = 20f;
        // �ؒf���[�h�̐�������
        private float _canCutTimer = 5f;
        // Ray���I�u�W�F�N�g��ؒf����
        private bool _isCutting = false;
        // �ؒf���̃X���[���
        private bool _isSlow = false;
        // �J�[�\�����b�N
        private bool _isCursorLock = true;
        #endregion

        // �萔�錾---------------------------------
        // ���X�g�̍Ō���𒲐�����
        const int _LIST_END = -1;
        // �ؒf�ł���LayerMask
        const int _CUT_LAYER_MASK = 1 << 8;
        [SerializeField, Range(1, 10), Header("@�ؒf�����I�u�W�F�N�g���m�̊Ԋu���������")]
        private int _CUT_DIVISION_FORCE = 3;
        // centerPos����邽�߂�Ray�̒���
        const float _CENTER_RAY_RANGE = 20f;
        // �ؒf���̃X�R�A
        const int _CUT_SCORE = 100;


        void Update()
        {
            // �Q�[���̏�Ԃ��Q�[�����ȊO�Ȃ珈�����Ȃ�
            if(GameManager.instance.game_State != GameManager.GameState.GameNow)
            {
                return;
            }

            // ���N���b�N��������Ă���
            if (Input.GetMouseButton(0))
            {
                // �J�[�\�����b�N�����Ă�����
                if (_isCursorLock)
                {
                    //  �J�[�\����\��
                    Cursor.lockState = CursorLockMode.None;
                    _isCursorLock = false;
                }

                CheckRayHit();

                // �X���[��Ԃ���Ȃ�������
                if (!_isSlow)
                {
                    Time.timeScale = 0.5f;
                    _isSlow = true;
                }
            }
            else
            {
                // �J�����̑��상�\�b�h���Ăяo��
                GetComponent<PlayerCameraController>().CameraControll();
                // �J�[�\�����b�N����������Ă�����
                if (!_isCursorLock)
                {
                    // �J�[�\�����b�N������
                    Cursor.lockState = CursorLockMode.Locked;
                    _isCursorLock = true;
                }

                // �X���[��Ԃ�������
                if (_isSlow)
                {
                    Time.timeScale = 1f;
                    _isSlow = false;
                }
            }
        }

        /// <summary>
        /// �ؒfRay���I�u�W�F�N�g�ɓ������āA�ؒf���郁�\�b�h
        /// </summary>
        private void CheckRayHit()
        {          
            // �ؒf�ʂ𐶐�����Ray
            Ray bladeRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(bladeRay.origin, bladeRay.direction * _cutRayDistance, Color.green);

            // Ray���I�u�W�F�N�g�������Ă��邩
            if (Physics.Raycast(bladeRay, out _hit, _cutRayDistance, _CUT_LAYER_MASK))
            {
                _tergetObject = _hit.transform.gameObject;
                _hitPositions.Add(_hit.point);
                _isCutting = true;
            }
            // �ؒfRay���I�u�W�F�N�g��ʉ߂�����
            else if (_isCutting)
            {
                // �ؒf�ʂ��v�Z����
                (Vector3 center, Vector3 normal) = CalculationCutSurface(_hitPositions[0], _hitPositions[_hitPositions.Count + _LIST_END]);
                //_meshCut.Cut(_tergetObject, center, normal, _material);
                (GameObject backObject, GameObject frontObject) = NewMeshCut.CutGameObject(_tergetObject, center, normal, _isCutSurfaceFill, _CutSurfaceMaterial);

                // �ؒf�Ώۂ�rigidbody���t���Ă�����
                if (frontObject && _tergetObject.GetComponent<Rigidbody>())
                {
                    // �X�R�A�����Z
                    GameManager.instance._nowScore = _CUT_SCORE;

                    // �ؒf�����I�u�W�F�N�g�̊Ԋu���J����
                    frontObject.GetComponent<Rigidbody>().AddForce(-normal * _CUT_DIVISION_FORCE, ForceMode.Impulse);
                    backObject.GetComponent<Rigidbody>().AddForce(normal * _CUT_DIVISION_FORCE, ForceMode.Impulse);
                }

                //���X�g�̏�����
                _hitPositions.Clear();
                _isCutting = false;
            }
        }

        /// <summary>
        /// �ؒf�ʂ��v�Z���郁�\�b�h
        /// </summary>
        /// <param name="startPos">�n�_�̍��W</param>
        /// <param name="endPos">�I�_�̍��W</param>
        /// <returns>�ؒf�ʂ̒��S�Ɛؒf�ʂ̖@���x�N�g��</returns>
        private (Vector3 center, Vector3 normal) CalculationCutSurface(Vector3 startPos, Vector3 endPos)
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
            if (Physics.Raycast(centerRay, out _hit, _CENTER_RAY_RANGE))
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
    }
}