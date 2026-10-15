using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] float tiempoAntesDeCaer = 1f;
    [SerializeField] float tiempoCaida = 4f;
    [SerializeField] float aceleracionCaida = 10f;
    [SerializeField, Range(0.05f, 0.95f)] float transparenciaFinal = 0.25f;
    [SerializeField] int parpadeos = 3;

    Material material;
    Collider colision;
    Color colorBase;
    float velocidad;
    int indice;
    bool activada;
    bool cayendo;

    void Awake()
    {
        material = GetComponent<Renderer>().material;
        colision = GetComponent<Collider>();
        colorBase = material.color;
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
        indice = nuevoIndice;
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
        PlatformManager.instance.Reaparecer(indice);
        StartCoroutine(Secuencia());
    }

    IEnumerator Secuencia()
    {
        StartCoroutine(Desvanecer(tiempoAntesDeCaer + tiempoCaida));

        yield return new WaitForSeconds(tiempoAntesDeCaer);

        colision.enabled = false;
        cayendo = true;

        yield return new WaitForSeconds(tiempoCaida);

        Destroy(gameObject);
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
}