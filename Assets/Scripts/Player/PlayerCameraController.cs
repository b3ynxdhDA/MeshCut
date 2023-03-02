using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    // プレイヤーのメインカメラ
    private GameObject _playerCamera;
    // カメラの向いている角度
    private Quaternion _cameraRot;
    // キャラクターの角度
    private Quaternion _characterRot;
    // X軸の感度
    private float Xsensityvity = 3f;
    // Y軸の感度
    private float Ysensityvity = 3f;
    // カーソルロック
    private bool cursorLock = true;

    // 旋回角度の制限
    const float minX = -90f;
    const float maxX = 90f;

    void Start()
    {
        // プレイヤーのメインカメラを取得
        _playerCamera = Camera.main.gameObject;
        _cameraRot = _playerCamera.transform.localRotation;
        _characterRot = transform.localRotation;
    }

    /// <summary>
    /// カメラの操作をするメソッド
    /// </summary>
    public void CameraControll()
    {
        float xRot = Input.GetAxis("Mouse X") * Ysensityvity;
        float yRot = Input.GetAxis("Mouse Y") * Xsensityvity;

        _cameraRot *= Quaternion.Euler(-yRot, 0, 0);
        _characterRot *= Quaternion.Euler(0, xRot, 0);

        //Updateの中で作成した関数を呼ぶ
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

        // カーソルを表示しない
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
    /// 角度制限関数
    /// </summary>
    /// <param name="q">x,y,z,w (x,y,zはベクトル（量と向き：wはスカラー座標とは無関係の量))</param>
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
