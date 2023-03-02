using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カメラからメッシュ切断をするRayを飛ばすクラス
/// </summary>
public class MeshCutRun : MonoBehaviour
{
    #region MeshCut関係の変数
    [SerializeField,Header("切断面を埋める")]
    private bool _isCutSurfaceFill = false;
    [SerializeField,Header("切断面のマテリアル(nullならオブジェクトのマテリアルから埋める)")]
    private Material _CutSurfaceMaterial = default;
    //カットするオブジェクト
    private GameObject _tergetObject = default;
    // 法線の向いている方向で新しくInstantiateしたオブジェクト
    private GameObject _frontObject = default;
    // 法線の反対方向で変形したオリジナルのオブジェクト
    private GameObject _backObject = default;
    // Rayが当たった座標の配列
    private List<Vector3> _hitPositions = new List<Vector3>();
    // 切断Rayの長さ
    private float _cutRayDistance = 30f;
    // RayのHit情報
    private RaycastHit _hit;
    // Rayがオブジェクトを切断中か
    private bool _isCutting = false;
    // リストの最後尾を調整する
    const int _LIST_END = -1;
    // 切断できるLayerMask
    const int _CUT_LAYER_MASK = 1 << 8;
    // 切断したオブジェクト同士の間隔をあける力
    const int _CUT_DIVISION_FORCE = 3;
    // centerPosを取るためのRayの長さ
    const float _CENTER_RAY_RANGE = 20f;
    #endregion

    void Update()
    {
        // 切断面を生成するRay
        Ray bladeRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(bladeRay.origin, bladeRay.direction * _cutRayDistance, Color.green);
        // 左クリックが押されている
        if (Input.GetMouseButton(0))
        {
            // Rayがオブジェクト当たっているか
            if (Physics.Raycast(bladeRay, out _hit, _cutRayDistance, _CUT_LAYER_MASK))
            {
                _tergetObject = _hit.transform.gameObject;
                _hitPositions.Add(_hit.point);
                _isCutting = true;
            }
            // 切断Rayがオブジェクトを通過したら
            else if (_isCutting)
            {
                (Vector3 center, Vector3 normal) = CalculationCutSurface(_hitPositions[0], _hitPositions[_hitPositions.Count + _LIST_END]);
                //_meshCut.Cut(_tergetObject, center, normal, _material);
                (_backObject, _frontObject) = NewMeshCut.CutGameObject(_tergetObject, center, normal, _isCutSurfaceFill, _CutSurfaceMaterial);

                // 切断対象にrigidbodyが付いていたら
                if (_frontObject && _tergetObject.GetComponent<Rigidbody>())
                {
                    // 切断したオブジェクトの間隔を開ける
                    _frontObject.GetComponent<Rigidbody>().AddForce(-normal * _CUT_DIVISION_FORCE);
                    _backObject.GetComponent<Rigidbody>().AddForce(normal * _CUT_DIVISION_FORCE);
                }

                //リストの初期化
                _hitPositions.Clear();
                _isCutting = false;
            }
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            GetComponent<PlayerCameraController>().CameraControll();
            Cursor.lockState = CursorLockMode.Locked;
        }

    }
    /// <summary>
    /// 切断面を計算するメソッド
    /// </summary>
    /// <param name="startPos">始点の座標</param>
    /// <param name="endPos">終点の座標</param>
    /// <returns>切断面の中心と切断面の法線ベクトル</returns>
    private (Vector3 center, Vector3 normal) CalculationCutSurface(Vector3 startPos, Vector3 endPos)
    {
        // 原点となるカメラのポジション
        Vector3 cameraPos = Camera.main.transform.position;

        // 面の中心の計算
        // 辺(startPos-endPos)の中央を求める
        Vector3 centerPosDirection = (startPos + endPos) / 2;
        // カメラから辺の中央にRayを出す
        Ray centerRay = new Ray(cameraPos, (centerPosDirection - cameraPos).normalized);
        // 中央の頂点
        Vector3 otherSidePos = default;
        if(Physics.Raycast(centerRay, out _hit, _CENTER_RAY_RANGE))
        {
            otherSidePos = _hit.point;
        }
        // 切断面の三点の中心を計算
        Vector3 centerPos = (startPos + endPos + otherSidePos) / 3;

        // 法線ベクトル計算
        // 原点から始点の方向
        Vector3 startDirection = (startPos - centerPos);
        // 原点から終点の方向
        Vector3 endDirection = (endPos - centerPos);
        // 面の法線
        Vector3 normalDirection = Vector3.Cross(startDirection, endDirection);

        return (centerPos, normalDirection);
    }
}