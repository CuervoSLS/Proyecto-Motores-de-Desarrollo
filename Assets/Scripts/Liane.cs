using UnityEngine;

public class Liane : MonoBehaviour
{
    public enum EjeTransform { X, Y, Z }

    [SerializeField] private bool _lianaQuieta = true;

    [Header("Movimiento")]
    [SerializeField] private bool _mover = false;
    [SerializeField] private EjeTransform _ejeMovimiento = EjeTransform.Y;
    [SerializeField] private float _movimientoMin = 0f;
    [SerializeField] private float _movimientoMax = 3f;
    [SerializeField] private float _velocidadMovimiento = 2f;

    [Header("Contacto")]
    [SerializeField] private float _tiempoSinContacto = 0.15f;

    private Vector3 _posicionInicial;
    private Vector3 _direccionMovimiento;
    private Vector3 _posicionAnterior;
    private PlayerMovement _jugador;
    private float _ultimoContacto;

    private void Start()
    {
        _posicionInicial = transform.position;
        _posicionAnterior = _posicionInicial;
        _direccionMovimiento = _ejeMovimiento == EjeTransform.X ? Vector3.right : _ejeMovimiento == EjeTransform.Y ? Vector3.up : Vector3.forward;

        if (_lianaQuieta)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if (!_lianaQuieta)
        {
            if (_mover)
            {
                float offset = _movimientoMin + Mathf.PingPong(Time.time * _velocidadMovimiento, _movimientoMax - _movimientoMin);
                transform.position = _posicionInicial + _direccionMovimiento * offset;
            }

            Vector3 posicion = transform.position;

            if (_jugador != null && Time.deltaTime > 0f)
            {
                _jugador._velocidadLiana = (posicion - _posicionAnterior) / Time.deltaTime;
            }

            _posicionAnterior = posicion;
        }

        if (_jugador != null && Time.time - _ultimoContacto > _tiempoSinContacto)
        {
            Soltar();
        }
    }

    private void OnDisable()
    {
        if (_jugador != null)
        {
            Soltar();
        }
    }

    public void RegistrarContacto(PlayerMovement jugador)
    {
        _ultimoContacto = Time.time;

        if (_jugador == jugador) return;

        _jugador = jugador;
        enabled = true;
        jugador.OnLiane();

        if (!_lianaQuieta)
        {
            jugador._enLianaMovil = true;
            _posicionAnterior = transform.position;
        }
    }

    public void Soltar()
    {
        PlayerMovement jugador = _jugador;
        if (jugador == null) return;

        _jugador = null;
        jugador.OffLiane();

        if (_lianaQuieta)
        {
            enabled = false;
            return;
        }

        jugador._enLianaMovil = false;
        jugador.RecibirImpulsoLiana(jugador._velocidadLiana);
        jugador._velocidadLiana = Vector3.zero;
    }
}