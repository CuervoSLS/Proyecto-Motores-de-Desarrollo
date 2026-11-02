using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class PlayerCamara : MonoBehaviour
{
    [System.Serializable]
    public struct Anillo
    {
        public float altura;
        public float radio;
    }

    [System.Serializable]
    public struct ModoCamara
    {
        public Anillo top;
        public Anillo center;
        public Anillo bottom;
        public Vector2 screenPosition;
        [Range(0f, 1f)] public float transparenciaPuntero;
    }

    public bool AplicarTransicion;

    [SerializeField] private CinemachineCamera camara;
    [SerializeField] private Image puntero;
    [SerializeField] private ModoCamara modoNormal;
    [SerializeField] private ModoCamara modoLanzamiento;
    [SerializeField, Min(0.01f)] private float duracionTransicion;
    [SerializeField] private AnimationCurve curva = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private CinemachineOrbitalFollow orbitalFollow;
    private CinemachineRotationComposer rotationComposer;
    private float progreso;

    private void Awake()
    {
        if (!ObtenerComponentes())
        {
            enabled = false;
            return;
        }

        orbitalFollow.OrbitStyle = CinemachineOrbitalFollow.OrbitStyles.ThreeRing;
    }

    private void Start()
    {
        progreso = AplicarTransicion ? 1f : 0f;
        Aplicar(curva.Evaluate(progreso));
    }

    private void Update()
    {
        float objetivo = AplicarTransicion ? 1f : 0f;
        if (Mathf.Approximately(progreso, objetivo)) return;

        progreso = Mathf.MoveTowards(progreso, objetivo, Time.deltaTime / duracionTransicion);
        Aplicar(curva.Evaluate(progreso));
    }

    private void Aplicar(float t)
    {
        var orbits = orbitalFollow.Orbits;

        orbits.Top.Height = Mathf.Lerp(modoNormal.top.altura, modoLanzamiento.top.altura, t);
        orbits.Top.Radius = Mathf.Lerp(modoNormal.top.radio, modoLanzamiento.top.radio, t);
        orbits.Center.Height = Mathf.Lerp(modoNormal.center.altura, modoLanzamiento.center.altura, t);
        orbits.Center.Radius = Mathf.Lerp(modoNormal.center.radio, modoLanzamiento.center.radio, t);
        orbits.Bottom.Height = Mathf.Lerp(modoNormal.bottom.altura, modoLanzamiento.bottom.altura, t);
        orbits.Bottom.Radius = Mathf.Lerp(modoNormal.bottom.radio, modoLanzamiento.bottom.radio, t);

        orbitalFollow.Orbits = orbits;

        var composicion = rotationComposer.Composition;
        composicion.ScreenPosition = Vector2.Lerp(modoNormal.screenPosition, modoLanzamiento.screenPosition, t);
        rotationComposer.Composition = composicion;

        if (puntero != null)
        {
            var color = puntero.color;
            color.a = Mathf.Lerp(modoNormal.transparenciaPuntero, modoLanzamiento.transparenciaPuntero, t);
            puntero.color = color;
        }
    }

    private bool ObtenerComponentes()
    {
        if (camara == null) return false;

        orbitalFollow = camara.GetComponent<CinemachineOrbitalFollow>();
        rotationComposer = camara.GetComponent<CinemachineRotationComposer>();

        return orbitalFollow != null && rotationComposer != null;
    }

    private ModoCamara LeerValoresActuales()
    {
        var modo = new ModoCamara();

        if (!ObtenerComponentes()) return modo;

        var orbits = orbitalFollow.Orbits;

        modo.top = new Anillo { altura = orbits.Top.Height, radio = orbits.Top.Radius };
        modo.center = new Anillo { altura = orbits.Center.Height, radio = orbits.Center.Radius };
        modo.bottom = new Anillo { altura = orbits.Bottom.Height, radio = orbits.Bottom.Radius };
        modo.screenPosition = rotationComposer.Composition.ScreenPosition;
        modo.transparenciaPuntero = puntero != null ? puntero.color.a : 0f;

        return modo;
    }

    [ContextMenu("Copiar valores actuales a Modo Normal")]
    private void CopiarAModoNormal()
    {
        modoNormal = LeerValoresActuales();
    }

    [ContextMenu("Copiar valores actuales a Modo Lanzamiento")]
    private void CopiarAModoLanzamiento()
    {
        modoLanzamiento = LeerValoresActuales();
    }
}