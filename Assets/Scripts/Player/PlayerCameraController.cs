using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    // �v���C���[�̃��C���J����
    private GameObject _playerCamera;
    // �J�����̌����Ă���p�x
    private Quaternion _cameraRot;
    // �L�����N�^�[�̊p�x
    private Quaternion _characterRot;
    // X���̊��x
    private float Xsensityvity = 3f;
    // Y���̊��x
    private float Ysensityvity = 3f;
    // �J�[�\�����b�N
    private bool cursorLock = true;

    // ����p�x�̐���
    const float minX = -90f;
    const float maxX = 90f;

    void Start()
    {
        // �v���C���[�̃��C���J�������擾
        _playerCamera = Camera.main.gameObject;
        _cameraRot = _playerCamera.transform.localRotation;
        _characterRot = transform.localRotation;
    }

    /// <summary>
    /// �J�����̑�������郁�\�b�h
    /// </summary>
    public void CameraControll()
    {
        float xRot = Input.GetAxis("Mouse X") * Ysensityvity;
        float yRot = Input.GetAxis("Mouse Y") * Xsensityvity;

        _cameraRot *= Quaternion.Euler(-yRot, 0, 0);
        _characterRot *= Quaternion.Euler(0, xRot, 0);

        //Update�̒��ō쐬�����֐����Ă�
        _cameraRot = ClampRotation(_cameraRot);

        _playerCamera.transform.localRotation = _cameraRot;
        transform.localRotation = _characterRot;

        //UpdateCursorLock();
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateCursorLock()
    {
        // 
        if (Input.GetMouseButton(0))
        {
            cursorLock = false;
        }
        else
        {
            cursorLock = true;
        }

        // �J�[�\����\�����Ȃ�
        if (cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (!cursorLock)
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    /// <summary>
    /// �p�x�����֐�
    /// </summary>
    /// <param name="q">x,y,z,w (x,y,z�̓x�N�g���i�ʂƌ����Fw�̓X�J���[���W�Ƃ͖��֌W�̗�))</param>
    /// <returns></returns>
    private Quaternion ClampRotation(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1f;

        float angleX = Mathf.Atan(q.x) * Mathf.Rad2Deg * 2f;

        angleX = Mathf.Clamp(angleX, minX, maxX);

        q.x = Mathf.Tan(angleX * Mathf.Deg2Rad * 0.5f);

        return q;
    }
}
