using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カメラからメッシュ切断をするRayを飛ばすクラス
/// </summary>
public class MeshCutRun : MonoBehaviour
{
    #region MeshCut関係の変数
    // 切断できるLayerMask
    const int _cutLayerMask = 1 << 8;
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
    //Rayが当たった座標の配列
    private List<Vector3> _hitPositions = new List<Vector3>();
    //RayのHit情報
    private RaycastHit _hit;
    //Rayがオブジェクトを切断中か
    private bool _isCutting = false;
    //リストの最後尾を調整する
    const int _LIST_END = -1;
    // centerPosを取るためのRayの長さ
    const float _CENTER_RAY_RANGE = 20f;
    #endregion

    #region DestroyObject関係の変数
    // 接地判定のRayの長さ
    private float _chechGroundDistance = 0.5f;
    // Rayが衝突するLayerMask
    // 接地判定を取るレイヤー
    const int _hitGroundLayer = 1 << 9;
    // 削除のディレイ
    const float _DESTROY_DELAY = 3f;
    #endregion

    void Update()
    {
        //切断面を生成するRay
        Ray bladeRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(bladeRay.origin, bladeRay.direction * 50f, Color.green);
        // 左クリックが押されているかつRayがオブジェクト当たっているか
        if (Input.GetMouseButton(0) && Physics.Raycast(bladeRay, out _hit, _cutLayerMask))
        {
            print(_cutLayerMask);
            print(_hit.transform.gameObject.layer);
            _tergetObject = _hit.transform.gameObject;
            _hitPositions.Add(_hit.point);
            _isCutting = true;
        }
        // 切断Rayがオブジェクトを通過したら
        else if (Input.GetMouseButton(0) && _isCutting)
        {
            (Vector3 center, Vector3 normal) = CalculationNormal(_hitPositions[0], _hitPositions[_hitPositions.Count + _LIST_END]);
            //_meshCut.Cut(_tergetObject, center, normal, _material);
            (_backObject, _frontObject) = NewMeshCut.CutGameObject(_tergetObject, center, normal, _isCutSurfaceFill, _CutSurfaceMaterial);

            // 切断対象にrigidbodyが付いていたら
            if (_tergetObject.GetComponent<Rigidbody>())
            {
                // 切断したオブジェクトの間を開ける
                _frontObject.GetComponent<Rigidbody>().AddForce(-normal);
                _backObject.GetComponent<Rigidbody>().AddForce(normal);
            }
            
            //リストの初期化
            _hitPositions.Clear();
            _isCutting = false;
        }
    }
    /// <summary>
    /// 法線ベクトルを計算するメソッド
    /// </summary>
    /// <param name="startPos">始点の座標</param>
    /// <param name="endPos">終点の座標</param>
    /// <returns>切断面の中心と切断面の法線ベクトル</returns>
    private (Vector3 center, Vector3 normal) CalculationNormal(Vector3 startPos, Vector3 endPos)
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
    /// <summary>
    /// 切断後のオブジェクトが接地したら削除するメソッド
    /// </summary>
    private void DestroyObject(GameObject destroyObj)
    {
        // 接地判定
        bool _isGround = false;

        // オブジェクトの内側より接地判定のRayを長くする
        _chechGroundDistance += this.transform.lossyScale.y / 2;

        int i = 0;

        while(!_isGround)
        {
            print("接地探索");
            Debug.DrawRay(transform.position, Vector3.down * _chechGroundDistance, Color.red);
            // オブジェクトが地面に着いたら
            if (!_isGround && Physics.Raycast(new Ray(transform.position, Vector3.down), _chechGroundDistance, _hitGroundLayer))
            {
                _isGround = true;
                Destroy(destroyObj, _DESTROY_DELAY);
            }

            i++;
            if(i > 100)
            {
                break;
            }
            
        }
    }
}