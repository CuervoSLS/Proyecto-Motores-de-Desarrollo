using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BalaProyectil3D : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidadBala = 20f;
    [SerializeField] private float tiempoDeVidaMaximo = 5f;

    [Header("Después del impacto")]
    [SerializeField] private float tiempoQuieta = 3f;
    [SerializeField] private float duracionDesvanecer = 1.5f;

    [Header("Jugador")]
    [SerializeField] private LayerMask capaJugador;
    [SerializeField] private bool desvanecerEnJugador;
    [SerializeField] private float vidaAQuitarMinima = 5f;
    [SerializeField] private float vidaAQuitarMaxima = 15f;

    [Header("Efectos")]
    [SerializeField] private TrailRenderer trail;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip FlechaPasando;
    [SerializeField] private AudioClip ImpactoPlayer;
    [SerializeField] private AudioClip ImpactoRoca;

    private Rigidbody rb;
    private Collider[] colliders;
    private Material[] materiales;
    private Color[] coloresOriginales;
    private int[] idsColor;
    private bool impacto;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        colliders = GetComponentsInChildren<Collider>();

        if (trail == null) trail = GetComponentInChildren<TrailRenderer>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        rb.AddForce(transform.forward * velocidadBala, ForceMode.VelocityChange);
        Invoke(nameof(Autodestruir), tiempoDeVidaMaximo);
        IniciarSonidoVuelo();
    }

    private void IniciarSonidoVuelo()
    {
        if (audioSource == null || FlechaPasando == null) return;

        audioSource.clip = FlechaPasando;
        audioSource.loop = true;
        audioSource.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (impacto) return;
        impacto = true;

        CancelInvoke();
        rb.isKinematic = true;

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        if (trail != null) trail.emitting = false;

        if (audioSource != null) audioSource.Stop();

        Collider otro = collision.collider;
        bool esJugador = (capaJugador.value & (1 << otro.gameObject.layer)) != 0;

        ReproducirImpacto(esJugador ? ImpactoPlayer : ImpactoRoca);

        if (esJugador)
        {
            PlayerHeart corazon = otro.GetComponentInParent<PlayerHeart>();
            if (corazon != null) corazon.BajarVida(Random.Range(vidaAQuitarMinima, vidaAQuitarMaxima));

            transform.SetParent(otro.transform, true);
            if (!desvanecerEnJugador) return;
        }

        StartCoroutine(Desvanecer());
    }

    private void ReproducirImpacto(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    private IEnumerator Desvanecer()
    {
        yield return new WaitForSeconds(tiempoQuieta);

        PrepararMateriales();

        float tiempo = 0f;
        while (tiempo < duracionDesvanecer)
        {
            tiempo += Time.deltaTime;
            float factor = 1f - Mathf.Clamp01(tiempo / duracionDesvanecer);

            for (int i = 0; i < materiales.Length; i++)
            {
                Color color = coloresOriginales[i];
                color.a *= factor;
                materiales[i].SetColor(idsColor[i], color);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private void PrepararMateriales()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        var lista = new System.Collections.Generic.List<Material>();

        for (int i = 0; i < renderers.Length; i++)
        {
            lista.AddRange(renderers[i].materials);
        }

        materiales = lista.ToArray();
        coloresOriginales = new Color[materiales.Length];
        idsColor = new int[materiales.Length];

        int baseColor = Shader.PropertyToID("_BaseColor");
        int color = Shader.PropertyToID("_Color");

        for (int i = 0; i < materiales.Length; i++)
        {
            idsColor[i] = materiales[i].HasProperty(baseColor) ? baseColor : color;
            coloresOriginales[i] = materiales[i].GetColor(idsColor[i]);
        }
    }

    private void Autodestruir()
    {
        Destroy(gameObject);
    }
}