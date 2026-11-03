using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AgarrarGancho : MonoBehaviour
{
    [Header("Agarrar")]
    [SerializeField] private Transform _padreDelGancho;
    [SerializeField] private float _delay = 0.3f;
    [SerializeField] private string _estadoAgarrarGancho = "AgarrarGancho";

    [Header("Lanzamiento")]
    [SerializeField] private DispararGancho _apuntado;
    [SerializeField] private string _estadoLanzamiento = "L_Lanzamiento";
    [SerializeField] private float _delayLanzamiento = 0.4f;
    [SerializeField] private float _fuerzaLanzamiento = 25f;

    private PlayerMovement _p;
    private PlayerAnimation _animacion;
    private Transform _camara;
    private bool _agarrando;
    private bool _lanzando;

    public GanchoConCuerda Cercano { get; set; }
    public GanchoConCuerda Sostenido { get; private set; }
    public bool TieneGancho { get; private set; }

    private void Awake()
    {
        _p = GetComponent<PlayerMovement>();
        _animacion = GetComponent<PlayerAnimation>();
        if (_apuntado == null) _apuntado = FindFirstObjectByType<DispararGancho>();
        if (Camera.main != null) _camara = Camera.main.transform;
    }

    public void OnAgarrarGancho(InputAction.CallbackContext context)
    {
        if (!context.performed || TieneGancho || _agarrando || _lanzando) return;

        GanchoConCuerda gancho = Cercano;
        if (gancho == null || gancho.Agarrado || _padreDelGancho == null) return;
        if (_p != null && (_p.Liana.EnLiana || _p.Rope.EnCuerda || _p.Dash.Dashing || !_p.Ground.Grounded)) return;

        _agarrando = true;
        Sostenido = gancho;
        if (_p != null) _p.AccionBloqueada = true;
        if (_animacion != null) _animacion.ReproducirUnaVez(_estadoAgarrarGancho, false);

        gancho.Agarrar(_padreDelGancho, _delay);
        StartCoroutine(TerminarAgarre());
    }

    public void OnTirarGancho(InputAction.CallbackContext context)
    {
        if (!context.performed || !TieneGancho || _agarrando || _lanzando) return;
        if (_p == null || !_p.Lanzamiento.Activo) return;

        _lanzando = true;
        if (_animacion != null) _animacion.ReproducirUnaVez(_estadoLanzamiento, false);
        StartCoroutine(LanzarTrasDelay(Sostenido));
    }

    private IEnumerator TerminarAgarre()
    {
        yield return null;

        while (_animacion != null && _animacion.EnAccion) yield return null;

        _agarrando = false;
        TieneGancho = Sostenido != null && Sostenido.Agarrado;
        if (_p != null) _p.AccionBloqueada = false;
    }

    private IEnumerator LanzarTrasDelay(GanchoConCuerda gancho)
    {
        if (_delayLanzamiento > 0f) yield return new WaitForSeconds(_delayLanzamiento);

        _lanzando = false;
        TieneGancho = false;
        Sostenido = null;

        if (gancho == null || !gancho.Agarrado) yield break;

        Vector3 origen = gancho.transform.position;
        Vector3 direccion;
        if (_apuntado != null) direccion = _apuntado.DireccionDesde(origen);
        else if (_camara != null) direccion = _camara.forward;
        else direccion = transform.forward;

        gancho.Lanzar(direccion * _fuerzaLanzamiento, transform);
        if (_p != null) _p.Lanzamiento.AlLanzar();
    }
}