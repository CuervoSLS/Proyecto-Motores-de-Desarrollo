using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

public class DispararGancho : MonoBehaviour
{
    public Transform player;
    public float distanciaMaxima;
    public Camera camera;
    [SerializeField] private PlayerMovement _p;
    private Collider[] playerCollider;

    public GameObject gancho;

    public void Init(PlayerMovement p)
    {
        _p = p;
    }
    void Start()
    {
        playerCollider = player.GetComponentsInChildren<Collider>();
    }
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        else if (_p.Rope.EnCuerda)
        {
            return;
        }

        else if (_p.Ground.Grounded || _p.Liana.EnLiana)
        {
            return;
        }
        else
        {

            RaycastHit[] hits = Physics.RaycastAll(camera.transform.position, camera.transform.forward, distanciaMaxima);
            Debug.DrawRay(
            camera.transform.position,
            camera.transform.forward * distanciaMaxima,
            Color.red);
            foreach (RaycastHit hit in hits)
            {
                bool isPlayer = false;
                foreach (Collider col in playerCollider)
                {
                    if (hit.collider == col)
                    {
                        isPlayer = true;
                        break;
                    }
                }
                if (isPlayer)
                {
                    continue;
                }
                if (hit.collider.CompareTag("Cuerda"))
                {
                    float distancia = Vector3.Distance(player.position, hit.point);
                    Debug.Log(distancia);
                    Vector3 puntoMedio = (player.position + hit.point) / 2;
                    Vector3 direccion = player.position - hit.point;
                    Quaternion rotacion = Quaternion.FromToRotation(Vector3.up, direccion.normalized);
                    Instantiate(gancho, hit.point, rotacion);

                }
                break;
            }
        }

        

    }
}
