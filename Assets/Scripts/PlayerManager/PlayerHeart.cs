using UnityEngine;
using UnityEngine.UI;

public class PlayerHeart : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private float _vidaMaxima = 100f;

    [Header("Barras")]
    [SerializeField] private Slider _barraVida;
    [SerializeField] private Slider _barraRetraso;

    [Header("Color de la barra de vida")]
    [SerializeField] private Gradient _degradado = CrearDegradado();

    [Header("Barra con retraso")]
    [SerializeField] private float _tiempoRetraso = 0.6f;
    [SerializeField] private float _velocidadBarraRetraso = 40f;

    [Header("Reaparicion")]
    [SerializeField] private PlayerSpawn _spawn;

    [Header("Recuperacion")]
    [SerializeField] private float _tiempoParaRecuperar = 4f;
    [SerializeField] private float _velocidadRecuperacion = 8f;

    private float _vida;
    private float _vidaRetraso;
    private float _tiempoDesdeDanio;
    private float _esperaRetraso;
    private PlayerMovement _movimiento;
    private Image _rellenoVida;

    public event System.Action Murio;
    public event System.Action<float> Danio;

    public float Vida => _vida;
    public float VidaMaxima => _vidaMaxima;

    private void Awake()
    {
        _vidaMaxima = Mathf.Max(1f, _vidaMaxima);

        if (_spawn == null) _spawn = GetComponentInParent<PlayerSpawn>();
        _movimiento = GetComponentInParent<PlayerMovement>();

        _vida = _vidaMaxima;
        _vidaRetraso = _vidaMaxima;

        ConfigurarBarra(_barraVida);
        if (_barraVida != null && _barraVida.fillRect != null) _rellenoVida = _barraVida.fillRect.GetComponent<Image>();
        ConfigurarBarra(_barraRetraso);
        MostrarVida();
        MostrarRetraso();

        enabled = false;
    }

    public void SumarVida(float cantidad)
    {
        if (cantidad <= 0f) return;

        _vida = Mathf.Min(_vidaMaxima, _vida + cantidad);
        if (_vidaRetraso < _vida) _vidaRetraso = _vida;

        MostrarVida();
        MostrarRetraso();

        if (_vida < _vidaMaxima || _vidaRetraso > _vida) enabled = true;
    }

    public void BajarVida(float cantidad)
    {
        if (cantidad <= 0f || _vida <= 0f) return;

        _vida = Mathf.Max(0f, _vida - cantidad);
        _tiempoDesdeDanio = 0f;
        _esperaRetraso = _tiempoRetraso;

        MostrarVida();
        enabled = true;

        if (_vida <= 0f) Reaparecer();
        else Danio?.Invoke(cantidad);
    }

    private void Reaparecer()
    {
        Murio?.Invoke();

        if (_spawn != null) _spawn.Respawn();
        if (_movimiento != null) _movimiento.VerticalVelocity = 0f;

        _vida = _vidaMaxima;
        _vidaRetraso = _vidaMaxima;
        _tiempoDesdeDanio = 0f;
        _esperaRetraso = 0f;

        MostrarVida();
        MostrarRetraso();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        Recuperar(dt);
        SeguirRetraso(dt);

        bool lleno = _vida >= _vidaMaxima && _vidaRetraso <= _vida;
        if (lleno) enabled = false;
    }

    private void Recuperar(float dt)
    {
        if (_vida >= _vidaMaxima) return;

        _tiempoDesdeDanio += dt;
        if (_tiempoDesdeDanio < _tiempoParaRecuperar) return;

        _vida = Mathf.MoveTowards(_vida, _vidaMaxima, _velocidadRecuperacion * dt);
        MostrarVida();

        if (_vidaRetraso < _vida)
        {
            _vidaRetraso = _vida;
            MostrarRetraso();
        }
    }

    private void SeguirRetraso(float dt)
    {
        if (_vidaRetraso <= _vida) return;

        if (_esperaRetraso > 0f)
        {
            _esperaRetraso -= dt;
            return;
        }

        _vidaRetraso = Mathf.MoveTowards(_vidaRetraso, _vida, _velocidadBarraRetraso * dt);
        MostrarRetraso();
    }

    private void ConfigurarBarra(Slider barra)
    {
        if (barra == null) return;

        barra.minValue = 0f;
        barra.maxValue = _vidaMaxima;
        barra.wholeNumbers = false;
    }

    private void MostrarVida()
    {
        if (_barraVida != null) _barraVida.SetValueWithoutNotify(_vida);
        if (_rellenoVida != null) _rellenoVida.color = _degradado.Evaluate(1f - _vida / _vidaMaxima);
    }

    private void MostrarRetraso()
    {
        if (_barraRetraso != null) _barraRetraso.SetValueWithoutNotify(_vidaRetraso);
    }

    private static Gradient CrearDegradado()
    {
        Gradient degradado = new Gradient();
        degradado.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.2f, 0.85f, 0.25f), 0f),
                new GradientColorKey(new Color(0.95f, 0.85f, 0.15f), 0.5f),
                new GradientColorKey(new Color(0.9f, 0.1f, 0.1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            });
        return degradado;
    }
}