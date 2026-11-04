using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("Animaciones")]
    [SerializeField] private float _tiempoTransicion = 0.1f;
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
    [SerializeField] private string _estadoCuerda = "";
    [SerializeField] private string _estadoDashInicio = "DashInicio";
    [SerializeField] private string _estadoDashFinal = "DashFinal";

    [Header("Estados modo lanzamiento")]
    [SerializeField] private string _estadoLanzarQuieto = "L_Idle";
    [SerializeField] private string _estadoLanzarAdelante = "L_CaminarAdelante";
    [SerializeField] private string _estadoLanzarAtras = "L_CaminarAtras";
    [SerializeField] private string _estadoLanzarIzq = "L_CaminarIzquierda";
    [SerializeField] private string _estadoLanzarDer = "L_CaminarDerecha";
    [SerializeField] private string _estadoLanzarCorrer = "L_CorrerAdelante";

    [Header("Acciones de una sola vez")]
    [SerializeField] private float _duracionMaximaAccion = 4f;

    private PlayerMovement _p;
    private bool _enAire;
    private string _estadoAnimActual = "";
    private PlayerVoice _respiracion;
    private string _estadoAccion = "";
    private bool _accionCancelable = true;
    private float _accionInicio;

    public string EstadoDashFinal => _estadoDashFinal;
    public bool EnAccion => _estadoAccion.Length > 0;

    public void CancelarAccion()
    {
        _estadoAccion = "";
    }

    public void ReproducirUnaVez(string estado, bool cancelarAlMoverse = true)
    {
        if (string.IsNullOrEmpty(estado)) return;

        _estadoAccion = estado;
        _accionCancelable = cancelarAlMoverse;
        _accionInicio = Time.time;
        _estadoAnimActual = "";
    }

    public void Init(PlayerMovement p)
    {
        _p = p;
        _respiracion = GetComponent<PlayerVoice>();
    }

    public void Actualizar(float distanciaSuelo)
    {
        Animator animator = _p.animator;
        if (animator == null) return;

        bool liana = _p.Liana.EnLiana;
        bool enCuerda = _p.Rope.EnCuerda;
        bool grounded = _p.Ground.Grounded;
        bool sliding = _p.Slide.Sliding;
        bool dashing = _p.Dash.Dashing;
        float verticalVelocity = _p.VerticalVelocity;
        Vector2 move = _p.Move;

        bool wallRun = _p.WallRun.WallRunning && !liana;
        bool agarrado = liana || enCuerda;

        if (_enAire)
        {
            if (grounded || agarrado || wallRun || (verticalVelocity <= 0f && distanciaSuelo <= _distanciaAterrizaje))
            {
                _enAire = false;
            }
        }
        else if (!grounded && !agarrado && !wallRun && (verticalVelocity > 0.01f || distanciaSuelo > _distanciaAterrizaje))
        {
            _enAire = true;
        }

        if (_p.Dash.EnDashFinal && (_enAire || liana || wallRun || sliding || move.sqrMagnitude > 0.01f))
        {
            _p.Dash.EnDashFinal = false;
        }

        bool enDashFinal = _p.Dash.EnDashFinal;

        if (_estadoAccion.Length > 0 && (agarrado || wallRun || dashing || (_accionCancelable && move.sqrMagnitude > 0.01f) || Time.time - _accionInicio > _duracionMaximaAccion))
        {
            _estadoAccion = "";
        }

        string estadoPared = _p.WallRun.LadoPared < 0 ? _estadoWallRunDer : _estadoWallRunIzq;
        string estado;
        if (_estadoAccion.Length > 0) estado = _estadoAccion;
        else if (dashing && !_p.Dash.DashEnAire && !string.IsNullOrEmpty(_estadoDashInicio)) estado = _estadoDashInicio;
        else if (enDashFinal) estado = _estadoDashFinal;
        else if (liana && !string.IsNullOrEmpty(_estadoLiana)) estado = _estadoLiana;
        else if (enCuerda && !string.IsNullOrEmpty(_estadoCuerda)) estado = _estadoCuerda;
        else if (wallRun && !string.IsNullOrEmpty(estadoPared)) estado = estadoPared;
        else if (_enAire) estado = _estadoEnElAire;
        else if (_p.Lanzamiento.Activo) estado = EstadoLanzamiento(move);
        else estado = EstadoLocomocion(move, sliding);

        if (string.IsNullOrEmpty(estado)) return;

        if (estado != _estadoAnimActual)
        {
            _estadoAnimActual = estado;
            animator.speed = VelocidadEstado(estado);
            animator.CrossFadeInFixedTime(estado, _tiempoTransicion, 0);
            return;
        }

        if (animator.IsInTransition(0))
        {
            animator.speed = VelocidadEstado(estado);
            return;
        }

        if (_estadoAccion.Length > 0 && estado == _estadoAccion)
        {
            animator.speed = 1f;
            AnimatorStateInfo infoAccion = animator.GetCurrentAnimatorStateInfo(0);
            if (infoAccion.IsName(_estadoAccion) && infoAccion.normalizedTime >= 1f)
            {
                _estadoAccion = "";
            }
            return;
        }

        if (enDashFinal && estado == _estadoDashFinal)
        {
            animator.speed = 1f;
            AnimatorStateInfo infoFinal = animator.GetCurrentAnimatorStateInfo(0);
            if (infoFinal.IsName(_estadoDashFinal) && infoFinal.normalizedTime >= 1f)
            {
                _p.Dash.EnDashFinal = false;
            }
            return;
        }

        if (liana && estado == _estadoLiana)
        {
            float dir = move.y > 0.1f ? 1f : move.y < -0.1f ? -1f : 0f;
            animator.speed = dir;

            if (dir != 0f)
            {
                float t = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                if (dir > 0f && t >= 1f) animator.Play(estado, 0, 0f);
                else if (dir < 0f && t <= 0f) animator.Play(estado, 0, 0.999f);
            }
            return;
        }

        if (enCuerda && estado == _estadoCuerda)
        {
            animator.speed = 0f;
            return;
        }

        animator.speed = VelocidadEstado(estado);
    }

    private float VelocidadEstado(string estado)
    {

        if (_respiracion != null && estado == _estadoQuieto)
        {
            return _respiracion.VelocidadAnimacionQuieto;
        }

        return 1f;
    }

    private string EstadoLanzamiento(Vector2 move)
    {
        if (move.sqrMagnitude < 0.01f) return _estadoLanzarQuieto;

        if (move.y > 0.1f) return _p.Sprint.Sprinting ? _estadoLanzarCorrer : _estadoLanzarAdelante;
        if (move.y < -0.1f) return _estadoLanzarAtras;

        return move.x < 0f ? _estadoLanzarIzq : _estadoLanzarDer;
    }

    private string EstadoLocomocion(Vector2 move, bool sliding)
    {
        if (move.sqrMagnitude < 0.01f) return _estadoQuieto;

        bool izq = move.x < -0.1f;
        bool der = move.x > 0.1f;

        if (_p.Sprint.Sprinting || sliding)
        {
            return izq ? _estadoCorrerIzq : der ? _estadoCorrerDer : _estadoCorrerAdelante;
        }

        if (move.y > 0.1f)
        {
            return izq ? _estadoCaminarAdelanteIzq : der ? _estadoCaminarAdelanteDer : _estadoCaminarAdelante;
        }

        if (move.y < -0.1f)
        {
            return izq ? _estadoCaminarAtrasIzq : der ? _estadoCaminarAtrasDer : _estadoCaminarAtras;
        }

        return izq ? _estadoCaminarIzq : der ? _estadoCaminarDer : _estadoQuieto;
    }
}