using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MeshCutメソッドを実行するためのクラス
/// </summary>
public class MeshCutRun : MonoBehaviour
{
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
Vector3 normal = default;

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
            isCutting = true;
        }
        else if (Input.GetMouseButton(0) && isCutting)
        {
            //Cutに渡す切断面の中心
            Vector3 center = CalculationNormal(_hitPositions[0], _hitPositions[_hitPositions.Count + LIST_END]).center;
            //Cutに渡す法線ベクトル
            normal = CalculationNormal(_hitPositions[0], _hitPositions[_hitPositions.Count + LIST_END]).normal;
            _meshCut.Cut(_tergetObject, center, normal, _material);
            print(normal);
            //リストの初期化
            _hitPositions.Clear();
            isCutting = false;
        }
        Debug.DrawRay(_tergetObject.transform.position, normal * 20f, Color.blue);
        Debug.DrawRay(centerRay.origin, centerRay.direction * 10, Color.black);
    }
    Ray centerRay;
    /// <summary>
    /// 法線ベクトルを計算するメソッド
    /// </summary>
    /// <param name="startPos">始点の座標</param>
    /// <param name="endPos">終点の座標</param>
    /// <returns>法線ベクトル</returns>
    private (Vector3 normal, Vector3 center) CalculationNormal(Vector3 startPos, Vector3 endPos)
    {
        // 2値を割るための変数
        const int TWO = 2;
        // 3値を割るための変数
        const int THREE = 3;
        // 原点となるカメラのポジション
        Vector3 cameraPos = Camera.main.transform.position;

        // 辺の中央を求める
        Vector3 centerPosDirection = (startPos + endPos) / TWO;
        // オブジェクトの反対側から辺の中央にRayを出す
        centerRay = new Ray(cameraPos, (centerPosDirection - cameraPos).normalized);
        // オブジェクトの反対側の頂点
        Vector3 otherSidePos = default;
        if(Physics.Raycast(centerRay, out hit, 20f))
        {
            otherSidePos = hit.point;
        }

        //法線ベクトル計算
        //原点から始点の方向
        Vector3 startDirection = (startPos - otherSidePos);
        //原点から終点の方向
        Vector3 endDirection = (endPos - otherSidePos);

        //切断面の中心を計算
        Vector3 centerPos = (startPos + endPos + otherSidePos) / THREE;

        print("スタート" + startPos);
        print("エンド" + endPos);
        print("頂点" + otherSidePos);

        print("中心" + centerPos);

        return (Vector3.Cross(startDirection, endDirection), centerPos);
    }
}
