using UnityEngine;

public class FreeCam : MonoBehaviour
{
    [SerializeField] private float MouseSensitivity = 0.1f;
    [SerializeField] private float CamSpeed = 10f;
    //[SerializeField] private GameObject CameraTarget;

    private float _pitch = 0f;
    private float _yaw = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mouse_delta = new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        mouse_delta *= MouseSensitivity * Mathf.Clamp(Time.timeScale, 0f, 1f);
        _pitch = Mathf.Clamp(_pitch - mouse_delta.y, -88, 88);
        _yaw += mouse_delta.x;

        transform.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);

        float inputH = Input.GetAxis("Horizontal");
        float inputV = Input.GetAxis("Vertical");

        Vector3 velocity = new(inputH, 0, inputV);
        velocity.Normalize();
        velocity = transform.TransformDirection(velocity);
        velocity = velocity.normalized * CamSpeed;

        transform.position += velocity * Time.deltaTime;
    }
}
