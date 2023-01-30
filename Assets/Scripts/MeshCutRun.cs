using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCutRun : MonoBehaviour
{
    [SerializeField] Material _material = default;

    private MeshCut _meshCut = default;

    [SerializeField] private GameObject game = default;

    void Start()
    {
        _meshCut = new MeshCut();
        _meshCut.Cut(game, Vector3.zero, Vector3.right, _material);
    }
    void Update()
    {

    }
}
