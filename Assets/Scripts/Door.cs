using Mirror;
using UnityEngine;

public class Door : NetworkBehaviour
{
    [SerializeField] private HingeJoint _joint;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform _parent;

    private bool _isDragging;
    private Transform _player;
    private Camera _camera;
    private Vector3 _lastPullPoint;
    private Vector3 _hingeAxis;
    private Vector3 _hingePivot;
    private float _sendTimer;
    private float _previousMouseDelta;
    private Vector3 _lastPosition;
    private float _initialPlayerSide;

    [SyncVar(hook = nameof(OnVelocityChanged))]
    private float _syncedVelocity;

    [SerializeField] private float _targetMotorSpeed = 90f;
    [SerializeField] private float _angleDeadZone = 0.5f;
    [Tooltip("1 если петли слева от игрока, -1 если справа")] [SerializeField] private int _hingeSide = 1;

    private void Awake()
    {
        if (_joint == null) _joint = GetComponent<HingeJoint>();
        if (_rb == null) _rb = GetComponent<Rigidbody>();

        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        _joint.useMotor = true;
        JointMotor motor = _joint.motor;
        if (motor.force <= 0f)
        {
            motor.force = 1000f;
        }
        _joint.motor = motor;

        _hingeAxis = transform.TransformDirection(_joint.axis).normalized;
        _hingePivot = transform.TransformPoint(_joint.anchor);
    }

    public void Bind(Vector3 playerPosition)
    {
        _isDragging = true;
        _sendTimer = 0f;

        Vector3 doorRight = transform.right;
        Vector3 toPlayer = playerPosition - _hingePivot;
        toPlayer.y = 0f;

        float dotRight = Vector3.Dot(doorRight, toPlayer);
        _initialPlayerSide = dotRight >= 0f ? 1f : -1f;
    }

    public void Unbind()
    {
        _isDragging = false;
        _previousMouseDelta = 0f;
        _initialPlayerSide = 0f;
        ApplyMotor(0);
    }

    public void Move(float m, Vector3 playerPosition)
    {
        if (!_isDragging) return;

        float targetVelocity = m * _targetMotorSpeed * _hingeSide * _initialPlayerSide;
        ApplyMotor(targetVelocity);

        _sendTimer += Time.deltaTime;
        if (_sendTimer >= 0.05f)
        {
            CmdSetVelocity(targetVelocity);
            _sendTimer = 0f;
        }
    }

    private bool TryGetPullPoint(Vector3 mousePosition, out Vector3 point)
    {
        point = Vector3.zero;
        if (_camera == null)
            return false;

        Ray ray = _camera.ScreenPointToRay(mousePosition);
        Plane plane = new Plane(_hingeAxis, _hingePivot);
        if (!plane.Raycast(ray, out float enter))
        {
            return false;
        }

        point = ray.GetPoint(enter);
        return true;
    }

    private void ApplyMotor(float velocity)
    {
        JointMotor motor = _joint.motor;
        motor.targetVelocity = velocity;
        _joint.motor = motor;
    }

    [Command(requiresAuthority = false)]
    private void CmdSetVelocity(float v)
    {
        _syncedVelocity = v;
    }

    private void OnVelocityChanged(float oldV, float newV)
    {
        ApplyMotor(newV);
    }
}
