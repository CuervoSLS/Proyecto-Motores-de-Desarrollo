using System.Collections;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager instance;

    [SerializeField] Transform[] puntos;
    [SerializeField] GameObject[] plataformas;
    [SerializeField] float tiempoReaparicion = 5f;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void Start()
    {
        for (int i = 0; i < puntos.Length; i++)
        {
            Crear(i);
        }
    }

    public void Reaparecer(int indice)
    {
        StartCoroutine(EsperarYCrear(indice));
    }

    IEnumerator EsperarYCrear(int indice)
    {
        yield return new WaitForSeconds(tiempoReaparicion);
        Crear(indice);
    }

    void Crear(int indice)
    {
        GameObject prefab = ObtenerPrefab(indice);

        if (prefab == null || puntos[indice] == null)
        {
            return;
        }

        GameObject plataforma = Instantiate(prefab, puntos[indice].position, prefab.transform.rotation);

        if (plataforma.TryGetComponent(out FallingPlatform falling))
        {
            falling.Iniciar(indice);
        }
    }

    GameObject ObtenerPrefab(int indice)
    {
        if (plataformas == null || plataformas.Length == 0)
        {
            return null;
        }

        if (plataformas.Length == 1)
        {
            return plataformas[0];
        }

        return indice < plataformas.Length ? plataformas[indice] : null;
    }
}