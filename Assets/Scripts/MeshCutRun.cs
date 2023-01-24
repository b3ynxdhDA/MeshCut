using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCutRun : MonoBehaviour
{
    [SerializeField] Material _material = default;

    private MeshCut _meshCut = default;

    GameObject game = default;

    RaycastHit hit;

    void Start()
    {

        _meshCut = GetComponent<MeshCut>();
        //_meshCut.Cut(this.gameObject, Vector3.zero, Vector3.zero, _material);
    }
    void Update()
    {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        Debug.DrawRay(ray.origin, hit.point - ray.origin, Color.blue, 10, false);
        if(Physics.Raycast(ray, out hit))
        {

        }
    }
}
