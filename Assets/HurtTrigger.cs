using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Health hs))
        {
            hs.Damage(null, 1000, Vector3.zero);
        }
    }
}
