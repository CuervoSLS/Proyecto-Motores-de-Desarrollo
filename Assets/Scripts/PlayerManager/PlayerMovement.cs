using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerGround))]
[RequireComponent(typeof(PlayerSprint))]
[RequireComponent(typeof(PlayerSlide))]
[RequireComponent(typeof(PlayerDash))]
[RequireComponent(typeof(PlayerJump))]
[RequireComponent(typeof(PlayerWallRun))]
[RequireComponent(typeof(PlayerLiana))]
[RequireComponent(typeof(PlayerRope))]
[RequireComponent(typeof(PlayerLocomotion))]
[RequireComponent(typeof(PlayerRotation))]
[RequireComponent(typeof(PlayerCameraRecentering))]
[RequireComponent(typeof(PlayerAnimation))]
[RequireComponent(typeof(PlayerPush))]
public class PlayerMovement : MonoBehaviour
{
    public CharacterController _controller;
    public Animator animator;
    public bool _enLianaMovil = false;
    public Vector3 _velocidadLiana = Vector3.zero;

    private Vector2 _input;
    private Vector2 _move;
    private Vector2 _look;
    private float _rotate;
    private bool _inputBloqueado;
    private Vector2 _inputUtil;

    public Vector2 Move => _move;
    public Vector2 InputUtil => _inputUtil;
    public Vector2 Look => _look;
    public float Rotate => _rotate;
    public bool InputBloqueado { get => _inputBloqueado; set => _inputBloqueado = value; }
    public float VerticalVelocity { get; set; }

    public PlayerGround Ground { get; private set; }
    public PlayerSprint Sprint { get; private set; }
    public PlayerSlide Slide { get; private set; }
    public PlayerDash Dash { get; private set; }
    public PlayerJump Jump { get; private set; }
    public PlayerWallRun WallRun { get; private set; }
    public PlayerLiana Liana { get; private set; }
    public PlayerRope Rope { get; private set; }
    public PlayerLocomotion Locomotion { get; private set; }
    public PlayerRotation Rotation { get; private set; }
    public PlayerCameraRecentering Recentering { get; private set; }
    public PlayerAnimation Anim { get; private set; }

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        Ground = GetComponent<PlayerGround>();
        Sprint = GetComponent<PlayerSprint>();
        Slide = GetComponent<PlayerSlide>();
        Dash = GetComponent<PlayerDash>();
        Jump = GetComponent<PlayerJump>();
        WallRun = GetComponent<PlayerWallRun>();
        Liana = GetComponent<PlayerLiana>();
        Rope = GetComponent<PlayerRope>();
        Locomotion = GetComponent<PlayerLocomotion>();
        Rotation = GetComponent<PlayerRotation>();
        Recentering = GetComponent<PlayerCameraRecentering>();
        Anim = GetComponent<PlayerAnimation>();

        Ground.Init(this);
        Sprint.Init(this);
        Slide.Init(this);
        Dash.Init(this);
        Jump.Init(this);
        WallRun.Init(this);
        Liana.Init(this);
        Rope.Init(this);
        Locomotion.Init(this);
        Rotation.Init(this);
        Recentering.Init(this);
        Anim.Init(this);
        GetComponent<PlayerPush>().Init(this);
    }

    private void Update()
    {
        if (_inputBloqueado && _input.sqrMagnitude < 0.01f)
        {
            _inputBloqueado = false;
        }

        _inputUtil = _inputBloqueado ? Vector2.zero : _input;
        _move = Liana.EnLiana ? new Vector2(0f, _inputUtil.y) : _inputUtil;

        float distanciaSuelo = Ground.Actualizar();

        Rope.LiberarIgnorada();
        if (!Rope.EnCuerda) Rope.Buscar();

        Recentering.Actualizar();

        if (Rope.EnCuerda)
        {
            TickTimers();
            Rope.Manejar();
            Rotation.Manejar();
            Anim.Actualizar(distanciaSuelo);
            return;
        }

        Sprint.Detectar();
        TickTimers();
        WallRun.Manejar();
        Locomotion.Manejar();
        Rotation.Manejar();
        Anim.Actualizar(distanciaSuelo);
    }

    private void TickTimers()
    {
        Liana.Tick();
        Rope.Tick();
        Jump.Tick();
        Slide.Tick();
        Dash.Tick();
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _look = context.ReadValue<Vector2>();
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        _rotate = context.ReadValue<float>();
    }

    public void OnSlide(InputAction.CallbackContext context)
    {
        Slide.OnSlide(context);
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        Dash.OnDash(context);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Jump.OnJump(context);
    }

    public void OnLiane()
    {
        Liana.OnLiane();
    }

    public void OffLiane()
    {
        Liana.OffLiane();
    }

    public void RecibirImpulsoLiana(Vector3 impulso)
    {
        Liana.RecibirImpulso(impulso);
    }
}