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

    [Header("Objeto")]
    public Transform objetoLiana;
    [SerializeField] private float _posicionX = 0f;
    [SerializeField] private float _posicionZ = 0f;
    [SerializeField] private float _velocidadObjeto = 10f;

    private Transform _transform;
    private Vector3 _posicionInicial;
    private Vector3 _direccionMovimiento;
    private Vector3 _posicionAnterior;
    private PlayerMovement _jugador;
    private Transform _transformJugador;
    private float _offsetY;
    private float _ultimoContacto;

    private Transform _padreOriginal;
    private Vector3 _posLocalInicial;
    private Quaternion _rotLocalInicial = Quaternion.identity;
    private bool _restaurando;

    private void Start()
    {
        _transform = transform;
        _posicionInicial = _transform.position;
        _posicionAnterior = _posicionInicial;
        _direccionMovimiento = _ejeMovimiento == EjeTransform.X ? Vector3.right : _ejeMovimiento == EjeTransform.Y ? Vector3.up : Vector3.forward;

        if (objetoLiana != null)
        {
            _padreOriginal = objetoLiana.parent;
            _posLocalInicial = objetoLiana.localPosition;
            _rotLocalInicial = objetoLiana.localRotation;
        }

        if (_lianaQuieta)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (!_lianaQuieta)
        {
            if (_mover)
            {
                float offset = _movimientoMin + Mathf.PingPong(Time.time * _velocidadMovimiento, _movimientoMax - _movimientoMin);
                _transform.position = _posicionInicial + _direccionMovimiento * offset;
            }

            Vector3 posicion = _transform.position;

            if (_jugador != null && dt > 0f)
            {
                _jugador._velocidadLiana = (posicion - _posicionAnterior) / dt;
            }

            _posicionAnterior = posicion;
        }

        if (_jugador != null && Time.time - _ultimoContacto > _tiempoSinContacto)
        {
            Soltar();
        }

        if (_jugador == null && _lianaQuieta && !_restaurando)
        {
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (objetoLiana == null) return;

        float dt = Time.deltaTime;
        if (_jugador != null) AcomodarObjeto(dt);
        else if (_restaurando) RestaurarObjeto(dt);
    }

    private void OnDisable()
    {
        if (_jugador != null)
        {
            Soltar();
        }

        if (_restaurando)
        {
            FinalizarRestauracion();
        }
    }

    public void RegistrarContacto(PlayerMovement jugador)
    {
        _ultimoContacto = Time.time;

        if (_jugador == jugador) return;

        _jugador = jugador;
        enabled = true;
        jugador.OnLiane();

        if (objetoLiana != null)
        {
            objetoLiana.SetParent(_transform, true);
            _restaurando = false;
            _transformJugador = jugador.transform;
            _offsetY = objetoLiana.localPosition.y - _transform.InverseTransformPoint(_transformJugador.position).y;
        }

        if (!_lianaQuieta)
        {
            jugador._enLianaMovil = true;
            _posicionAnterior = _transform.position;
        }
    }

    public void Soltar()
    {
        PlayerMovement jugador = _jugador;
        if (jugador == null) return;

        _jugador = null;
        jugador.OffLiane();

        if (objetoLiana != null && objetoLiana.parent == _transform)
        {
            objetoLiana.SetParent(_padreOriginal, true);
            _restaurando = true;
        }

        if (_lianaQuieta) return;

        jugador._enLianaMovil = false;
        jugador.RecibirImpulsoLiana(jugador._velocidadLiana);
        jugador._velocidadLiana = Vector3.zero;
    }

    private void AcomodarObjeto(float dt)
    {
        if (objetoLiana.parent != _transform) return;

        float yJugador = _transform.InverseTransformPoint(_transformJugador.position).y;
        Vector3 destino = new Vector3(_posicionX, yJugador + _offsetY, _posicionZ);
        float paso = 1f - Mathf.Exp(-_velocidadObjeto * dt);

        Vector3 nueva = Vector3.Lerp(objetoLiana.localPosition, destino, paso);
        nueva.y = destino.y;
        objetoLiana.localPosition = nueva;
    }

    private void RestaurarObjeto(float dt)
    {
        if (objetoLiana.parent != _padreOriginal)
        {
            _restaurando = false;
            return;
        }

        float paso = 1f - Mathf.Exp(-_velocidadObjeto * dt);
        Vector3 pos = Vector3.Lerp(objetoLiana.localPosition, _posLocalInicial, paso);
        Quaternion rot = Quaternion.Slerp(objetoLiana.localRotation, _rotLocalInicial, paso);

        if ((pos - _posLocalInicial).sqrMagnitude < 0.0001f && Quaternion.Angle(rot, _rotLocalInicial) < 0.1f)
        {
            pos = _posLocalInicial;
            rot = _rotLocalInicial;
            _restaurando = false;
        }

        objetoLiana.localPosition = pos;
        objetoLiana.localRotation = rot;
    }

    private void FinalizarRestauracion()
    {
        _restaurando = false;

        if (objetoLiana == null || objetoLiana.parent != _padreOriginal) return;

        objetoLiana.localPosition = _posLocalInicial;
        objetoLiana.localRotation = _rotLocalInicial;
    }
}