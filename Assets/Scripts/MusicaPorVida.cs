using UnityEngine;
using UnityEngine.Audio;

public class MusicaPorVida : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerHeart _corazon;
    [SerializeField] private AudioMixer _mixer;

    [Header("Lowpass de la musica")]
    [SerializeField] private string _parametroCutoff = "CutoffMusica";
    [SerializeField] private float _cutoffAbierto = 22000f;
    [SerializeField] private float _cutoffCerrado = 600f;
    [SerializeField] private float _velocidadFiltro = 2f;

    [Header("Intensidad con poca vida")]
    [SerializeField] private float _vidaBaja = 30f;
    [SerializeField, Range(0f, 1f)] private float _intensidadVidaBaja = 1f;
    [SerializeField] private float _velocidadSubida = 3f;
    [SerializeField] private float _velocidadBajada = 0.5f;

    private float _cierre;
    private float _intensidadZona;
    private bool _zonaDefinida;

    public static MusicaPorVida Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        _cutoffAbierto = Mathf.Max(20f, _cutoffAbierto);
        _cutoffCerrado = Mathf.Clamp(_cutoffCerrado, 20f, _cutoffAbierto);

        if (_corazon == null) _corazon = FindFirstObjectByType<PlayerHeart>();
    }

    private void Start()
    {
        AplicarCutoff();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void EstablecerIntensidadZona(float intensidad)
    {
        _intensidadZona = intensidad;
        _zonaDefinida = true;
    }

    private void Update()
    {
        if (_corazon == null) return;

        float dt = Time.deltaTime;
        float vida = _corazon.Vida;

        float cierreObjetivo = 1f - Mathf.Clamp01(vida / _corazon.VidaMaxima);
        if (!Mathf.Approximately(_cierre, cierreObjetivo))
        {
            _cierre = Mathf.MoveTowards(_cierre, cierreObjetivo, _velocidadFiltro * dt);
            AplicarCutoff();
        }

        ActualizarIntensidad(vida <= _vidaBaja, dt);
    }

    private void AplicarCutoff()
    {
        if (_mixer == null) return;

        float cutoff = _cutoffAbierto * Mathf.Pow(_cutoffCerrado / _cutoffAbierto, _cierre);
        _mixer.SetFloat(_parametroCutoff, cutoff);
    }

    private void ActualizarIntensidad(bool vidaBaja, float dt)
    {
        LayeredMusic musica = LayeredMusic.Instance;
        if (musica == null) return;

        if (!_zonaDefinida)
        {
            _intensidadZona = musica.intensity;
            _zonaDefinida = true;
        }

        float objetivo = vidaBaja ? Mathf.Max(_intensidadZona, _intensidadVidaBaja) : _intensidadZona;
        float actual = musica.intensity;
        if (Mathf.Approximately(actual, objetivo)) return;

        float velocidad = objetivo > actual ? _velocidadSubida : _velocidadBajada;
        musica.intensity = Mathf.MoveTowards(actual, objetivo, velocidad * dt);
    }
}