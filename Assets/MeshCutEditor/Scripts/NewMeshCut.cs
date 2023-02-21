using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewMeshCut : MonoBehaviour
{
    // 切断対象のメッシュ
    static Mesh _targetMesh = default;

    // 参照渡し防止のため配列であらかじめ宣言
    // 切断対象のメッシュの頂点(ポリゴン)
    static Vector3[] _targetVertices = default;
    // 切断対象のメッシュの法線
    static Vector3[] _targetNormals = default;
    // 切断対象のメッシュのUV
    static Vector2[] _targetUVs = default;

    //平面の方程式はn・r=h(nは法線,rは位置ベクトル,hはconst(=_planeValue))
    static Vector3 _planeNormal = default;  //nの部分
    static float _planeValue = default;  //rの部分

    // 頂点が切断面に対して表にあるか裏にあるか
    static bool[] _isFront = default;
    // 切断後のメッシュでの切断前の頂点の番号
    static int[] _trackedArray = default;

    // 切断面を生成済みか
    static bool _makeCutSurface = default;

    // 頂点の向きを管理する連想配列
    static Dictionary<int, (int, int)> newVertexDirection = new Dictionary<int, (int, int)>(101);

    // @
    static FragmentList fragmentList = new FragmentList();

    //@
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
    public static (Mesh frontside, Mesh backside) CutMesh(Mesh targetMesh, Transform targetTransform, Vector3 planeAnchorPoint, Vector3 planeNormalDirection, bool makeCutSurface = true, bool addNewMeshIndices = false)
    {
        // 切断面の法線ベクトルがゼロだったら
        if (planeNormalDirection == Vector3.zero)
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
                if (_isFront[i] == (planeNormal_x * (targetVerticesPos.x - anchor_x) +
                                   planeNormal_y * (targetVerticesPos.y - anchor_y) +
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
            if (frontCont == 0 || backCont == 0)
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

            // ポリゴンを形成する頂点の番号を入れるintの配列(submeshごとに追加)
            List<int> frontIndices = new List<int>();
            _frontSubmeshIndices.Add(frontIndices);
            List<int> backIndices = new List<int>();
            _backSubmeshIndices.Add(backIndices);

            // ポリゴンの情報は頂点3つで1セットなので3つ飛ばしでループ
            for (int i = 0; i < indices.Length; i += 3)
            {
                int peak1;
                int peak2;
                int peak3;
                peak1 = indices[i];
                peak2 = indices[i + 1];
                peak3 = indices[i + 2];

                // 予め計算しておいた結果を持ってくる(ここで計算すると同じ頂点にたいして何回も同じ計算をすることになるから)
                // 三角形の頂点が切断面の表裏どちら側にあるか
                bool side1 = _isFront[peak1];
                bool side2 = _isFront[peak2];
                bool side3 = _isFront[peak3];

                // 3つとも同じ側にある時はそのまま
                if (side1 && side2 && side3)
                {
                    // indicesは切断前のMeshの頂点番号が入っているので_trackedArryを経由することで切断後のMeshでの番号に変えている
                    frontIndices.Add(_trackedArray[peak1]);
                    frontIndices.Add(_trackedArray[peak2]);
                    frontIndices.Add(_trackedArray[peak3]);
                }
                else if (!side1 && !side2 && !side3)
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
            roopCollection.MakeCutSurface(_frontSubmeshIndices.Count - 1, targetTransform);
        }
        #endregion

        #region 2つのMeshを新規に作ってそれぞれに情報を追加して出力
        Mesh frontMesh = new Mesh();
        frontMesh.name = "Split Mesh front";

        frontMesh.vertices = _frontVertices.ToArray();
        frontMesh.normals = _frontNormals.ToArray();
        frontMesh.uv = _frontUVs.ToArray();

        frontMesh.subMeshCount = _frontSubmeshIndices.Count;
        for (int i = 0; i < _frontSubmeshIndices.Count; i++)
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
        for (int i = 0; i < _backSubmeshIndices.Count; i++)
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
    public static (GameObject copy_normaliside, GameObject original_anitiNormalside) CutObject(GameObject targetGameObject, Vector3 planeAnchorPoint, Vector3 planeNormalDirection, bool makeCutSurface, Material cutSurfaceMaterial = null)
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
        if (makeCutSurface && cutSurfaceMaterial != null)
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

        if (originMesh == null || fragMeah == null)
        {
            return (null, null);
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

        if (targetGameObject.GetComponent<MeshCollider>())
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
    /// <param name="vertexIndices">三角形の頂点の番号</param>
    /// <param name="submesh">サブメッシュの番号</param>
    private static void Sepalate(bool[] sides, int[] vertexIndices, int submesh)
    {
        // 頂点のindex番号を格納するのに使用
        // 三角形の左側にある辺を作る点
        int frontLeft = 0;
        int backLeft = 0;
        // 三角形の右側にある辺を作る点
        int frontRight = 0;
        int backRight = 0;

        // 切断面のどちら側に頂点が2つあるか
        bool twoPointsInFrontSide = default;

        // ポリゴンの向きを揃える
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

        // 切断前のポリゴンの頂点の座標を取得(内2つは重複あり)
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

        #region 新しい頂点の座標
        // 分割パラメータをベクトル[backPoint0 - frontPoint0]を何倍したら切断平面に到達するかで求める
        // 平面の式: dot(r,n)=A ,Aは定数,nは法線, 
        // 今回    r =frontPoint0+k*(backPoint0 - frontPoint0), (0 ≦ k ≦ 1)
        // これは, 新しくできる頂点が2つの頂点を何対何に内分してできるのかを意味している
        float dividingParameter0 = (_planeValue - Vector3.Dot(_planeNormal, frontPoint0)) / (Vector3.Dot(_planeNormal, backPoint0 - frontPoint0));
        // Lerpで切断によってうまれる新しい頂点の座標を生成
        Vector3 newVertexPos0 = Vector3.Lerp(frontPoint0, backPoint0, dividingParameter0);

        float dividingParameter1 = (_planeValue - Vector3.Dot(_planeNormal, frontPoint1)) / (Vector3.Dot(_planeNormal, backPoint1 - frontPoint1));
        Vector3 newVertexPos1 = Vector3.Lerp(frontPoint1, backPoint1, dividingParameter1);
        #endregion

        // 新しい頂点の生成, ここではNormalとUVは計算せず後から計算できるように
        // 頂点のindex(_trackedArray[f0], _trackedArray[b0],)と内分点の情報(dividingParameter0)を持っておく
        NewVertex vertex0 = new NewVertex(_trackedArray[frontLeft], _trackedArray[backLeft], dividingParameter0, newVertexPos0);
        NewVertex vertex1 = new NewVertex(_trackedArray[frontRight], _trackedArray[backRight], dividingParameter1, newVertexPos1);

        // 切断できる辺(これが同じポリゴンは後で結合して頂点数を抑える)
        Vector3 cutLine = (newVertexPos1 - newVertexPos0).normalized;
        // Vector3だと処理が重そうなのでintにしておく, ついでに丸め誤差を切り落とす
        int KEY_CUTLINE = MakeIntFromVector3_ErrorCut(cutLine);

        // 切断情報を含んだFragmentクラス
        Fragment fragment = new Fragment(vertex0, vertex1, twoPointsInFrontSide, KEY_CUTLINE, submesh);
        // Listに追加してListの中で同一平面のFragmentは結合する
        fragmentList.Add(fragment, KEY_CUTLINE);



    }

    #region クラス
    /// <summary>
    /// 
    /// </summary>
    class RoopFragment
    {
        // 
        public RoopFragment next = default;
        // 
        public Vector3 rightPosition = default;
        public RoopFragment(Vector3 _rightPosition)
        {
            next = null;
            rightPosition = _rightPosition;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    class Roop
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
        public int verticesCount = default;
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
        public Roop(RoopFragment _left, RoopFragment _right, Vector3 _startPos, Vector3 _endPos, Vector3 rightPos)
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
    /// 
    /// </summary>
    public class RoopFragmentCollection
    {
        Dictionary<Vector3, Roop> leftPointDictionary = new Dictionary<Vector3, Roop>();
        Dictionary<Vector3, Roop> rightPointDictionary = new Dictionary<Vector3, Roop>();

        /// <summary>
        /// 切断片を追加していって隣り合う辺がすでにあればくっつける.最終的にループを作る
        /// </summary>
        /// <param name="leftVertecPos">切断辺の左の点のPosition</param>
        /// <param name="rightVertexPos">切断辺の右の点のPosition</param>
        public void Add(Vector3 leftVertexPos, Vector3 rightVertexPos)
        {
            // 新しくループ辺を作る
            RoopFragment target = new RoopFragment(rightVertexPos);

            Roop roop1 = null;
            bool find1 = default;
            // 自分の左手とくっつくのは相手の右手なので右手disctionaryの中に既にleftVertexPosのkeyが入っていないかチェック
            if (find1 = rightPointDictionary.ContainsKey(leftVertexPos))
            {
                roop1 = rightPointDictionary[leftVertexPos];

                // roopの右端(終端)の右(next)にtargetをくっつける
                roop1.end.next = target;
                // roopの右端をtargetに変える(roopは左端と右端の情報だけを持っている)
                roop1.end = target;
                roop1.endPos = rightVertexPos;

                // roop1をリストから外す(あとで右手Listの自分の右手indexの場所に移すため)
                rightPointDictionary.Remove(leftVertexPos);
            }

            Roop roop2 = null;
            bool find2 = default;
            // 自分の右手とくっつくのは相手の左手なので左手disctionaryの中に既にleftVertexPosのkeyが入っていないかチェック
            if (find2 = leftPointDictionary.ContainsKey(rightVertexPos))
            {
                roop2 = leftPointDictionary[rightVertexPos];

                if (roop1 == roop2)
                {
                    roop1.verticesCount++;
                    roop1.sum_rightPosition += rightVertexPos;
                    // roop1==roop2のとき, roopが完成したのでreturn
                    return;
                }
                // targetの右にroop2の左端(始端)をくっつける
                target.next = roop2.start;
                // roop2の左端をtargetに変更
                roop2.start = target;
                roop2.startPos = leftVertexPos;

                // roop2をリストから外す
                leftPointDictionary.Remove(rightVertexPos);
            }

            if (find1)
            {
                // 2つのroopがくっついたとき
                if (find2)
                {
                    // roop1 + target + roop2の順でつながっているのでroop1にroop2の情報を追加する
                    // 終端を変更
                    roop1.end = roop2.end;
                    roop1.endPos = roop2.endPos;
                    // targetの分+1する
                    roop1.verticesCount += roop2.verticesCount + 1;
                    roop1.sum_rightPosition += roop2.sum_rightPosition + rightVertexPos;
                    Vector3 key = roop2.endPos;
                    // dictionaryに入れておかないと最後に面を貼れない
                    rightPointDictionary[key] = roop1;
                }
                // 自分の左手とroopの右手がくっついたとき, 右手dictionaryの自分の右手Positionの場所にroopをついか
                else
                {
                    roop1.verticesCount++;
                    roop1.sum_rightPosition += rightVertexPos;
                    if (leftPointDictionary.ContainsKey(leftVertexPos))
                    {
                        // 既にdictionaryに追加されている場合はreturn (変なポリゴン構成になっているとそうなる)
                        return;
                    }
                    rightPointDictionary.Add(rightVertexPos, roop1);
                }
            }
            else
            {
                if (find2)
                {
                    roop2.verticesCount++;
                    roop2.sum_rightPosition += rightVertexPos;
                    if (leftPointDictionary.ContainsKey(leftVertexPos))
                    {
                        // 既にdictionaryに追加されている場合はreturn (変なポリゴン構成になっているとそうなる)
                        return;
                    }
                    rightPointDictionary.Add(rightVertexPos, roop1);
                }
                // どこにもくっつかなかったとき、roopを作成、追加
                else
                {
                    Roop newRoop = new Roop(target, target, leftVertexPos, rightVertexPos, rightVertexPos);
                    rightPointDictionary.Add(rightVertexPos, newRoop);
                    leftPointDictionary.Add(leftVertexPos, newRoop);
                }
            }
        }
        /// <summary>
        /// 切断面を生成するクラス
        /// </summary>
        /// <param name="submesh"></param>
        /// <param name="targetTransform"></param>
        public void MakeCutSurface(int submesh, Transform targetTransform)
        {
            Vector3 scale = targetTransform.localScale;
            // ワールド座標の上方向をオブジェクト座標に変換
            Vector3 world_Up = Vector3.Scale(scale, targetTransform.InverseTransformDirection(Vector3.up)).normalized;
            // ワールド座標の右方向をオブジェクト座標に変換
            Vector3 world_Right = Vector3.Scale(scale, targetTransform.InverseTransformDirection(Vector3.right)).normalized;

            // オブジェクト空間上でのUVのU軸
            Vector3 uVector = default;
            // オブジェクト空間上でのUVのV軸
            Vector3 vVector = default;
            // u軸は切断面の法線とY軸との外積
            uVector = Vector3.Cross(world_Up, _planeNormal);
            // 切断面の法線がZ軸方向のときはuVectorがゼロベクトルになるので場合分け
            uVector = (uVector.sqrMagnitude != 0) ? uVector.normalized : world_Right;

            //vV軸はu軸と切断平面のノーマルとの外積
            vVector = Vector3.Cross(_planeNormal, uVector).normalized;
            // v軸の方向をワールド座標上方向に揃える
            if (Vector3.Dot(vVector, world_Up) < 0)
            {
                vVector *= -1;
            }

            // 
            float u_min = default;
            float u_max = default;
            float u_range = default;

            float v_min = default;
            float v_max = default;
            float v_range = default;

            foreach (Roop roop in leftPointDictionary.Values)
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

                    // 無限ループを回避する
                    if (count > 1000)
                    {
                        Debug.LogError("Something is wrong?");
                        break;
                    }
                    count++;
                }
                while ((fragment = fragment.next) != null);

                u_range = u_max = u_min;
                v_range = v_max = v_min;

                // roopFragmentのnextをたどっていくことでroopを一周する

                // 切断面の中心に頂点を追加して頂点番号を返す
                MakeVertex(roop.sum_rightPosition / roop.verticesCount, out int center_f, out int center_b);

                RoopFragment nowFragment = roop.start;

                // ループの始端の頂点を追加して頂点番号を返す
                MakeVertex(nowFragment.rightPosition, out int first_f, out int first_b);
                int previous_f = first_f;
                int previous_b = first_b;

                // 終端に達するまでループ
                while (nowFragment != null)
                {
                    nowFragment = nowFragment.next;

                    // 新しく頂点を追加して頂点番号を返す
                    MakeVertex(nowFragment.rightPosition, out int index_f, out int index_b);

                    _frontSubmeshIndices[submesh].Add(center_f);
                    _frontSubmeshIndices[submesh].Add(index_f);
                    _frontSubmeshIndices[submesh].Add(previous_f);

                    _frontSubmeshIndices[submesh].Add(center_b);
                    _frontSubmeshIndices[submesh].Add(index_b);
                    _frontSubmeshIndices[submesh].Add(previous_b);

                    previous_f = index_f;
                    previous_b = index_b;
                }
                _frontSubmeshIndices[submesh].Add(center_f);
                _frontSubmeshIndices[submesh].Add(first_f);
                _frontSubmeshIndices[submesh].Add(previous_f);

                _frontSubmeshIndices[submesh].Add(center_b);
                _frontSubmeshIndices[submesh].Add(first_b);
                _frontSubmeshIndices[submesh].Add(previous_b);
            }

            void MakeVertex(Vector3 vertexPos, out int findex, out int bindex)
            {
                findex = _frontVertices.Count;
                bindex = _backVertices.Count;
                Vector2 uv = default;
                // positionをUVに変換
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
                _backNormals.Add(-_planeNormal);
                // UVを左右反転する
                _backUVs.Add(new Vector2(1 - uv.x, uv.y));
            }
        }
        /// <summary>
        /// Dictionaryを一括でClearするメソッド
        /// </summary>
        public void Clear()
        {
            leftPointDictionary.Clear();
            rightPointDictionary.Clear();
        }
    }

    /// <summary>
    /// 切断片に関するクラス
    /// </summary>
    public class Fragment
    {
        public NewVertex vertex0 = default;
        public NewVertex vertex1 = default;
        public int KEY_CUTLINE = default;
        // submesh番号(どのマテリアルを当てるか)
        public int submesh = default;
        // ポリゴンの4つ(3つ)の頂点の情報
        public Point firstPoint_f = default;
        public Point lastPoint_f = default;
        public Point firstPoint_b = default;
        public Point lastPoint_b = default;
        // front側,back側の頂点数
        public int count_f;
        public int count_b;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_vertex0"></param>
        /// <param name="_vertex1"></param>
        /// <param name="_twoPointsInFrontSide">切断面のどちら側に頂点が2つあるか</param>
        /// <param name="_KEY_CUTLINE"></param>
        /// <param name="_submesh"></param>
        public Fragment(NewVertex _vertex0, NewVertex _vertex1, bool _twoPointsInFrontSide, int _KEY_CUTLINE, int _submesh)
        {
            vertex0 = _vertex0;
            vertex1 = _vertex1;
            KEY_CUTLINE = _KEY_CUTLINE;
            submesh = _submesh;

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
            // Vertexの中で新しく生成された頂点を登録してその番号だけを返している
            (int findex0, int bindex0) = vertex0.GetIndex();
            (int findex1, int bindex1) = vertex1.GetIndex();

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
                _frontSubmeshIndices[submesh].Add(findex0);
                preIndex = index;
            }
            _frontSubmeshIndices[submesh].Add(preIndex);
            _frontSubmeshIndices[submesh].Add(findex0);
            _frontSubmeshIndices[submesh].Add(findex1);
            int elseCount = count_f - halfcount - 1;
            for (int i = 0; i < elseCount; i++)
            {
                point = point.next;
                int index = point.index;
                _frontSubmeshIndices[submesh].Add(index);
                _frontSubmeshIndices[submesh].Add(preIndex);
                _frontSubmeshIndices[submesh].Add(findex1);
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
                _backSubmeshIndices[submesh].Add(bindex0);
                _backSubmeshIndices[submesh].Add(preIndex);
                preIndex = index;
            }
            _backSubmeshIndices[submesh].Add(preIndex);
            _backSubmeshIndices[submesh].Add(bindex1);
            _backSubmeshIndices[submesh].Add(bindex0);
            elseCount = count_b - halfcount - 1;
            for (int i = 0; i < elseCount; i++)
            {
                point = point.next;
                int index = point.index;
                _backSubmeshIndices[submesh].Add(index);
                _backSubmeshIndices[submesh].Add(bindex1);
                _backSubmeshIndices[submesh].Add(preIndex);
                preIndex = index;
            }

            if (_makeCutSurface)
            {
                // 切断平面を形成する準備
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
        public int frontsideindex_of_frontMesh = default;
        public int backsideindex_of_backMash = default;
        // 新しい頂点(frontsideindex_of_frontMeshとbacksideindex_of_backMashでできる辺に対する点)の内分点
        public float dividingParameter = default;
        public int KEY_VERTEX = default;
        public Vector3 position = default;

        /// <summary>
        /// 
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

        public (int frontindex, int backindex) GetIndex()
        {
            // 法線とUVの情報を生成する
            Vector3 frontNormal = default;
            Vector3 backNormal = default;
            Vector2 frontUV = default;
            Vector2 backUV = default;

            frontNormal = _frontNormals[frontsideindex_of_frontMesh];
            frontUV = _frontUVs[frontsideindex_of_frontMesh];

            backNormal = _backNormals[backsideindex_of_backMash];
            backUV = _backUVs[backsideindex_of_backMash];

            Vector3 newNormal = Vector3.Lerp(frontNormal, backNormal, dividingParameter);
            Vector2 newUV = Vector2.Lerp(frontUV, backUV, dividingParameter);

            int frontindex = default;
            int backindex = default;
            (int, int) trackNumPair;
            // 同じ2つの点の間に生成される頂点は1つにまとめたいのでDictionaryを使う
            if (newVertexDirection.TryGetValue(KEY_VERTEX, out trackNumPair))
            {
                // 新しい頂点が表側のMeshで何番目か
                frontindex = trackNumPair.Item1;
                backindex = trackNumPair.Item2;
            }
            else
            {
                frontindex = _frontVertices.Count;
                _frontVertices.Add(position);
                _frontNormals.Add(newNormal);
                _frontUVs.Add(newUV);

                backindex = _backVertices.Count;
                _backVertices.Add(position);
                _backNormals.Add(newNormal);
                _backUVs.Add(newUV);

                newVertexDirection.Add(KEY_VERTEX, (frontindex, backindex));
            }
            return (frontindex, backindex);
        }
    }
    /// <summary>
    /// 
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
            List<Fragment> fragmentList;
            // 各切断片の1つめのポリゴンについては新しくkey-valueを追加
            if (!cutLineDictionary.TryGetValue(KEY_CUTLINE, out fragmentList))
            {
                fragmentList = new List<Fragment>();
                cutLineDictionary.Add(KEY_CUTLINE, fragmentList);
            }

            // 
            bool connect = false;
            // 格納されているFragmentからくっつけられるやつを探す
            for (int i = fragmentList.Count - 1; i >= 0; i--)
            {
                Fragment compareFragment = fragmentList[i];
                // 同じ切断辺をもつか判断
                if (fragment.KEY_CUTLINE == compareFragment.KEY_CUTLINE)
                {
                    Fragment left = default;
                    Fragment right = default;
                    // fragmentがcompareFragmentに右側からくっつく場合
                    if (fragment.vertex0.KEY_VERTEX == compareFragment.vertex1.KEY_VERTEX)
                    {
                        right = fragment;
                        left = compareFragment;
                    }
                    // 左側からくっつく場合
                    else if (fragment.vertex1.KEY_VERTEX == compareFragment.vertex0.KEY_VERTEX)
                    {
                        left = fragment;
                        right = compareFragment;
                    }
                    else
                    {
                        // どっちでもないときは次のループへ
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
                        fragmentList.Remove(right);

                        break;
                    }

                    fragmentList[i] = left;
                    fragment = left;
                    connect = true;
                }
            }

            if (!connect)
            {
                fragmentList.Add(fragment);
            }
        }
        /// <summary>
        /// 
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
        /// 
        /// </summary>
        public void Clear()
        {
            cutLineDictionary.Clear();
        }
    }

    const int FILTER = 0x000003FF;

    // 丸め誤差を落とすためにやや低めの倍率がかかっている
    const int AMP = 1 << 10;

    public static int MakeIntFromVector3_ErrorCut(Vector3 vector)
    {
        int cutLineX = ((int)(vector.x * AMP) & FILTER) << 20;
        int cutLineY = ((int)(vector.y * AMP) & FILTER) << 10;
        int cutLineZ = ((int)(vector.z * AMP) & FILTER);

        return cutLineX | cutLineY | cutLineZ;
    }
    #endregion
}
