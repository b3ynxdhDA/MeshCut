using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 処理速度を意識したメッシュ切断クラス
/// </summary>
public class NewMeshCut : MonoBehaviour
{
    // 切断対象のメッシュ
    static Mesh _targetMesh;

    // 参照渡し防止のため配列であらかじめ宣言
    // @この3つはめっちゃ大事でこれ書かないと10倍くらい重くなる(for文中で使うから参照渡しだとやばい)
    // 切断対象のメッシュの頂点(ポリゴン)
    static Vector3[] _targetVertices;
    // 切断対象のメッシュの法線
    static Vector3[] _targetNormals;
    // 切断対象のメッシュのUV
    static Vector2[] _targetUVs;   

    //平面の方程式はn・r=h(nは法線,rは位置ベクトル,hはconst(=_planeValue))
    static Vector3 _planeNormal; //nの部分
    static float _planeValue;    //rの部分

    //頂点が切断面に対して表にあるか裏にあるか
    static bool[] _isFront;
    //切断後のMeshでの切断前の頂点の番号を追跡する配列
    static int[] _trackedArray;

    // 切断面を生成済みか
    static bool _isMakeCutSurface;

    // 頂点の向きを管理する連想配列
    static Dictionary<int, (int, int)> newVertexDic = new Dictionary<int, (int, int)>(101);

    // FragmentListクラスの参照
    static FragmentList fragmentList = new FragmentList();

    // RoopFragmentCallectionクラスの参照
    static RoopFragmentCollection roopCollection = new RoopFragmentCollection();

    // 正面側の頂点
    static List<Vector3> _frontVertices = new List<Vector3>();
    // 後ろ側の頂点
    static List<Vector3> _backVertices = new List<Vector3>();
    // 正面側の法線
    static List<Vector3> _frontNormals = new List<Vector3>();
    // 後ろ側の法線
    static List<Vector3> _backNormals = new List<Vector3>();
    // 正面側のUV
    static List<Vector2> _frontUVs = new List<Vector2>();
    // 後ろ側のUV
    static List<Vector2> _backUVs = new List<Vector2>();

    // 正面側のサブメッシュ情報
    static List<List<int>> _frontSubmeshIndices = new List<List<int>>();
    // 後ろ側のサブメッシュ情報
    static List<List<int>> _backSubmeshIndices = new List<List<int>>();

    /// <summary>
    /// NewMeshCutクラス内で呼び出されるメソッド
    /// <para>メッシュを切断して2つのMeshにして返します</para>
    /// <para>何度も切るようなオブジェクトでも頂点数が増えないように処理をしてあるほか, 簡単な物体なら切断面を縫い合わせることもできます</para>
    /// </summary>
    /// <param name="targetMesh">切断するMesh</param>
    /// <param name="targetTransform">切断するMeshのTransform</param>
    /// <param name="planeAnchorPoint">切断面上のワールド空間上での1点</param>
    /// <param name="planeNormalDirection">切断面のワールド空間上での法線</param>
    /// <param name="makeCutSurface">切断後にMeshを縫い合わせるか否か</param>
    /// <param name="addNewMeshIndices">新しいsubMeshを作るか(切断面に新しいマテリアルを割り当てる場合にはtrue, すでに切断面のマテリアルがRendererについてる場合はfalse)</param>
    /// <returns>frontsideが切断面の法線に対して表側, backsideが裏側です</returns>
    public static (Mesh frontside, Mesh backside) CutMesh(Mesh targetMesh, Transform targetTransform, Vector3 planeAnchorPoint, Vector3 planeNormalDirection, bool makeCutSurface = true, bool addNewMeshIndices = false)
    {
        // 切断面の法線ベクトルがゼロだったら
        if (planeNormalDirection == Vector3.zero)
        {
            Debug.LogError("法線ベクトルの大きさはゼロです!");

            Mesh empty = new Mesh();
            empty.vertices = new Vector3[] { };
            return (null, null);
        }


        #region 初期化
        //Mesh情報取得
        _targetMesh = targetMesh;
        //for文で_targetMeshから呼ぶのは非常に重くなるのでここで配列に格納してfor文ではここから渡す(Mesh.verticesなどは参照ではなくて毎回コピーしたものを返してるらしい)
        _targetVertices = _targetMesh.vertices;
        _targetNormals = _targetMesh.normals;
        _targetUVs = _targetMesh.uv;


        // 頂点の長さ
        int verticesLength = _targetVertices.Length;
        _isMakeCutSurface = makeCutSurface;

        _trackedArray = new int[verticesLength];
        _isFront = new bool[verticesLength];
        newVertexDic.Clear();
        roopCollection.Clear();
        fragmentList.Clear();

        _frontVertices.Clear();
        _frontNormals.Clear();
        _frontUVs.Clear();
        _frontSubmeshIndices.Clear();

        _backVertices.Clear();
        _backNormals.Clear();
        _backUVs.Clear();
        _backSubmeshIndices.Clear();

        //localscaleに合わせてPlaneに入れるnormalに補正をかける
        Vector3 scale = targetTransform.localScale;
        _planeNormal = Vector3.Scale(scale, targetTransform.InverseTransformDirection(planeNormalDirection)).normalized;
        #endregion

        #region 最初に頂点の情報だけを入力していく

        // ローカル空間上での切断面の座標
        Vector3 anchor = targetTransform.transform.InverseTransformPoint(planeAnchorPoint);
        // 切断面の法線とanchorの内積
        _planeValue = Vector3.Dot(_planeNormal, anchor);
        {
            float planeNormal_x = _planeNormal.x;
            float planeNormal_y = _planeNormal.y;
            float planeNormal_z = _planeNormal.z;

            float anchor_x = anchor.x;
            float ancy = anchor.y;
            float anchor_z = anchor.z;

            int frontCount = 0;
            int backCount = 0;
            for (int i = 0; i < _targetVertices.Length; i++)
            {
                Vector3 pos = _targetVertices[i];
                // 頂点がplaneの表側にあるか裏側にあるかを判定
                if (_isFront[i] = (planeNormal_x * (pos.x - anchor_x) +
                    planeNormal_y * (pos.y - ancy) +
                    planeNormal_z * (pos.z - anchor_z)) > 0)
                {
                    //頂点情報を追加していく
                    _frontVertices.Add(pos);
                    _frontNormals.Add(_targetNormals[i]);
                    _frontUVs.Add(_targetUVs[i]);
                    //もとのMeshのn番目の頂点が新しいMeshで何番目になるのかを記録(面を貼るときに使う)
                    _trackedArray[i] = frontCount++;
                }
                else
                {
                    // 頂点情報を追加
                    _backVertices.Add(pos);
                    _backNormals.Add(_targetNormals[i]);
                    _backUVs.Add(_targetUVs[i]);
                    // もとのMeshのn番目の頂点が新しいMeshで何番目になるかを記録
                    _trackedArray[i] = backCount++;
                }
            }

            // 片側に全部寄った場合はここで終了
            if (frontCount == 0 || backCount == 0)
            {
                return (null, null);
            }
        }
        #endregion

        #region 三角形のポリゴン情報を追加

        // submeshの番号
        int submeshCount = _targetMesh.subMeshCount;

        for (int sub = 0; sub < submeshCount; sub++)
        {
            // ポリゴン情報の配列(indices:indexの複数形)
            int[] indices = _targetMesh.GetIndices(sub);

            //ポリゴンを形成する頂点の番号を入れるintの配列を作っている.(submeshごとに追加)
            List<int> frontIndices = new List<int>();
            _frontSubmeshIndices.Add(frontIndices);
            List<int> backIndices = new List<int>();
            _backSubmeshIndices.Add(backIndices);

            //ポリゴンの情報は頂点3つで1セットなので3つ飛ばしでループ
            for (int i = 0; i < indices.Length; i += 3)
            {
                int peak1;
                int peak2;
                int peak3;
                peak1 = indices[i];
                peak2 = indices[i + 1];
                peak3 = indices[i + 2];


                //予め計算しておいた結果を持ってくる(ここで計算すると同じ頂点にたいして何回も同じ計算をすることになるから)
                // 三角形の頂点が切断面の表裏どちら側にあるか
                bool side1 = _isFront[peak1];
                bool side2 = _isFront[peak2];
                bool side3 = _isFront[peak3];


                //3つとも表側, 3つとも裏側のときはそのまま出力
                if (side1 && side2 && side3)
                {
                    //indicesは切断前のMeshの頂点番号が入っているので_trackedArrayを通すことで切断後のMeshでの番号に変えている
                    frontIndices.Add(_trackedArray[peak1]);
                    frontIndices.Add(_trackedArray[peak2]);
                    frontIndices.Add(_trackedArray[peak3]);
                }
                else if (!side1 && !side2 && !side3)
                {
                    backIndices.Add(_trackedArray[peak1]);
                    backIndices.Add(_trackedArray[peak2]);
                    backIndices.Add(_trackedArray[peak3]);
                }
                else
                {
                    //三角ポリゴンを形成する各点で面に対する表裏が異なる場合, つまり切断面と重なっている平面は分割する.
                    Sepalate(new bool[3] { side1, side2, side3 }, new int[3] { peak1, peak2, peak3 }, sub);
                }

            }

        }
        #endregion

        // 切断されたポリゴンはここでそれぞれのMeshに追加される
        fragmentList.MakeTriangle();

        if (makeCutSurface)
        {
            if (addNewMeshIndices)
            {
                //submeshが増えるのでリスト追加
                _frontSubmeshIndices.Add(new List<int>());
                _backSubmeshIndices.Add(new List<int>());
            }
            roopCollection.MakeCutSurface(_frontSubmeshIndices.Count - 1, targetTransform);
        }

        #region 2つのMeshを新規に作ってそれぞれに情報を追加して出力

        // 切断面の表側にあるメッシュ
        Mesh frontMesh = new Mesh();
        frontMesh.name = "Split Mesh front";

        frontMesh.vertices = _frontVertices.ToArray();
        frontMesh.normals = _frontNormals.ToArray();
        frontMesh.uv = _frontUVs.ToArray();

        // サブメッシュのカウントを進める
        frontMesh.subMeshCount = _frontSubmeshIndices.Count;
        for (int i = 0; i < _frontSubmeshIndices.Count; i++)
        {
            // メッシュの生成
            frontMesh.SetIndices(_frontSubmeshIndices[i].ToArray(), MeshTopology.Triangles, i, false);
        }

        // 切断面の裏側にあるメッシュ
        Mesh backMesh = new Mesh();
        backMesh.name = "Split Mesh back";
        backMesh.vertices = _backVertices.ToArray();
        backMesh.normals = _backNormals.ToArray();
        backMesh.uv = _backUVs.ToArray();

        // サブメッシュのカウントを進める
        backMesh.subMeshCount = _backSubmeshIndices.Count;
        for (int i = 0; i < _backSubmeshIndices.Count; i++)
        {
            // メッシュ生成
            backMesh.SetIndices(_backSubmeshIndices[i].ToArray(), MeshTopology.Triangles, i, false);
        }
        #endregion

        return (frontMesh, backMesh);
    }

    /// <summary>
    /// 外のクラスから呼び出すメソッド
    /// GameObjectを切断して２つのGameObjectを返す
    /// </summary>
    /// <param name="targetGameObject">切断されるGameObject</param>
    /// <param name="planeAnchorPoint">切断平面上のどこか1点(ワールド座標)</param>
    /// <param name="planeNormalDirection">切断平面の法線(ワールド座標)</param>
    /// <param name="makeCutSurface">切断面を作るかどうか</param>
    /// <param name="cutSurfaceMaterial">切断面に割り当てるマテリアル(nullの場合は適当なマテリアルを割り当てる)</param>
    /// <returns>copy_normalsideが法線の向いている方向で新しくInstantiateしたもの,original_anitiNormalsideが法線と反対方向で入力したもの</returns>
    public static (GameObject copy_normalside, GameObject original_anitiNormalside) CutGameObject(GameObject targetGameObject, Vector3 planeAnchorPoint, Vector3 planeNormalDirection, bool makeCutSurface = true, Material cutSurfaceMaterial = null)
    {
        // 切断対象にMeshFilterがアタッチされているか
        if (!targetGameObject.GetComponent<MeshFilter>())
        {
            Debug.LogError("引数のオブジェクトにはMeshFilterをアタッチしろ!");
            return (null, null);
        }
        // 切断対象にMeshRendererがアタッチされているか
        else if (!targetGameObject.GetComponent<MeshRenderer>())
        {
            Debug.LogError("引数のオブジェクトにはMeshrendererをアタッチしろ!");
            return (null, null);
        }

        Mesh mesh = targetGameObject.GetComponent<MeshFilter>().mesh;
        Transform transform = targetGameObject.transform;
        // 切断面にマテリアルが割り当てられているかどうか
        bool addNewMaterial;

        MeshRenderer renderer = targetGameObject.GetComponent<MeshRenderer>();
        //materialにアクセスするとその瞬間にmaterialの個別のインスタンスが作られてマテリアル名に(instance)がついてしまうので読み込みはsharedMaterialで行う
        Material[] materials = renderer.sharedMaterials;
        // 切断面を埋めるか && 切断面に新しいマテリアルを設定するか
        if (makeCutSurface && cutSurfaceMaterial != null)
        {
            // すでに切断マテリアルが追加されているときはそれを使うので追加しない
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

        (Mesh fragMesh, Mesh originMesh) = CutMesh(mesh, transform, planeAnchorPoint, planeNormalDirection, makeCutSurface, addNewMaterial);

        // いづれかのメッシュが生成できていなければ
        if (originMesh == null || fragMesh == null)
        {
            return (null, null);

        }
        // 新しいマテリアルを追加するか
        if (addNewMaterial)
        {
            int matLength = materials.Length;
            Material[] newMats = new Material[matLength + 1];
            materials.CopyTo(newMats, 0);
            newMats[matLength] = cutSurfaceMaterial;


            renderer.sharedMaterials = newMats;
        }

        // 元のオブジェクトのメッシュを切断面に対して裏側のメッシュに変える
        targetGameObject.GetComponent<MeshFilter>().mesh = originMesh;
        // 切れたオブジェクトが地面に着くと消えるスクリプトがついていなかったら
        if (targetGameObject.GetComponent<DestroyObject>() == null)
        {
            targetGameObject.AddComponent<DestroyObject>();
        }
        
        // 切断面に対して表側のメッシュのオブジェクトを生成
        Transform originTransform = targetGameObject.transform;
        GameObject fragment = Instantiate(targetGameObject, originTransform.position, originTransform.rotation, originTransform.parent);
        fragment.transform.parent = null;
        fragment.GetComponent<MeshFilter>().mesh = fragMesh;
        fragment.GetComponent<MeshRenderer>().sharedMaterials = targetGameObject.GetComponent<MeshRenderer>().sharedMaterials;
        // 切断対象が動いていたらvelocityを揃える
        if (fragment.GetComponent<Rigidbody>())
        {
            fragment.GetComponent<Rigidbody>().velocity = targetGameObject.GetComponent<Rigidbody>().velocity;
        }

        // 切断対象のオブジェクトにコライダーがついているか
        if (targetGameObject.GetComponent<Collider>())
        {
            // 切断対象のオブジェクトにMeshColliderがついているか
            if (targetGameObject.GetComponent<MeshCollider>())
            {
                // 頂点が1点に重なっている場合にはエラーが出るので, 直したい場合はmesh.RecalculateBoundsのあとでmesh.bounds.size.magnitude<0.00001などで条件分けして対処してください
                targetGameObject.GetComponent<MeshCollider>().sharedMesh = originMesh;
                fragment.GetComponent<MeshCollider>().sharedMesh = fragMesh;
            }
            // MeshCollider以外のColliderの場合は
            else
            {
                // 既にあるコライダーを削除する
                Destroy(targetGameObject.GetComponent<Collider>());
                Destroy(fragment.GetComponent<Collider>());
                // MeshColliderをメッシュを入れてアタッチ
                targetGameObject.AddComponent<MeshCollider>().sharedMesh = originMesh;
                fragment.AddComponent<MeshCollider>().sharedMesh = fragMesh;
                // MeshColliderのconvexをTrueにする
                targetGameObject.GetComponent<MeshCollider>().convex = true;
                fragment.GetComponent<MeshCollider>().convex = true;
            }
        }


        return (fragment, targetGameObject);
    }

    /// <summary>
    /// ポリゴンを分離する
    /// ポリゴンは切断面の表側と裏側に分割される.
    /// このとき三角ポリゴンを表面から見て, なおかつ切断面の表側にある頂点が下に来るように見て,
    /// 三角形の辺を形成する頂点は3つなので被りが存在する
    /// ここでポリゴンの向きを決めておくと後々とても便利
    /// 以降左側にあるものは0,右側にあるものは1をつけて扱う
    /// (ひょっとすると実際の向きは逆かもしれないけどvertexIndicesと同じまわり方で出力してるので逆でも問題はない)
    /// </summary>
    /// <param name="sides"></param>
    /// <param name="vertexIndices">三角形の頂点の番号</param>
    /// <param name="submesh">サブメッシュの番号</param>
    private static void Sepalate(bool[] sides, int[] vertexIndices, int submesh)
    {
        #region 頂点のindex番号を格納する
        // 三角形の左側にある辺を作る点
        int frontLeft = 0;
        int backLeft = 0;
        // 三角形の右側にある辺を作る点
        int frontRight = 0;
        int backRight = 0;

        //どちらがに頂点が2つあるか
        bool twoPointsInFrontSide;

        //ポリゴンの向きを揃える
        if (sides[0])
        {
            if (sides[1])
            {
                frontLeft = vertexIndices[1];
                frontRight = vertexIndices[0];
                backLeft = backRight = vertexIndices[2];
                twoPointsInFrontSide = true;
            }
            else
            {
                if (sides[2])
                {
                    frontLeft = vertexIndices[0];
                    frontRight = vertexIndices[2];
                    backLeft = backRight = vertexIndices[1];
                    twoPointsInFrontSide = true;
                }
                else
                {
                    frontLeft = frontRight = vertexIndices[0];
                    backLeft = vertexIndices[1];
                    backRight = vertexIndices[2];
                    twoPointsInFrontSide = false;
                }
            }
        }
        else
        {
            if (sides[1])
            {
                if (sides[2])
                {
                    frontLeft = vertexIndices[2];
                    frontRight = vertexIndices[1];
                    backLeft = backRight = vertexIndices[0];
                    twoPointsInFrontSide = true;
                }
                else
                {
                    frontLeft = frontRight = vertexIndices[1];
                    backLeft = vertexIndices[2];
                    backRight = vertexIndices[0];
                    twoPointsInFrontSide = false;
                }
            }
            else
            {
                frontLeft = frontRight = vertexIndices[2];
                backLeft = vertexIndices[0];
                backRight = vertexIndices[1];
                twoPointsInFrontSide = false;
            }
        }
        #endregion

        #region 切断前のポリゴンの頂点の座標を取得(内2つは重複あり)
        Vector3 frontPoint0 = default;
        Vector3 frontPoint1 = default;
        Vector3 backPoint0 = default;
        Vector3 backPoint1 = default;
        if (twoPointsInFrontSide)
        {
            frontPoint0 = _targetVertices[frontLeft];
            frontPoint1 = _targetVertices[frontRight];
            backPoint0 = backPoint1 = _targetVertices[backLeft];
        }
        else
        {
            frontPoint0 = frontPoint1 = _targetVertices[frontLeft];
            backPoint0 = _targetVertices[backLeft];
            backPoint1 = _targetVertices[backRight];
        }
        #endregion

        // 新しい頂点の座標
        // 分割パラメータをベクトル[backPoint0 - frontPoint0]を何倍したら切断平面に到達するかは以下の式で表される
        // 平面の式: dot(r ,n)=A ,Aは定数,nは法線, 
        // 今回    r =frontPoint0+k*(backPoint0 - frontPoint0), (0 ≦ k ≦ 1)
        // これは, 新しくできる頂点が2つの頂点を何対何に内分してできるのかを意味している
        float dividingParameter0 = (_planeValue - Vector3.Dot(_planeNormal, frontPoint0)) / (Vector3.Dot(_planeNormal, backPoint0 - frontPoint0));
        // Lerpで切断によってうまれる新しい頂点の座標を生成
        Vector3 newVertexPos0 = Vector3.Lerp(frontPoint0, backPoint0, dividingParameter0);

        float dividingParameter1 = (_planeValue - Vector3.Dot(_planeNormal, frontPoint1)) / (Vector3.Dot(_planeNormal, backPoint1 - frontPoint1));
        Vector3 newVertexPos1 = Vector3.Lerp(frontPoint1, backPoint1, dividingParameter1);

        // 新しい頂点の生成, ここではNormalとUVは計算せず後から計算できるように頂点のindex(_trackedArray[frontLeft], _trackedArray[backLeft],)と内分点の情報(dividingParameter0)を持っておく
        NewVertex vertex0 = new NewVertex(_trackedArray[frontLeft], _trackedArray[backLeft], dividingParameter0, newVertexPos0);
        NewVertex vertex1 = new NewVertex(_trackedArray[frontRight], _trackedArray[backRight], dividingParameter1, newVertexPos1);

        //切断でできる辺(これが同じポリゴンは後で結合して頂点数の増加を抑える)
        Vector3 cutLine = (newVertexPos1 - newVertexPos0).normalized;
        int KEY_CUTLINE = MakeIntFromVector3_ErrorCut(cutLine);//Vector3だと処理が重そうなのでintにしておく, ついでに丸め誤差を切り落とす

        //切断情報を含んだFragmentクラスからメソッドを呼び出す
        Fragment fragment = new Fragment(vertex0, vertex1, twoPointsInFrontSide, KEY_CUTLINE, submesh);
        //Listに追加してListの中で同一平面のFragmentは結合とかする
        fragmentList.Add(fragment, KEY_CUTLINE);
    }

    #region クラス

    /// <summary>
    /// 
    /// </summary>
    class RoopFragment
    {
        public RoopFragment next; //右隣のやつ
        public Vector3 rightPosition;//右側の座標(左側の座標は左隣のやつがもってる)
        public RoopFragment(Vector3 _rightPosition)
        {
            next = null;
            rightPosition = _rightPosition;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    class RooP
    {
        // ループの左端
        public RoopFragment start = default;
        // ループの右側
        public RoopFragment end = default;
        // ループの左端のPosition
        public Vector3 startPos = default;
        // ループの右側のPosition
        public Vector3 endPos = default;
        // 含まれる頂点数(ループが閉じるまでは頂点数-1の値になる)
        public int verticesCount;
        // Positionの和.これをcountで割ると図形の中点が得られる
        public Vector3 sum_rightPosition;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_left"></param>
        /// <param name="_right"></param>
        /// <param name="_startPos"></param>
        /// <param name="_endPos"></param>
        /// <param name="rightPos"></param>
        public RooP(RoopFragment _left, RoopFragment _right, Vector3 _startPos, Vector3 _endPos, Vector3 rightPos)
        {
            start = _left;
            end = _right;
            startPos = _startPos;
            endPos = _endPos;
            verticesCount = 1;
            sum_rightPosition = rightPos;
        }
    }
    /// <summary>
    /// 切断面に重なっている頂点に関する処理
    /// </summary>
    public class RoopFragmentCollection
    {
        Dictionary<Vector3, RooP> leftPointDic = new Dictionary<Vector3, RooP>();
        Dictionary<Vector3, RooP> rightPointDic = new Dictionary<Vector3, RooP>();

        /// <summary>
        /// 切断辺を追加していって隣り合う辺がすでにあればくっつける.最終的にループを作る
        /// </summary>
        /// <param name="leftVertexPos">切断辺の左の点のPosition</param>
        /// <param name="rightVertexPos">切断辺の右の点のPosition</param>
        public void Add(Vector3 leftVertexPos, Vector3 rightVertexPos)
        {
            // 新しいループ辺を作る
            RoopFragment target = new RoopFragment(rightVertexPos);


            RooP roop1 = null;
            bool find1 = default;
            // 自分の左手とくっつくのは相手の右手なので右手disctionaryの中に既にleftVertexPosのkeyが入っていないかチェック
            if (find1 = rightPointDic.ContainsKey(leftVertexPos))
            {
                roop1 = rightPointDic[leftVertexPos];

                //roopの右端(終端)の右(next)にtargetをくっつける
                roop1.end.next = target;
                //roopの右端をtargetに変える(roopは左端と右端の情報だけを持っている)
                roop1.end = target;
                roop1.endPos = rightVertexPos;

                //roop1をリストから外す(あとで右手Listの自分の右手indexの場所に移すため)
                rightPointDic.Remove(leftVertexPos);
            }

            RooP roop2 = null;
            bool find2 = default;

            // 自分の右手とくっつくのは相手の左手なので左手disctionaryの中に既にrightVertexPosのkeyが入っていないかチェック
            if (find2 = leftPointDic.ContainsKey(rightVertexPos))
            {
                roop2 = leftPointDic[rightVertexPos];
                // roop1==roop2のとき, roopが完成したのでreturn
                if (roop1 == roop2)
                {
                    roop1.verticesCount++;
                    roop1.sum_rightPosition += rightVertexPos;
                    return;
                }

                // targetの右にroop2の左端(始端)をくっつける
                target.next = roop2.start;
                // roop2の左端をtargetに変更
                roop2.start = target;
                roop2.startPos = leftVertexPos;

                // roop2をリストから外す
                leftPointDic.Remove(rightVertexPos);
            }

             // 
            if (find1)
            {
                // 2つのroopがくっついたとき 
                if (find2)
                {
                    //roop1+target+roop2の順でつながっているはずなのでroop1にroop2の情報を追加する
                    roop1.end = roop2.end;//終端を変更
                    roop1.endPos = roop2.endPos;
                    roop1.verticesCount += roop2.verticesCount + 1;//+1はtargetの分
                    roop1.sum_rightPosition += roop2.sum_rightPosition + rightVertexPos;
                    var key = roop2.endPos;
                    //dictionaryに入れておかないと最後に面を貼れないので適当に突っ込んでおく.
                    rightPointDic[key] = roop1;
                }
                //自分の左手とroopの右手がくっついたとき, 右手dictionaryの自分の右手Positionの場所にroopをついか
                else
                {
                    roop1.verticesCount++;
                    roop1.sum_rightPosition += rightVertexPos;
                    if (leftPointDic.ContainsKey(leftVertexPos))
                    {
                        //既にdictionaryに追加されている場合はreturn (変なポリゴン構成になっているとそうなる)
                        return;
                    }
                    rightPointDic.Add(rightVertexPos, roop1);
                }
            }
            else
            {
                if (find2)
                {
                    roop2.verticesCount++;
                    roop2.sum_rightPosition += rightVertexPos;
                    if (leftPointDic.ContainsKey(leftVertexPos))
                    {
                        //既にdictionaryに追加されている場合はreturn (変なポリゴン構成になっているとそうなる)
                        return;
                    }
                    leftPointDic.Add(leftVertexPos, roop2);
                }
                //どこにもくっつかなかったとき, roopを作成, 追加
                else
                {
                    RooP newRoop = new RooP(target, target, leftVertexPos, rightVertexPos, rightVertexPos);
                    rightPointDic.Add(rightVertexPos, newRoop);
                    leftPointDic.Add(leftVertexPos, newRoop);
                }
            }
        }
        /// <summary>
        /// 切断面をつくる
        /// </summary>
        /// <param name="submesh"></param>
        /// <param name="targetTransform"></param>
        public void MakeCutSurface(int submesh, Transform targetTransform)
        {
            Vector3 scale = targetTransform.localScale;
            // ワールド座標の上方向をオブジェクト座標に変換
            Vector3 world_Up = Vector3.Scale(scale, targetTransform.InverseTransformDirection(Vector3.up)).normalized;//ワールド座標の上方向をオブジェクト座標に変換
            // ワールド座標の右方向をオブジェクト座標に変換
            Vector3 world_Right = Vector3.Scale(scale, targetTransform.InverseTransformDirection(Vector3.right)).normalized;//ワールド座標の右方向をオブジェクト座標に変換


            // オブジェクト空間上でのUVのU軸
            Vector3 uVector = default;
            // オブジェクト空間上でのUVのV軸
            Vector3 vVector = default;
            //U軸は切断面の法線とY軸との外積
            uVector = Vector3.Cross(world_Up, _planeNormal);
            //切断面の法線がZ軸方向のときはuVectorがゼロベクトルになるので場合分け
            uVector = (uVector.sqrMagnitude != 0) ? uVector.normalized : world_Right;

            //V軸はU軸と切断平面のノーマルとの外積
            vVector = Vector3.Cross(_planeNormal, uVector).normalized;
            //v軸の方向をワールド座標上方向に揃える.
            if (Vector3.Dot(vVector, world_Up) < 0)
            {
                vVector *= -1;
            }

            // u軸の各値
            float u_min = default;
            float u_max = default;
            float u_range = default;
            // v軸の各値
            float v_min = default;
            float v_max = default;
            float v_range = default;

            foreach (RooP roop in leftPointDic.Values)
            {
                {
                    u_min = u_max = Vector3.Dot(uVector, roop.startPos);
                    v_min = v_max = Vector3.Dot(vVector, roop.startPos);
                    RoopFragment fragment = roop.start;

                    int count = 0;
                    do
                    {
                        float u_value = Vector3.Dot(uVector, fragment.rightPosition);
                        u_min = Mathf.Min(u_min, u_value);
                        u_max = Mathf.Max(u_max, u_value);

                        float v_value = Vector3.Dot(vVector, fragment.rightPosition);
                        v_min = Mathf.Min(v_min, v_value);
                        v_max = Mathf.Max(v_max, v_value);


                        // 無限ループを回避する処理
                        if (count > 1000)
                        {
                            Debug.LogError("Something is wrong?");
                            break;
                        }
                        count++;

                    }
                    while ((fragment = fragment.next) != null);

                    u_range = u_max - u_min;
                    v_range = v_max - v_min;

                }

                // roopFragmentのnextをたどっていくことでroopを一周できる

                // 切断面の中心に頂点を追加して頂点番号を返す
                MakeVertex(roop.sum_rightPosition / roop.verticesCount, out int center_f, out int center_b);

                RoopFragment nowFragment = roop.start;

                //ループの始端の頂点を追加して頂点番号を返す
                MakeVertex(nowFragment.rightPosition, out int first_f, out int first_b);
                int previous_f = first_f;
                int previous_b = first_b;

                //終端に達するまでループ
                while (nowFragment.next != null)
                {
                    nowFragment = nowFragment.next;

                    //新しく頂点を追加して頂点番号を返す
                    MakeVertex(nowFragment.rightPosition, out int index_f, out int index_b);

                    _frontSubmeshIndices[submesh].Add(center_f);
                    _frontSubmeshIndices[submesh].Add(index_f);
                    _frontSubmeshIndices[submesh].Add(previous_f);

                    _backSubmeshIndices[submesh].Add(center_b);
                    _backSubmeshIndices[submesh].Add(previous_b);
                    _backSubmeshIndices[submesh].Add(index_b);

                    previous_f = index_f;
                    previous_b = index_b;
                }
                _frontSubmeshIndices[submesh].Add(center_f);
                _frontSubmeshIndices[submesh].Add(first_f);
                _frontSubmeshIndices[submesh].Add(previous_f);

                _backSubmeshIndices[submesh].Add(center_b);
                _backSubmeshIndices[submesh].Add(previous_b);
                _backSubmeshIndices[submesh].Add(first_b);
            }

            void MakeVertex(Vector3 vertexPos, out int findex, out int bindex)
            {
                findex = _frontVertices.Count;
                bindex = _backVertices.Count;
                Vector2 uv = default;
                //positionをUVに変換
                {
                    float uValue = Vector3.Dot(uVector, vertexPos);
                    float normalizedU = (uValue - u_min) / u_range;
                    float vValue = Vector3.Dot(vVector, vertexPos);
                    float normalizedV = (vValue - v_min) / v_range;

                    uv = new Vector2(normalizedU, normalizedV);
                }
                _frontVertices.Add(vertexPos);
                _frontNormals.Add(-_planeNormal);
                _frontUVs.Add(uv);

                _backVertices.Add(vertexPos);
                _backNormals.Add(_planeNormal);
                //UVを左右反転する
                _backUVs.Add(new Vector2(1 - uv.x, uv.y));
            }
        }
        /// <summary>
        /// Dictionaryを一括でClearするメソッド
        /// </summary>
        public void Clear()
        {
            leftPointDic.Clear();
            rightPointDic.Clear();
        }
    }
    /// <summary>
    /// 切断面を跨ぐ頂点に関するクラス
    /// </summary>
    public class Fragment
    {
        // 切断面と重なっている新しい頂点
        public NewVertex vertex0 = default;
        public NewVertex vertex1 = default;
        public int KEY_CUTLINE;
        // submesh番号(どのマテリアルを当てるか)
        public int submesh;
        // ポリゴンの4つ(3つ)の頂点の情報
        public Point firstPoint_f = default;
        public Point lastPoint_f = default;
        public Point firstPoint_b = default;
        public Point lastPoint_b = default;
        // front側,back側の頂点数
        public int count_f;
        public int count_b;

        /// <summary>
        /// 切断片の切断情報
        /// </summary>
        /// <param name="_vertex0"></param>
        /// <param name="_vertex1"></param>
        /// <param name="_twoPointsInFrontSide">切断面のどちら側に頂点が2つあるか</param>
        /// <param name="_KEY_CUTLINE"></param>
        /// <param name="_submesh">サブメッシュ</param>
        public Fragment(NewVertex _vertex0, NewVertex _vertex1, bool _twoPointsInFrontSide, int _KEY_CUTLINE, int _submesh)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            KEY_CUTLINE = _KEY_CUTLINE;
            submesh = _submesh;

            // 切断面の表側に頂点が2つある場合
            if (_twoPointsInFrontSide)
            {
                firstPoint_f = new Point(_vertex0.frontsideindex_of_frontMesh);
                lastPoint_f = new Point(_vertex1.frontsideindex_of_frontMesh);
                firstPoint_f.next = lastPoint_f;
                firstPoint_b = new Point(vertex0.backsideindex_of_backMash);
                lastPoint_b = firstPoint_b;
                count_f = 2;
                count_b = 1;
            }
            else
            {
                firstPoint_f = new Point(_vertex0.frontsideindex_of_frontMesh);
                lastPoint_f = firstPoint_f;
                firstPoint_b = new Point(vertex0.backsideindex_of_backMash);
                lastPoint_b = new Point(vertex1.backsideindex_of_backMash);
                firstPoint_b.next = lastPoint_b;
                count_f = 1;
                count_b = 2;
            }
        }
        /// <summary>
        /// サブメッシュの三角形を追加するメソッド
        /// </summary>
        public void AddTriangle()
        {
            //Vertexの中で新しく生成された頂点を登録してその番号だけを返している
            (int frontindex0, int backindex0) = vertex0.GetIndex();
            (int frontindex1, int backindex1) = vertex1.GetIndex();

            Point point = firstPoint_f;
            int preIndex = point.index;

            int count = count_f;
            int halfcount = count_f / 2;
            for (int i = 0; i < halfcount; i++)
            {
                point = point.next;
                int index = point.index;
                _frontSubmeshIndices[submesh].Add(index);
                _frontSubmeshIndices[submesh].Add(preIndex);
                _frontSubmeshIndices[submesh].Add(frontindex0);
                preIndex = index;
            }
            _frontSubmeshIndices[submesh].Add(preIndex);
            _frontSubmeshIndices[submesh].Add(frontindex0);
            _frontSubmeshIndices[submesh].Add(frontindex1);
            int elseCount = count_f - halfcount - 1;
            for (int i = 0; i < elseCount; i++)
            {
                point = point.next;
                int index = point.index;
                _frontSubmeshIndices[submesh].Add(index);
                _frontSubmeshIndices[submesh].Add(preIndex);
                _frontSubmeshIndices[submesh].Add(frontindex1);
                preIndex = index;
            }


            point = firstPoint_b;
            preIndex = point.index;
            count = count_b;
            halfcount = count_b / 2;

            for (int i = 0; i < halfcount; i++)
            {
                point = point.next;
                int index = point.index;
                _backSubmeshIndices[submesh].Add(index);
                _backSubmeshIndices[submesh].Add(backindex0);
                _backSubmeshIndices[submesh].Add(preIndex);
                preIndex = index;
            }
            _backSubmeshIndices[submesh].Add(preIndex);
            _backSubmeshIndices[submesh].Add(backindex1);
            _backSubmeshIndices[submesh].Add(backindex0);
            elseCount = count_b - halfcount - 1;
            for (int i = 0; i < elseCount; i++)
            {
                point = point.next;
                int index = point.index;
                _backSubmeshIndices[submesh].Add(index);
                _backSubmeshIndices[submesh].Add(backindex1);
                _backSubmeshIndices[submesh].Add(preIndex);
                preIndex = index;
            }

            if (_isMakeCutSurface)
            {
                //切断平面を形成する準備
                roopCollection.Add(vertex0.position, vertex1.position);
            }
        }
    }

    /// <summary>
    /// 新しい頂点のNormalとUVは最後に生成するので, 
    /// もともとある頂点をどの比で混ぜるかをdividingParameterが持っている
    /// </summary>
    public class NewVertex
    {
        // frontVertices,frontNormals,frontUVsでの頂点の番号
        // (frontsideindex_of_frontMeshとbacksideindex_of_backMashでできる辺の間に新しい頂点ができる)
        public int frontsideindex_of_frontMesh;
        public int backsideindex_of_backMash;
        // 新しい頂点の(frontsideindex_of_frontMeshとbacksideindex_of_backMashでできる辺に対する)内分点
        public float dividingParameter;
        public int KEY_VERTEX;
        public Vector3 position;

        /// <summary>
        /// 新しい頂点を設定
        /// </summary>
        /// <param name="front"></param>
        /// <param name="back"></param>
        /// <param name="parameter"></param>
        /// <param name="vertexPosition"></param>
        public NewVertex(int front, int back, float parameter, Vector3 vertexPosition)
        {
            frontsideindex_of_frontMesh = front;
            backsideindex_of_backMash = back;
            KEY_VERTEX = (front << 16) | back;
            dividingParameter = parameter;
            position = vertexPosition;
        }

        /// <summary>
        /// 表裏の頂点情報を取得
        /// </summary>
        /// <returns></returns>
        public (int frontindex, int backindex) GetIndex()
        {
            //法線とUVの情報はここで生成する
            Vector3 frontNormal, backNormal;
            Vector2 frontUV, backUV;

            frontNormal = _frontNormals[frontsideindex_of_frontMesh];
            frontUV = _frontUVs[frontsideindex_of_frontMesh];

            backNormal = _backNormals[backsideindex_of_backMash];
            backUV = _backUVs[backsideindex_of_backMash];



            Vector3 newNormal = Vector3.Lerp(frontNormal, backNormal, dividingParameter);
            Vector2 newUV = Vector2.Lerp(frontUV, backUV, dividingParameter);

            int findex, bindex;
            (int, int) trackNumPair;
            //同じ2つの点の間に生成される頂点は1つにまとめたいのでDictionaryを使う
            if (newVertexDic.TryGetValue(KEY_VERTEX, out trackNumPair))
            {
                //新しい頂点が表側のMeshで何番目か
                findex = trackNumPair.Item1;
                bindex = trackNumPair.Item2;
            }
            else
            {

                findex = _frontVertices.Count;
                _frontVertices.Add(position);
                _frontNormals.Add(newNormal);
                _frontUVs.Add(newUV);

                bindex = _backVertices.Count;
                _backVertices.Add(position);
                _backNormals.Add(newNormal);
                _backUVs.Add(newUV);

                newVertexDic.Add(KEY_VERTEX, (findex, bindex));
            }
            return (findex, bindex);
        }
    }
    /// <summary>
    /// Fragmentクラスで使われる頂点情報のポインタ
    /// </summary>
    public class Point
    {
        public Point next;
        public int index;
        public Point(int _index)
        {
            index = _index;
            next = null;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public class FragmentList
    {

        // 同じ切断辺をもったFragmentをリストにまとめる
        Dictionary<int, List<Fragment>> cutLineDictionary = new Dictionary<int, List<Fragment>>();

        /// <summary>
        /// 切断片を追加して隣り合う切断片はくっつけてしまう
        /// </summary>
        /// <param name="fragment">追加する切断片</param>
        /// <param name="KEY_CUTLINE">切断された辺の向きをintに変換したもの</param>
        public void Add(Fragment fragment, int KEY_CUTLINE)
        {
            List<Fragment> flist;

            //各切断辺の1つめのポリゴンについては新しくkey-valueを追加
            if (!cutLineDictionary.TryGetValue(KEY_CUTLINE, out flist))
            {
                flist = new List<Fragment>();
                cutLineDictionary.Add(KEY_CUTLINE, flist);
            }


            bool connect = false;
            //格納されているFragmentからくっつけられるやつを探す
            for (int i = flist.Count - 1; i >= 0; i--)
            {
                Fragment compareFragment = flist[i];
                //同じ切断辺をもつか判断
                if (fragment.KEY_CUTLINE == compareFragment.KEY_CUTLINE)
                {
                    Fragment left, right;
                    //fragmentがcompareFragmentに右側からくっつく場合
                    if (fragment.vertex0.KEY_VERTEX == compareFragment.vertex1.KEY_VERTEX)
                    {
                        right = fragment;
                        left = compareFragment;
                    }
                    //左側からくっつく場合
                    else if (fragment.vertex1.KEY_VERTEX == compareFragment.vertex0.KEY_VERTEX)
                    {
                        left = fragment;
                        right = compareFragment;
                    }
                    else
                    {
                        //どっちでもないときは次のループへ
                        continue;
                    }

                    //Pointクラスのつなぎ合わせ. 
                    //firstPoint.nextがnullということは頂点を1つしか持っていない. 
                    //またその頂点はleftのlastPointとかぶっているので頂点が増えることはない
                    //(left.lastPoint_fとright.lastPoint_fは同じ点を示すが別のインスタンスなのでnextがnullのときに入れ替えるとループが途切れてしまう)
                    if ((left.lastPoint_f.next = right.firstPoint_f.next) != null)
                    {
                        left.lastPoint_f = right.lastPoint_f;
                        left.count_f += right.count_f - 1;
                    }
                    if ((left.lastPoint_b.next = right.firstPoint_b.next) != null)
                    {
                        left.lastPoint_b = right.lastPoint_b;
                        left.count_b += right.count_b - 1;
                    }

                    //結合を行う
                    //Fragmentがより広くなるように頂点情報を変える
                    left.vertex1 = right.vertex1;
                    right.vertex0 = left.vertex0;

                    //connectがtrueになっているということは2つのFragmentのあいだに新しいやつがはまって3つが1つになったということ
                    //connect==trueのとき, rightもleftもListにすでに登録されてるやつなのでどっちかを消してやる
                    if (connect)
                    {
                        flist.Remove(right);

                        break;
                    }

                    flist[i] = left;
                    fragment = left;
                    connect = true;
                }
            }

            if (!connect)
            {
                flist.Add(fragment);
            }
        }
        /// <summary>
        /// 切断されたポリゴンをそれぞれのMeshに追加
        /// </summary>
        public void MakeTriangle()
        {
            int sum = 0;
            foreach (List<Fragment> list in cutLineDictionary.Values)
            {
                foreach (Fragment f in list)
                {
                    f.AddTriangle();
                    sum++;
                }
            }
        }
        /// <summary>
        /// MeshCutクラスで連想配列を
        /// </summary>
        public void Clear()
        {
            cutLineDictionary.Clear();
        }
    }

    const int filter = 0x000003FF;
    //丸め誤差を落とすためにやや低めの倍率がかかっている
    const int amp = 1 << 10;
    /// <summary>
    /// Vector3から丸め誤差を落としてintに変換
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static int MakeIntFromVector3_ErrorCut(Vector3 vec)
    {
        int cutLineX = ((int)(vec.x * amp) & filter) << 20;
        int cutLineY = ((int)(vec.y * amp) & filter) << 10;
        int cutLineZ = ((int)(vec.z * amp) & filter);

        return cutLineX | cutLineY | cutLineZ;
    }
    #endregion
}
