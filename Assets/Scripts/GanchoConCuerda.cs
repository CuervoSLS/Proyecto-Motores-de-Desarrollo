using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class GanchoConCuerda : MonoBehaviour
{
    private const float TiempoMinimoReposo = 0.25f;

    [Header("Referencias")]
    [SerializeField] private Cuerda _cuerda;
    [SerializeField] private Collider _colliderAgarre;
    [SerializeField] private Collider _colliderCuerpo;

    [Header("Posicion cuando es hijo de la mano")]
    public Vector3 posicionEnMano;
    public Vector3 rotacionEnMano;

    [Header("Vuelo")]
    [SerializeField] private LayerMask _capasPegar = ~0;
    [SerializeField] private float _tiempoMaximoVuelo = 6f;

    [Header("Reinicio al tocar otras capas")]
    [SerializeField] private LayerMask _capasSinReinicio;
    [SerializeField] private float _delayReinicio = 3f;
    [SerializeField] private float _duracionAparicion = 2f;

    private Transform _padre;
    private float _tiempoRestante;
    private AgarrarGancho _jugador;
    private bool _volando;
    private float _tiempoVuelo;
    private Rigidbody _rb;
    private bool _cuerpoEraTrigger;
    private bool _reiniciando;
    private Transform _padreInicial;
    private Vector3 _posLocalInicial;
    private Quaternion _rotLocalInicial;
    private Vector3 _escalaLocalInicial;
    private Vector3 _escalaMundoInicial;
    private FundidoAparicion _fundido;

    public bool Agarrado { get; private set; }
    public bool Volando => _volando;
    public bool Pegado { get; private set; }
    public bool JugadorDentro => _jugador != null;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
        GetComponent<Rigidbody>().isKinematic = true;
    }

    private void Awake()
    {
        if (_cuerda == null) _cuerda = GetComponentInChildren<Cuerda>();
        if (_cuerda == null) _cuerda = GetComponentInParent<Cuerda>();

        ResolverColliders();
        _rb = GetComponent<Rigidbody>();
        _rb.isKinematic = true;
        _rb.useGravity = false;

        _padreInicial = transform.parent;
        _posLocalInicial = transform.localPosition;
        _rotLocalInicial = transform.localRotation;
        _escalaLocalInicial = transform.localScale;
        _escalaMundoInicial = transform.lossyScale;
        if (!TryGetComponent(out _fundido)) _fundido = gameObject.AddComponent<FundidoAparicion>();

        enabled = false;
    }

    private void ResolverColliders()
    {
        Collider[] todos = GetComponents<Collider>();

        if (_colliderAgarre == null && todos.Length > 1)
        {
            float mayor = -1f;
            for (int i = 0; i < todos.Length; i++)
            {
                float volumen = todos[i].bounds.size.sqrMagnitude;
                if (volumen > mayor)
                {
                    mayor = volumen;
                    _colliderAgarre = todos[i];
                }
            }
        }

        if (_colliderCuerpo == null)
        {
            for (int i = 0; i < todos.Length; i++)
            {
                if (todos[i] != _colliderAgarre)
                {
                    _colliderCuerpo = todos[i];
                    break;
                }
            }
        }

        if (_colliderCuerpo == null) _colliderCuerpo = _colliderAgarre != null ? _colliderAgarre : todos[0];
        if (_colliderAgarre == null) _colliderAgarre = _colliderCuerpo;

        _cuerpoEraTrigger = _colliderCuerpo.isTrigger;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Agarrado || _reiniciando) return;

        AgarrarGancho jugador = other.GetComponentInParent<AgarrarGancho>();
        if (jugador == null) return;

        _jugador = jugador;
        jugador.Cercano = this;
    }

    private void OnTriggerExit(Collider other)
    {
        if (_jugador == null) return;

        AgarrarGancho jugador = other.GetComponentInParent<AgarrarGancho>();
        if (jugador != _jugador) return;

        if (jugador.Cercano == this) jugador.Cercano = null;
        _jugador = null;
    }

    private void OnDisable()
    {
        if (_jugador != null && _jugador.Cercano == this) _jugador.Cercano = null;
    }

    public void Agarrar(Transform padre, float delay)
    {
        if (Agarrado || padre == null || _reiniciando) return;

        Agarrado = true;
        _volando = false;
        if (_cuerda != null) _cuerda.Bloqueada = true;

        if (_jugador != null && _jugador.Cercano == this) _jugador.Cercano = null;
        _jugador = null;

        if (delay <= 0f)
        {
            Emparentar(padre);
            return;
        }

        _padre = padre;
        _tiempoRestante = delay;
        enabled = true;
    }

    public void Soltar()
    {
        if (!Agarrado) return;

        enabled = false;
        _padre = null;
        transform.SetParent(null, true);
        transform.localScale = _escalaMundoInicial;
        Agarrado = false;

        if (_cuerda == null) return;

        _cuerda.Bloqueada = false;
        _cuerda.SostenerEnMano();
    }

    public void Lanzar(Vector3 velocidad, Transform ignorar)
    {
        if (!Agarrado) return;

        Soltar();

        IgnorarColisiones(ignorar);

        if (_colliderAgarre != _colliderCuerpo) _colliderAgarre.enabled = false;
        _colliderCuerpo.isTrigger = false;
        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        _rb.AddForce(velocidad, ForceMode.VelocityChange);

        Pegado = false;
        _tiempoVuelo = 0f;
        _volando = true;
        enabled = true;
    }

    private void IgnorarColisiones(Transform jugador)
    {
        if (jugador != null)
        {
            Collider[] delJugador = jugador.GetComponentsInChildren<Collider>();
            for (int i = 0; i < delJugador.Length; i++)
            {
                Physics.IgnoreCollision(_colliderCuerpo, delJugador[i], true);
            }
        }

        if (_cuerda == null) return;

        Collider[] deLaCuerda = _cuerda.GetComponentsInChildren<Collider>();
        for (int i = 0; i < deLaCuerda.Length; i++)
        {
            if (deLaCuerda[i] != _colliderCuerpo) Physics.IgnoreCollision(_colliderCuerpo, deLaCuerda[i], true);
        }
    }

    private void Emparentar(Transform padre)
    {
        transform.SetParent(padre, false);
        transform.localPosition = posicionEnMano;
        transform.localRotation = Quaternion.Euler(rotacionEnMano);

        Vector3 escalaPadre = padre.lossyScale;
        transform.localScale = new Vector3(
            escalaPadre.x != 0f ? _escalaMundoInicial.x / escalaPadre.x : _escalaMundoInicial.x,
            escalaPadre.y != 0f ? _escalaMundoInicial.y / escalaPadre.y : _escalaMundoInicial.y,
            escalaPadre.z != 0f ? _escalaMundoInicial.z / escalaPadre.z : _escalaMundoInicial.z);
    }

    private void Update()
    {
        if (_volando)
        {
            _tiempoVuelo += Time.deltaTime;
            if (_tiempoVuelo >= _tiempoMaximoVuelo || (_tiempoVuelo > TiempoMinimoReposo && _rb.IsSleeping()))
            {
                Detener();
                IniciarReinicio();
            }

            return;
        }

        _tiempoRestante -= Time.deltaTime;
        if (_tiempoRestante > 0f) return;

        if (_padre != null) Emparentar(_padre);
        _padre = null;
        enabled = false;
    }

    private void OnCollisionEnter(Collision colision)
    {
        if (!_volando) return;
        if (colision.GetContact(0).thisCollider != _colliderCuerpo) return;

        int mascara = 1 << colision.gameObject.layer;

        if ((_capasPegar.value & mascara) != 0)
        {
            Pegado = true;
            Detener();
            transform.SetParent(colision.transform, true);
            return;
        }

        if ((_capasSinReinicio.value & mascara) != 0) return;

        Detener();
        IniciarReinicio();
    }

    private void IniciarReinicio()
    {
        if (_reiniciando) return;

        _reiniciando = true;
        if (_jugador != null && _jugador.Cercano == this) _jugador.Cercano = null;
        _jugador = null;

        StartCoroutine(ReiniciarTrasDelay());
    }

    private IEnumerator ReiniciarTrasDelay()
    {
        if (_delayReinicio > 0f) yield return new WaitForSeconds(_delayReinicio);

        while (_cuerda != null && _cuerda.Agarrada) yield return null;

        transform.SetParent(_padreInicial, false);
        transform.localPosition = _posLocalInicial;
        transform.localRotation = _rotLocalInicial;
        transform.localScale = _escalaLocalInicial;
        Pegado = false;

        if (_cuerda != null)
        {
            _cuerda.Bloqueada = false;
            _cuerda.Reiniciar(_duracionAparicion);
        }

        _fundido.Iniciar(_duracionAparicion);

        _reiniciando = false;
    }

    private void Detener()
    {
        _volando = false;
        _rb.useGravity = false;
        _rb.isKinematic = true;
        _rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        _colliderCuerpo.isTrigger = _colliderAgarre == _colliderCuerpo || _cuerpoEraTrigger;
        _colliderAgarre.enabled = true;
        enabled = false;
    }
}