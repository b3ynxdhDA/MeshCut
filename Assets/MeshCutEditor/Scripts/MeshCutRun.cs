using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    //平均値を取るための
    const int TWO = 2;

    void Start()
    {
        _meshCut = new MeshCut();
    }
    void Update()
    {
        //切断面を生成するRay
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButton(0) && Physics.Raycast(ray, out hit))
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
            Vector3 normal = CalculationNormal(_hitPositions[0], _hitPositions[_hitPositions.Count + LIST_END]).normal;
            _meshCut.Cut(_tergetObject, center, normal, _material);
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
    private (Vector3 normal, Vector3 center) CalculationNormal(Vector3 startPos, Vector3 endPos)
    {
        //切断面の中心を計算
        Vector3 centerPos = (startPos + endPos) / TWO;


        //法線ベクトル計算
        //原点となるカメラのポジション
        Vector3 cameraPos = Camera.main.transform.position;
        //原点から始点の方向
        Vector3 startDirection = (startPos - cameraPos);
        //原点から終点の方向
        Vector3 endDirection = (endPos - cameraPos);

        return (Vector3.Cross(startDirection, endDirection), centerPos);
    }
}
