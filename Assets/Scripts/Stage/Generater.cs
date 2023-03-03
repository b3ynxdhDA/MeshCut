using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 切断オブジェクトをランダムに生成しプレイヤーに向けて飛ばす
/// </summary>
public class Generater : MonoBehaviour
{
    [SerializeField, Header("生成するオブジェクトのプレハブ")]
    private GameObject[] _generatedObjectPrefab;

    // オブジェクトを生成する位置のゲームオブジェクト
    private GameObject[] _generatPoint = new GameObject[1];
    // 発射角度の最小
    const float _ANGLE_MIN = 10;
    // 発射角度の最小
    const float _ANGLE_MAX = 71;
    // プレイヤーの位置
    readonly Vector3 _PLAYER_POSITION = new Vector3(0f, 2f, 0f);


    void Start()
    {
        int childObjectCount = 0;
        foreach(Transform childObject in this.gameObject.transform)
        {
            _generatPoint[childObjectCount] = childObject.gameObject;
            childObjectCount++;
        }

        Throwing();
    }

    void Update()
    {
        
    }

    /// <summary>
    /// オブジェクトを射出する
    /// </summary>
    private void Throwing()
    {
        GameObject generatedObject = Instantiate(_generatedObjectPrefab[0], _generatPoint[0].transform.position, Quaternion.identity);

        // 射出角度をランダムに決める
        float throwAngle = Random.Range(_ANGLE_MIN, _ANGLE_MAX);

        // 射出速度を算出
        Vector3 velocity = CalculateVelocity(_generatPoint[0].transform.position, _PLAYER_POSITION, throwAngle);

        print(velocity);
        // 射出
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
        print("rad" + rad);
        // 水平方向の距離x
        float horizontalDistance = Vector3.Distance(new Vector3(targetPos.x,0, targetPos.z), new Vector3(throwPos.x,0, throwPos.z));
        print("horiDis" + horizontalDistance);

        // 垂直方向の距離y
        float verticalDistance = targetPos.y - throwPos.y;
        print("verDis" + verticalDistance);

        // 斜方投射の公式を初速度について解く
        float speed = Mathf.Sqrt(-Physics.gravity.y * Mathf.Pow(horizontalDistance, 2) / (2 * Mathf.Pow(Mathf.Cos(rad), 2) * (horizontalDistance * Mathf.Tan(rad) + verticalDistance)));
        print(speed);

        if (float.IsNaN(speed))
        {     
            print("isnan");
            // 条件を満たす初速を算出できなければVector3.zeroを返す
            return Vector3.zero;
        }
        else
        {
            return (new Vector3(throwPos.x - targetPos.x, horizontalDistance * Mathf.Tan(rad), throwPos.z - targetPos.z).normalized * speed);
        }
    }
}
