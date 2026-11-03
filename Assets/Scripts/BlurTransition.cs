using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Transición fluida entre pantalla normal y pantalla borrosa animando directamente
/// los valores del override Depth of Field (modo Gaussian) de un Volume.
/// - Borroso: Start 0, End 0, Max Radius 1.5
/// - Normal:  Start 1000, End 1000, Max Radius 0.5
/// Requisitos: URP, Volume con Is Global activado y Post Processing tildado en la cámara.
/// </summary>
[RequireComponent(typeof(Volume))]
public class BlurTransition : MonoBehaviour
{
    [Header("Control")]
    [Tooltip("Activado = pantalla borrosa. Desactivado = pantalla normal.")]
    [SerializeField] private bool borroso = false;

    [Header("Transición")]
    [Tooltip("Segundos que tarda en pasar de normal a borroso (y viceversa).")]
    [SerializeField, Min(0.01f)] private float duracion = 0.75f;

    [Tooltip("Forma de la transición. EaseInOut = arranca y termina suave.")]
    [SerializeField] private AnimationCurve curva = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("Si está activo, funciona aunque Time.timeScale sea 0 (por ejemplo en un menú de pausa).")]
    [SerializeField] private bool usarTiempoSinEscala = true;

    [Header("Valores cuando está BORROSO")]
    [SerializeField] private float borrosoStart = 0f;
    [SerializeField] private float borrosoEnd = 0f;
    [SerializeField] private float borrosoMaxRadius = 1.5f;

    [Header("Valores cuando está NORMAL")]
    [SerializeField] private float normalStart = 1000f;
    [SerializeField] private float normalEnd = 1000f;
    [SerializeField] private float normalMaxRadius = 0.5f;

    [Header("Opciones")]
    [Tooltip("Interpola Start y End de forma logarítmica. Como van de 1000 a 0, con una interpolación lineal " +
             "el blur recién se notaría al final. La logarítmica reparte el cambio durante toda la transición.")]
    [SerializeField] private bool interpolacionLogaritmica = true;

    [Tooltip("Desactiva el efecto Depth of Field cuando no hay blur, para no gastar rendimiento.")]
    [SerializeField] private bool apagarEfectoSinBlur = true;

    private Volume volume;
    private DepthOfField dof;
    private float progreso; // 0 = normal, 1 = totalmente borroso

    /// <summary>Activa o desactiva el blur desde otros scripts.</summary>
    public bool Borroso
    {
        get => borroso;
        set => borroso = value;
    }

    private void Awake()
    {
        volume = GetComponent<Volume>();
        volume.weight = 1f;

        // volume.profile devuelve una copia del profile, así no se modifica el asset original.
        if (!volume.profile.TryGet(out dof))
        {
            dof = volume.profile.Add<DepthOfField>(true);
        }

        dof.mode.Override(DepthOfFieldMode.Gaussian);
        dof.gaussianStart.overrideState = true;
        dof.gaussianEnd.overrideState = true;
        dof.gaussianMaxRadius.overrideState = true;

        // Arranca directamente en el estado que marca el bool, sin transición.
        progreso = borroso ? 1f : 0f;
        Aplicar();
    }

    private void Update()
    {
        float objetivo = borroso ? 1f : 0f;

        if (!Mathf.Approximately(progreso, objetivo))
        {
            float dt = usarTiempoSinEscala ? Time.unscaledDeltaTime : Time.deltaTime;

            // Se mueve hacia el objetivo desde donde esté, así si cambiás el bool
            // a mitad de la transición, revierte de forma fluida sin saltos.
            progreso = Mathf.MoveTowards(progreso, objetivo, dt / duracion);
            Aplicar();
        }
    }

    private void Aplicar()
    {
        float t = curva.Evaluate(progreso);

        dof.gaussianStart.value = Interpolar(normalStart, borrosoStart, t);
        dof.gaussianEnd.value = Interpolar(normalEnd, borrosoEnd, t);
        dof.gaussianMaxRadius.value = Mathf.Lerp(normalMaxRadius, borrosoMaxRadius, t);

        if (apagarEfectoSinBlur)
        {
            dof.active = progreso > 0f;
        }
    }

    private float Interpolar(float desde, float hasta, float t)
    {
        if (!interpolacionLogaritmica) return Mathf.Lerp(desde, hasta, t);

        // En los extremos devuelve el valor exacto (así llega a 0 y a 1000 sin error).
        if (t <= 0f) return desde;
        if (t >= 1f) return hasta;

        // El logaritmo no admite 0, así que se usa un mínimo chico solo para calcular.
        const float minimo = 0.01f;
        float logDesde = Mathf.Log(Mathf.Max(desde, minimo));
        float logHasta = Mathf.Log(Mathf.Max(hasta, minimo));
        return Mathf.Exp(Mathf.Lerp(logDesde, logHasta, t));
    }

    // Métodos de ayuda (por ejemplo para botones de la UI)
    public void ActivarBlur() => borroso = true;
    public void DesactivarBlur() => borroso = false;
    public void Alternar() => borroso = !borroso;

    private void OnValidate()
    {
        duracion = Mathf.Max(0.01f, duracion);
    }
}