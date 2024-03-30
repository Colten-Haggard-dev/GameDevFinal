using UnityEngine;

public class GrabScript : MonoBehaviour
{
    [SerializeField] private LayerMask GrabMask;
    [SerializeField] private FixedJoint GrabJoint;
    [SerializeField] private float GrabDelay = 1f;
    [SerializeField] private float GrabReach = 2f;
    [SerializeField] private float ThrowForce = 1.0f;

    private bool _grabbed = false;
    private float _grabTimer = 0f;

    private void Start()
    {
        GrabJoint.autoConfigureConnectedAnchor = false;
        GrabJoint.connectedAnchor = Vector3.zero;
    }

    // Update is called once per frame
    private void Update()
    {
        bool throw_obj = Input.GetButtonUp("Fire1");

        if (Input.GetButtonUp("Use") || throw_obj)
        {
            Physics.Raycast(transform.position, transform.forward, out RaycastHit grab_hit, GrabReach, GrabMask.value, QueryTriggerInteraction.Ignore);

            float cur_force = throw_obj ? ThrowForce : 0f;

            if (Time.time - _grabTimer >= GrabDelay && _grabbed)
            {
                GrabJoint.connectedBody.constraints = RigidbodyConstraints.None;
                GrabJoint.connectedBody.AddRelativeForce(transform.forward * cur_force);
                GrabJoint.connectedBody = null;
                _grabbed = false;
                _grabTimer = Time.time;
            }
            else if (grab_hit.rigidbody != null && Time.time - _grabTimer >= GrabDelay && !throw_obj)
            {
                GrabJoint.connectedBody = grab_hit.rigidbody;
                GrabJoint.connectedBody.rotation = Quaternion.identity;
                GrabJoint.connectedBody.constraints = RigidbodyConstraints.FreezeRotation;
                _grabbed = true;
                _grabTimer = Time.time;
            }
        }
    }
}
