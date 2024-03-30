using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpdateBBox : MonoBehaviour
{
    [SerializeField] private Collider[] meshes;

    private BoxCollider _bbox;

    private void Start()
    {
        _bbox = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        Bounds bounds = meshes[0].bounds;

        for (int i = 1; i < meshes.Length; ++i)
        {
            bounds.Encapsulate(meshes[i].bounds);
        }

        _bbox.center = bounds.center - transform.position;
        _bbox.size = bounds.size;
    }
}
