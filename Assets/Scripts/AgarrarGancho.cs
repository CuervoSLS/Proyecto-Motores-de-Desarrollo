using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AgarrarGancho : MonoBehaviour
{
    [Header("Gancho")]
    [SerializeField] private Transform _padreDelGancho;
    [SerializeField] private float _delay = 0.3f;

    [Header("Animacion")]
    [SerializeField] private string _estadoAgarrarGancho = "AgarrarGancho";

    [Header("Lanzamiento")]
    [SerializeField] private DispararGancho _apuntado;
    [SerializeField] private string _estadoLanzamiento = "L_Lanzamiento";
    [SerializeField] private float _delayLanzamiento = 0.4f;
    [SerializeField] private float _fuerzaLanzamiento = 25f;

    private PlayerMovement _p;
    private PlayerAnimation _animacion;
    private Transform _camara;
    private bool _lanzando;

    public GanchoConCuerda Cercano { get; set; }
    public GanchoConCuerda Sostenido { get; private set; }

    private void Awake()
    {
        _p = GetComponent<PlayerMovement>();
        _animacion = GetComponent<PlayerAnimation>();
        if (_apuntado == null) _apuntado = FindFirstObjectByType<DispararGancho>();
        if (Camera.main != null) _camara = Camera.main.transform;
    }

    public void OnAgarrarGancho(InputAction.CallbackContext context)
    {
        if (!context.performed || _lanzando) return;
        if (_animacion != null && _animacion.EnAccion) return;
        if (_p != null && (_p.Liana.EnLiana || _p.Rope.EnCuerda || _p.Dash.Dashing)) return;

        if (Sostenido != null && Sostenido.Agarrado)
        {
            IniciarLanzamiento();
            return;
        }

        GanchoConCuerda gancho = Cercano;
        if (gancho == null || gancho.Agarrado || _padreDelGancho == null) return;

        if (_animacion != null) _animacion.ReproducirUnaVez(_estadoAgarrarGancho, true);
        gancho.Agarrar(_padreDelGancho, _delay);
        Sostenido = gancho;
    }

    private void IniciarLanzamiento()
    {
        if (_p == null || !_p.Lanzamiento.Activo) return;

        _lanzando = true;
        if (_animacion != null) _animacion.ReproducirUnaVez(_estadoLanzamiento, false);
        StartCoroutine(LanzarTrasDelay(Sostenido));
    }

    private IEnumerator LanzarTrasDelay(GanchoConCuerda gancho)
    {
        if (_delayLanzamiento > 0f) yield return new WaitForSeconds(_delayLanzamiento);

        _lanzando = false;
        if (gancho == null || !gancho.Agarrado) yield break;

        Vector3 origen = gancho.transform.position;
        Vector3 direccion;
        if (_apuntado != null) direccion = _apuntado.DireccionDesde(origen);
        else if (_camara != null) direccion = _camara.forward;
        else direccion = transform.forward;

        gancho.Lanzar(direccion * _fuerzaLanzamiento, transform);
        if (_p != null) _p.Lanzamiento.AlLanzar();
        if (Sostenido == gancho) Sostenido = null;
    }
}