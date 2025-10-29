using Mirror;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharacterMovement : NetworkBehaviour
{
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _gravity = -9.8f;
    [SerializeField] private float _jumpHeight = 2f;
    
    [SerializeField] private float _sensitivity = 200f;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _ui;
    [SerializeField] private Animator _animator;
    
    private CharacterController _characterController;
    private Vector3 _velocity;
    private bool _isGrounded;
    private float _xRotation = 0f;
    
    private Vector2 _lastMousePosition;
    private bool _isFirstFrame = true;
    private bool _restrict = false;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        _restrict = !hasFocus;
    }

    public override void OnStartLocalPlayer()
    {
        _camera.enabled = true;
        _ui.SetActive(true);
    }
    
    private void Start()
    {
       Cursor.lockState = CursorLockMode.Locked;
       Cursor.visible = false;

       Input.GetAxis("Mouse X");
       Input.GetAxis("Mouse Y");
    }

    private void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleJump();
    }

    [Client]
    private void HandleMovement()
    {
        if (_restrict) return;
        if (!isLocalPlayer) return;
        
        _isGrounded = _characterController.isGrounded;
        
        if (_isGrounded && _velocity.y < 0)
            _velocity.y = -2f;
        
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        Vector3 move = transform.right * x + transform.forward * z;
        _characterController.Move(move * _speed * Time.deltaTime);
        
        _velocity.y += _gravity * Time.deltaTime; 
        _characterController.Move(_velocity * Time.deltaTime);
        
        _animator.SetFloat("MoveSpeed", move.magnitude);
        _animator.SetBool("Grounded", _isGrounded);
    }

    [Client]
    private void HandleMouseLook()
    {
        if (_restrict) return;
        if (!isLocalPlayer) return;
        
        float mouseX = _isFirstFrame ? 0f : Input.GetAxis("Mouse X") * _sensitivity * Time.deltaTime;
        float mouseY = _isFirstFrame ? 0f : Input.GetAxis("Mouse Y") * _sensitivity * Time.deltaTime;

        _isFirstFrame = false;
        
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);
        _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        
        transform.Rotate(Vector3.up * mouseX);
    }

    [Client]
    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && _isGrounded)
            _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (ReferenceEquals(obj, null)) return;
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (ReferenceEquals(child, null)) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
