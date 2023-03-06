using System.Collections;
using UnityEngine;

/// <summary>
/// �ؒf���ꂽ�I�u�W�F�N�g���n�ʂɐڐG��������ł�����
/// </summary>
public class DestroyObject : MonoBehaviour
{
    // �ϐ��錾--------------------------
    // MeshRenderer�̃}�e���A���̔ԍ�(�����̏ꍇ)
    private int _materialNumber = 0;
    // �ڒn����
    private bool _isGround = false;

    // �萔�錾------------------------
    // Ray���Փ˂���LayerMask
    const int _GROUND_LAYER = 1 << 9;
    // �폜�̃f�B���C
    const float _DESTROY_DELAY = 3f;

    private void OnCollisionEnter(Collision collision)
    {
        // GrandLayer�ɐڐG������
        if (!_isGround && (1 << collision.gameObject.layer) == _GROUND_LAYER)
        {
            _isGround = true;
            StartCoroutine("Transparent");
        }
    }
    /// <summary>
    /// �I�u�W�F�N�g�����X�ɏ����R���[�`��
    /// �t�F�[�h�Ɏ��Ԃ�������̂ōċA����
    /// </summary>
    /// <returns></returns>
    IEnumerator Transparent()
    {
        //print(_materialNumber);
        // �}�e���A����RenderingMode��Fade�ɕς���
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        Material fadeMaterial = meshRenderer.materials[_materialNumber];
        fadeMaterial.SetFloat("_Mode", 2);
        fadeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        fadeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        fadeMaterial.SetInt("_ZWrite", 0);
        fadeMaterial.DisableKeyword("_ALPHATEST_ON");
        fadeMaterial.EnableKeyword("_ALPHABLEND_ON");
        fadeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        // �}�e���A���̔ԍ���+1����
        _materialNumber++;

        // �}�e���A������������ꍇ�̎��̃}�e���A���̃t�F�[�h���J�n
        if (_materialNumber < meshRenderer.materials.Length)
        {
            StartCoroutine("Transparent");
        }

        // �}�e���A���̓����x�����X�ɉ�����
        for (int i = 0; i < 255; i++)
        {
            fadeMaterial.color = fadeMaterial.color - new Color32(0, 0, 0, 1);
            yield return new WaitForSeconds(0.01f);
        }

        // �����ɂȂ�����I�u�W�F�N�g���폜����
        Destroy(this.gameObject, _DESTROY_DELAY);
    }
}
