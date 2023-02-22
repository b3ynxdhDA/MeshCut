using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カメラからメッシュ切断をするRayを飛ばすクラス
/// </summary>
public class MeshCutRun : MonoBehaviour
{
    //カットするオブジェクト
    private GameObject _tergetObject = default;
    [SerializeField]
    private bool _isCutSurfaceMaterial = false;
    //切断面に貼るマテリアル
    [SerializeField] Material _material = default;
    //Rayが当たった座標の配列
    private List<Vector3> _hitPositions = new List<Vector3>();
    //RayのHit情報
    private RaycastHit hit;
    //Rayがオブジェクトを切断中か
    private bool isCutting = false;
    //リストの最後尾を調整する
    const int LIST_END = -1;

    void Update()
    {
        
        //切断面を生成するRay
        Ray bladeRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(bladeRay.origin, bladeRay.direction * 50f, Color.green);
        if (Input.GetMouseButton(0) && Physics.Raycast(bladeRay, out hit) && hit.transform.GetComponent<MeshFilter>())
        {
            _tergetObject = hit.transform.gameObject;
            _hitPositions.Add(hit.point);
            isCutting = true;
        }
        else if (Input.GetMouseButton(0) && isCutting)
        {
            (Vector3 center, Vector3 normal) = CalculationNormal(_hitPositions[0], _hitPositions[_hitPositions.Count + LIST_END]);
            //_meshCut.Cut(_tergetObject, center, normal, _material);
            NewMeshCut.CutMesh(_tergetObject, center, normal, false, _material);
            //リストの初期化
            _hitPositions.Clear();
            isCutting = false;
        }
    }
    /// <summary>
    /// 法線ベクトルを計算するメソッド
    /// </summary>
    /// <param name="startPos">始点の座標</param>
    /// <param name="endPos">終点の座標</param>
    /// <returns>法線ベクトル</returns>
    private (Vector3 center, Vector3 normal) CalculationNormal(Vector3 startPos, Vector3 endPos)
    {
        // 2で割るための変数
        const int TWO = 2;
        // 3で割るための変数
        const int THREE = 3;
        // 原点となるカメラのポジション
        Vector3 cameraPos = Camera.main.transform.position;

        // 面の中心の計算
        // 辺(startPos-endPos)の中央を求める
        Vector3 centerPosDirection = (startPos + endPos) / TWO;
        // カメラから辺の中央にRayを出す
        Ray centerRay = new Ray(cameraPos, (centerPosDirection - cameraPos).normalized);
        // 中央の頂点
        Vector3 otherSidePos = default;
        if(Physics.Raycast(centerRay, out hit, 20f))
        {
            otherSidePos = hit.point;
        }
        // 切断面の中心を計算
        Vector3 centerPos = (startPos + endPos + otherSidePos) / THREE;


        // 法線ベクトル計算
        // 原点から始点の方向
        Vector3 startDirection = (startPos - cameraPos);
        // 原点から終点の方向
        Vector3 endDirection = (endPos - cameraPos);
        // 面の法線
        Vector3 normalDirection = Vector3.Cross(startDirection, endDirection);

        return (centerPos, normalDirection);
    }
}
