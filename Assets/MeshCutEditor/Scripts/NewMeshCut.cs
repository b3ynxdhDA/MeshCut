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

    // 頂点の向きを管理する連想配列
    private Dictionary<int, (int, int)> newVertexDirection = new Dictionary<int, (int, int)>(101);

    // @
    private FragmentList fragmentList = new FragmentList();

    //@
    private RoopFragmentCollection roopFragmentCollection = new RoopFragmentCollection();

    // 正面側の頂点
    private List<Vector3> _frontVertices = new List<Vector3>();
    // 後ろ側の頂点
    private List<Vector3> _backVertices = new List<Vector3>();
    // 正面側の法線
    private List<Vector3> _frontNormals = new List<Vector3>();
    // 後ろ側の法線
    private List<Vector3> _backNormals = new List<Vector3>();
    // 正面側のUV
    private List<Vector2> _frontUVs = new List<Vector2>();
    // 後ろ側のUV
    private List<Vector2> _backUVs = new List<Vector2>();

    // 正面側のサブメッシュ情報
    private List<List<int>> _frontSubmeshIndices = new List<List<int>>();
    // 後ろ側のサブメッシュ情報
    private List<List<int>> _backSubmeshIndices = new List<List<int>>();

    /// <summary>
    /// gameObjectを切断して2つのMeshにして返します.1つ目のMeshが切断面の法線に対して表側, 2つ目が裏側です
    /// 何度も切るようなオブジェクトでも頂点数が増えないように処理をしてあるほか, 簡単な物体なら切断面を縫い合わせることもできます
    /// </summary>
    /// <param name="targetMesh">切断するMesh</param>
    /// <param name="targetTransform">切断するMeshのTransform</param>
    /// <param name="planeAnchorPoint">切断面上のワールド空間上での1点</param>
    /// <param name="planeNormalDirection">切断面のワールド空間上での法線</param>
    /// <param name="makeCutSurface">切断後にMeshを縫い合わせるかどうか</param>
    /// <param name="addNewMeshIndices">新しいsubMeshを作るか(切断面に新しいマテリアルを割り当てる場合はtrue、既に切断面のマテリアルがRendererについている場合はfalse)</param>
    /// <returns>planeに対して正面側のMeshか後ろ側のMesh</returns>
    public (Mesh frontside, Mesh backside) CutMesh(Mesh targetMesh, Transform targetTransform, Vector3 planeAnchorPoint, Vector3 planeNormalDirection, bool makeCutSurface = true, bool addNewMeshIndices = false)
    {
        // 切断面の法線ベクトルがゼロだったら
        if(planeNormalDirection == Vector3.zero)
        {
            Debug.LogError("the normal vector magnitude is zero!");

            Mesh empty = new Mesh();
            empty.vertices = new Vector3[] { };
            return (null, null);
        }

        #region 初期化
            // Mesh情報取得
            _targetMesh = targetMesh;
            // for文で_targetMeshから呼ぶのは重くなるので配列に格納してfor文ではここから渡す(Mesh.verticesなどは参照ではなく毎回コピーを返してるらしい)
            _targetVertices = _targetMesh.vertices;
            _targetNormals = _targetMesh.normals;
            _targetUVs = _targetMesh.uv;

            // 頂点の長さ
            int verticesLangth = _targetVertices.Length;
            _makeCutSurface = makeCutSurface;

            _trackedArray = new int[verticesLangth];
            _isFront = new bool[verticesLangth];
            newVertexDirection.Clear();
            roopFragmentCollection.Clear();
            fragmentList.Clear();

            _frontVertices.Clear();
            _frontNormals.Clear();
            _frontUVs.Clear();
            _frontSubmeshIndices.Clear();

            _backVertices.Clear();
            _backNormals.Clear();
            _backUVs.Clear();
            _backSubmeshIndices.Clear();

            // localScaleに合わせてPlaneに入れるnormal補正をかける
            Vector3 scale = targetTransform.localScale;
            _planeNormal = Vector3.Scale(scale, targetTransform.InverseTransformDirection(planeAnchorPoint)).normalized;
        #endregion

        #region 最初に頂点の情報だけを入力する
        // ローカル空間上での切断面の座標
        Vector3 anchor = targetTransform.transform.InverseTransformPoint(planeAnchorPoint);
        // 切断面の法線とanchorの内積
        _planeValue = Vector3.Dot(_planeNormal, anchor);
        {
            float planeNormal_x = _planeNormal.x;
            float planeNormal_y = _planeNormal.y;
            float planeNormal_z = _planeNormal.z;

            float anchor_x = anchor.x;
            float anchor_y = anchor.y;
            float anchor_z = anchor.z;

            int frontCont = 0;
            int backCont = 0;
            for (int i = 0; i < _targetVertices.Length; i++)
            {
                Vector3 targetVerticesPos = _targetVertices[i];
                // planeの表側にあるか裏側にあるかを判定
                if(_isFront[i] == (planeNormal_x * (targetVerticesPos.x - anchor_x) + 
                                   planeNormal_y *(targetVerticesPos.y - anchor_y) + 
                                    planeNormal_z * (targetVerticesPos.z - anchor_z)) > 0)
                {
                    // 頂点情報を追加していく
                    _frontVertices.Add(targetVerticesPos);
                    _frontNormals.Add(_targetNormals[i]);
                    _frontUVs.Add(_targetUVs[i]);
                    // もとのMeshのn番目の頂点が新しいMeshで何番目になるかを記録(面を貼るときに使う)
                    _trackedArray[i] = frontCont++;
                }
                else
                {
                    // 頂点情報を追加
                    _backVertices.Add(targetVerticesPos);
                    _backNormals.Add(_targetNormals[i]);
                    _backUVs.Add(_targetUVs[i]);
                    // もとのMeshのn番目の頂点が新しいMeshで何番目になるかを記録
                    _trackedArray[i] = backCont++;
                }
            }
            // 頂点が全部片側に寄った場合は終了
            if(frontCont == 0 || backCont == 0)
            {
                return (null, null);
            }
        }
        #endregion

        #region 三角形のポリゴン情報を追加
        // submeshの番号
        int submeshCount = _targetMesh.subMeshCount;

        for(int sub = 0; sub < submeshCount; sub++)
        {
            // ポリゴン情報の配列(indices:indexの複数形)
            int[] indices = _targetMesh.GetIndices(sub);

            // ポリゴンを形成する頂点の番号を入れるintの配列(submeshごとに追加)
            List<int> frontIndices = new List<int>();
            _frontSubmeshIndices.Add(frontIndices);
            List<int> backIndices = new List<int>();
            _backSubmeshIndices.Add(backIndices);

            // ポリゴンの情報は頂点3つで1セットなので3つ飛ばしでループ
            for(int i = 0; i < indices.Length; i += 3)
            {
                int peak1;
                int peak2;
                int peak3;
                peak1 = indices[i];
                peak2 = indices[i + 1];
                peak3 = indices[i + 2];

                // 予め計算しておいた結果を持ってくる(ここで計算すると同じ頂点にたいして何回も同じ計算をすることになるから)
                bool side1 = _isFront[peak1];
                bool side2 = _isFront[peak2];
                bool side3 = _isFront[peak3];

                // 3つとも同じ側にある時はそのまま
                if(side1 && side2 && side3)
                {
                    // indicesは切断前のMeshの頂点番号が入っているので_trackedArryを経由することで切断後のMeshでの番号に変えている
                    frontIndices.Add(_trackedArray[peak1]);
                    frontIndices.Add(_trackedArray[peak2]);
                    frontIndices.Add(_trackedArray[peak3]);
                }
                else if(!side1 && !side2 && !side3)
                {
                    frontIndices.Add(_trackedArray[peak1]);
                    frontIndices.Add(_trackedArray[peak2]);
                    frontIndices.Add(_trackedArray[peak3]);
                }
                else
                {
                    // 三角ポリゴンを形成する各点で面に対する表裏が異なる場合, つまり切断面と重なっている平面は分割する
                    Sepalate(new bool[3] { side1, side2, side3 }, new int[3] { peak1, peak2, peak3 }, sub);
                }
            }
        }
        #endregion

        #region 切断されたポリゴンはここでそれぞれのメッシュに追加される
        fragmentList.MakeTriangle();

        if (makeCutSurface)
        {
            if (addNewMeshIndices)
            {
                // submeshが増えるのでリストを追加
                _frontSubmeshIndices.Add(new List<int>());
                _backSubmeshIndices.Add(new List<int>());
            }
            roopFragmentCollection.MakeCutSurface(_frontSubmeshIndices.Count - 1, targetTransform);
        }
        #endregion

        #region 2つのMeshを新規に作ってそれぞれに情報を追加して出力
        Mesh frontMesh = new Mesh();
        frontMesh.name = "Split Mesh front";

        frontMesh.vertices = _frontVertices.ToArray();
        frontMesh.normals = _frontNormals.ToArray();
        frontMesh.uv = _frontUVs.ToArray();

        frontMesh.subMeshCount = _frontSubmeshIndices.Count;
        for(int i = 0; i < _frontSubmeshIndices.Count; i++)
        {
            // メッシュの作成
            frontMesh.SetIndices(_frontSubmeshIndices[i].ToArray(), MeshTopology.Triangles, i, false);
        }

        Mesh backMesh = new Mesh();
        backMesh.name = "Splite Mesh back";
        backMesh.vertices = _backVertices.ToArray();
        backMesh.normals = _backNormals.ToArray();
        backMesh.uv = _backUVs.ToArray();

        backMesh.subMeshCount = _backSubmeshIndices.Count;
        for(int i = 0; i < _backSubmeshIndices.Count; i++)
        {
            // メッシュの作成
            backMesh.SetIndices(_backSubmeshIndices[i].ToArray(), MeshTopology.Triangles, i, false);
        }

        return (frontMesh, backMesh);
        #endregion
    }

    /// <summary>
    /// Meshを切断します. 
    /// 1つ目のGameObjectが法線の向いている方向で新しくInstantiateしたもの, 1つ目のGameObjectが法線と反対方向で入力したものを返します
    /// </summary>
    /// <param name="targetGameObject">切断されるGameObject</param>
    /// <param name="planeAnchorPoint">切断平面上のどこか1点(ワールド座標)</param>
    /// <param name="planeNormalDirection">切断平面の法線(ワールド座標)</param>
    /// <param name="makeCutSurface">切断面を作るかどうか</param>
    /// <param name="cutSurfaceMaterial">切断面に割り当てるマテリアル(nullの場合は適当なマテリアルを割り当てる)</param>
    /// <returns></returns>
    public (GameObject copy_normaliside,GameObject original_anitiNormalside)CutObject(GameObject targetGameObject, Vector3 planeAnchorPoint,Vector3 planeNormalDirection,bool makeCutSurface,Material cutSurfaceMaterial = null)
    {
        if (!targetGameObject.GetComponent<MeshFilter>())
        {
            Debug.LogError("引数のオブジェクトにはMeshFilterをアタッチしてください");
            return (null, null);
        }
        else if (!targetGameObject.GetComponent<MeshRenderer>())
        {
            Debug.LogError("引数のオブジェクトにはMeshrendererをアタッチしてください");
            return (null, null);
        }

        Mesh targetMesh = targetGameObject.GetComponent<MeshFilter>().mesh;
        Transform targetTransform = targetGameObject.transform;
        // 切断面にマテリアルが割り当てられているかどうか
        bool addNewMaterial;

        MeshRenderer renderer = targetGameObject.GetComponent<MeshRenderer>();
        // materialにアクセスするとその瞬間にmaterialの個別のインスタンスが作られてマテリアル名に(instance)がついてしまうので読み込みはsharedMaterialで行う
        Material[] materials = renderer.sharedMaterials;
        if(makeCutSurface && cutSurfaceMaterial != null)
        {
            // すでに切断面にマテリアルが追加されているときはそれを使うので追加しない
            if (materials[materials.Length - 1]?.name == cutSurfaceMaterial.name)
            {
                addNewMaterial = false;
            }
            else
            {
                addNewMaterial = true;
            }
        }
        else
        {
            addNewMaterial = false;
        }

        (Mesh fragMeah, Mesh originMesh) = CutMesh(targetMesh, targetTransform, planeAnchorPoint, planeNormalDirection, makeCutSurface, addNewMaterial);

        if(originMesh == null || fragMeah == null)
        {
            return(null, null);
        }

        if (addNewMaterial)
        {
            int materialLength = materials.Length;
            Material[] newMaterials = new Material[materialLength + 1];
            materials.CopyTo(newMaterials, 0);
            newMaterials[materialLength] = cutSurfaceMaterial;

            renderer.sharedMaterials = newMaterials;
        }

        targetGameObject.GetComponent<MeshFilter>().mesh = originMesh;

        Transform originTransform = targetTransform.transform;
        GameObject fragment = Instantiate(targetGameObject, originTransform.position, originTransform.rotation, originTransform.parent);
        fragment.transform.parent = null;
        fragment.GetComponent<MeshFilter>().mesh = fragMeah;
        fragment.GetComponent<MeshRenderer>().sharedMaterials = targetGameObject.GetComponent<MeshRenderer>().sharedMaterials;
        
        if(targetGameObject.GetComponent<MeshCollider>())
        {
            // 頂点が1点に重なっている場合はエラーが出るので、
            targetGameObject.GetComponent<MeshCollider>().sharedMesh = originMesh;
            fragment.GetComponent<MeshCollider>().sharedMesh = fragMeah;
        }
        return (fragment, targetGameObject);
    }

    /// <summary>
    /// ポリゴンを切断
    /// ポリゴンは切断面の表側と裏側に分割される.
    /// このとき三角ポリゴンを表面から見て, なおかつ切断面の表側にある頂点が下に来るように見て,
    /// 三角形の左側の辺を形成する点をfront0,back0, 右側にある辺を作る点をf1,b1とする.(fは表側にある点でbは裏側)(頂点は3つなので被りが存在する)
    /// ここでポリゴンの向きを決めておくと後々とても便利
    /// 以降左側にあるものは0,右側にあるものは1をつけて扱う
    /// (ひょっとすると実際の向きは逆かもしれないけどvertexIndicesと同じまわり方で出力してるので逆でも問題はない)
    /// </summary>
    /// <param name="sides"></param>
    /// <param name="vertexIndices"></param>
    /// <param name="submesh"></param>
    private static void Sepalate(bool[] sides, int[] vertexIndices, int submesh)
    {
        // 頂点のindex番号を格納するのに使用
        // 三角形の左側にある辺を作る点
        int frontLeft = 0;
        int backLeft = 0;
        // 三角形の右側にある辺を作る点
        int frontRight = 0;
        int backRight = 0;

        // どちらがに頂点が2つあるか
        bool twoPointsInFrontSide;

        // ポリゴンの向きを揃える
        if(sides[0])
        {
            if (sides[1])
            {
                frontLeft = vertexIndices[1];
                frontRight = vertexIndices[0];
                backLeft = backRight = vertexIndices[2];
                twoPointsInFrontSide = true;
            }
        }
    }
}
