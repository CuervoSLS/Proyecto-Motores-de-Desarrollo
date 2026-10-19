using UnityEngine;

public class PlayerPush : MonoBehaviour
{
    [Header("Peso")]
    [SerializeField] private float _masa = 50f;
    [SerializeField] private float _aceleracionEmpuje = 6f;

    private PlayerMovement _p;

    public void Init(PlayerMovement p)
    {
        _p = p;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.TryGetComponent(out Liane _)) return;

        FallingPlatform plataforma = hit.collider.GetComponentInParent<FallingPlatform>();
        if (plataforma != null)
        {
            plataforma.Activar();
            return;
        }

        Rigidbody rb = hit.collider.attachedRigidbody;
        if (rb == null || rb.isKinematic || hit.moveDirection.y < -0.3f) return;

        Vector3 direccion = new Vector3(hit.moveDirection.x, 0f, hit.moveDirection.z).normalized;
        rb.AddForceAtPosition(direccion * (_masa * _aceleracionEmpuje), hit.point, ForceMode.Force);
    }
}