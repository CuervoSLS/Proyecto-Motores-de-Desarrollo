using UnityEngine;

public class Liane : MonoBehaviour
{
    public enum EjeTransform { X, Y, Z }

    [SerializeField] private bool _lianaQuieta = true;

    [SerializeField] private bool _mover = false;
    [SerializeField] private EjeTransform _ejeMovimiento = EjeTransform.Y;
    [SerializeField] private float _movimientoMin = 0f;
    [SerializeField] private float _movimientoMax = 3f;
    [SerializeField] private float _velocidadMovimiento = 2f;

    [SerializeField] private bool _rotar = false;
    [SerializeField] private EjeTransform _ejeRotacion = EjeTransform.Y;
    [SerializeField] private float _rotacionMin = 0f;
    [SerializeField] private float _rotacionMax = 45f;
    [SerializeField] private float _velocidadRotacion = 30f;

    private Vector3 _posicionInicial;
    private Quaternion _rotacionInicial;
    private Vector3 _posicionAnterior;
    private PlayerMovement _jugadorEnLiana;
    private Vector3 _offsetLocalJugador;
    private Vector3 _posicionAnteriorJugador;

    void Start()
    {
        _posicionInicial = transform.position;
        _rotacionInicial = transform.rotation;
        _posicionAnterior = transform.position;
    }

    void Update()
    {
        if (_lianaQuieta == false)
        {
            if (_mover == true)
            {
                float rango = _movimientoMax - _movimientoMin;
                float offset = _movimientoMin + Mathf.PingPong(Time.time * _velocidadMovimiento, rango);
                Vector3 direccion = ObtenerDireccion(_ejeMovimiento);
                transform.position = _posicionInicial + direccion * offset;
            }

            if (_rotar == true)
            {
                float rangoRotacion = _rotacionMax - _rotacionMin;
                float anguloOffset = _rotacionMin + Mathf.PingPong(Time.time * _velocidadRotacion, rangoRotacion);
                Vector3 ejeRotacion = ObtenerDireccion(_ejeRotacion);
                transform.rotation = _rotacionInicial * Quaternion.AngleAxis(anguloOffset, ejeRotacion);
            }

            if (_jugadorEnLiana != null)
            {
                Vector3 nuevaPosicionJugador = transform.TransformPoint(_offsetLocalJugador);
                Vector3 velocidadActual = (nuevaPosicionJugador - _posicionAnteriorJugador) / Time.deltaTime;
                _jugadorEnLiana._velocidadLiana = velocidadActual;
                _posicionAnteriorJugador = nuevaPosicionJugador;
            }
        }

        _posicionAnterior = transform.position;
    }

    private Vector3 ObtenerDireccion(EjeTransform eje)
    {
        if (eje == EjeTransform.X)
        {
            return Vector3.right;
        }
        else if (eje == EjeTransform.Y)
        {
            return Vector3.up;
        }
        else
        {
            return Vector3.forward;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement controller = other.gameObject.GetComponent<PlayerMovement>();
            controller.OnLiane();

            if (_lianaQuieta == false)
            {
                _jugadorEnLiana = controller;
                controller._enLianaMovil = true;
                _offsetLocalJugador = transform.InverseTransformPoint(other.transform.position);
                _posicionAnteriorJugador = other.transform.position;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerMovement controller = other.gameObject.GetComponent<PlayerMovement>();
            controller.Invoke("OffLiane", 0);

            if (controller == _jugadorEnLiana)
            {
                controller._enLianaMovil = false;
                controller.RecibirImpulsoLiana(controller._velocidadLiana);
                controller._velocidadLiana = Vector3.zero;
                _jugadorEnLiana = null;
            }
        }
    }
}