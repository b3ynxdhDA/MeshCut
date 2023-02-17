using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewMeshCut : MonoBehaviour
{
    // @変数宣言はstaticからprivateに
    // 切断対象のメッシュ
    private Mesh _targetMesh = default;

    // 参照渡し防止のため配列であらかじめ宣言
    // 切断対象のメッシュの頂点(ポリゴン)
    private Vector3[] _targetVertices = default;
    // 切断対象のメッシュの法線
    private Vector3[] _targetNormals = default;
    // 切断対象のメッシュのUV
    private Vector2[] _targetUVs = default;

    //平面の方程式はn・r=h(nは法線,rは位置ベクトル,hはconst(=_planeValue))
    private Vector3 _planeNormal = default;  //nの部分
    private float _planeValue = default;  //rの部分

    // 頂点が切断面に対して表にあるか裏にあるか
    private bool[] _isFront = default;
    // 切断後のメッシュでの切断前の頂点の番号
    private int[] _trackedArray = default;

    // 切断面を生成済みか
    private bool _makeCutSurface = default;

    // 
    private Dictionary<int, (int, int)> newVertexDic = new Dictionary<int, (int, int)>(101);

}
