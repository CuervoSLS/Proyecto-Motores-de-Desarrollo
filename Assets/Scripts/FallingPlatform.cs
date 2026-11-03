using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] float tiempoAntesDeCaer = 1f;
    [SerializeField] float tiempoCaida = 4f;
    [SerializeField] float tiempoReaparicion = 3f;
    [SerializeField] float duracionAparicion = 0.4f;
    [SerializeField] float aceleracionCaida = 10f;
    [SerializeField, Range(0.05f, 0.95f)] float transparenciaFinal = 0.25f;
    [SerializeField] int parpadeos = 3;
    [SerializeField] float amplitudTemblor = 0.05f;
    [SerializeField] float frecuenciaTemblorInicial = 6f;
    [SerializeField] float frecuenciaTemblorFinal = 25f;

    Renderer render;
    Material material;
    Color colorBase;
    Vector3 posicionInicial;
    Quaternion rotacionInicial;
    WaitForSeconds esperaCaida;
    WaitForSeconds esperaReaparicion;
    float velocidad;
    bool activada;
    bool cayendo;

    void Awake()
    {
        render = GetComponent<Renderer>();
        material = render.material;
        colorBase = material.color;
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
        esperaCaida = new WaitForSeconds(tiempoCaida);
        esperaReaparicion = new WaitForSeconds(tiempoReaparicion);
    }

    void Update()
    {
        if (!cayendo)
        {
            return;
        }

        velocidad += aceleracionCaida * Time.deltaTime;
        transform.Translate(Vector3.down * velocidad * Time.deltaTime, Space.World);
    }

    void OnDestroy()
    {
        Destroy(material);
    }

    public void Iniciar(int nuevoIndice)
    {
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Player")
        {
            Activar();
        }
    }

    public void Activar()
    {
        if (activada)
        {
            return;
        }

        activada = true;
        StartCoroutine(Secuencia());
    }

    IEnumerator Secuencia()
    {
        StartCoroutine(Desvanecer(tiempoAntesDeCaer + tiempoCaida));

        yield return Temblar();

        cayendo = true;

        yield return esperaCaida;

        cayendo = false;
        render.enabled = false;

        yield return esperaReaparicion;
        yield return Aparecer();
    }

    IEnumerator Temblar()
    {
        float tiempo = 0f;
        float fase = 0f;

        while (tiempo < tiempoAntesDeCaer)
        {
            float delta = Time.deltaTime;
            tiempo += delta;
            float progreso = Mathf.Clamp01(tiempo / tiempoAntesDeCaer);
            float frecuencia = Mathf.Lerp(frecuenciaTemblorInicial, frecuenciaTemblorFinal, progreso * progreso);
            float amplitud = amplitudTemblor * Mathf.Lerp(0.4f, 1f, progreso);

            fase += frecuencia * Mathf.PI * 2f * delta;

            Vector3 desplazamiento = new Vector3(Mathf.Sin(fase), 0f, Mathf.Cos(fase * 1.7f)) * amplitud;
            transform.position = posicionInicial + desplazamiento;

            yield return null;
        }

        transform.position = posicionInicial;
    }

    IEnumerator Desvanecer(float duracion)
    {
        float multiplicador = Mathf.Max(1, parpadeos) * 2 - 1;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float pasos = Mathf.PingPong(tiempo / duracion * multiplicador, 1f);
            colorBase.a = Mathf.Lerp(1f, transparenciaFinal, pasos);
            material.color = colorBase;
            yield return null;
        }

        colorBase.a = transparenciaFinal;
        material.color = colorBase;
    }

    IEnumerator Aparecer()
    {
        transform.SetPositionAndRotation(posicionInicial, rotacionInicial);
        velocidad = 0f;
        colorBase.a = 0f;
        material.color = colorBase;
        render.enabled = true;

        float tiempo = 0f;

        while (tiempo < duracionAparicion)
        {
            tiempo += Time.deltaTime;
            colorBase.a = Mathf.Clamp01(tiempo / duracionAparicion);
            material.color = colorBase;
            yield return null;
        }

        colorBase.a = 1f;
        material.color = colorBase;
        activada = false;
    }
}