using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerScript : MonoBehaviour, IControllable
{
    private static PlayerScript Instance;

    [SerializeField] private Rigidbody PlayerBody;
    [SerializeField] private BoxCollider PlayerCollider;
    [SerializeField] private Camera PlayerCamera;
    [SerializeField] private GameObject CameraTarget;
    [SerializeField] private GameObject Feet;
    [SerializeField] private float MouseSensitivity = 1f;
    [SerializeField] private float MaxSpeed = 10f;
    [SerializeField] private float Acceleration = 100f;
    [SerializeField] private float JumpHeight = 2f;
    [SerializeField] private LayerMask MovementMask;
    [SerializeField] private float GroundDrag = 5f;
    [SerializeField] private float AirDrag = 0f;
    [SerializeField] private float Width = 1f;
    [SerializeField] private float Height = 2f;
    [SerializeField] private float CrouchDelay = 0.1f;
    [SerializeField] private float JumpDelay = 0.1f;

    private Health PHealth;

    private bool _useMouse = false;
    private float _yaw = 0f;
    private float _pitch = 0f;
    private bool _isGrounded = false;
    private bool _above = false;
    private bool _jump = false;
    private bool _crouch = false;
    private float _inputX = 0f;
    private float _inputY = 0f;
    private float _crouchTimer = 0f;
    private float _jumpTimer = 0f;
    private bool _isShrunk = false;
    private float _largestY = 0f;

    private string _curDebugText = "mona lisa";

    public static Collider GetPlayerCollider()
    {
        return Instance.PlayerCollider;
    }

    public static Vector3 GetPlayerPos()
    {
        return Instance.transform.position;
    }

    public void AddRecoil(Vector3 recoil)
    {
        _pitch += recoil.x;
        _yaw += recoil.y;
    }

    private void GrabMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _useMouse = true;
    }

    private void ReleaseMouse()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _useMouse = false;
    }

    private void ShrinkBody()
    {
        float half_height = Height / 2f;
        PlayerCollider.size = new(Width, half_height, Width);
        CameraTarget.transform.localPosition = new(0f, 0.25f, 0f);
        _isShrunk = true;
    }

    private void UnshrinkBody()
    {
        PlayerCollider.size = new(Width, Height, Width);
        CameraTarget.transform.localPosition = new(0f, 0.75f, 0f);
        _isShrunk = false;
    }

    private void OnEnable()
    {
        GrabMouse();
    }

    private void OnDisable()
    {
        ReleaseMouse();
    }

    private void Start()
    {
        Instance = this;
        PHealth = GetComponent<Health>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!_useMouse)
        {
            return;
        }

        Vector2 mouse_delta = new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        mouse_delta *= MouseSensitivity * Mathf.Clamp(Time.timeScale, 0f, 1f);
        _pitch = Mathf.Clamp(_pitch - mouse_delta.y, -88, 88);
        _yaw += mouse_delta.x;

        _jump = Input.GetButton("Jump");
        _crouch = Input.GetButton("Crouch");
    }

    private void FixedUpdate()
    {
        CameraTarget.transform.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Feet.transform.localRotation = Quaternion.Euler(0f, _yaw, 0f);

        float inputH = _inputX;
        float inputV = _inputY;

        if (_crouch && !_isShrunk && Time.time - _crouchTimer >= CrouchDelay)
        {
            ShrinkBody();
            _crouchTimer = Time.time;
        }

        if (_jump && _isGrounded && Time.time - _jumpTimer >= JumpDelay)
        {
            PlayerBody.AddRelativeForce(new(0f, JumpHeight, 0f), ForceMode.VelocityChange);
            _jumpTimer = Time.time;
        }

        if (PlayerBody.position.y > _largestY)
            _largestY = PlayerBody.position.y;

        _curDebugText = _largestY.ToString();

        Vector3 velocity = new(inputH, 0, inputV);
        velocity.Normalize();
        velocity = Feet.transform.TransformDirection(velocity);
        velocity.Normalize();

        float target_max = _isShrunk ? MaxSpeed / 4 : MaxSpeed;
        target_max *= _isGrounded ? 1f : 0.5f;
        float accel_ratio = 1f - Mathf.Clamp(PlayerBody.velocity.magnitude / target_max, 0f, 1f);
        velocity *= Acceleration * accel_ratio;

        Vector3 half = new(0.5f, 0.525f, 0.5f);
        Vector3 half_short = _isShrunk ? new(0.5f, 0.525f / 4f, 0.5f) : new(0.5f, 0.525f, 0.5f);
        Vector3 player_pos = PlayerBody.transform.position;
        Vector3 player_half_height = new(0f, Height / 4f, 0f);

        bool unshrink = Physics.CheckBox(player_pos - player_half_height, half, Quaternion.identity, MovementMask.value, QueryTriggerInteraction.Ignore);
        _isGrounded = Physics.CheckBox(player_pos - player_half_height, half_short, Quaternion.identity, MovementMask.value, QueryTriggerInteraction.Ignore);
        _above = Physics.CheckBox(player_pos + player_half_height, half, Quaternion.identity, MovementMask.value, QueryTriggerInteraction.Ignore);

        //DisplayBox(player_pos - player_half_height, half_short, Color.white);

        if (!unshrink)
        {
            ShrinkBody();
        }
        else if (unshrink && _isShrunk && !_crouch && !_above)
        {
            UnshrinkBody();
        }

        PlayerBody.AddRelativeForce(velocity, ForceMode.Acceleration);

        float drag = _isGrounded ? GroundDrag : AirDrag;
        Vector3 hvel_drag = new Vector3(PlayerBody.velocity.x, 0f, PlayerBody.velocity.z) * drag;
        PlayerBody.AddRelativeForce(-hvel_drag, ForceMode.Acceleration);
    }

    //private void OnDrawGizmos()
    //{
    //    Vector3 look_dir_scaled = CameraTarget.transform.forward * 5f;
    //    look_dir_scaled += CameraTarget.transform.position;
    //    Handles.Label(look_dir_scaled, _curDebugText);
    //}

    //private static void DisplayBox(Vector3 center, Vector3 HalfExtend, Color color, float Duration = 0)
    //{
    //    Vector3[] Vertices = new Vector3[8];
    //    int i = 0;
    //    for (int x = -1; x < 2; x += 2)
    //    {
    //        for (int y = -1; y < 2; y += 2)
    //        {
    //            for (int z = -1; z < 2; z += 2)
    //            {
    //                Vertices[i] = center + new Vector3(HalfExtend.x * x, HalfExtend.y * y, HalfExtend.z * z);
    //                i++;
    //            }
    //        }
    //    }

    //    Debug.DrawLine(Vertices[0], Vertices[1], color, Duration);
    //    Debug.DrawLine(Vertices[1], Vertices[3], color, Duration);
    //    Debug.DrawLine(Vertices[2], Vertices[3], color, Duration);
    //    Debug.DrawLine(Vertices[2], Vertices[0], color, Duration);
    //    Debug.DrawLine(Vertices[4], Vertices[0], color, Duration);
    //    Debug.DrawLine(Vertices[4], Vertices[6], color, Duration);
    //    Debug.DrawLine(Vertices[2], Vertices[6], color, Duration);
    //    Debug.DrawLine(Vertices[7], Vertices[6], color, Duration);
    //    Debug.DrawLine(Vertices[7], Vertices[3], color, Duration);
    //    Debug.DrawLine(Vertices[7], Vertices[5], color, Duration);
    //    Debug.DrawLine(Vertices[1], Vertices[5], color, Duration);
    //    Debug.DrawLine(Vertices[4], Vertices[5], color, Duration);
    //}

    public void SendSignal(params object[] args)
    {
        switch ((int)args[0])
        {
            case 0:
                _inputX = (float)args[1];
                _inputY = (float)args[2];
                break;
            case 1:
                _jump = (bool)args[1];
                break;
            case 2:
                _crouch = (bool)args[1];
                break;
        }
    }

    public object Report()
    {
        throw new System.NotImplementedException();
    }

    public string StringReport()
    {
        return "Pmove";
    }
}
