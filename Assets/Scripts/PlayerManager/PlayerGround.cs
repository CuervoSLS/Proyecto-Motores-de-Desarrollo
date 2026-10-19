using UnityEngine;

public class PlayerGround : MonoBehaviour
{
    [Header("Deteccion de piso")]
    [SerializeField] private LayerMask _groundMask = ~0;
    [SerializeField] private float _groundCheckDistance = 0.1f;

    private const float DistanciaMaximaSuelo = 200f;

    private PlayerMovement _p;

    public bool Grounded { get; private set; }

    public void Init(PlayerMovement p)
    {
        _p = p;
    }

    public float Actualizar()
    {
        float distancia = DistanciaAlSuelo();
        Grounded = _p.VerticalVelocity <= 0.01f && distancia <= _groundCheckDistance;
        return distancia;
    }

    private float DistanciaAlSuelo()
    {
        CharacterController controller = _p._controller;
        float radio = controller.radius * 0.9f;
        Vector3 origen = transform.TransformPoint(controller.center);

        if (Physics.SphereCast(origen, radio, Vector3.down, out RaycastHit hit, DistanciaMaximaSuelo, _groundMask, QueryTriggerInteraction.Ignore))
        {
            return Mathf.Max(0f, hit.distance + radio - controller.height * 0.5f);
        }

        return float.PositiveInfinity;
    }
}