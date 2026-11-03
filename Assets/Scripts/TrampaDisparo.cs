using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class TrampaDisparo : MonoBehaviour
{
    [Header("Configuración del Disparo")]
    [SerializeField] private Transform controladorDisparo; // El cañón/agujero de la pared
    [SerializeField] private float distanciaMaximaLaser = 30f; // Largo del pasillo
    [SerializeField] private LayerMask capasObstaculos;     // Capas que cortan el láser (Paredes y Jugador)

    [Header("Detección del Jugador")]
    [SerializeField] private float distanciaActivacion = 15f; // A qué distancia del jugador empieza a disparar
    [SerializeField] private bool laserSoloCuandoActiva = false; // Si está activo, el láser solo se ve con el jugador cerca

    [Header("Configuración del Tiempo")]
    [SerializeField] private float tiempoEntreDisparos = 2f; // Cada cuántos segundos dispara
    [SerializeField] private float tiempoEsperaDisparo = 0.3f; // Retraso para la animación

    [Header("Referencias")]
    [SerializeField] private GameObject balaEnemigo;       // Prefab de la bala 3D
    [SerializeField] private Animator animator;

    private LineRenderer lineRenderer;
    private Transform jugador;
    private float tiempoUltimoDisparo;
    private bool jugadorCerca;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();

        // Buscamos al jugador por su tag
        GameObject objetoJugador = GameObject.FindGameObjectWithTag("Player");
        if (objetoJugador != null)
        {
            jugador = objetoJugador.transform;
        }
        else
        {
            Debug.LogWarning("TrampaDisparo: no se encontró ningún objeto con el tag 'Player'.", this);
        }

        // Configuramos el LineRenderer para que sea un hilo láser delgado
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.03f;
        lineRenderer.endWidth = 0.03f;

        tiempoUltimoDisparo = Time.time;
    }

    void Update()
    {
        if (controladorDisparo == null || jugador == null) return;

        // 1. COMPROBAR SI EL JUGADOR ESTÁ LO SUFICIENTEMENTE CERCA
        ActualizarDeteccionJugador();

        // 2. DIBUJAR EL LÁSER EN TIEMPO REAL
        lineRenderer.enabled = jugadorCerca || !laserSoloCuandoActiva;
        if (lineRenderer.enabled)
        {
            ActualizarLaserVisual();
        }

        // 3. SI EL JUGADOR NO ESTÁ CERCA, NO DISPARA
        if (!jugadorCerca) return;

        // 4. TEMPORIZADOR DEL DISPARO CONSTANTE
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

    void ActualizarDeteccionJugador()
    {
        // sqrMagnitude evita calcular la raíz cuadrada (más eficiente)
        float distanciaCuadrada = (jugador.position - transform.position).sqrMagnitude;
        bool estaCerca = distanciaCuadrada <= distanciaActivacion * distanciaActivacion;

        if (estaCerca && !jugadorCerca)
        {
            // El jugador acaba de entrar en rango: reinicia el temporizador
            // para que no dispare de inmediato, sino tras el tiempo entre disparos
            tiempoUltimoDisparo = Time.time;
        }
        else if (!estaCerca && jugadorCerca)
        {
            // El jugador salió del rango: cancela un disparo que estuviera pendiente
            CancelInvoke(nameof(Disparar));
        }

        jugadorCerca = estaCerca;
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

    // Dibuja el rango de activación en la vista Scene al seleccionar la trampa
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaActivacion);
    }
}