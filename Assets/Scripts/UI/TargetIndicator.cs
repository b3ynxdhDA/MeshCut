using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 画面のターゲットの方向を示す矢印を表示する
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class TargetIndicator : MonoBehaviour
{
    [SerializeField]  //ターゲットをつけるオブジェクト
    private Transform _targetObject = default;
    [SerializeField]  //矢印の画像
    private Image _arrow = default;

    private Camera _mainCamera;
    private RectTransform __rectTransform;

    private void Start()
    {
        _mainCamera = Camera.main;
        __rectTransform = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        // ルート（Canvas）のスケール値を取得する
        float canvasScale = transform.root.localScale.z;

        Vector3 center = 0.5f * new Vector3(Screen.width, Screen.height);

        //（画面中心を原点(0,0)とした）ターゲットのスクリーン座標を求める
        Vector3 pos = _mainCamera.WorldToScreenPoint(_targetObject.position) - center;
        // カメラ後方にあるターゲットのスクリーン座標は、画面外に移動する
        if (pos.z < 0f)
        {
            pos.x = -pos.x;
            pos.y = -pos.y;

            //カメラと水平なターゲットのスクリーン座標を補正する
            if (Mathf.Approximately(pos.y, 0f))
            {
                pos.y = -center.y;
            }
        }

        // 画面端のUIが見切れないように調整する
        // UI座標系の値をスクリーン座標系の値に変換する
        Vector3 halfSize = 0.5f * canvasScale * __rectTransform.sizeDelta;
        float d = Mathf.Max(
            Mathf.Abs(pos.x / (center.x - halfSize.x)),
            Mathf.Abs(pos.y / (center.y - halfSize.y))
        );

        //ターゲットのスクリーン座標が画面外なら、画面端になるよう調整する
        bool isOffscreen = (pos.z < 0f || d > 1f);
        if (isOffscreen)
        {
            pos.x /= d;
            pos.y /= d;
        }
        // スクリーン座標系の値をUI座標系の値に変換する
        __rectTransform.anchoredPosition = pos / canvasScale;

        // ターゲットのスクリーン座標が画面外なら、ターゲットの方向を指す矢印を表示する
        _arrow.enabled = isOffscreen;
        if (isOffscreen)
        {
            _arrow.rectTransform.eulerAngles = new Vector3(
                0f, 0f,
                Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg
            );
        }
    }
}
