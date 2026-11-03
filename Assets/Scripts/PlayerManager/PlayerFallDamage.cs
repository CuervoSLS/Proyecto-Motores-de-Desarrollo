using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerFallDamage : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerHeart _vida;

    [Header("Danio por caida")]
    [SerializeField] private float _alturaMinima = 12f;
    [SerializeField] private float _danioPorMetro = 6f;
    [SerializeField] private float _danioMaximo = 100f;

    [Header("Teletransporte")]
    [SerializeField] private float _saltoMaximoPorCuadro = 5f;

    private PlayerMovement _p;
    private Vector3 _posicionAnterior;
    private float _alturaMaxima;
    private bool _enAire;

    private void Start()
    {
        _p = GetComponent<PlayerMovement>();

        if (_vida == null) _vida = GetComponentInParent<PlayerHeart>();
        if (_vida == null) _vida = FindFirstObjectByType<PlayerHeart>();

        _posicionAnterior = transform.position;
        _alturaMaxima = _posicionAnterior.y;
    }

    private void LateUpdate()
    {
        if (_p == null || _vida == null) return;

        Vector3 posicion = transform.position;
        bool teletransportado = (posicion - _posicionAnterior).sqrMagnitude > _saltoMaximoPorCuadro * _saltoMaximoPorCuadro;
        _posicionAnterior = posicion;

        if (teletransportado)
        {
            _enAire = false;
            _alturaMaxima = posicion.y;
            return;
        }

        bool enSuelo = _p.Ground.Grounded;
        bool agarrado = _p.Liana.EnLiana || _p.Rope.EnCuerda || _p.WallRun.WallRunning;

        if (agarrado)
        {
            _enAire = false;
            _alturaMaxima = posicion.y;
            return;
        }

        if (!enSuelo)
        {
            if (!_enAire)
            {
                _enAire = true;
                _alturaMaxima = posicion.y;
            }
            else if (posicion.y > _alturaMaxima)
            {
                _alturaMaxima = posicion.y;
            }
            return;
        }

        if (_enAire)
        {
            _enAire = false;
            Aplicar(_alturaMaxima - posicion.y);
        }

        _alturaMaxima = posicion.y;
    }

    private void Aplicar(float caida)
    {
        if (caida <= _alturaMinima) return;

        float danio = Mathf.Min(_danioMaximo, (caida - _alturaMinima) * _danioPorMetro);
        if (danio > 0f) _vida.BajarVida(danio);
    }
}