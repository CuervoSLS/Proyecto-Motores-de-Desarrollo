using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private float _dashForce = 40f;
    [SerializeField] private float _dashDistance = 4f;
    [SerializeField] private float _dashDuration = 0.25f;
    [SerializeField] private float _dashCooldown = 1.5f;
    [SerializeField] private float _deceleracionImpulsoAire = 25f;
    [SerializeField] private float _impulsoAireResidual = 4f;
    [SerializeField] private TrailRenderer[] _trails;

    [Header("UI Cooldown")]
    [SerializeField] private Slider _cooldownSlider;
    [Tooltip("Velocidad (unidades/seg) con la que el slider baja a 0 al usar el dash y sigue al progreso del cooldown.")]
    [SerializeField] private float _velocidadSlider = 8f;

    private const float EpsilonInercia = 0.0001f;
    private const float EpsilonDistancia = 0.01f;

    private PlayerMovement _p;
    private Transform _transform;
    private Vector3 _dashDireccion;
    private float _dashTimer;
    private float _dashCooldownTimer;
    private float _dashRecorrido;
    private bool _trailsEmitiendo;

    public bool Dashing { get; private set; }
    public bool EnDashFinal { get; set; }
    public bool DashEnAire { get; private set; }
    public Vector3 Inercia { get; private set; }

    public void Init(PlayerMovement p)
    {
        _p = p;
        _transform = transform;
        _trailsEmitiendo = true;
        SetTrailsEmitting(false);

        if (_cooldownSlider != null)
        {
            _cooldownSlider.minValue = 0f;
            _cooldownSlider.maxValue = 1f;
            _cooldownSlider.interactable = false;
            _cooldownSlider.SetValueWithoutNotify(1f);
        }
    }

    private void OnDisable()
    {
        if (_p == null) return;

        Dashing = false;
        Inercia = Vector3.zero;
        SetTrailsEmitting(false);
    }

    public void LimpiarInercia()
    {
        Inercia = Vector3.zero;
    }

    public void Tick()
    {
        float dt = Time.deltaTime;

        if (Dashing)
        {
            _dashTimer -= dt;
            if (_dashTimer <= 0f || _p.Liana.EnLiana)
            {
                Terminar();
            }
        }
        else if (_dashCooldownTimer > 0f)
        {
            _dashCooldownTimer -= dt;
        }

        ActualizarSlider(dt);
    }

    public void AplicarInercia(ref Vector3 move, float dt)
    {
        float magnitud = Inercia.magnitude;
        if (magnitud * magnitud <= EpsilonInercia) return;

        move += Inercia;

        float objetivo = _p.Ground.Grounded ? 0f : _impulsoAireResidual;
        float nuevaMagnitud = Mathf.MoveTowards(magnitud, objetivo, _deceleracionImpulsoAire * dt);
        Inercia *= nuevaMagnitud / magnitud;
    }

    public void Mover(float dt)
    {
        if (!Dashing || dt <= 0f) return;

        PlayerLocomotion loco = _p.Locomotion;

        if (_p.Ground.Grounded && _p.VerticalVelocity < 0f)
        {
            _p.VerticalVelocity = loco.GroundedStickVelocity;
        }

        float gravedadDash = loco.Gravity;
        if (_p.VerticalVelocity < 0f)
        {
            gravedadDash *= loco.MultiplicadorCaida;
        }

        _p.VerticalVelocity += gravedadDash * dt;

        float restante = _dashDistance - _dashRecorrido;
        float velocidadDash = Mathf.Min(_dashForce, restante / dt);

        Vector3 dashMove = _dashDireccion * velocidadDash;
        dashMove.y = _p.VerticalVelocity;

        Vector3 posicionAntes = _transform.position;
        _p._controller.Move(dashMove * dt);

        Vector3 desplazamiento = _transform.position - posicionAntes;
        desplazamiento.y = 0f;
        _dashRecorrido += desplazamiento.magnitude;

        if (_dashRecorrido >= _dashDistance - EpsilonDistancia)
        {
            Terminar();
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed || !PuedeHacerDash()) return;

        Dashing = true;
        DashEnAire = !_p.Ground.Grounded;
        EnDashFinal = false;
        _dashTimer = _dashDuration;
        _dashRecorrido = 0f;
        _dashDireccion = _transform.forward;
        SetTrailsEmitting(true);
    }

    private bool PuedeHacerDash()
    {
        if (Dashing || _dashCooldownTimer > 0f) return false;
        if (_p.Liana.EnLiana || _p.Slide.Sliding || _p.WallRun.WallRunning || _p.Rope.EnCuerda) return false;
        if (!_p.Sprint.Sprinting) return false;
        return _p.Move.y > 0.1f && Mathf.Abs(_p.Move.x) < 0.1f;
    }

    private void Terminar()
    {
        // Evita terminar dos veces en el mismo frame (Tick + Mover).
        if (!Dashing) return;

        Dashing = false;

        bool enLiana = _p.Liana.EnLiana;
        Inercia = DashEnAire && !enLiana ? _dashDireccion * _dashForce : Vector3.zero;
        EnDashFinal = !DashEnAire && !enLiana && _p.Ground.Grounded && !string.IsNullOrEmpty(_p.Anim.EstadoDashFinal);
        _dashCooldownTimer = _dashCooldown;

        SetTrailsEmitting(false);
    }

    private void ActualizarSlider(float dt)
    {
        if (_cooldownSlider == null) return;

        // Mientras dashea baja a 0; luego sube de 0 a 1 en exactamente el tiempo del cooldown.
        float objetivo;
        if (Dashing)
        {
            objetivo = 0f;
        }
        else if (_dashCooldownTimer > 0f && _dashCooldown > 0f)
        {
            objetivo = 1f - Mathf.Clamp01(_dashCooldownTimer / _dashCooldown);
        }
        else
        {
            objetivo = 1f;
        }

        float actual = _cooldownSlider.value;
        if (Mathf.Approximately(actual, objetivo)) return;

        _cooldownSlider.SetValueWithoutNotify(Mathf.MoveTowards(actual, objetivo, _velocidadSlider * dt));
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
}