using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの名前空間
/// </summary>
namespace Player
{
    /// <summary>
    /// カメラからメッシュ切断をするRayを飛ばすクラス
    /// </summary>
    public class MeshCutController : MonoBehaviour
    {
        #region 変数宣言
        [SerializeField, Header("切断面を埋める")]
        private bool _isCutSurfaceFill = false;
        [SerializeField, Header("切断面のマテリアル(nullならオブジェクトのマテリアルから埋める)")]
        private Material _CutSurfaceMaterial = default;
        //カットするオブジェクト
        private GameObject _tergetObject = default;
        // Rayが当たった座標の配列
        private List<Vector3> _hitPositions = new List<Vector3>();
        // RayのHit情報
        private RaycastHit _hit;
        // 切断Rayの長さ
        private float _cutRayDistance = 20f;
        // 切断モードの制限時間
        private float _canCutTimer = 5f;
        // Rayがオブジェクトを切断中か
        private bool _isCutting = false;
        // 切断中のスロー状態
        private bool _isSlow = false;
        // カーソルロック
        private bool _isCursorLock = true;
        #endregion

        // 定数宣言---------------------------------
        // リストの最後尾を調整する
        const int _LIST_END = -1;
        // 切断できるLayerMask
        const int _CUT_LAYER_MASK = 1 << 8;
        [SerializeField, Range(1, 10), Header("@切断したオブジェクト同士の間隔をあける力")]
        private int _CUT_DIVISION_FORCE = 3;
        // centerPosを取るためのRayの長さ
        const float _CENTER_RAY_RANGE = 20f;
        // 切断時のスコア
        const int _CUT_SCORE = 100;


        void Update()
        {
            // ゲームの状態がゲーム中以外なら処理しない
            if(GameManager.instance.game_State != GameManager.GameState.GameNow)
            {
                return;
            }

            // 左クリックが押されている
            if (Input.GetMouseButton(0))
            {
                // カーソルロックをしていたら
                if (_isCursorLock)
                {
                    //  カーソルを表示
                    Cursor.lockState = CursorLockMode.None;
                    _isCursorLock = false;
                }

                CheckRayHit();

                // スロー状態じゃなかったら
                if (!_isSlow)
                {
                    Time.timeScale = 0.5f;
                    _isSlow = true;
                }
            }
            else
            {
                // カメラの操作メソッドを呼び出す
                GetComponent<PlayerCameraController>().CameraControll();
                // カーソルロックが解除されていたら
                if (!_isCursorLock)
                {
                    // カーソルロックをする
                    Cursor.lockState = CursorLockMode.Locked;
                    _isCursorLock = true;
                }

                // スロー状態だったら
                if (_isSlow)
                {
                    Time.timeScale = 1f;
                    _isSlow = false;
                }
            }
        }

        /// <summary>
        /// 切断Rayがオブジェクトに当たって、切断するメソッド
        /// </summary>
        private void CheckRayHit()
        {          
            // 切断面を生成するRay
            Ray bladeRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(bladeRay.origin, bladeRay.direction * _cutRayDistance, Color.green);

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
                // 切断面を計算する
                (Vector3 center, Vector3 normal) = CalculationCutSurface(_hitPositions[0], _hitPositions[_hitPositions.Count + _LIST_END]);
                //_meshCut.Cut(_tergetObject, center, normal, _material);
                (GameObject backObject, GameObject frontObject) = NewMeshCut.CutGameObject(_tergetObject, center, normal, _isCutSurfaceFill, _CutSurfaceMaterial);

                // 切断対象にrigidbodyが付いていたら
                if (frontObject && _tergetObject.GetComponent<Rigidbody>())
                {
                    // スコアを加算
                    GameManager.instance._nowScore = _CUT_SCORE;

                    // 切断したオブジェクトの間隔を開ける
                    frontObject.GetComponent<Rigidbody>().AddForce(-normal * _CUT_DIVISION_FORCE, ForceMode.Impulse);
                    backObject.GetComponent<Rigidbody>().AddForce(normal * _CUT_DIVISION_FORCE, ForceMode.Impulse);
                }

                //リストの初期化
                _hitPositions.Clear();
                _isCutting = false;
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
            if (Physics.Raycast(centerRay, out _hit, _CENTER_RAY_RANGE))
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
}