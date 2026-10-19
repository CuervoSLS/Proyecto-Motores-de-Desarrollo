using UnityEngine;
using UnityEngine.InputSystem;

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

    private PlayerMovement _p;
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
        _trailsEmitiendo = true;
        SetTrailsEmitting(false);
    }

    public void LimpiarInercia()
    {
        Inercia = Vector3.zero;
    }

    public void Tick()
    {
        if (Dashing)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f || _p.Liana.EnLiana)
            {
                Terminar();
            }
        }
        else if (_dashCooldownTimer > 0f)
        {
            _dashCooldownTimer -= Time.deltaTime;
        }
    }

    public void AplicarInercia(ref Vector3 move, float dt)
    {
        if (Inercia.sqrMagnitude <= 0.0001f) return;

        move += Inercia;
        float objetivo = _p.Ground.Grounded ? 0f : _impulsoAireResidual;
        float velocidadInercia = Mathf.MoveTowards(Inercia.magnitude, objetivo, _deceleracionImpulsoAire * dt);
        Inercia = Inercia.normalized * velocidadInercia;
    }

    public void Mover(float dt)
    {
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

        Vector3 posicionAntes = transform.position;
        _p._controller.Move(dashMove * dt);

        Vector3 desplazamiento = transform.position - posicionAntes;
        desplazamiento.y = 0f;
        _dashRecorrido += desplazamiento.magnitude;

        if (_dashRecorrido >= _dashDistance - 0.01f)
        {
            Terminar();
        }
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed || !PuedeHacerDash()) return;

        Dashing = true;
        DashEnAire = !_p.Ground.Grounded;
        SetTrailsEmitting(true);
        EnDashFinal = false;
        _dashTimer = _dashDuration;
        _dashRecorrido = 0f;
        _dashDireccion = transform.forward;
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
        Dashing = false;
        Inercia = DashEnAire && !_p.Liana.EnLiana ? _dashDireccion * _dashForce : Vector3.zero;
        SetTrailsEmitting(false);
        EnDashFinal = !DashEnAire && _p.Ground.Grounded && !_p.Liana.EnLiana && !string.IsNullOrEmpty(_p.Anim.EstadoDashFinal);
        _dashCooldownTimer = _dashCooldown;
        _p.Sprint.Sprinting = false;
        _p.Sprint.ReiniciarToque();
        _p.InputBloqueado = !DashEnAire;
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