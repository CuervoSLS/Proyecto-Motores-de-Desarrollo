using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float _normalSpeed = 7.5f;
    [SerializeField] private float _backSpeed = 5.5f;
    [SerializeField] private float _sprintSpeed = 15f;
    [SerializeField] private float _lianeSpeed = 5f;
    [SerializeField] private float _slideSpeed = 17f;
    [SerializeField] private float _slideDuration = 1f;
    [SerializeField] private float _lianePegado = 1f;
    [SerializeField] private float _bloqueoLianaTrasSalto = 0.4f;

    [Header("Sprint (doble toque de la W)")]
    [SerializeField] private float _doubleTapWindow = 0.3f;
    [SerializeField] private bool _bloquearSprintEnAire = true;
    [SerializeField] private float _deceleracionAire = 4f;

    [Header("Rotacion")]
    [SerializeField] private float _rotateSpeed = 100f;

    [Header("Salto y gravedad")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _gravity = -9.8f;
    [SerializeField] private float _gravityLiane = -9.8f;
    [SerializeField] private float _groundedStickVelocity = -2f;

    [Header("Peso")]
    [SerializeField] private float _masa = 50f;
    [SerializeField] private float _aceleracionEmpuje = 6f;
    [SerializeField] private float _multiplicadorCaida = 1.5f;

    [Header("Deteccion de piso")]
    [SerializeField] private LayerMask _groundMask = ~0;
    [SerializeField] private float _groundCheckDistance = 0.1f;

    [Header("Doble salto")]
    [SerializeField] private int _maxJumps = 2;

    [Header("Wall Run")]
    [SerializeField] private LayerMask _wallMask = ~0;
    [SerializeField] private float _wallCheckDistance = 0.7f;
    [SerializeField] private float _wallRunSpeed = 8f;
    [SerializeField] private float _wallRunGravity = -1f;

    [Header("Estado")]
    [SerializeField] private bool _liane = false;
    public CharacterController _controller;

    [Header("Animaciones")]
    public Animator animator;
    [SerializeField] private float _tiempoTransicion = 0.1f;
    [SerializeField] private bool _repetirAnimacionesEnLoop = true;
    [SerializeField] private float _distanciaAterrizaje = 0.5f;

    [Header("Estados del Animator")]
    [SerializeField] private string _estadoQuieto = "Quieto";
    [SerializeField] private string _estadoEnElAire = "Enelaire";
    [SerializeField] private string _estadoCaminarAdelante = "CaminarAdelante";
    [SerializeField] private string _estadoCaminarAdelanteIzq = "CaminarAdelanteIzquierda";
    [SerializeField] private string _estadoCaminarAdelanteDer = "CaminarAdelanteDerecha";
    [SerializeField] private string _estadoCaminarIzq = "CaminarIzquierda";
    [SerializeField] private string _estadoCaminarDer = "CaminarDerecha";
    [SerializeField] private string _estadoCaminarAtras = "CaminarAtras";
    [SerializeField] private string _estadoCaminarAtrasIzq = "CaminarAtrasIzquierda";
    [SerializeField] private string _estadoCaminarAtrasDer = "CaminarAtrasDerecha";
    [SerializeField] private string _estadoCorrerAdelante = "CorrerAdelante";
    [SerializeField] private string _estadoCorrerIzq = "CorrerIzquierda";
    [SerializeField] private string _estadoCorrerDer = "CorrerDerecha";
    [SerializeField] private string _estadoWallRunIzq = "";
    [SerializeField] private string _estadoWallRunDer = "";
    [SerializeField] private string _estadoLiana = "";

    public bool _enLianaMovil = false;
    public Vector3 _velocidadLiana = Vector3.zero;

    private const float DistanciaMaximaSuelo = 200f;

    private Vector2 _input;
    private Vector2 _move;
    private int _ladoPared;
    private bool _paredBloqueada;
    private float _rotate;
    private Vector2 _look;
    private float _verticalVelocity;
    private Vector3 _lianeNormal;
    private float _velocidadActual;
    private bool _grounded;
    private bool _sprinting;
    private bool _sliding;
    private float _slideTimer;
    private int _jumpsRestantes;
    private bool _wallRunning;
    private Vector3 _wallNormal;
    private float _lastWTapTime = -10f;
    private bool _enAire;
    private float _lianaBloqueo;
    private Liane _lianaActual;
    private string _estadoAnimActual = "";

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        _jumpsRestantes = _maxJumps;
        _velocidadActual = _normalSpeed;
    }

    private void Update()
    {
        _move = _liane ? new Vector2(0f, _input.y) : _input;

        float distanciaSuelo = DistanciaAlSuelo();
        _grounded = _verticalVelocity <= 0.01f && distanciaSuelo <= _groundCheckDistance;

        DetectDoubleTapSprint();
        UpdateTimers();
        HandleWallRun();
        HandleMovement();
        HandleRotation();
        UpdateAnimations(distanciaSuelo);
    }

    private void LateUpdate()
    {
        if (_enLianaMovil && _controller.enabled)
        {
            _controller.Move(_velocidadLiana * Time.deltaTime);
        }
    }

    private void DetectDoubleTapSprint()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb[Key.W].wasPressedThisFrame)
        {
            if (Time.time - _lastWTapTime <= _doubleTapWindow)
            {
                _lastWTapTime = -10f;
                if (!_liane && !_sliding && (!_bloquearSprintEnAire || _grounded))
                {
                    _sprinting = true;
                }
            }
            else
            {
                _lastWTapTime = Time.time;
            }
        }

        if (_sprinting && _move.y <= 0.1f)
        {
            _sprinting = false;
        }
    }

    private float GetCurrentSpeed(float dt)
    {
        float baseSpeed = !_liane && _move.y < -0.1f ? _backSpeed : _normalSpeed;

        if (_bloquearSprintEnAire && !_grounded && !_liane)
        {
            _velocidadActual = Mathf.Max(baseSpeed, _velocidadActual - _deceleracionAire * dt);
        }
        else
        {
            _velocidadActual = _sliding ? _slideSpeed : (!_liane && _sprinting) ? _sprintSpeed : baseSpeed;
        }

        return _velocidadActual;
    }

    private void UpdateTimers()
    {
        if (_lianaBloqueo > 0f)
        {
            _lianaBloqueo -= Time.deltaTime;
        }

        if (_grounded && !_liane)
        {
            _jumpsRestantes = _maxJumps;
        }

        if (_sliding)
        {
            _slideTimer -= Time.deltaTime;
            if (_slideTimer <= 0f)
            {
                _sliding = false;
            }
        }
    }

    private void HandleWallRun()
    {
        _wallRunning = false;

        int lado = _move.x < -0.1f ? -1 : _move.x > 0.1f ? 1 : 0;

        if (_grounded || _liane || lado == 0)
        {
            _ladoPared = 0;
            _paredBloqueada = false;
            return;
        }

        if (_ladoPared != 0 && lado != _ladoPared)
        {
            _ladoPared = 0;
            _paredBloqueada = true;
        }

        if (_paredBloqueada) return;

        if (Physics.Raycast(transform.position, transform.right * lado, out RaycastHit hit, _wallCheckDistance, _wallMask))
        {
            _wallRunning = true;
            _ladoPared = lado;
            _wallNormal = hit.normal;
            _jumpsRestantes = _maxJumps;
        }
        else
        {
            _ladoPared = 0;
        }
    }

    private void HandleMovement()
    {
        float dt = Time.deltaTime;

        if (_liane && !(Keyboard.current != null && Keyboard.current.shiftKey.isPressed))
        {
            _verticalVelocity = 0f;

            Vector3 movement = _move.y > 0.1f ? Vector3.up : _move.y < -0.1f ? Vector3.down : Vector3.zero;

            _controller.Move((movement * _lianeSpeed - _lianeNormal * _lianePegado) * dt);
            return;
        }

        if (_wallRunning)
        {
            _verticalVelocity = _wallRunGravity;
            Vector3 wallMove = Vector3.ProjectOnPlane(transform.forward, _wallNormal).normalized * _wallRunSpeed;
            wallMove.y = _verticalVelocity;
            _controller.Move(wallMove * dt);
            return;
        }

        Vector3 move = (transform.forward * _move.y + transform.right * _move.x).normalized * GetCurrentSpeed(dt);

        if (_grounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = _groundedStickVelocity;
        }

        float gravedad = _liane ? _gravityLiane : _gravity;
        if (_verticalVelocity < 0f && !_liane)
        {
            gravedad *= _multiplicadorCaida;
        }

        _verticalVelocity += gravedad * dt;
        move.y = _verticalVelocity;
        _controller.Move(move * dt);
    }

    private void HandleRotation()
    {
        if (_liane) return;

        float rotation = (_rotate + _look.x) * _rotateSpeed * Time.deltaTime;
        if (rotation != 0f)
        {
            transform.Rotate(0f, rotation, 0f);
        }
    }

    private float DistanciaAlSuelo()
    {
        float radio = _controller.radius * 0.9f;
        Vector3 origen = transform.TransformPoint(_controller.center);

        if (Physics.SphereCast(origen, radio, Vector3.down, out RaycastHit hit, DistanciaMaximaSuelo, _groundMask, QueryTriggerInteraction.Ignore))
        {
            return Mathf.Max(0f, hit.distance + radio - _controller.height * 0.5f);
        }

        return float.PositiveInfinity;
    }

    private float JumpVelocity() => Mathf.Sqrt(_jumpForce * -2f * _gravity);

    private void UpdateAnimations(float distanciaSuelo)
    {
        if (animator == null) return;

        bool wallRun = _wallRunning && !_liane;

        if (_enAire)
        {
            if (_grounded || _liane || wallRun || (_verticalVelocity <= 0f && distanciaSuelo <= _distanciaAterrizaje))
            {
                _enAire = false;
            }
        }
        else if (!_grounded && !_liane && !wallRun && (_verticalVelocity > 0.01f || distanciaSuelo > _distanciaAterrizaje))
        {
            _enAire = true;
        }

        string estadoPared = _ladoPared < 0 ? _estadoWallRunDer : _estadoWallRunIzq;
        string estado;
        if (_liane && !string.IsNullOrEmpty(_estadoLiana)) estado = _estadoLiana;
        else if (wallRun && !string.IsNullOrEmpty(estadoPared)) estado = estadoPared;
        else if (_enAire) estado = _estadoEnElAire;
        else estado = EstadoLocomocion();

        if (string.IsNullOrEmpty(estado)) return;

        if (estado != _estadoAnimActual)
        {
            _estadoAnimActual = estado;
            animator.speed = 1f;
            animator.CrossFadeInFixedTime(estado, _tiempoTransicion, 0);
            return;
        }

        if (animator.IsInTransition(0))
        {
            animator.speed = 1f;
            return;
        }

        if (_liane && estado == _estadoLiana)
        {
            float dir = _move.y > 0.1f ? 1f : _move.y < -0.1f ? -1f : 0f;
            animator.speed = dir;

            if (dir != 0f)
            {
                float t = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                if (dir > 0f && t >= 1f) animator.Play(estado, 0, 0f);
                else if (dir < 0f && t <= 0f) animator.Play(estado, 0, 0.999f);
            }
            return;
        }

        animator.speed = 1f;

        if (_repetirAnimacionesEnLoop)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName(estado) && info.normalizedTime >= 1f)
            {
                animator.Play(estado, 0, 0f);
            }
        }
    }

    private string EstadoLocomocion()
    {
        if (_move.sqrMagnitude < 0.01f) return _estadoQuieto;

        bool izq = _move.x < -0.1f;
        bool der = _move.x > 0.1f;

        if (_sprinting || _sliding)
        {
            return izq ? _estadoCorrerIzq : der ? _estadoCorrerDer : _estadoCorrerAdelante;
        }

        if (_move.y > 0.1f)
        {
            return izq ? _estadoCaminarAdelanteIzq : der ? _estadoCaminarAdelanteDer : _estadoCaminarAdelante;
        }

        if (_move.y < -0.1f)
        {
            return izq ? _estadoCaminarAtrasIzq : der ? _estadoCaminarAtrasDer : _estadoCaminarAtras;
        }

        return izq ? _estadoCaminarIzq : der ? _estadoCaminarDer : _estadoQuieto;
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
        if (!context.performed || !_sprinting || !_grounded) return;

        _sliding = true;
        _sprinting = false;
        _slideTimer = _slideDuration;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (_grounded || _liane)
        {
            if (_liane)
            {
                if (_lianaActual != null) _lianaActual.Soltar();
                _liane = false;
                _lianaBloqueo = _bloqueoLianaTrasSalto;
            }

            _verticalVelocity = JumpVelocity();
            _jumpsRestantes = _maxJumps - 1;

            if (_sliding)
            {
                _sliding = false;
                _sprinting = true;
            }
            return;
        }

        if (_jumpsRestantes > 0)
        {
            _verticalVelocity = JumpVelocity();
            _jumpsRestantes--;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.TryGetComponent(out Liane liane))
        {
            if (_lianaBloqueo <= 0f)
            {
                _lianeNormal = hit.normal;
                _lianaActual = liane;
                liane.RegistrarContacto(this);
            }
            return;
        }

        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null || rb.isKinematic || hit.moveDirection.y < -0.3f) return;

        Vector3 direccion = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z).normalized;
        rb.AddForceAtPosition(direccion * (_masa * _aceleracionEmpuje), hit.point, ForceMode.Force);
    }

    public void OnLiane()
    {
        if (_lianaBloqueo <= 0f) _liane = true;
    }

    public void OffLiane()
    {
        _lianaActual = null;
        _liane = false;
    }

    public void RecibirImpulsoLiana(Vector3 impulso)
    {
        _verticalVelocity += impulso.y;
        if (_controller.enabled)
        {
            _controller.Move(new Vector3(impulso.x, 0f, impulso.z) * Time.deltaTime);
        }
    }
}