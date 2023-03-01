using UnityEditor;
using UnityEngine;

/// <summary>
/// メッシュの中心にピボットを移動
/// </summary>
public class MoveToCenterOfParent : MonoBehaviour
{
    private Vector3 _meshCentor = default;
    private void MoveToCenter()
    {

        Transform parent = Selection.activeTransform.parent;
        if (parent == null)
        {
            Debug.LogError("No Parent Found");
            return;
        }

        Renderer renderer = parent.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError("No Renderer Found");
            return;
        }

        print("Old Pos: " + Selection.activeTransform.position);
        Selection.activeTransform.position = renderer.bounds.center;
        print("New Pos: " + Selection.activeTransform.position);

        _meshCentor = renderer.bounds.center;
    }
}