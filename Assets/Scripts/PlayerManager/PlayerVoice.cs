using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerVoice : MonoBehaviour
{
    [Header("Fuente de audio de la voz")]
    [SerializeField] private AudioSource _fuenteVoz;

    [Header("Respiracion")]
    [SerializeField] private AudioClip _respirarNormal;
    [SerializeField] private AudioClip _respirarRapido;
    [SerializeField, Range(0f, 1f)] private float _volumenNormal = 0.6f;
    [SerializeField, Range(0f, 1f)] private float _volumenRapido = 0.8f;

    [Header("Salto del personaje")]
    [SerializeField] private GrupoSonido _saltarPersonaje;
    [SerializeField] private float _margenSilencio = 0.1f;

    [Header("Cambio de respiracion")]
    [SerializeField, Range(0f, 1f)] private float _umbralRapido = 0.6f;
    [SerializeField, Range(0f, 1f)] private float _umbralNormal = 0.25f;
    [SerializeField] private float _fundidoSalida = 6f;
    [SerializeField] private float _fundidoEntrada = 3f;

    [Header("Cansancio que se gana")]
    [SerializeField] private float _porSegundoCorrer = 0.15f;
    [SerializeField] private float _porSalto = 0.12f;
    [SerializeField] private float _porDash = 0.2f;
    [SerializeField] private float _porSegundoWallRun = 0.12f;
    [SerializeField] private float _porSegundoTrepar = 0.08f;

    [Header("Cansancio que se recupera")]
    [SerializeField] private float _recuperacionQuieto = 0.12f;
    [SerializeField] private float _recuperacionCaminando = 0.06f;

    [Header("Velocidad de la animacion Quieto")]
    [SerializeField] private float _velocidadAnimacionNormal = 0.5f;
    [SerializeField] private float _velocidadAnimacionRapida = 1f;
    [SerializeField] private float _velocidadCambioAnimacion = 1.5f;

    private PlayerMovement _p;
    private float _cansancio;
    private float _mezcla;
    private bool _agitado;
    private bool _dashAnterior;
    private float _tiempoHabla;
    private float _volumenActual;
    private AudioClip _clipActual;

    public float Cansancio => _cansancio;
    public bool Agitado => _agitado;
    public float VelocidadAnimacionQuieto => Mathf.Lerp(_velocidadAnimacionNormal, _velocidadAnimacionRapida, _mezcla);

    private void Start()
    {
        _p = GetComponent<PlayerMovement>();

        if (_fuenteVoz == null) _fuenteVoz = gameObject.AddComponent<AudioSource>();
        _fuenteVoz.playOnAwake = false;

        _p.Jump.Saltado += AlSaltar;
    }

    private void OnDestroy()
    {
        if (_p != null && _p.Jump != null) _p.Jump.Saltado -= AlSaltar;
    }

    private void Update()
    {
        if (_p == null) return;

        float dt = Time.deltaTime;

        ActualizarCansancio(dt);
        ActualizarEstado(dt);
        ActualizarVoz(dt);
    }

    private void ActualizarCansancio(float dt)
    {
        bool dashing = _p.Dash.Dashing;
        if (dashing && !_dashAnterior) _cansancio += _porDash;
        _dashAnterior = dashing;

        bool moviendo = _p.Move.sqrMagnitude > 0.01f;
        float ganancia = 0f;

        if (_p.Sprint.Sprinting) ganancia += _porSegundoCorrer;
        if (_p.WallRun.WallRunning) ganancia += _porSegundoWallRun;
        if (_p.Liana.EnLiana && Mathf.Abs(_p.Move.y) > 0.1f) ganancia += _porSegundoTrepar;
        if (_p.Rope.EnCuerda && moviendo) ganancia += _porSegundoTrepar;

        if (ganancia > 0f)
        {
            _cansancio += ganancia * dt;
        }
        else
        {
            float recuperacion = moviendo ? _recuperacionCaminando : _recuperacionQuieto;
            _cansancio -= recuperacion * dt;
        }

        _cansancio = Mathf.Clamp01(_cansancio);
    }

    private void ActualizarEstado(float dt)
    {
        if (!_agitado && _cansancio >= _umbralRapido) _agitado = true;
        else if (_agitado && _cansancio <= _umbralNormal) _agitado = false;

        _mezcla = Mathf.MoveTowards(_mezcla, _agitado ? 1f : 0f, _velocidadCambioAnimacion * dt);
    }

    private void ActualizarVoz(float dt)
    {
        if (_fuenteVoz == null) return;

        if (_tiempoHabla > 0f)
        {
            _tiempoHabla -= dt;
            if (_tiempoHabla > 0f) return;

            _fuenteVoz.Stop();
            _clipActual = null;
            _volumenActual = 0f;
        }

        AudioClip deseado = _agitado ? _respirarRapido : _respirarNormal;
        float volumenBase = _agitado ? _volumenRapido : _volumenNormal;

        if (_clipActual != null && _clipActual != deseado)
        {
            _volumenActual = Mathf.MoveTowards(_volumenActual, 0f, _fundidoSalida * dt);
            _fuenteVoz.volume = _volumenActual;
            if (_volumenActual > 0.001f) return;

            _fuenteVoz.Stop();
            _clipActual = null;
        }

        if (deseado == null) return;

        if (_clipActual == null)
        {
            _volumenActual = 0f;
            _fuenteVoz.clip = deseado;
            _fuenteVoz.loop = true;
            _fuenteVoz.volume = 0f;
            _fuenteVoz.Play();
            _clipActual = deseado;
        }

        _volumenActual = Mathf.MoveTowards(_volumenActual, volumenBase, _fundidoEntrada * dt);
        _fuenteVoz.volume = _volumenActual;
    }

    private void AlSaltar()
    {
        _cansancio = Mathf.Clamp01(_cansancio + _porSalto);

        if (_fuenteVoz == null || _saltarPersonaje == null) return;

        AudioClip clip = _saltarPersonaje.Elegir();
        if (clip == null) return;

        _fuenteVoz.Stop();
        _fuenteVoz.clip = clip;
        _fuenteVoz.loop = false;
        _fuenteVoz.volume = _saltarPersonaje.volumen;
        _fuenteVoz.Play();

        _clipActual = null;
        _volumenActual = 0f;
        _tiempoHabla = clip.length + _margenSilencio;
    }
}