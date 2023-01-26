using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshCutRun : MonoBehaviour
{
    [SerializeField] Material _material = default;

    private MeshCut _meshCut = default;

    [SerializeField] private GameObject game = default;

    private GameObject[] _test;

    void Start()
    {

        _meshCut = new MeshCut();
        _test = _meshCut.Cut(game, Vector3.zero, Vector3.up, _material);
    }
    void Update()
    {

    }
}
