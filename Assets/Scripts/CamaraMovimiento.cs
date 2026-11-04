using UnityEngine;

/// <summary>
/// Movimiento leve y constante de cámara para que la escena principal no quede quieta.
/// Poné este script en la Main Camera (o en un objeto padre de la cámara).
/// Usa Perlin Noise para que el movimiento sea suave y orgánico.
/// </summary>
public class CamaraMovimiento : MonoBehaviour
{
    [Header("Movimiento de posición")]
    [Tooltip("Cuánto se desplaza la cámara en cada eje (en unidades)")]
    public Vector3 amplitudPosicion = new Vector3(0.15f, 0.1f, 0.05f);
    [Tooltip("Qué tan rápido se mueve")]
    public float velocidadPosicion = 0.4f;

    [Header("Movimiento de rotación")]
    [Tooltip("Cuánto rota la cámara en grados")]
    public Vector3 amplitudRotacion = new Vector3(0.5f, 0.8f, 0.3f);
    [Tooltip("Qué tan rápido rota")]
    public float velocidadRotacion = 0.3f;

    [Header("Opciones")]
    [Tooltip("Suavizado general del movimiento (más alto = más suave)")]
    public float suavizado = 5f;
    [Tooltip("Activar para que siga moviéndose aunque Time.timeScale = 0 (por ejemplo, en pausa)")]
    public bool ignorarTimeScale = false;

    private Vector3 posicionInicial;
    private Quaternion rotacionInicial;
    private Vector3 semillaPos;
    private Vector3 semillaRot;
    private float tiempo;

    void Start()
    {
        // Guardamos la posición y rotación originales como punto de referencia
        posicionInicial = transform.localPosition;
        rotacionInicial = transform.localRotation;

        // Semillas aleatorias para que cada eje se mueva distinto
        semillaPos = new Vector3(Random.value * 100f, Random.value * 100f, Random.value * 100f);
        semillaRot = new Vector3(Random.value * 100f, Random.value * 100f, Random.value * 100f);
    }

    void LateUpdate()
    {
        float dt = ignorarTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
        tiempo += dt;

        // --- Posición ---
        Vector3 offsetPos = new Vector3(
            Ruido(semillaPos.x, velocidadPosicion) * amplitudPosicion.x,
            Ruido(semillaPos.y, velocidadPosicion) * amplitudPosicion.y,
            Ruido(semillaPos.z, velocidadPosicion) * amplitudPosicion.z
        );
        Vector3 posObjetivo = posicionInicial + offsetPos;

        // --- Rotación ---
        Vector3 offsetRot = new Vector3(
            Ruido(semillaRot.x, velocidadRotacion) * amplitudRotacion.x,
            Ruido(semillaRot.y, velocidadRotacion) * amplitudRotacion.y,
            Ruido(semillaRot.z, velocidadRotacion) * amplitudRotacion.z
        );
        Quaternion rotObjetivo = rotacionInicial * Quaternion.Euler(offsetRot);

        // --- Aplicar con suavizado ---
        float t = 1f - Mathf.Exp(-suavizado * dt);
        transform.localPosition = Vector3.Lerp(transform.localPosition, posObjetivo, t);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, rotObjetivo, t);
    }

    // Devuelve un valor entre -1 y 1 que cambia suavemente en el tiempo
    private float Ruido(float semilla, float velocidad)
    {
        return (Mathf.PerlinNoise(semilla, tiempo * velocidad) - 0.5f) * 2f;
    }
}