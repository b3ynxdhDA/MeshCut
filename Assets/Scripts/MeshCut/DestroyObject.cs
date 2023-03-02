using System.Collections;
using UnityEngine;

/// <summary>
/// �ؒf���ꂽ�I�u�W�F�N�g���n�ʂɐڐG��������ł�����
/// </summary>
public class DestroyObject : MonoBehaviour
{
    // Ray���Փ˂���LayerMask
    const int _GROUND_LAYER = 1 << 9;
    // �폜�̃f�B���C
    const float _DESTROY_DELAY = 3f;
    // �I�u�W�F�N�g�̃}�e���A���̔ԍ�(�����̏ꍇ)
    private int _number = 0;

    private void OnCollisionEnter(Collision collision)
    {
        // GrandLayer�ɐڐG������
        if ((1 << collision.gameObject.layer) == _GROUND_LAYER)
        {
            StartCoroutine("Transparent");
            Destroy(this.gameObject, _DESTROY_DELAY);
        }
    }
    /// <summary>
    /// �I�u�W�F�N�g�����X�ɏ����R���[�`��
    /// �t�F�[�h�Ɏ��Ԃ�������̂ōċA����
    /// </summary>
    /// <returns></returns>
    IEnumerator Transparent()
    {
        // �}�e���A����RenderingMode��Fade�ɕς���
        Material fadeMaterial = GetComponent<MeshRenderer>().materials[_number];
        fadeMaterial.SetFloat("_Mode", 2);
        fadeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        fadeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        fadeMaterial.SetInt("_ZWrite", 0);
        fadeMaterial.DisableKeyword("_ALPHATEST_ON");
        fadeMaterial.EnableKeyword("_ALPHABLEND_ON");
        fadeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        _number++;
        // �}�e���A������������ꍇ�̎��̃}�e���A���̃t�F�[�h���J�n
        if (_number < GetComponent<MeshRenderer>().materials.Length)
        {
            StartCoroutine("Transparent");
        }

        // �}�e���A���̓����x�����X�ɉ�����
        for (int i = 0; i < 255; i++)
        {
            fadeMaterial.color = fadeMaterial.color - new Color32(0, 0, 0, 1);
            yield return new WaitForSeconds(0.01f);
        }

    }
}
