using JetBrains.Annotations;
using UnityEngine;

public class CheckDistanciaParaGancho : MonoBehaviour
{
    public Transform player;
    public float distanciaMaxima;
    public Camera camera;

    private Collider[] playerCollider;

    public GameObject gancho;

    void Start()
    {
        playerCollider = player.GetComponentsInChildren<Collider>();
        InvokeRepeating("Disparar", 1, 1);
    }

    void Disparar()
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
                if(hit.collider == col)
                {
                    isPlayer = true;
                    break;
                }
            }
            if (isPlayer)
            {
                continue;
            }
            if(hit.collider.CompareTag("Cuerda"))
            {
                float distancia = Vector3.Distance(player.position, hit.point);
                Debug.Log(distancia);
                Vector3 puntoMedio = (player.position + hit.point) / 2;
                Vector3 direccion = player.position - hit.point;
                Quaternion rotacion = Quaternion.FromToRotation(Vector3.up, direccion.normalized);
                Instantiate(gancho, puntoMedio, rotacion);
                
            }
            break;
        }
    }
}
