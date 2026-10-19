using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class Cuerda : MonoBehaviour
{
    public static readonly List<Cuerda> Todas = new List<Cuerda>();

    private const float PasoMaximo = 0.033f;
    private const float VelocidadGiroBloque = 12f;
    private const float Epsilon = 0.0001f;
    private const float VelocidadRelajacion = 8f;

    [Header("Cuerda")]
    [SerializeField] private Transform _anclaje;
    [SerializeField] private float _longitud = 8f;
    [SerializeField] private int _segmentos = 16;
    [SerializeField] private float _gravedad = -20f;
    [SerializeField] private float _amortiguacion = 0.95f;
    [SerializeField] private float _tiempoEnganche = 0.12f;
    [SerializeField] private float _anguloMaximo = 25f;
    [SerializeField] private float _rigidezAngulo = 0.6f;
    [SerializeField] private int _iteraciones = 10;
    [SerializeField] private float _radioAgarre = 0.8f;
    [SerializeField] private float _largoMinimoAgarre = 1.5f;
    [SerializeField, Min(1)] private int _segmentosAgarrados = 3;
    [SerializeField] private float _velocidadMaxima = 30f;

    [Header("Malla")]
    [SerializeField] private float _radio = 0.05f;
    [SerializeField] private int _lados = 8;
    [SerializeField] private float _repeticionTextura = 0.5f;

    [Header("Colision")]
    [SerializeField] private bool _usarColision = true;
    [SerializeField] private float _radioColision = 0.08f;
    [SerializeField] private string _nombreCapa = "Cuerda";

    private Transform _transform;
    private Vector3[] _pos;
    private Vector3[] _prev;
    private Vector3[] _inicioBloque;
    private Vector3[] _pinBloque;
    private float _largoSegmento;
    private float _largoPreBloque;
    private int _indiceAgarre = -1;
    private int _bloqueInicio = -1;
    private int _bloqueFin = -2;
    private Vector3 _posAgarre;
    private Vector3 _dirBloque;
    private float _tEnganche;
    private Transform _puntoAgarre;
    private bool _ejeX = true;
    private bool _ejeZ = true;
    private float _yawPunto;
    private int _zonaPre;

    private Mesh _mesh;
    private Vector3[] _vertices;
    private Vector3[] _normales;
    private Vector2[] _uvs;
    private int[] _triangulos;
    private float[] _cos;
    private float[] _sin;

    private CapsuleCollider[] _capsulas;
    private Transform[] _transCapsulas;
    private float[] _alturas;

    public Vector3 Anclaje => _anclaje != null ? _anclaje.position : _transform.position;

    private void Awake()
    {
        _transform = transform;
        _puntoAgarre = new GameObject("PuntoAgarre").transform;
        _segmentos = Mathf.Max(2, _segmentos);
        _lados = Mathf.Max(3, _lados);

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        int nodos = _segmentos + 1;
        _largoSegmento = _longitud / _segmentos;
        _largoPreBloque = _largoSegmento;
        _pos = new Vector3[nodos];
        _prev = new Vector3[nodos];
        _inicioBloque = new Vector3[nodos];
        _pinBloque = new Vector3[nodos];

        Vector3 origen = Anclaje;
        for (int i = 0; i < nodos; i++)
        {
            _pos[i] = origen + Vector3.down * (_largoSegmento * i);
            _prev[i] = _pos[i];
        }

        _cos = new float[_lados + 1];
        _sin = new float[_lados + 1];
        for (int j = 0; j <= _lados; j++)
        {
            float angulo = (float)j / _lados * Mathf.PI * 2f;
            _cos[j] = Mathf.Cos(angulo);
            _sin[j] = Mathf.Sin(angulo);
        }

        CrearMalla(nodos);
        if (_usarColision) CrearColliders();

        ActualizarMalla();
        if (_usarColision) ActualizarColliders();
    }

    private void OnEnable()
    {
        Todas.Add(this);
    }

    private void OnDisable()
    {
        Todas.Remove(this);
    }

    private void OnDestroy()
    {
        if (_mesh != null) Destroy(_mesh);

        if (_puntoAgarre != null)
        {
            _puntoAgarre.DetachChildren();
            Destroy(_puntoAgarre.gameObject);
        }
    }

    private void CrearMalla(int nodos)
    {
        int anillo = _lados + 1;
        int baseTapa = nodos * anillo;
        int totalVertices = baseTapa + anillo + 1;

        _vertices = new Vector3[totalVertices];
        _normales = new Vector3[totalVertices];
        _uvs = new Vector2[totalVertices];
        _triangulos = new int[(nodos - 1) * _lados * 6 + _lados * 3];

        int t = 0;
        for (int i = 0; i < nodos - 1; i++)
        {
            for (int j = 0; j < _lados; j++)
            {
                int a = i * anillo + j;
                int b = a + 1;
                int c = a + anillo;
                int d = c + 1;

                _triangulos[t++] = a;
                _triangulos[t++] = b;
                _triangulos[t++] = c;

                _triangulos[t++] = b;
                _triangulos[t++] = d;
                _triangulos[t++] = c;
            }
        }

        for (int j = 0; j < _lados; j++)
        {
            _triangulos[t++] = baseTapa + anillo;
            _triangulos[t++] = baseTapa + j;
            _triangulos[t++] = baseTapa + j + 1;
        }

        float repeticion = Mathf.Max(0.01f, _repeticionTextura);
        for (int i = 0; i < nodos; i++)
        {
            float v = i * _largoSegmento / repeticion;
            for (int j = 0; j <= _lados; j++)
            {
                _uvs[i * anillo + j] = new Vector2((float)j / _lados, v);
            }
        }

        float vFinal = (nodos - 1) * _largoSegmento / repeticion;
        float escalaTapa = _radio / repeticion;
        for (int j = 0; j <= _lados; j++)
        {
            _uvs[baseTapa + j] = new Vector2(0.5f + 0.5f * _cos[j], vFinal + _sin[j] * escalaTapa);
        }
        _uvs[baseTapa + anillo] = new Vector2(0.5f, vFinal);

        _mesh = new Mesh();
        _mesh.name = "CuerdaMesh";
        _mesh.MarkDynamic();
        _mesh.vertices = _vertices;
        _mesh.normals = _normales;
        _mesh.uv = _uvs;
        _mesh.triangles = _triangulos;

        GetComponent<MeshFilter>().sharedMesh = _mesh;
    }

    private void CrearColliders()
    {
        int capa = LayerMask.NameToLayer(_nombreCapa);
        if (capa < 0) capa = gameObject.layer;

        _capsulas = new CapsuleCollider[_segmentos];
        _transCapsulas = new Transform[_segmentos];
        _alturas = new float[_segmentos];

        float altura = _largoSegmento + _radioColision * 2f;
        for (int i = 0; i < _segmentos; i++)
        {
            GameObject segmento = new GameObject("Segmento_" + i);
            segmento.layer = capa;
            segmento.transform.SetParent(_transform, false);

            CapsuleCollider capsula = segmento.AddComponent<CapsuleCollider>();
            capsula.direction = 1;
            capsula.radius = _radioColision;
            capsula.height = altura;
            capsula.isTrigger = false;

            _capsulas[i] = capsula;
            _transCapsulas[i] = segmento.transform;
            _alturas[i] = altura;
        }
    }

    public void IgnorarColision(Collider otro, bool ignorar)
    {
        if (_capsulas == null || otro == null) return;

        for (int i = 0; i < _capsulas.Length; i++)
        {
            Physics.IgnoreCollision(otro, _capsulas[i], ignorar);
        }
    }

    public bool TryAgarrar(Vector3 punto, out int indice)
    {
        indice = -1;
        float mejor = _radioAgarre * _radioAgarre;

        for (int i = 1; i < _pos.Length; i++)
        {
            if (_largoSegmento * i < _largoMinimoAgarre) continue;

            float d = (_pos[i] - punto).sqrMagnitude;
            if (d <= mejor)
            {
                mejor = d;
                indice = i;
            }
        }

        return indice >= 0;
    }

    public void Agarrar(int indice, float largo, Collider jugador)
    {
        int ultimo = _pos.Length - 1;
        int cantidad = Mathf.Clamp(_segmentosAgarrados, 1, ultimo);

        int inicio = Mathf.Clamp(indice, 1, ultimo);

        _indiceAgarre = indice;
        _bloqueInicio = inicio;
        _bloqueFin = Mathf.Min(ultimo, inicio + cantidad - 1);
        _posAgarre = _pos[indice];
        _tEnganche = 0f;

        float largoAntes = Mathf.Max(0.01f, largo - (indice - inicio) * _largoSegmento);
        _largoPreBloque = largoAntes / inicio;
        _zonaPre = inicio;

        for (int k = _bloqueInicio; k <= _bloqueFin; k++)
        {
            _inicioBloque[k] = _pos[k];
            _pinBloque[k] = _pos[k];
        }

        Vector3 dir = _pos[_bloqueFin] - _pos[_bloqueInicio - 1];
        _dirBloque = dir.sqrMagnitude > Epsilon ? dir.normalized : Vector3.down;

        ActualizarPuntoAgarre();
        IgnorarColision(jugador, true);
    }

    public void ActualizarAgarre(Vector3 posicion)
    {
        _posAgarre = posicion;
    }

    public bool Agarrada => _indiceAgarre >= 0;

    public Transform PuntoAgarre => _puntoAgarre;

    public void ConfigurarEjes(bool x, bool z, float yaw)
    {
        _ejeX = x;
        _ejeZ = z;
        _yawPunto = yaw;
        if (_indiceAgarre >= 0) ActualizarPuntoAgarre();
    }

    private void ActualizarPuntoAgarre()
    {
        Vector3 euler = Quaternion.FromToRotation(Vector3.up, -_dirBloque).eulerAngles;
        Quaternion rotacion = Quaternion.Euler(_ejeX ? euler.x : 0f, 0f, _ejeZ ? euler.z : 0f) * Quaternion.Euler(0f, _yawPunto, 0f);
        _puntoAgarre.SetPositionAndRotation(_pos[_indiceAgarre], rotacion);
    }

    public void Soltar(Vector3 velocidad)
    {
        if (_indiceAgarre < 0) return;

        Vector3 desplazamiento = Vector3.ClampMagnitude(velocidad, _velocidadMaxima) * Time.deltaTime;
        for (int k = _bloqueInicio; k <= _bloqueFin; k++)
        {
            _prev[k] = _pos[k] - desplazamiento;
        }

        _indiceAgarre = -1;
        _bloqueInicio = -1;
        _bloqueFin = -2;
    }

    private bool EnBloque(int i)
    {
        return i >= _bloqueInicio && i <= _bloqueFin;
    }

    private void LateUpdate()
    {
        Simular();
        if (_indiceAgarre >= 0) ActualizarPuntoAgarre();
        ActualizarMalla();
        if (_usarColision) ActualizarColliders();
    }

    private void ActualizarBloque(float dt, Vector3 ancla)
    {
        _tEnganche += dt;
        float w = Mathf.Clamp01(_tEnganche / Mathf.Max(0.001f, _tiempoEnganche));
        float s = w * w * (3f - 2f * w);

        Vector3 haciaMano = _posAgarre - ancla;
        if (haciaMano.sqrMagnitude > 0.01f)
        {
            float giro = 1f - Mathf.Exp(-VelocidadGiroBloque * dt);
            _dirBloque = Vector3.Slerp(_dirBloque, haciaMano.normalized, giro).normalized;
        }

        for (int k = _bloqueInicio; k <= _bloqueFin; k++)
        {
            Vector3 objetivo = _posAgarre + _dirBloque * (_largoSegmento * (k - _indiceAgarre));
            _pinBloque[k] = Vector3.Lerp(_inicioBloque[k], objetivo, s);
        }
    }

    private void Simular()
    {
        float dt = Mathf.Min(Time.deltaTime, PasoMaximo);
        bool agarrada = _indiceAgarre >= 0;
        float factor = Mathf.Pow(_amortiguacion, dt * 60f);
        Vector3 impulsoGravedad = Vector3.up * (_gravedad * dt * dt);
        Vector3 ancla = Anclaje;
        int nodos = _pos.Length;
        bool limitar = _anguloMaximo < 179f;
        float cosMaximo = Mathf.Cos(_anguloMaximo * Mathf.Deg2Rad);

        float desplazamientoMaximo = _velocidadMaxima * dt;
        float desplazamientoMaximoCuadrado = desplazamientoMaximo * desplazamientoMaximo;

        if (agarrada) ActualizarBloque(dt, ancla);
        else if (_zonaPre > 0)
        {
            _largoPreBloque = Mathf.Lerp(_largoPreBloque, _largoSegmento, 1f - Mathf.Exp(-VelocidadRelajacion * dt));
            if (Mathf.Abs(_largoPreBloque - _largoSegmento) < 0.001f)
            {
                _largoPreBloque = _largoSegmento;
                _zonaPre = 0;
            }
        }

        for (int i = 1; i < nodos; i++)
        {
            if (EnBloque(i))
            {
                i = _bloqueFin;
                continue;
            }

            Vector3 vel = (_pos[i] - _prev[i]) * factor;
            if (vel.sqrMagnitude > desplazamientoMaximoCuadrado) vel = vel.normalized * desplazamientoMaximo;
            _prev[i] = _pos[i];
            _pos[i] += vel + impulsoGravedad;
        }

        for (int it = 0; it < _iteraciones; it++)
        {
            _pos[0] = ancla;
            for (int k = _bloqueInicio; k <= _bloqueFin; k++)
            {
                _pos[k] = _pinBloque[k];
            }

            for (int i = 0; i < nodos - 1; i++)
            {
                bool fijoA = i == 0 || EnBloque(i);
                bool fijoB = EnBloque(i + 1);
                if (fijoA && fijoB) continue;

                float largo = i < _zonaPre ? _largoPreBloque : _largoSegmento;

                Vector3 delta = _pos[i + 1] - _pos[i];
                float dist = delta.magnitude;
                if (dist < Epsilon) continue;

                Vector3 correccion = delta * ((dist - largo) / dist);

                if (fijoA) _pos[i + 1] -= correccion;
                else if (fijoB) _pos[i] += correccion;
                else
                {
                    correccion *= 0.5f;
                    _pos[i] += correccion;
                    _pos[i + 1] -= correccion;
                }
            }

            if (limitar) LimitarAngulos(cosMaximo);
        }

        for (int k = _bloqueInicio; k <= _bloqueFin; k++)
        {
            _prev[k] = _pos[k];
        }
    }

    private void LimitarAngulos(float cosMaximo)
    {
        float anguloRad = _anguloMaximo * Mathf.Deg2Rad;

        for (int i = 0; i < _pos.Length - 2; i++)
        {
            Vector3 d1 = _pos[i + 1] - _pos[i];
            Vector3 d2 = _pos[i + 2] - _pos[i + 1];
            float l1 = d1.magnitude;
            float l2 = d2.magnitude;
            if (l1 < Epsilon || l2 < Epsilon) continue;

            d1 /= l1;
            d2 /= l2;

            if (Vector3.Dot(d1, d2) >= cosMaximo) continue;

            Vector3 limitada = Vector3.RotateTowards(d1, d2, anguloRad, 0f);
            Vector3 correccion = (_pos[i + 1] + limitada * l2 - _pos[i + 2]) * _rigidezAngulo;

            if (!EnBloque(i + 2)) _pos[i + 2] += correccion;
            else if (!EnBloque(i + 1)) _pos[i + 1] -= correccion * 0.5f;
        }
    }

    private Vector3 Tangente(int i)
    {
        Vector3 t;
        if (i == 0) t = _pos[1] - _pos[0];
        else if (i == _pos.Length - 1) t = _pos[i] - _pos[i - 1];
        else t = _pos[i + 1] - _pos[i - 1];

        if (t.sqrMagnitude < 0.000001f) return Vector3.down;
        return t.normalized;
    }

    private void ActualizarMalla()
    {
        int anillo = _lados + 1;
        Matrix4x4 aLocal = _transform.worldToLocalMatrix;
        Quaternion rotInversa = Quaternion.Inverse(_transform.rotation);

        Vector3 tangente = Tangente(0);
        Vector3 normal = Vector3.Cross(tangente, Vector3.right);
        if (normal.sqrMagnitude < 0.001f) normal = Vector3.Cross(tangente, Vector3.forward);
        normal.Normalize();

        Vector3 anterior = tangente;
        Vector3 centro = Vector3.zero;

        for (int i = 0; i < _pos.Length; i++)
        {
            tangente = Tangente(i);
            normal = (Quaternion.FromToRotation(anterior, tangente) * normal).normalized;
            anterior = tangente;

            Vector3 binormal = Vector3.Cross(tangente, normal);

            centro = aLocal.MultiplyPoint3x4(_pos[i]);
            Vector3 normalLocal = aLocal.MultiplyVector(normal) * _radio;
            Vector3 binormalLocal = aLocal.MultiplyVector(binormal) * _radio;
            Vector3 normalRot = rotInversa * normal;
            Vector3 binormalRot = rotInversa * binormal;

            int baseIdx = i * anillo;
            for (int j = 0; j <= _lados; j++)
            {
                float c = _cos[j];
                float s = _sin[j];

                _vertices[baseIdx + j] = centro + normalLocal * c + binormalLocal * s;
                _normales[baseIdx + j] = normalRot * c + binormalRot * s;
            }
        }

        int baseTapa = _pos.Length * anillo;
        int baseUltimo = (_pos.Length - 1) * anillo;
        Vector3 normalTapa = rotInversa * tangente;

        for (int j = 0; j <= _lados; j++)
        {
            _vertices[baseTapa + j] = _vertices[baseUltimo + j];
            _normales[baseTapa + j] = normalTapa;
        }

        _vertices[baseTapa + anillo] = centro;
        _normales[baseTapa + anillo] = normalTapa;

        _mesh.SetVertices(_vertices);
        _mesh.SetNormals(_normales);
        _mesh.RecalculateBounds();
    }

    private void ActualizarColliders()
    {
        for (int i = 0; i < _capsulas.Length; i++)
        {
            Vector3 a = _pos[i];
            Vector3 b = _pos[i + 1];
            Vector3 d = b - a;
            float largo = d.magnitude;
            if (largo < Epsilon) continue;

            _transCapsulas[i].SetPositionAndRotation((a + b) * 0.5f, Quaternion.FromToRotation(Vector3.up, d / largo));

            float altura = largo + _radioColision * 2f;
            if (Mathf.Abs(altura - _alturas[i]) > 0.001f)
            {
                _alturas[i] = altura;
                _capsulas[i].height = altura;
            }
        }
    }
}