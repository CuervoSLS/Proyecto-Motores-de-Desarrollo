using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float _normalSpeed = 7.5f;
    [SerializeField] private float _sprintSpeed = 15f;


    [SerializeField] private float _lianeSpeed = 5f;
    [SerializeField] private float _slideSpeed = 17f;

    [Header("Dash")]
    [SerializeField] private float _dashForce = 40f;
    [SerializeField] private float _dashDistance = 4f;
    [SerializeField] private float _dashDuration = 0.25f;
    [SerializeField] private float _dashCooldown = 1.5f;
    [SerializeField] private float _deceleracionImpulsoAire = 25f;
    [SerializeField] private float _impulsoAireResidual = 4f;
    [SerializeField] private TrailRenderer[] _trails;

    [Header("Rotacion")]

    [SerializeField] private float _rotateSpeed = 100f;

    [Header("Salto y gravedad")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _gravity = -9.8f;

    [SerializeField] private float _gravityLiane = -9.8f;

    [Header("Doble salto")]
    [Tooltip("Cantidad total de saltos disponibles antes de volver a tocar el piso (2 = salto normal + 1 salto extra en el aire)")]
    [SerializeField] private int _maxJumps = 2;
    private int _jumpsRestantes;

    [Header("Wall Run")]
    [Tooltip("Capas que cuentan como pared para poder correr por ellas")]
    [SerializeField] private LayerMask _wallMask = ~0;
    [SerializeField] private float _wallCheckDistance = 0.7f;
    [SerializeField] private float _wallRunSpeed = 8f;
    [Tooltip("Gravedad reducida mientras se corre por la pared (para que no se caiga de golpe)")]
    [SerializeField] private float _wallRunGravity = -1f;
    [SerializeField] private float _wallJumpSideForce = 8f;
    [SerializeField] private float _wallJumpUpForce = 6f;
    [SerializeField] private float _wallJumpCooldown = 0.25f;

    private bool _wallRunning = false;
    private Vector3 _wallNormal;
    private float _wallJumpCooldownTimer = 0f;

    [Header("Deteccion de piso")]
    [Tooltip("Distancia del sondeo hacia abajo para detectar el piso real (evita confundir una pared con el piso)")]
    [SerializeField] private float _groundCheckDistance = 0.3f;
    [SerializeField] private LayerMask _groundMask = ~0;

    [Header("Carama (modo hijo")]

    [SerializeField] private Transform _camaraTransform;
    [Header("Objeto a instanciar")]
    [SerializeField] GameObject GameObjectPrefab;

    [SerializeField] private bool _liane = false;
    public CharacterController _controller;
<<<<<<< Updated upstream
    [SerializeField] private float _speed;
=======

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
    [SerializeField] private string _estadoDashInicio = "DashInicio";
    [SerializeField] private string _estadoDashFinal = "DashFinal";
    [SerializeField] private string _estadoWallRunIzq = "";
    [SerializeField] private string _estadoWallRunDer = "";
    [SerializeField] private string _estadoLiana = "";

    public bool _enLianaMovil = false;
    public Vector3 _velocidadLiana = Vector3.zero;

    private const float DistanciaMaximaSuelo = 200f;

    private Vector2 _input;
>>>>>>> Stashed changes
    private Vector2 _move;
    private float _rotate;
    private Vector2 _look;
    private float _verticalVelocity;
<<<<<<< Updated upstream
    private float _pitch;
    private bool _sprinting = false;
    private bool _sliding = false;

=======
    private Vector3 _lianeNormal;
    private float _velocidadActual;
    private bool _grounded;
    private bool _sprinting;
    private bool _sliding;
    private float _slideTimer;
    private bool _dashing;
    private bool _enDashFinal;
    private bool _dashEnAire;
    private Vector3 _inercia;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private float _dashRecorrido;
    private bool _inputBloqueado;
    private Vector2 _inputUtil;
    private Vector3 _dashDireccion;
    private int _jumpsRestantes;
    private bool _wallRunning;
    private Vector3 _wallNormal;
    private float _lastWTapTime = -10f;
    private bool _enAire;
    private float _lianaBloqueo;
    private Liane _lianaActual;
    private string _estadoAnimActual = "";
    private bool _trailsEmitiendo;
>>>>>>> Stashed changes

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
<<<<<<< Updated upstream
=======
        if (animator == null) animator = GetComponentInChildren<Animator>();
        _jumpsRestantes = _maxJumps;
        _velocidadActual = _normalSpeed;

        _trailsEmitiendo = true;
        SetTrailsEmitting(false);
>>>>>>> Stashed changes
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _speed = _normalSpeed;
        _jumpsRestantes = _maxJumps;
    }

    // Update is called once per frame
    private void Update()
    {
<<<<<<< Updated upstream
        ActualizarSaltos();
=======
        if (_inputBloqueado && _input.sqrMagnitude < 0.01f)
        {
            _inputBloqueado = false;
        }

        _inputUtil = _inputBloqueado ? Vector2.zero : _input;
        _move = _liane ? new Vector2(0f, _inputUtil.y) : _inputUtil;

        float distanciaSuelo = DistanciaAlSuelo();
        _grounded = _verticalVelocity <= 0.01f && distanciaSuelo <= _groundCheckDistance;

        DetectDoubleTapSprint();
        UpdateTimers();
>>>>>>> Stashed changes
        HandleWallRun();
        HandleMovement();
        HandleRotation();
        HandleLook();
    }
    private void FixedUpdate()
    {

    }

    private void ActualizarSaltos()
    {
        // mientras esta en el piso siempre tiene disponibles todos sus saltos
        if (_controller.isGrounded && !_liane)
        {
            _jumpsRestantes = _maxJumps;
        }

        if (_wallJumpCooldownTimer > 0f)
        {
            _wallJumpCooldownTimer -= Time.deltaTime;
        }
    }

    private bool CheckWall(Vector3 direccion, out RaycastHit hit)
    {
        return Physics.Raycast(transform.position, direccion, out hit, _wallCheckDistance, _wallMask);
    }

    private void HandleWallRun()
    {
        // no se puede correr por la pared si esta en el piso, en una liana, o recien hizo un wall jump
        if (_controller.isGrounded || _liane == true || _wallJumpCooldownTimer > 0f)
        {
            _wallRunning = false;
            return;
        }

        bool paredDerecha = CheckWall(transform.right, out RaycastHit hitDerecha);
        bool paredIzquierda = CheckWall(-transform.right, out RaycastHit hitIzquierda);

        bool intentaMoverse = _move.magnitude > 0.1f;

        if ((paredDerecha || paredIzquierda) && intentaMoverse)
        {
            _wallRunning = true;
            _wallNormal = paredDerecha ? hitDerecha.normal : hitIzquierda.normal;

            // al pegarse a la pared renueva el/los salto(s) extra, para poder saltar de pared en pared
            _jumpsRestantes = _maxJumps;
        }
        else
        {
            _wallRunning = false;
        }
    }

    private void HandleMovement()
    {
        if (_liane == true) //comprueba si esta en liana
        {
            if (Keyboard.current.shiftKey.isPressed == false) //comprueba el shift ya que con eso podes moverte libremente por la liana
            {
                _verticalVelocity = 0;
                _speed = _lianeSpeed;
                Vector3 movement = transform.right * _move.x;

                if (Keyboard.current.wKey.isPressed)
                {
                    movement = Vector3.up;
                }
                else if (Keyboard.current.sKey.isPressed)
                {
                    movement = Vector3.down;
                }

                movement = movement.normalized * _speed;

                _controller.Move(movement * Time.deltaTime);


                return;
            }

        }

        if (_wallRunning == true) //comprueba si esta corriendo por una pared
        {
            // se mueve pegado a la pared en la direccion hacia la que esta mirando
            Vector3 wallForward = Vector3.ProjectOnPlane(transform.forward, _wallNormal).normalized;
            Vector3 wallMove = wallForward * _wallRunSpeed;

            _verticalVelocity = _wallRunGravity; // gravedad reducida para "pegarse" a la pared
            wallMove.y = _verticalVelocity;

            _controller.Move(wallMove * Time.deltaTime);

            return;
        }

        Vector3 move = transform.forward * _move.y + transform.right * _move.x; //el movimiento normal, no se pone dentro de un else porque no afecta a nada.

        move = move.normalized * _speed;

        if (_liane == true) //comprueba si esta en liana para asi hacer que caiga mas lento si esta en esta
        {
            _verticalVelocity += _gravityLiane * Time.deltaTime;
        }
        else
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }
        move.y = _verticalVelocity;
        _controller.Move(move * Time.deltaTime);
    }
    private void HandleRotation()
    {
        float rotation = _rotate * _rotateSpeed * Time.deltaTime;
        transform.Rotate(0, rotation, 0);
    }
    private void HandleLook()
    {
        float mouseX = _look.x * _rotateSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        {
            _move = context.ReadValue<Vector2>();
        }

    }
    public void OnLook(InputAction.CallbackContext context)
    {
        _look = context.ReadValue<Vector2>();
    }
    public void OnRotate(InputAction.CallbackContext context)
    {
        _rotate = context.ReadValue<float>();
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_liane == false)
            {
                _speed = _sprintSpeed;
                _sprinting = true;
            }
            else
            {
                _sprinting = true;
                _speed = _normalSpeed;
            }
        }
        else if (context.canceled)
        {
            _sprinting = false;
            if (_sliding == false)
            {
                _speed = _normalSpeed;
            }

        }
    }
    public void OnSlide(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_sprinting == true && _controller.isGrounded)
            {
                _speed = _slideSpeed;
                _sliding = true;
                _sprinting = false;
                Invoke("CancelSlide", 1);
            }
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        if (_controller.isGrounded || _liane == true)
        {

            _verticalVelocity = Mathf.Sqrt(_jumpForce * -2f * _gravity);
            _jumpsRestantes = _maxJumps - 1; // ya se uso el salto principal, quedan los extra para el doble salto
            if (_sliding == true)
            {
                _sliding = false;
                CancelInvoke("CancelSlide");
                _speed = _sprintSpeed;
                _sprinting = true;
            }
            return;
        }

        // doble salto: esta en el aire (no grounded, no liana, no wall run) y le quedan saltos extra
        if (_jumpsRestantes > 0)
        {
            _verticalVelocity = Mathf.Sqrt(_jumpForce * -2f * _gravity);
            _jumpsRestantes--;
        }
    }
    public void OnLiane()
    {

        _liane = true;
    }
    public void OffLiane()
    {
        _liane = false;
        if (_sprinting == true)
        {
            _speed = _sprintSpeed;
        }
    }
    public void CancelSlide()
    {

        _sliding = false;
        if (_sprinting == true)
        {
            _speed = _sprintSpeed;
        }
        else
        {
            _speed = _normalSpeed;
        }
    }

    public bool _enLianaMovil = false;
    public Vector3 _velocidadLiana = Vector3.zero;

    private void LateUpdate()
    {
        if (_enLianaMovil && _controller.enabled)
        {
            _controller.Move(_velocidadLiana * Time.deltaTime);
        }
    }

<<<<<<< Updated upstream
=======
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

        if (_dashing)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f || _liane)
            {
                TerminarDash();
            }
        }
        else if (_dashCooldownTimer > 0f)
        {
            _dashCooldownTimer -= Time.deltaTime;
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

    private void SetTrailsEmitting(bool emitir)
    {
        if (_trailsEmitiendo == emitir || _trails == null) return;

        _trailsEmitiendo = emitir;

        for (int i = 0; i < _trails.Length; i++)
        {
            TrailRenderer trail = _trails[i];
            if (trail == null) continue;

            if (!emitir) trail.Clear();
            trail.emitting = emitir;
        }
    }

    private void TerminarDash()
    {
        _dashing = false;
        _inercia = _dashEnAire && !_liane ? _dashDireccion * _dashForce : Vector3.zero;
        SetTrailsEmitting(false);
        _enDashFinal = !_dashEnAire && _grounded && !_liane && !string.IsNullOrEmpty(_estadoDashFinal);
        _dashCooldownTimer = _dashCooldown;
        _sprinting = false;
        _lastWTapTime = -10f;
        _inputBloqueado = !_dashEnAire;
    }

    private bool PuedeHacerDash()
    {
        if (_dashing || _dashCooldownTimer > 0f) return false;
        if (_liane || _sliding || _wallRunning) return false;
        if (!_sprinting) return false;
        return _move.y > 0.1f && Mathf.Abs(_move.x) < 0.1f;
    }

    private void HandleMovement()
    {
        float dt = Time.deltaTime;

        if (_liane && !(Keyboard.current != null && Keyboard.current.shiftKey.isPressed))
        {
            _verticalVelocity = 0f;
            _inercia = Vector3.zero;

            Vector3 movement = _move.y > 0.1f ? Vector3.up : _move.y < -0.1f ? Vector3.down : Vector3.zero;

            _controller.Move((movement * _lianeSpeed - _lianeNormal * _lianePegado) * dt);
            return;
        }

        if (_wallRunning)
        {
            _verticalVelocity = _wallRunGravity;
            _inercia = Vector3.zero;
            Vector3 wallMove = Vector3.ProjectOnPlane(transform.forward, _wallNormal).normalized * _wallRunSpeed;
            wallMove.y = _verticalVelocity;
            _controller.Move(wallMove * dt);
            return;
        }

        if (_dashing)
        {
            if (_grounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = _groundedStickVelocity;
            }

            float gravedadDash = _gravity;
            if (_verticalVelocity < 0f)
            {
                gravedadDash *= _multiplicadorCaida;
            }

            _verticalVelocity += gravedadDash * dt;

            float restante = _dashDistance - _dashRecorrido;
            float velocidadDash = Mathf.Min(_dashForce, restante / dt);

            Vector3 dashMove = _dashDireccion * velocidadDash;
            dashMove.y = _verticalVelocity;

            Vector3 posicionAntes = transform.position;
            _controller.Move(dashMove * dt);

            Vector3 desplazamiento = transform.position - posicionAntes;
            desplazamiento.y = 0f;
            _dashRecorrido += desplazamiento.magnitude;

            if (_dashRecorrido >= _dashDistance - 0.01f)
            {
                TerminarDash();
            }
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
        if (_inercia.sqrMagnitude > 0.0001f)
        {
            move += _inercia;
            float objetivo = _grounded ? 0f : _impulsoAireResidual;
            float velocidadInercia = Mathf.MoveTowards(_inercia.magnitude, objetivo, _deceleracionImpulsoAire * dt);
            _inercia = _inercia.normalized * velocidadInercia;
        }

        move.y = _verticalVelocity;
        _controller.Move(move * dt);
    }

    private void HandleRotation()
    {
        if (_liane || _dashing) return;

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

        if (_enDashFinal && (_enAire || _liane || wallRun || _sliding || _move.sqrMagnitude > 0.01f))
        {
            _enDashFinal = false;
        }

        string estadoPared = _ladoPared < 0 ? _estadoWallRunDer : _estadoWallRunIzq;
        string estado;
        if (_dashing && !_dashEnAire && !string.IsNullOrEmpty(_estadoDashInicio)) estado = _estadoDashInicio;
        else if (_enDashFinal) estado = _estadoDashFinal;
        else if (_liane && !string.IsNullOrEmpty(_estadoLiana)) estado = _estadoLiana;
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

        if (_enDashFinal && estado == _estadoDashFinal)
        {
            animator.speed = 1f;
            AnimatorStateInfo infoFinal = animator.GetCurrentAnimatorStateInfo(0);
            if (infoFinal.IsName(_estadoDashFinal) && infoFinal.normalizedTime >= 1f)
            {
                _enDashFinal = false;
            }
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
        if (!context.performed || !_sprinting || !_grounded || _dashing) return;

        _sliding = true;
        _sprinting = false;
        _slideTimer = _slideDuration;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed || !PuedeHacerDash()) return;

        _dashing = true;
        _dashEnAire = !_grounded;
        SetTrailsEmitting(true);
        _enDashFinal = false;
        _dashTimer = _dashDuration;
        _dashRecorrido = 0f;
        _dashDireccion = transform.forward;
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
            if (_lianaBloqueo <= 0f && (_liane || (_inputUtil.y > 0.1f && Mathf.Abs(_inputUtil.x) < 0.1f)))
            {
                _lianeNormal = hit.normal;
                _lianaActual = liane;
                liane.RegistrarContacto(this);
            }
            return;
        }

        FallingPlatform plataforma = hit.collider.GetComponentInParent<FallingPlatform>();
        if (plataforma != null)
        {
            plataforma.Activar();
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

>>>>>>> Stashed changes
    public void RecibirImpulsoLiana(Vector3 impulso)
    {
        _verticalVelocity += impulso.y;
        if (_controller.enabled)
        {
            _controller.Move(new Vector3(impulso.x, 0f, impulso.z) * Time.deltaTime);
        }
    }
}