using UnityEngine;
using UnityEngine.InputSystem;

public class DispararGancho : MonoBehaviour
{
    private const int IntentosMaximos = 8;

    [Header("Referencias")]
    public Transform player;
    public Camera camera;
    [SerializeField] private PlayerMovement _p;
    public GameObject gancho;

    [Header("Disparo")]
    public float distanciaMaxima = 50f;
    [SerializeField] private string _tagPunto = "Cuerda";
    [SerializeField] private LayerMask _mascara = ~0;
    [SerializeField] private bool _reemplazarGanchoAnterior = true;

    private Transform _raizJugador;
    private Transform _camaraTransform;
    private GameObject _ganchoActual;
    private bool _listo;

    public void Init(PlayerMovement p)
    {
        _p = p;
        Resolver();
    }

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

        _listo = _p != null && player != null && camera != null && gancho != null;
        if (!_listo) return;

        _raizJugador = _p.transform;
        _camaraTransform = camera.transform;
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!context.performed || !_listo) return;
        if (_p.Rope.EnCuerda || _p.Liana.EnLiana || _p.Ground.Grounded) return;
        if (!BuscarPunto(out RaycastHit golpe)) return;
        if (!golpe.collider.CompareTag(_tagPunto)) return;

        Vector3 direccion = player.position - golpe.point;
        Quaternion rotacion = direccion.sqrMagnitude > 0.0001f
            ? Quaternion.FromToRotation(Vector3.up, direccion.normalized)
            : Quaternion.identity;

        if (_reemplazarGanchoAnterior && _ganchoActual != null) Destroy(_ganchoActual);

        _ganchoActual = Instantiate(gancho, golpe.point, rotacion);
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