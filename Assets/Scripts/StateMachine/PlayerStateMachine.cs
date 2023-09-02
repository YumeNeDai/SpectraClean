using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
    #region Serialized Fields

    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    [SerializeField] private float moveSpeed = 2.0f;

    [Tooltip("Running speed of the character in m/s")]
    [SerializeField] private float runningSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    [SerializeField] private float rotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    [SerializeField] private float speedChangeRate = 10.0f;

    [Range(0, 1)] public float footstepAudioVolume = 0.5f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    [SerializeField] private float gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    [SerializeField] private float attackTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    [SerializeField] private float fallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    [SerializeField] private bool grounded = true;

    [Tooltip("Useful for rough ground")]
    [SerializeField] private float groundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    [SerializeField] private float groundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    [SerializeField] private LayerMask groundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    [SerializeField] private GameObject cinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    [SerializeField] private float topClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    [SerializeField] private float bottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    [SerializeField] private float cameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    [SerializeField] private bool lockCameraPosition = false;

    [SerializeField] private AudioClip LandingAudioClip;
    [SerializeField] private AudioClip[] FootstepAudioClips;
    [Range(0, 1)] [SerializeField] private float FootstepAudioVolume = 0.5f;

    #endregion

    #region Properties

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetRotation = 0.0f;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _attackTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDAttack;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

    //colors
    private Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
    private Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

    // state variables
    private PlayerAbstractState _currentState;
    private StateFactory _states;

    private PlayerInput _playerInput;
    private Animator _animator;
    private CharacterController _controller;
    private StarterAssetsInputs _input;
    private Transform _mainCamera;

    private const float _threshold = 0.01f;
    private const float _speedOffset = 0.1f;

    private bool isCurrentDeviceMouse
    {
        get
        {
            return PlayerInput.currentControlScheme == "KeyboardMouse";
        }
    }

    #endregion

    #region Getters and Setter

    public PlayerAbstractState CurrentState { get => _currentState; set => _currentState = value; }
    public PlayerInput PlayerInput { get => _playerInput; set => _playerInput = value; }
    public Animator Animator { get => _animator; set => _animator = value; }
    public CharacterController Controller { get => _controller; set => _controller = value; }
    public StarterAssetsInputs Input { get => _input; set => _input = value; }
    public Transform MainCamera { get => _mainCamera; set => _mainCamera = value; }
    public LayerMask GroundLayers { get => groundLayers; set => groundLayers = value; }

    public int AnimIDSpeed { get => _animIDSpeed; set => _animIDSpeed = value; }
    public int AnimIDGrounded { get => _animIDGrounded; set => _animIDGrounded = value; }
    public int AnimIDAttack { get => _animIDAttack; set => _animIDAttack = value; }
    public int AnimIDFreeFall { get => _animIDFreeFall; set => _animIDFreeFall = value; }
    public int AnimIDMotionSpeed { get => _animIDMotionSpeed; set => _animIDMotionSpeed = value; }

    public bool Grounded { get => grounded; set => grounded = value; }

    public float Speed { get => _speed; set => _speed = value; }
    public float AnimationBlend { get => _animationBlend; set => _animationBlend = value; }
    public float TargetRotation { get => _targetRotation; set => _targetRotation = value; }
    public float VerticalVelocity { get => _verticalVelocity; set => _verticalVelocity = value; }
    public float TerminalVelocity { get => _terminalVelocity; set => _terminalVelocity = value; }
    public float AttackTimeoutDelta { get => _attackTimeoutDelta; set => _attackTimeoutDelta = value; }
    public float FallTimeoutDelta { get => _fallTimeoutDelta; set => _fallTimeoutDelta = value; }
    public float Gravity { get => gravity; set => gravity = value; }
    public float CinemachineTargetYaw { get => _cinemachineTargetYaw; set => _cinemachineTargetYaw = value; }
    public float CinemachineTargetPitch { get => _cinemachineTargetPitch; set => _cinemachineTargetPitch = value; }
    public float SpeedChangeRate { get => speedChangeRate; set => speedChangeRate = value; }
    public float RotationSmoothTime { get => rotationSmoothTime; set => rotationSmoothTime = value; }
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
    public float RunningSpeed { get => runningSpeed; set => runningSpeed = value; }
    public float AttackTimeout { get => attackTimeout; set => attackTimeout = value; }
    public float FallTimeout { get => fallTimeout; set => fallTimeout = value; }
    public float GroundedOffset { get => groundedOffset; set => groundedOffset = value; }
    public float GroundedRadius { get => groundedRadius; set => groundedRadius = value; }

    public static float Threshold => _threshold;
    public static float SpeedOffset => _speedOffset;


    #endregion

    #region Awake and Updates
    void Awake()
    {
        // camera
        MainCamera = Camera.main.transform;
        CinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;

        // component getters
        Controller = GetComponent<CharacterController>();
        Input = GetComponent<StarterAssetsInputs>();
        PlayerInput = GetComponent<PlayerInput>();
        Animator = GetComponent<Animator>();

        AssignAnimationIDs();

        // reset our timeouts on start
        AttackTimeoutDelta = AttackTimeout;
        FallTimeoutDelta = FallTimeout;

        // setup state
        _states = new StateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(_currentState);
        _currentState.UpdateStates();
        GroundedCheck();

    }

    private void FixedUpdate()
    {

    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    #endregion

    #region Camera

    private void CameraRotation()
    {  
        
        // if there is an input and camera position is not fixed
        Vector2 look = Input.GetRotation;
        if (look.sqrMagnitude >= Threshold && !lockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = isCurrentDeviceMouse ? 1.0f : Time.deltaTime;
            CinemachineTargetYaw += look.x * deltaTimeMultiplier;
            //CinemachineTargetPitch += look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        CinemachineTargetYaw = ClampAngle(CinemachineTargetYaw, float.MinValue, float.MaxValue);
        CinemachineTargetPitch = ClampAngle(CinemachineTargetPitch, bottomClamp, topClamp);

        // Cinemachine will follow this target
        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(CinemachineTargetPitch + cameraAngleOverride,
            CinemachineTargetYaw, 0.0f);

        
        
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    #endregion Camera

    #region Animations
    private void AssignAnimationIDs()
    {
        AnimIDSpeed = Animator.StringToHash("Speed");
        AnimIDGrounded = Animator.StringToHash("Grounded");
        AnimIDFreeFall = Animator.StringToHash("FreeFall");
        AnimIDMotionSpeed = Animator.StringToHash("MotionSpeed");

    }
    #endregion

    #region GroundCheck
    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
            QueryTriggerInteraction.Ignore);

    }
    #endregion

    #region Foley
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }
    #endregion
}
