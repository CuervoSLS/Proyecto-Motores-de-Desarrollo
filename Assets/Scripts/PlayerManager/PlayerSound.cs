using UnityEngine;

[System.Serializable]
public class GrupoSonido
{
    public AudioClip[] clips;
    [Range(0f, 1f)] public float volumen = 1f;

    public bool TieneClips()
    {
        return clips != null && clips.Length > 0;
    }

    public AudioClip Elegir()
    {
        if (!TieneClips()) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}

[RequireComponent(typeof(PlayerMovement))]
public class PlayerSound : MonoBehaviour
{
    [Header("Fuentes de audio")]
    [SerializeField] private AudioSource _fuentePasos;
    [SerializeField] private AudioSource _fuenteEfectos;

    [Header("Deteccion de superficie")]
    [SerializeField] private string _nombreLayerTierra = "PisoTierra";
    [SerializeField] private string _nombreLayerCesped = "PisoCesped";
    [SerializeField] private string _nombreLayerParedWallRun = "ParedWallRun";
    [SerializeField] private bool _depurar = true;

    [Header("Pasos en tierra")]
    [SerializeField] private GrupoSonido _caminarTierra;
    [SerializeField] private GrupoSonido _correrTierra;

    [Header("Pasos en cesped")]
    [SerializeField] private GrupoSonido _caminarCesped;
    [SerializeField] private GrupoSonido _correrCesped;

    [Header("Pasos")]
    [SerializeField] private float _intervaloCaminar = 0.5f;
    [SerializeField] private float _intervaloCorrer = 0.3f;
    [SerializeField] private float _variacionPitchPasos = 0.1f;

    [Header("Wall run")]
    [SerializeField] private GrupoSonido _wallRunPiedra;
    [SerializeField] private float _intervaloWallRun = 0.35f;

    [Header("Salto y dash")]
    [SerializeField] private GrupoSonido _saltar;
    [SerializeField] private GrupoSonido _dash;

    [Header("Liana")]
    [SerializeField] private GrupoSonido _subirLiana;
    [SerializeField] private float _distanciaPorSonido = 0.6f;

    private PlayerMovement _p;
    private float _temporizadorPaso;
    private float _temporizadorWallRun;
    private bool _enLianaAnterior;
    private bool _dashAnterior;
    private float _ultimaAltura;
    private float _alturaAcumulada;
    private int _capaTierra = -1;
    private int _capaCesped = -1;
    private int _capaParedWallRun = -1;
    private string _ultimaClaveDebug = "";

    private void Start()
    {
        _p = GetComponent<PlayerMovement>();

        if (_fuentePasos == null) _fuentePasos = CrearFuente();
        if (_fuenteEfectos == null) _fuenteEfectos = CrearFuente();

        _capaTierra = LayerMask.NameToLayer(_nombreLayerTierra);
        _capaCesped = LayerMask.NameToLayer(_nombreLayerCesped);
        _capaParedWallRun = LayerMask.NameToLayer(_nombreLayerParedWallRun);

        if (_capaTierra < 0) Debug.LogWarning("PlayerSound: no existe el layer " + _nombreLayerTierra, this);
        if (_capaCesped < 0) Debug.LogWarning("PlayerSound: no existe el layer " + _nombreLayerCesped, this);
        if (_capaParedWallRun < 0) Debug.LogWarning("PlayerSound: no existe el layer " + _nombreLayerParedWallRun, this);

        _p.Jump.Saltado += AlSaltar;
        _ultimaAltura = transform.position.y;
    }

    private void OnDestroy()
    {
        if (_p != null && _p.Jump != null) _p.Jump.Saltado -= AlSaltar;
    }

    private void LateUpdate()
    {
        if (_p == null) return;

        bool enLiana = _p.Liana.EnLiana;
        bool enCuerda = _p.Rope.EnCuerda;
        bool dashing = _p.Dash.Dashing;

        DetectarDash(dashing);
        DetectarAlturaLiana(enLiana);
        ActualizarPasos(enLiana, enCuerda, dashing);
        ActualizarWallRun();

        _enLianaAnterior = enLiana;
        _dashAnterior = dashing;
        _ultimaAltura = transform.position.y;
    }

    private void DetectarDash(bool dashing)
    {
        if (dashing && !_dashAnterior)
        {
            Reproducir(_dash, _fuenteEfectos);
        }
    }

    private void DetectarAlturaLiana(bool enLiana)
    {
        if (!enLiana || !_enLianaAnterior)
        {
            _alturaAcumulada = 0f;
            return;
        }

        _alturaAcumulada += transform.position.y - _ultimaAltura;

        if (_alturaAcumulada >= _distanciaPorSonido)
        {
            _alturaAcumulada = 0f;
            Reproducir(_subirLiana, _fuenteEfectos);
        }
        else if (_alturaAcumulada <= -_distanciaPorSonido)
        {
            _alturaAcumulada = 0f;
        }
    }

    private void ActualizarPasos(bool enLiana, bool enCuerda, bool dashing)
    {
        bool moviendo = _p.Move.sqrMagnitude > 0.01f;
        bool enSuelo = _p.Ground.Grounded && !enLiana && !enCuerda && !dashing && !_p.Slide.Sliding && !_p.WallRun.WallRunning;

        if (!moviendo || !enSuelo)
        {
            _temporizadorPaso = 0f;
            return;
        }

        _temporizadorPaso -= Time.deltaTime;
        if (_temporizadorPaso > 0f) return;

        bool corriendo = _p.Sprint.Sprinting;
        _temporizadorPaso = corriendo ? _intervaloCorrer : _intervaloCaminar;

        GrupoSonido grupo = ElegirPasos(corriendo);
        if (grupo == null) return;

        _fuentePasos.pitch = 1f + Random.Range(-_variacionPitchPasos, _variacionPitchPasos);
        Reproducir(grupo, _fuentePasos);
    }

    private void ActualizarWallRun()
    {
        if (!_p.WallRun.WallRunning)
        {
            _temporizadorWallRun = 0f;
            return;
        }

        _temporizadorWallRun -= Time.deltaTime;
        if (_temporizadorWallRun > 0f) return;

        _temporizadorWallRun = _intervaloWallRun;

        Collider pared = _p.WallRun.Pared;
        int capa = pared != null ? pared.gameObject.layer : -1;
        GrupoSonido grupo = capa >= 0 && capa == _capaParedWallRun ? _wallRunPiedra : null;

        if (_depurar) Depurar("wallrun", capa, grupo);

        if (grupo == null) return;

        _fuentePasos.pitch = 1f + Random.Range(-_variacionPitchPasos, _variacionPitchPasos);
        Reproducir(grupo, _fuentePasos);
    }

    private GrupoSonido ElegirPasos(bool corriendo)
    {
        int capa = CapaDelPiso();
        GrupoSonido grupo = null;

        if (capa >= 0)
        {
            if (capa == _capaTierra) grupo = corriendo ? _correrTierra : _caminarTierra;
            else if (capa == _capaCesped) grupo = corriendo ? _correrCesped : _caminarCesped;
        }

        if (_depurar) Depurar("pasos", capa, grupo);

        return grupo;
    }

    private int CapaDelPiso()
    {
        Collider suelo = _p.Ground.Suelo;
        return suelo != null ? suelo.gameObject.layer : -1;
    }

    private void Depurar(string contexto, int capa, GrupoSonido grupo)
    {
        string nombreCapa = capa >= 0 ? LayerMask.LayerToName(capa) : "ninguno";
        string estado;

        if (capa < 0) estado = "no se detecto ningun collider";
        else if (grupo == null) estado = "el layer no corresponde a ningun sonido";
        else if (!grupo.TieneClips()) estado = "el grupo de sonido no tiene clips asignados";
        else estado = "ok";

        string clave = contexto + "|" + nombreCapa + "|" + estado;
        if (clave == _ultimaClaveDebug) return;
        _ultimaClaveDebug = clave;

        Debug.Log("PlayerSound " + contexto + " -> layer: " + nombreCapa + " | " + estado, this);
    }

    private void AlSaltar()
    {
        Reproducir(_saltar, _fuenteEfectos);
    }

    private void Reproducir(GrupoSonido grupo, AudioSource fuente)
    {
        if (grupo == null || fuente == null) return;

        AudioClip clip = grupo.Elegir();
        if (clip == null) return;

        fuente.PlayOneShot(clip, grupo.volumen);
    }

    private AudioSource CrearFuente()
    {
        AudioSource fuente = gameObject.AddComponent<AudioSource>();
        fuente.playOnAwake = false;
        return fuente;
    }
}