using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrampaDisparo : MonoBehaviour
{
    [Header("Configuración del Disparo")]
    [SerializeField] private Transform controladorDisparo; // El cañón/agujero de la pared
    [SerializeField] private float distanciaMaximaLaser = 30f; // Largo del pasillo
    [SerializeField] private LayerMask capasObstaculos;     // Capas que cortan el láser (Paredes y Jugador)

    [Header("Configuración del Tiempo")]
    [SerializeField] private float tiempoEntreDisparos = 2f; // Cada cuántos segundos dispara
    [SerializeField] private float tiempoEsperaDisparo = 0.3f; // Retraso para la animación

    [Header("Referencias")]
    [SerializeField] private GameObject balaEnemigo;       // Prefab de la bala 3D
    [SerializeField] private Animator animator;

    private LineRenderer lineRenderer;
    private float tiempoUltimoDisparo;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();

        // Configuramos el LineRenderer para que sea un hilo láser delgado
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.03f;
        lineRenderer.endWidth = 0.03f;

        tiempoUltimoDisparo = Time.time;
    }

    void Update()
    {
        if (controladorDisparo == null) return;

        // 1. DIBUJAR EL LÁSER EN TIEMPO REAL
        ActualizarLaserVisual();

        // 2. TEMPORIZADOR DEL DISPARO CONSTANTE
        if (Time.time > tiempoEntreDisparos + tiempoUltimoDisparo)
        {
            tiempoUltimoDisparo = Time.time;

            if (animator != null)
            {
                animator.SetTrigger("Disparar");
            }

            // Llama a la función de disparar tras el pequeño retraso
            Invoke(nameof(Disparar), tiempoEsperaDisparo);
        }
    }

    void ActualizarLaserVisual()
    {
        RaycastHit hit;
        Vector3 origen = controladorDisparo.position;
        Vector3 direccion = controladorDisparo.forward;
        Vector3 puntoFinalLaser = origen + (direccion * distanciaMaximaLaser);

        lineRenderer.SetPosition(0, origen);

        // El Raycast calcula dónde choca el láser (la pared de enfrente o el jugador)
        if (Physics.Raycast(origen, direccion, out hit, distanciaMaximaLaser, capasObstaculos))
        {
            puntoFinalLaser = hit.point;
        }

        // Ajusta el extremo visual de la línea al punto de impacto
        lineRenderer.SetPosition(1, puntoFinalLaser);
    }

    private void Disparar()
    {
        if (balaEnemigo != null && controladorDisparo != null)
        {
            // Crea la bala en la posición y rotación fija del cañón
            Instantiate(balaEnemigo, controladorDisparo.position, controladorDisparo.rotation);
        }
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Disparar));
    }
}