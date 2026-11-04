using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

[System.Serializable]
public class GrupoSonido
{
    public AudioClip[] clips;
    [Range(0f, 1f)] public float volumen = 1f;

    public bool TieneClips()
    {
        return clips != null && clips.Length > 0;
    }

    private int _ultimoMasUno;

    public AudioClip Elegir()
    {
        if (!TieneClips()) return null;

        int cantidad = clips.Length;
        if (cantidad == 1) return clips[0];

        int indice;
        if (_ultimoMasUno <= 0 || _ultimoMasUno > cantidad)
        {
            indice = Random.Range(0, cantidad);
        }
        else
        {
            indice = Random.Range(0, cantidad - 1);
            if (indice >= _ultimoMasUno - 1) indice++;
        }

        _ultimoMasUno = indice + 1;
        return clips[indice];
    }
}

[RequireComponent(typeof(PlayerMovement))]
public class PlayerSound : MonoBehaviour
{
    private const int Tierra = 0;
    private const int Cesped = 1;
    private const int Roca = 2;
    private const int Madera = 3;

    [Header("Fuentes de audio")]
    [SerializeField] private AudioSource _fuentePasos;
    [SerializeField] private AudioSource _fuenteEfectos;

    [Header("Grupos del Audio Mixer")]
    [SerializeField] private AudioMixerGroup _grupoPasos;
    [SerializeField] private AudioMixerGroup _grupoEfectos;

    [Header("Deteccion de superficie")]
    [SerializeField] private string _nombreLayerTierra = "PisoTierra";
    [SerializeField] private string _nombreLayerCesped = "PisoCesped";
    [SerializeField] private string _nombreLayerRoca = "PisoRoca";
    [SerializeField] private string _nombreLayerMadera = "PisoMadera";
    [SerializeField] private string _nombreLayerParedWallRun = "ParedWallRun";

    [Header("Pasos en tierra")]
    [SerializeField] private GrupoSonido _caminarTierra;
    [SerializeField] private GrupoSonido _correrTierra;

    [Header("Pasos en cesped")]
    [SerializeField] private GrupoSonido _caminarCesped;
    [SerializeField] private GrupoSonido _correrCesped;

    [Header("Pasos en roca (el wall run usa Correr Roca)")]
    [SerializeField] private GrupoSonido _caminarRoca;
    [SerializeField] private GrupoSonido _correrRoca;

    [Header("Pasos en madera")]
    [SerializeField] private GrupoSonido _caminarMadera;
    [SerializeField] private GrupoSonido _correrMadera;

    [Header("Pasos")]
    [SerializeField] private float _intervaloCaminar = 0.5f;
    [SerializeField] private float _intervaloCorrer = 0.3f;
    [SerializeField] private float _variacionPitchPasos = 0.1f;

    [Header("Wall run")]
    [SerializeField] private float _intervaloWallRun = 0.35f;

    [Header("Salto segun la superficie")]
    [SerializeField, FormerlySerializedAs("_saltar")] private GrupoSonido _saltarTierra;
    [SerializeField] private GrupoSonido _saltarCesped;
    [SerializeField] private GrupoSonido _saltarRoca;
    [SerializeField] private GrupoSonido _saltarMadera;

    [Header("Aterrizaje segun la superficie")]
    [SerializeField] private GrupoSonido _aterrizarTierra;
    [SerializeField] private GrupoSonido _aterrizarCesped;
    [SerializeField] private GrupoSonido _aterrizarRoca;
    [SerializeField] private GrupoSonido _aterrizarMadera;
    [SerializeField] private float _tiempoMinimoEnAire = 0.2f;

    [Header("Dash")]
    [SerializeField] private GrupoSonido _dash;

    [Header("Liana")]
    [SerializeField] private GrupoSonido _subirLiana;
    [SerializeField] private float _distanciaPorSonido = 0.6f;

    private PlayerMovement _p;
    private GrupoSonido[] _gruposCaminar;
    private GrupoSonido[] _gruposCorrer;
    private GrupoSonido[] _gruposSalto;
    private GrupoSonido[] _gruposAterrizaje;
    private float _temporizadorPaso;
    private float _temporizadorWallRun;
    private bool _siguiendoLiana;
    private bool _dashAnterior;
    private bool _enSueloAnterior;
    private float _tiempoEnAire;
    private float _ultimaAltura;
    private float _alturaAcumulada;
    private int _capaTierra = -1;
    private int _capaCesped = -1;
    private int _capaRoca = -1;
    private int _capaMadera = -1;
    private int _capaParedWallRun = -1;

    private void Start()
    {
        _p = GetComponent<PlayerMovement>();

        if (_fuentePasos == null) _fuentePasos = CrearFuente();
        if (_fuenteEfectos == null) _fuenteEfectos = CrearFuente();

        if (_grupoPasos != null) _fuentePasos.outputAudioMixerGroup = _grupoPasos;
        if (_grupoEfectos != null) _fuenteEfectos.outputAudioMixerGroup = _grupoEfectos;

        _capaTierra = LayerMask.NameToLayer(_nombreLayerTierra);
        _capaCesped = LayerMask.NameToLayer(_nombreLayerCesped);
        _capaRoca = LayerMask.NameToLayer(_nombreLayerRoca);
        _capaMadera = LayerMask.NameToLayer(_nombreLayerMadera);
        _capaParedWallRun = LayerMask.NameToLayer(_nombreLayerParedWallRun);

        _gruposCaminar = new[] { _caminarTierra, _caminarCesped, _caminarRoca, _caminarMadera };
        _gruposCorrer = new[] { _correrTierra, _correrCesped, _correrRoca, _correrMadera };
        _gruposSalto = new[] { _saltarTierra, _saltarCesped, _saltarRoca, _saltarMadera };
        _gruposAterrizaje = new[] { _aterrizarTierra, _aterrizarCesped, _aterrizarRoca, _aterrizarMadera };

        _p.Jump.Saltado += AlSaltar;
    }

    private void OnDestroy()
    {
        if (_p != null && _p.Jump != null) _p.Jump.Saltado -= AlSaltar;
    }

    private void LateUpdate()
    {
        if (_p == null) return;

        bool enSuelo = _p.Ground.Grounded;
        bool enLiana = _p.Liana.EnLiana;
        bool enCuerda = _p.Rope.EnCuerda;
        bool dashing = _p.Dash.Dashing;

        DetectarDash(dashing);
        DetectarAlturaLiana(enLiana);
        ActualizarPasos(enSuelo, enLiana, enCuerda, dashing);
        ActualizarWallRun();
        DetectarAterrizaje(enSuelo);

        _enSueloAnterior = enSuelo;
        _dashAnterior = dashing;
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
        if (!enLiana)
        {
            _siguiendoLiana = false;
            _alturaAcumulada = 0f;
            return;
        }

        float altura = transform.position.y;

        if (!_siguiendoLiana)
        {
            _siguiendoLiana = true;
            _ultimaAltura = altura;
            _alturaAcumulada = 0f;
            return;
        }

        _alturaAcumulada += altura - _ultimaAltura;
        _ultimaAltura = altura;

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

    private void ActualizarPasos(bool enSuelo, bool enLiana, bool enCuerda, bool dashing)
    {
        bool caminando = enSuelo
            && _p.Move.sqrMagnitude > 0.01f
            && !enLiana
            && !enCuerda
            && !dashing
            && !_p.Slide.Sliding
            && !_p.WallRun.WallRunning;

        if (!caminando)
        {
            _temporizadorPaso = 0f;
            return;
        }

        _temporizadorPaso -= Time.deltaTime;
        if (_temporizadorPaso > 0f) return;

        bool corriendo = _p.Sprint.Sprinting;
        _temporizadorPaso = corriendo ? _intervaloCorrer : _intervaloCaminar;

        int superficie = IndiceSuperficie();
        if (superficie < 0) return;

        ReproducirPaso(corriendo ? _gruposCorrer[superficie] : _gruposCaminar[superficie]);
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
        if (pared == null || pared.gameObject.layer != _capaParedWallRun) return;

        ReproducirPaso(_correrRoca);
    }

    private void DetectarAterrizaje(bool enSuelo)
    {
        if (!enSuelo)
        {
            _tiempoEnAire += Time.deltaTime;
            return;
        }

        bool aterrizo = !_enSueloAnterior && _tiempoEnAire >= _tiempoMinimoEnAire;
        _tiempoEnAire = 0f;

        if (!aterrizo) return;

        int superficie = IndiceSuperficie();
        if (superficie >= 0) Reproducir(_gruposAterrizaje[superficie], _fuenteEfectos);
    }

    private void AlSaltar()
    {
        if (!_p.Ground.Grounded) return;

        int superficie = IndiceSuperficie();
        if (superficie >= 0) Reproducir(_gruposSalto[superficie], _fuenteEfectos);
    }

    private int IndiceSuperficie()
    {
        Collider suelo = _p.Ground.Suelo;
        if (suelo == null) return -1;

        int capa = suelo.gameObject.layer;

        if (capa == _capaTierra) return Tierra;
        if (capa == _capaCesped) return Cesped;
        if (capa == _capaRoca) return Roca;
        if (capa == _capaMadera) return Madera;

        return -1;
    }

    private void ReproducirPaso(GrupoSonido grupo)
    {
        _fuentePasos.pitch = 1f + Random.Range(-_variacionPitchPasos, _variacionPitchPasos);
        Reproducir(grupo, _fuentePasos);
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