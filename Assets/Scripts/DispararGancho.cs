using UnityEngine;

public class DispararGancho : MonoBehaviour
{
    private const int IntentosMaximos = 8;

    [Header("Referencias")]
    public Transform player;
    public Camera camera;
    [SerializeField] private PlayerMovement _p;

    [Header("Apuntado")]
    public float distanciaMaxima = 50f;
    [SerializeField] private LayerMask _mascara = ~0;

    private Transform _raizJugador;
    private Transform _camaraTransform;
    private bool _listo;

    private void Start()
    {
        Resolver();
    }

    private void Resolver()
    {
        if (_p == null && player != null) _p = player.GetComponentInParent<PlayerMovement>();
        if (_p == null) _p = FindFirstObjectByType<PlayerMovement>();
        if (player == null && _p != null) player = _p.transform;
        if (camera == null) camera = Camera.main;

        _listo = _p != null && player != null && camera != null;
        if (!_listo) return;

        _raizJugador = _p.transform;
        _camaraTransform = camera.transform;
    }

    public Vector3 DireccionDesde(Vector3 origen)
    {
        if (!_listo) return camera != null ? camera.transform.forward : transform.forward;

        Vector3 destino = BuscarPunto(out RaycastHit golpe)
            ? golpe.point
            : _camaraTransform.position + _camaraTransform.forward * distanciaMaxima;

        Vector3 direccion = destino - origen;
        return direccion.sqrMagnitude > 0.0001f ? direccion.normalized : _camaraTransform.forward;
    }

    private bool BuscarPunto(out RaycastHit resultado)
    {
        Vector3 direccion = _camaraTransform.forward;
        Vector3 origen = _camaraTransform.position;
        float restante = distanciaMaxima;

        for (int i = 0; i < IntentosMaximos; i++)
        {
            if (!Physics.Raycast(origen, direccion, out resultado, restante, _mascara, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            if (!resultado.collider.transform.IsChildOf(_raizJugador)) return true;

            origen = resultado.point + direccion * 0.02f;
            restante = distanciaMaxima - Vector3.Distance(_camaraTransform.position, origen);
            if (restante <= 0f) break;
        }

        resultado = default;
        return false;
    }
}