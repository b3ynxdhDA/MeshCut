using System.Collections;
using UnityEngine;

/// <summary>
/// 切断されたオブジェクトが地面に接触したら消滅させる
/// </summary>
public class DestroyObject : MonoBehaviour
{
    // 変数宣言--------------------------
    // MeshRendererのマテリアルの番号(複数の場合)
    private int _materialNumber = 0;
    // 接地判定
    private bool _isGround = false;

    // 定数宣言------------------------
    // Rayが衝突するLayerMask
    const int _GROUND_LAYER = 1 << 9;
    // 削除のディレイ
    const float _DESTROY_DELAY = 3f;

    private void OnCollisionEnter(Collision collision)
    {
        // GrandLayerに接触したら
        if (!_isGround && (1 << collision.gameObject.layer) == _GROUND_LAYER)
        {
            _isGround = true;
            StartCoroutine("Transparent");
        }
    }
    /// <summary>
    /// オブジェクトを徐々に消すコルーチン
    /// フェードに時間がかかるので再帰処理
    /// </summary>
    /// <returns></returns>
    IEnumerator Transparent()
    {
        //print(_materialNumber);
        // マテリアルのRenderingModeをFadeに変える
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        Material fadeMaterial = meshRenderer.materials[_materialNumber];
        fadeMaterial.SetFloat("_Mode", 2);
        fadeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        fadeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        fadeMaterial.SetInt("_ZWrite", 0);
        fadeMaterial.DisableKeyword("_ALPHATEST_ON");
        fadeMaterial.EnableKeyword("_ALPHABLEND_ON");
        fadeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        // マテリアルの番号を+1する
        _materialNumber++;

        // マテリアルが複数ある場合の次のマテリアルのフェードを開始
        if (_materialNumber < meshRenderer.materials.Length)
        {
            StartCoroutine("Transparent");
        }

        // マテリアルの透明度を徐々に下げる
        for (int i = 0; i < 255; i++)
        {
            fadeMaterial.color = fadeMaterial.color - new Color32(0, 0, 0, 1);
            yield return new WaitForSeconds(0.01f);
        }

        // 透明になったらオブジェクトを削除する
        Destroy(this.gameObject, _DESTROY_DELAY);
    }
}
