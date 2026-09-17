using UnityEngine;

public class BalaProyectil3D : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadBala = 20f;
    [SerializeField] private float tiempoDeVidaMaximo = 5f; // Autodestrucción por si sale del mapa

    void Start()
    {
        // Se destruye automáticamente tras X segundos si no choca con nada
        Destroy(gameObject, tiempoDeVidaMaximo);
    }

    void Update()
    {
        // Mueve la bala hacia adelante en línea recta basándose en su rotación global
        transform.position += transform.forward * velocidadBala * Time.deltaTime;
    }

    // Se activa cuando la bala entra en el Collider de otro objeto (debe ser Is Trigger)
    private void OnTriggerEnter(Collider other)
    {
        // Ignoramos si choca con otra bala o con la propia trampa al nacer
        if (other.CompareTag("Bala") || other.gameObject.name.Contains("Trampa"))
        {
            return;
        }

        Debug.Log("La bala impactó en: " + other.name);

        if (other.CompareTag("Player"))
        {
            // Aquí le restas vida al jugador
        }

        // Al activar Is Trigger, esta línea se encarga de DESTRUIR la bala 
        // en cuanto toque la pared de enfrente (o cualquier obstáculo)
        Destroy(gameObject);
    }
}