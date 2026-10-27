using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerBreathing : MonoBehaviour
{
    [Header("Sonidos")]
    [SerializeField] private AudioClip _respirarNormal;
    [SerializeField] private AudioClip _respirarRapido;
    [SerializeField, Range(0f, 1f)] private float _volumenNormal = 0.6f;
    [SerializeField, Range(0f, 1f)] private float _volumenRapido = 0.8f;

    [Header("Fuentes de audio")]
    [SerializeField] private AudioSource _fuenteNormal;
    [SerializeField] private AudioSource _fuenteRapida;

    [Header("Cambio de respiracion")]
    [SerializeField, Range(0f, 1f)] private float _umbralRapido = 0.6f;
    [SerializeField, Range(0f, 1f)] private float _umbralNormal = 0.25f;
    [SerializeField] private float _velocidadFundido = 1.5f;

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

    [Header("Silencio cuando el personaje hace otro sonido")]
    [SerializeField] private float _fundidoCorte = 20f;
    [SerializeField] private float _fundidoRegreso = 3f;
    [SerializeField] private float _margenSilencio = 0.1f;

    private PlayerMovement _p;
    private float _cansancio;
    private float _mezcla;
    private bool _agitado;
    private bool _dashAnterior;
    private float _silencio;
    private float _factorSilencio = 1f;

    public float Cansancio => _cansancio;
    public bool Agitado => _agitado;
    public float VelocidadAnimacionQuieto => Mathf.Lerp(_velocidadAnimacionNormal, _velocidadAnimacionRapida, _mezcla);

    public void Silenciar(float duracion)
    {
        _silencio = Mathf.Max(_silencio, duracion + _margenSilencio);
    }

    private void Start()
    {
        _p = GetComponent<PlayerMovement>();

        if (_fuenteNormal == null) _fuenteNormal = CrearFuente();
        if (_fuenteRapida == null) _fuenteRapida = CrearFuente();

        ConfigurarFuente(_fuenteNormal, _respirarNormal);
        ConfigurarFuente(_fuenteRapida, _respirarRapido);

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
        ActualizarSilencio(dt);
        ActualizarVolumenes();
    }

    private void ActualizarSilencio(float dt)
    {
        bool silenciado = _silencio > 0f;
        if (silenciado) _silencio -= dt;

        float objetivo = silenciado ? 0f : 1f;
        float velocidad = silenciado ? _fundidoCorte : _fundidoRegreso;
        _factorSilencio = Mathf.MoveTowards(_factorSilencio, objetivo, velocidad * dt);
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

        _mezcla = Mathf.MoveTowards(_mezcla, _agitado ? 1f : 0f, _velocidadFundido * dt);
    }

    private void ActualizarVolumenes()
    {
        float angulo = _mezcla * Mathf.PI * 0.5f;
        AplicarVolumen(_fuenteNormal, Mathf.Cos(angulo) * _volumenNormal);
        AplicarVolumen(_fuenteRapida, Mathf.Sin(angulo) * _volumenRapido);
    }

    private void AplicarVolumen(AudioSource fuente, float volumenBase)
    {
        if (fuente == null || fuente.clip == null) return;

        if (volumenBase > 0.001f)
        {
            fuente.volume = volumenBase * _factorSilencio;
            if (!fuente.isPlaying) fuente.Play();
        }
        else if (fuente.isPlaying)
        {
            fuente.Stop();
        }
    }

    private void AlSaltar()
    {
        _cansancio = Mathf.Clamp01(_cansancio + _porSalto);
    }

    private void ConfigurarFuente(AudioSource fuente, AudioClip clip)
    {
        fuente.clip = clip;
        fuente.loop = true;
        fuente.playOnAwake = false;
        fuente.volume = 0f;
    }

    private AudioSource CrearFuente()
    {
        return gameObject.AddComponent<AudioSource>();
    }
}