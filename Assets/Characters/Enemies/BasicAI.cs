using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BasicAI : MonoBehaviour
{
    private Health _HP;
    private NavMeshAgent _agent;

    private int _curState = 0;

    [SerializeField] private LayerMask CheckMask = new();
    [SerializeField] private Animator AnimStateMachine;
    [SerializeField] private GameObject BBox;
    [SerializeField] private Rigidbody[] Skeleton = new Rigidbody[0];

    private float DetectionRange = 20f;
    private float ShootRange = 10f;

    private float CurRange = 0f;

    private float _updatePath = 2f;
    private float _curUpdate = 0f;

    private void LookAtPoint(Vector3 point, float speed = 1f)
    {
        Vector3 dir = (point - transform.position).normalized;
        transform.forward = Vector3.Lerp(transform.forward, dir, speed * Time.deltaTime);

        if (Vector3.Distance(transform.forward, dir) < 0.1)
        {
            transform.forward = dir;
        }

        transform.eulerAngles = Vector3.Scale(transform.eulerAngles, Vector3.up);
    }

    // Start is called before the first frame update
    void Start()
    {
        _HP = GetComponent<Health>();
        _agent = GetComponent<NavMeshAgent>();

        foreach (Rigidbody rb in Skeleton)
        {
            rb.isKinematic = true;
        }
    }

    private void Percieve()
    {
        if (!_HP.IsAlive())
        {
            _curState = -1;
            return;
        }

        CurRange = Vector3.Distance(PlayerScript.GetPlayerPos(), transform.position);

        if (CurRange < ShootRange && Physics.Raycast(transform.position, PlayerScript.GetPlayerPos() - transform.position, out RaycastHit ray_hit, 50f, CheckMask, QueryTriggerInteraction.Ignore))
        {
            if (ray_hit.collider == PlayerScript.GetPlayerCollider())
            {
                _curState = 1;
                //Debug.Log("Player found!");
            }
            else
            {
                _curState = 2;
            }
        }
        else if (CurRange > DetectionRange)
        {
            _curState = 0;
        }
        else
        {
            _curState = 2;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Percieve();

        switch (_curState)
        {
            case -1:
                AnimStateMachine.StopPlayback();
                AnimStateMachine.enabled = false;
                BBox.SetActive(false);

                foreach (Rigidbody rb in Skeleton)
                {
                    rb.isKinematic = false;
                }

                _agent.enabled = false;
                break;
            case 0:
                _agent.enabled = false;
                AnimStateMachine.SetBool("IsShooting", false);
                AnimStateMachine.SetBool("IsMoving", false);
                AnimStateMachine.SetBool("IsTargetVisible", false);
                break;
            case 1:
                _agent.enabled = false;
                AnimStateMachine.SetBool("IsShooting", true);
                AnimStateMachine.SetBool("IsMoving", false);
                AnimStateMachine.SetBool("IsTargetVisible", true);

                LookAtPoint(PlayerScript.GetPlayerPos(), 5f);
                break;
            case 2:
                AnimStateMachine.SetBool("IsShooting", false);
                AnimStateMachine.SetBool("IsMoving", true);
                AnimStateMachine.SetBool("IsTargetVisible", false);

                _agent.enabled = true;

                _curUpdate += Time.deltaTime;

                if (_curUpdate >= _updatePath)
                {
                    _curUpdate = 0f;
                    _agent.SetDestination(PlayerScript.GetPlayerPos());
                }

                //Vector3 dir = (_agent.nextPosition - transform.position).normalized;
                //transform.forward = dir;
                //transform.eulerAngles = Vector3.Scale(transform.eulerAngles, Vector3.up);

                LookAtPoint(_agent.nextPosition, 10f);
                break;
        }
    }
}
