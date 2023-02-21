using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MeshCutメソッドを実行するためのクラス
/// </summary>
public class MeshCutRun : MonoBehaviour
{
    [SerializeField] private GameObject p = default;//@

    //MeshCutクラス
    private MeshCut _meshCut = default;
    //カットするオブジェクト
    private GameObject _tergetObject = default;
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


Vector3 normal = default;//@
    Vector3 center = default;

    void Start()
    {
        _meshCut = new MeshCut();
    }
    void Update()
    {
        
        //切断面を生成するRay
        Ray bladeRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(bladeRay.origin, bladeRay.direction * 50f, Color.green);
        if (Input.GetMouseButton(0) && Physics.Raycast(bladeRay, out hit) && hit.transform.GetComponent<MeshFilter>())
        {
            _tergetObject = hit.transform.gameObject;
            _hitPositions.Add(hit.point);
            //_hitPositions.Add(hit.transform.InverseTransformPoint(hit.point));
            isCutting = true;
        }
        else if (Input.GetMouseButton(0) && isCutting)
        {
            //Cutに渡す切断面の中心
            center = CalculationNormal(_hitPositions[0], _hitPositions[_hitPositions.Count + LIST_END]).center;
            //Cutに渡す法線ベクトル
            normal = CalculationNormal(_hitPositions[0], _hitPositions[_hitPositions.Count + LIST_END]).normal;
            //_meshCut.Cut(_tergetObject, center, normal, _material);
            //MeshCut.CutMesh(_tergetObject, center, normal, true, _material);
            //NewMeshCut.CutObject(_tergetObject, center, normal, true, _material);
            print("MeshCutRun:法線" + normal);
            //リストの初期化
            _hitPositions.Clear();
            isCutting = false;
        }
        Debug.DrawRay(center, -normal * 20f, Color.blue);
        Debug.DrawRay(centerRay.origin, centerRay.direction * 20f, Color.black);
    }
    Ray centerRay;//@デバッグのためフィールド化
    /// <summary>
    /// 法線ベクトルを計算するメソッド
    /// </summary>
    /// <param name="startPos">始点の座標</param>
    /// <param name="endPos">終点の座標</param>
    /// <returns>法線ベクトル</returns>
    private (Vector3 normal, Vector3 center) CalculationNormal(Vector3 startPos, Vector3 endPos)
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
        centerRay = new Ray(cameraPos, (centerPosDirection - cameraPos).normalized);
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

        print("開始"+startPos);
        print("終了"+endPos);

        return (normalDirection, centerPos);
    }
}
