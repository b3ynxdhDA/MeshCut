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

    // 定数宣言---------------------
    // 発射角度の最小
    const float _ANGLE_MIN = 30;
    // 発射角度の最小
    const float _ANGLE_MAX = 71;
    // 投擲間隔の最小
    const int _THROW_INTERVAL_MIN = 1;
    // 投擲間隔の最大
    const int _THROW_INTERVAL_MAX = 4;

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
        if (GameManager.instance.game_State != GameManager.GameState.GameNow)
        {
            return;
        }

        // インターバルを1秒づつ減らす
        _throwInterval -= Time.deltaTime;
        // インターバルが0より小さくなれば
        if(_throwInterval < 0)
        {
            RandomThrow();
            // 投擲の間隔をランダムに設定する
            _throwInterval = Random.Range(_THROW_INTERVAL_MIN, _THROW_INTERVAL_MAX);
        }


        // @スペースでランダム投擲
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RandomThrow();
        }
    }

    /// <summary>
    /// ランダムなオブジェクトをランダムなポジションから投擲する
    /// </summary>
    private void RandomThrow()
    {
        // ランダムな投擲するオブジェクトプレハブの引数
        int randomThrowObjectArgument = Random.Range(0, _generatedObjectPrefab.Length);
        // ランダムな投擲位置
        int randomThrowPositionArgument = Random.Range(0, _generatPoint.Count);
        // ランダムなオブジェクトをランダムなポジションから投擲する
        Throwing(_generatedObjectPrefab[randomThrowObjectArgument], _generatPoint[randomThrowPositionArgument].transform.position);
    }
    /// <summary>
    /// オブジェクトを射出する
    /// </summary>
    private void Throwing(GameObject throwObject, Vector3 throwPosition)
    {
        GameObject generatedObject = Instantiate(throwObject, throwPosition, Quaternion.identity);

        // 射出角度をランダムに決める
        float throwAngle = Random.Range(_ANGLE_MIN, _ANGLE_MAX);

        // 射出速度を算出
        Vector3 velocity = CalculateVelocity(throwPosition, _playerTransform.position, throwAngle);

        // AddForceで射出する
        Rigidbody rid = generatedObject.GetComponent<Rigidbody>();
        rid.AddForce(velocity * rid.mass, ForceMode.Impulse);
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
