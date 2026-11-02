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

    private Transform _padre;
    private float _tiempoRestante;
    private AgarrarGancho _jugador;
    private bool _volando;
    private float _tiempoVuelo;
    private Rigidbody _rb;
    private bool _cuerpoEraTrigger;

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
        if (Agarrado) return;

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
        if (Agarrado || padre == null) return;

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
    }

    private void Update()
    {
        if (_volando)
        {
            _tiempoVuelo += Time.deltaTime;
            if (_tiempoVuelo >= _tiempoMaximoVuelo || (_tiempoVuelo > TiempoMinimoReposo && _rb.IsSleeping())) Detener();
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

        if ((_capasPegar.value & (1 << colision.gameObject.layer)) == 0) return;

        Pegado = true;
        Detener();
        transform.SetParent(colision.transform, true);
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