using UnityEngine;

public class PlayerRope : MonoBehaviour
{
    [Header("Cuerda")]
    [SerializeField] private float _alturaMano = 1.7f;
    [SerializeField] private float _fuerzaColumpio = 12f;
    [SerializeField] private float _gravedadCuerda = -14f;
    [SerializeField] private float _arrastreCuerda = 0.25f;
    [SerializeField] private float _velocidadMaxCuerda = 22f;
    [SerializeField] private float _impulsoSaltoCuerda = 6f;
    [SerializeField] private float _bloqueoCuerdaTrasSalto = 0.5f;
    [SerializeField] private float _frenoAireTrasCuerda = 1.5f;
    [SerializeField] private float _frenoSueloTrasCuerda = 20f;

    private PlayerMovement _p;
    private Cuerda _cuerdaActual;
    private float _largoCuerda;
    private Vector3 _velCuerda;
    private Vector3 _velSalida;
    private float _cuerdaBloqueo;
    private Cuerda _cuerdaIgnorada;

    public bool EnCuerda { get; private set; }
    public float ImpulsoSalto => _impulsoSaltoCuerda;

    public void Init(PlayerMovement p)
    {
        _p = p;
    }

    public void Tick()
    {
        if (_cuerdaBloqueo > 0f)
        {
            _cuerdaBloqueo -= Time.deltaTime;
        }
    }

    public void LiberarIgnorada()
    {
        if (_cuerdaIgnorada != null && !EnCuerda && _cuerdaBloqueo <= 0f)
        {
            _cuerdaIgnorada.IgnorarColision(_p._controller, false);
            _cuerdaIgnorada = null;
        }
    }

    public void AplicarSalida(ref Vector3 move, float dt)
    {
        if (_velSalida.sqrMagnitude <= 0.0001f) return;

        move += _velSalida;
        float freno = _p.Ground.Grounded ? _frenoSueloTrasCuerda : _frenoAireTrasCuerda;
        _velSalida = Vector3.MoveTowards(_velSalida, Vector3.zero, freno * dt);
    }

    public void Buscar()
    {
        if (_p.Liana.EnLiana || _p.Dash.Dashing || _cuerdaBloqueo > 0f || Cuerda.Todas.Count == 0) return;

        Vector3 mano = PuntoMano();
        for (int i = 0; i < Cuerda.Todas.Count; i++)
        {
            Cuerda cuerda = Cuerda.Todas[i];
            if (cuerda.TryAgarrar(mano, out int indice))
            {
                Iniciar(cuerda, indice);
                return;
            }
        }
    }

    public void Soltar()
    {
        if (!EnCuerda) return;

        _cuerdaActual.Soltar(_velCuerda);
        EnCuerda = false;
        _cuerdaActual = null;
        _cuerdaBloqueo = _bloqueoCuerdaTrasSalto;
        _p.VerticalVelocity = _velCuerda.y;
        _velSalida = new Vector3(_velCuerda.x, 0f, _velCuerda.z);
    }

    public void Manejar()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector3 ancla = _cuerdaActual.Anclaje;
        Vector3 mano = PuntoMano();
        Vector3 adelante = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        _velCuerda += (Vector3.up * _gravedadCuerda + adelante * (_p.Move.y * _fuerzaColumpio)) * dt;
        _velCuerda /= 1f + _arrastreCuerda * dt;
        _velCuerda = Vector3.ClampMagnitude(_velCuerda, _velocidadMaxCuerda);

        Vector3 haciaMano = mano - ancla;
        float distancia = haciaMano.magnitude;
        if (distancia >= _largoCuerda - 0.02f)
        {
            Vector3 dir = haciaMano / distancia;
            float radial = Vector3.Dot(_velCuerda, dir);
            if (radial > 0f) _velCuerda -= dir * radial;
        }

        Vector3 objetivo = mano + _velCuerda * dt;
        Vector3 rel = objetivo - ancla;
        if (rel.magnitude > _largoCuerda) objetivo = ancla + rel.normalized * _largoCuerda;

        _p._controller.Move(objetivo - mano);

        Vector3 nueva = PuntoMano();
        _velCuerda = (nueva - mano) / dt;
        _cuerdaActual.ActualizarAgarre(nueva);
    }

    private Vector3 PuntoMano()
    {
        return transform.position + Vector3.up * _alturaMano;
    }

    private void Iniciar(Cuerda cuerda, int indice)
    {
        EnCuerda = true;
        _cuerdaActual = cuerda;
        _largoCuerda = Vector3.Distance(cuerda.Anclaje, PuntoMano());
        cuerda.Agarrar(indice, _largoCuerda, _p._controller);
        _cuerdaIgnorada = cuerda;

        _velCuerda = _p._controller.velocity;
        _velSalida = Vector3.zero;
        _p.Dash.LimpiarInercia();
        _p.VerticalVelocity = 0f;
        _p.Sprint.Sprinting = false;
        _p.Slide.Sliding = false;
        _p.Jump.JumpsRestantes = _p.Jump.MaxJumps;
    }
}