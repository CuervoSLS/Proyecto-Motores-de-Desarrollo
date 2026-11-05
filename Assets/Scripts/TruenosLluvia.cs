using UnityEngine;

public class TruenosLluvia : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AudioSource _fuente;
    [SerializeField] private GameObject _lluvia;
    [SerializeField] private Transform _centro;

    [Header("Truenos")]
    [SerializeField] private GrupoSonido _truenos;
    [SerializeField] private float _tiempoMinimo = 8f;
    [SerializeField] private float _tiempoMaximo = 20f;
    [SerializeField, Range(0f, 0.5f)] private float _variacionPitch = 0.1f;

    [Header("Posicion aleatoria")]
    [SerializeField] private float _distanciaMinima = 10f;
    [SerializeField] private float _distanciaMaxima = 30f;
    [SerializeField] private float _altura = 20f;

    private float _temporizador;

    public bool Activo { get; set; } = true;

    private void Awake()
    {
        if (_fuente == null) _fuente = GetComponent<AudioSource>();
        if (_fuente != null) _fuente.playOnAwake = false;

        if (_centro == null)
        {
            PlayerMovement jugador = FindFirstObjectByType<PlayerMovement>();
            if (jugador != null) _centro = jugador.transform;
        }

        Reiniciar();
    }

    private void Update()
    {
        if (!Activo || _fuente == null) return;
        if (_lluvia != null && !_lluvia.activeInHierarchy) return;

        _temporizador -= Time.deltaTime;
        if (_temporizador > 0f) return;

        Reiniciar();
        Sonar();
    }

    private void Reiniciar()
    {
        float minimo = Mathf.Max(0.1f, _tiempoMinimo);
        _temporizador = Random.Range(minimo, Mathf.Max(minimo, _tiempoMaximo));
    }

    private void Sonar()
    {
        AudioClip clip = _truenos.Elegir();
        if (clip == null) return;

        Colocar();

        _fuente.pitch = 1f + Random.Range(-_variacionPitch, _variacionPitch);
        _fuente.PlayOneShot(clip, _truenos.volumen);
    }

    private void Colocar()
    {
        float angulo = Random.value * Mathf.PI * 2f;
        float distancia = Random.Range(_distanciaMinima, Mathf.Max(_distanciaMinima, _distanciaMaxima));
        Vector3 centro = _centro != null ? _centro.position : Vector3.zero;

        _fuente.transform.position = new Vector3(
            centro.x + Mathf.Cos(angulo) * distancia,
            centro.y + _altura,
            centro.z + Mathf.Sin(angulo) * distancia);
    }
}