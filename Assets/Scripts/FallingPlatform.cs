using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] float tiempoAntesDeCaer = 1f;
    [SerializeField] float tiempoCaida = 4f;
    [SerializeField] float tiempoReaparicion = 3f;
    [SerializeField] float aceleracionCaida = 10f;
    [SerializeField, Range(0.05f, 0.95f)] float transparenciaFinal = 0.25f;
    [SerializeField] int parpadeos = 3;

    Renderer render;
    Material material;
    Collider colision;
    Color colorBase;
    Vector3 posicionInicial;
    Quaternion rotacionInicial;
    WaitForSeconds esperaAntesDeCaer;
    WaitForSeconds esperaCaida;
    WaitForSeconds esperaReaparicion;
    float velocidad;
    bool activada;
    bool cayendo;

    void Awake()
    {
        render = GetComponent<Renderer>();
        material = render.material;
        colision = GetComponent<Collider>();
        colorBase = material.color;
        posicionInicial = transform.position;
        rotacionInicial = transform.rotation;
        esperaAntesDeCaer = new WaitForSeconds(tiempoAntesDeCaer);
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

        yield return esperaAntesDeCaer;

        colision.enabled = false;
        cayendo = true;

        yield return esperaCaida;

        cayendo = false;
        render.enabled = false;

        yield return esperaReaparicion;

        Reiniciar();
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

    void Reiniciar()
    {
        transform.SetPositionAndRotation(posicionInicial, rotacionInicial);
        velocidad = 0f;
        colorBase.a = 1f;
        material.color = colorBase;
        render.enabled = true;
        colision.enabled = true;
        activada = false;
    }
}