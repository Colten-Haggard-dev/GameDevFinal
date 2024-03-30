using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    private Rigidbody _rB;

    private bool _raise = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<BoxCollider>().enabled = false;
            _raise = true;
        }
    }

    private void Start()
    {
        _rB = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (_raise)
        {
            Debug.Log("raise elevator");
            //_rB.AddRelativeForce(Vector3.up * 10f, ForceMode.Acceleration);
            //_rB.MovePosition(Vector3.up * Time.deltaTime);
            _rB.position += 10 * Time.deltaTime * Vector3.up;
        }
    }
}
