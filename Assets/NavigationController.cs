using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationController : MonoBehaviour
{
    [SerializeField] private GameObject MovePos;

    private NavMeshAgent _agent;

    // Start is called before the first frame update
    void Start()
    {
        if (!TryGetComponent(out _agent))
        {
            Debug.Log("GetComponent fail");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_agent.destination != MovePos.transform.position)
        {
            _agent.SetDestination(MovePos.transform.position);
        }
    }
}
