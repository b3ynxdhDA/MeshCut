using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 切断オブジェクトをランダムに生成しプレイヤーに向けて飛ばす
/// </summary>
public class Generater : MonoBehaviour
{
    // 変数宣言----------------------------------
    [SerializeField, Header("プレイヤーの位置")]
    private Transform _playerTransform;
    // 生成するオブジェクトのプレハブ
    private GameObject[] _generatedObjectPrefab;
    // オブジェクトを生成する位置のゲームオブジェクトのリスト
    private List<GameObject> _generatPoint = new List<GameObject>();
    // 投擲インターバル
    float _throwInterval = default;

    // 定数宣言------------------------------------
    // 発射角度の最小
    const float _ANGLE_MIN = 30;
    // 発射角度の最大
    const float _ANGLE_MAX = 61;
    // 投擲間隔の最小
    const int _THROW_INTERVAL_MIN = 1;
    // 投擲間隔の最大
    const int _THROW_INTERVAL_MAX = 4;
    // 投擲間隔の最小
    const float _OBJECT_QUATERNION_MIN = -180f;
    // 投擲間隔の最大
    const float _OBJECT_QUATERNION_MAX = 181f;

    void Start()
    {
        // Generaterの子オブジェクトをListに格納
        foreach(Transform childObject in this.gameObject.transform)
        {
            _generatPoint.Add(childObject.gameObject);
        }

        // CutTargetフォルダからプレハブを取得
        _generatedObjectPrefab = Resources.LoadAll<GameObject>("CutTarget");

        // 投擲の間隔をランダムに設定する
        _throwInterval = Random.Range(_THROW_INTERVAL_MIN, _THROW_INTERVAL_MAX);

    }

    void Update()
    {
        // ゲームの状態がゲーム中以外なら処理しない
        if (GameManager.instance.GameStateProperty != GameManager.GameState.GameNow)
        {
            return;
        }

        // インターバルを1秒づつ減らす
        _throwInterval -= Time.deltaTime;
        // インターバルが0より小さくなれば
        if (_throwInterval < 0)
        {
            // オブジェクトを投擲する
            RandomThrow();

            // 制限時間が_FINAL_STAGE_TIME以下なら追加でオブジェクトを投擲する
            if (GameManager.instance.IsNearTimeLimit)
            {
                RandomThrow();
            }

            // 投擲の間隔をランダムに設定する
            _throwInterval = Random.Range(_THROW_INTERVAL_MIN, _THROW_INTERVAL_MAX);
        }
    }

    /// <summary>
    /// ランダムなオブジェクトをランダムなポジションから投擲する
    /// </summary>
    private void RandomThrow()
    {
        // ランダムな投擲するオブジェクトプレハブの引数
        int randomThrowObjectArgument = Random.Range(0, _generatedObjectPrefab.Length);
        // ランダムな投擲位置の引数
        int randomThrowPositionArgument = Random.Range(0, _generatPoint.Count);
        // ランダムなオブジェクトをランダムなポジションから投擲する
        Throwing(_generatedObjectPrefab[randomThrowObjectArgument], _generatPoint[randomThrowPositionArgument].transform.position);
    }
    /// <summary>
    /// オブジェクトを射出する
    /// </summary>
    private void Throwing(GameObject throwObject, Vector3 throwPosition)
    {
        // ランダムなQuaternionの値
        float randomQuaternion =  Random.Range(_OBJECT_QUATERNION_MIN, _OBJECT_QUATERNION_MAX);

        // 投擲するオブジェクトの角度をランダムに設定
        Quaternion objectQuaternion = Quaternion.Euler(randomQuaternion, randomQuaternion, randomQuaternion);

        // 投擲するオブジェクトを作成する
        GameObject generatedObject = Instantiate(throwObject, throwPosition, objectQuaternion);

        // 射出角度をランダムに決める
        float throwAngle = Random.Range(_ANGLE_MIN, _ANGLE_MAX);

        // 射出速度を算出
        Vector3 velocity = CalculateVelocity(throwPosition, _playerTransform.position, throwAngle);

        // AddForceで射出する
        Rigidbody rid = generatedObject.GetComponent<Rigidbody>();
        rid.AddForce(velocity * rid.mass, ForceMode.Impulse);

        // 投擲したときのSEを鳴らす
        GameManager.instance._audioManager.Throwing_SE();
    }

    /// <summary>
    /// 射出速度の計算
    /// </summary>
    /// <param name="targetPos">射出開始座標</param>
    /// <param name="throwPos">標的の座標</param>
    /// <param name="angle">射出速度</param>
    /// <returns></returns>
    private Vector3 CalculateVelocity(Vector3 targetPos, Vector3 throwPos, float angle)
    {
        // 射出角をラジアンに変換
        float rad = angle * Mathf.PI / 180;

        // 水平方向の距離x
        float horizontalDistance = Vector3.Distance(new Vector3(targetPos.x, 0, targetPos.z), new Vector3(throwPos.x, 0, throwPos.z));

        // 垂直方向の距離y
        float verticalDistance = targetPos.y - throwPos.y;

        // 斜方投射の公式を初速度について解く
        float speed = Mathf.Sqrt(-Physics.gravity.y * Mathf.Pow(horizontalDistance, 2) / (2 * Mathf.Pow(Mathf.Cos(rad), 2) * (horizontalDistance * Mathf.Tan(rad) + verticalDistance)));

        // 条件を満たす初速を算出できなければVector3.zeroを返す
        if (float.IsNaN(speed))
        {
            return Vector3.zero;
        }
        else
        {
            return (new Vector3(throwPos.x - targetPos.x, horizontalDistance * Mathf.Tan(rad), throwPos.z - targetPos.z).normalized * speed);
        }
    }
}
