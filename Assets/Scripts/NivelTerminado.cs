using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NivelTerminado : MonoBehaviour
{
    [Header("Nivel (1 o 2)")]
    public int nivel = 1;

    [Header("UI")]
    [Tooltip("Panel del Canvas que se abre al terminar el nivel (debe empezar desactivado)")]
    public GameObject panel;
    [Tooltip("Image del panel. Si se deja vacío, se usa la Image del propio panel")]
    public Image imagenPanel;
    [Tooltip("Texto del panel que cuenta de 00:00 hasta el tiempo final")]
    public TextMeshProUGUI textoTiempo;
    [Tooltip("Texto que muestra el tiempo actual mientras se juega")]
    public TextMeshProUGUI textoTiempoActual;

    [Header("Record")]
    public TextMeshProUGUI textoRecord;
    public string mensajeRecord = "ES TU RECORD ACTUAL!";

    [Header("Textos animados (aparecen y se achican)")]
    public TextMeshProUGUI[] textosAnimados;
    public Vector3 escalaInicial = new Vector3(3f, 3.6f, 1f);
    public Vector3 escalaFinal = new Vector3(1f, 1.2f, 1f);

    [Header("Animación")]
    public float duracionAnimacion = 2f;

    private float tiempoTranscurrido = 0f;
    private bool nivelTerminado = false;

    private void Start()
    {
        // Por si venimos de una escena donde el tiempo quedó congelado
        Time.timeScale = 1f;

        if (panel != null)
        {
            if (imagenPanel == null) imagenPanel = panel.GetComponent<Image>();
            panel.SetActive(false);
        }

        if (textoTiempoActual != null) textoTiempoActual.text = FormatearTiempo(0f);
    }

    private void Update()
    {
        // Contador desde que arranca la escena hasta que el jugador toca el trigger
        if (!nivelTerminado)
        {
            tiempoTranscurrido += Time.deltaTime;

            if (textoTiempoActual != null)
            {
                textoTiempoActual.text = FormatearTiempo(tiempoTranscurrido);
            }
        }
    }

    // Para proyectos 3D
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) TerminarNivel(other.gameObject);
    }

    // Para proyectos 2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) TerminarNivel(other.gameObject);
    }

    private void TerminarNivel(GameObject jugador)
    {
        if (nivelTerminado) return;
        nivelTerminado = true;

        // El jugador se desactiva al tocar el trigger
        jugador.SetActive(false);

        // Se sale del modo Look: el mouse se libera y se hace visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Tiempo actual queda con el valor final
        if (textoTiempoActual != null)
        {
            textoTiempoActual.text = FormatearTiempo(tiempoTranscurrido);
        }

        bool esRecord = GuardarRecord(tiempoTranscurrido);

        // Si es record muestra el mensaje, si no, no dice nada
        if (textoRecord != null)
        {
            textoRecord.text = esRecord ? mensajeRecord : "";
        }

        panel.SetActive(true);
        StartCoroutine(AnimarPanel(tiempoTranscurrido));
    }

    private IEnumerator AnimarPanel(float tiempoFinal)
    {
        // Estado inicial: imagen y textos transparentes, textos grandes, tiempo en 00:00
        SetAlpha(0f);
        SetTextosAnimados(0f);
        textoTiempo.text = FormatearTiempo(0f);

        float t = 0f;
        while (t < duracionAnimacion)
        {
            t += Time.unscaledDeltaTime;
            float progreso = Mathf.Clamp01(t / duracionAnimacion);

            SetAlpha(progreso); // de 0 a 100% de opacidad
            SetTextosAnimados(progreso);
            textoTiempo.text = FormatearTiempo(Mathf.Lerp(0f, tiempoFinal, progreso));

            yield return null;
        }

        // Valores exactos al terminar
        SetAlpha(1f);
        SetTextosAnimados(1f);
        textoTiempo.text = FormatearTiempo(tiempoFinal);

        // Al terminar la animación se congela el tiempo
        Time.timeScale = 0f;
    }

    private void SetAlpha(float alpha)
    {
        if (imagenPanel == null) return;
        Color c = imagenPanel.color;
        c.a = alpha;
        imagenPanel.color = c;
    }

    // progreso 0 = transparente y escala inicial, progreso 1 = opaco y escala final
    private void SetTextosAnimados(float progreso)
    {
        if (textosAnimados == null) return;

        foreach (TextMeshProUGUI texto in textosAnimados)
        {
            if (texto == null) continue;

            Color c = texto.color;
            c.a = progreso;
            texto.color = c;

            texto.rectTransform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, progreso);
        }
    }

    // ---------- Record (se guarda el mejor tiempo = el más bajo) ----------

    // Devuelve true si el tiempo es un nuevo record (o si es la primera vez que se completa el nivel)
    private bool GuardarRecord(float tiempo)
    {
        string clave = Clave(nivel);

        if (!PlayerPrefs.HasKey(clave) || tiempo < PlayerPrefs.GetFloat(clave))
        {
            PlayerPrefs.SetFloat(clave, tiempo);
            PlayerPrefs.Save();
            return true;
        }

        return false;
    }

    private static string Clave(int nivel)
    {
        return "RecordNivel_" + nivel;
    }

    // Devuelve el record del nivel, o -1 si todavía no hay ninguno
    public static float ObtenerRecord(int nivel)
    {
        string clave = Clave(nivel);
        return PlayerPrefs.HasKey(clave) ? PlayerPrefs.GetFloat(clave) : -1f;
    }

    // Formato MM:SS
    public static string FormatearTiempo(float tiempo)
    {
        int minutos = Mathf.FloorToInt(tiempo / 60f);
        int segundos = Mathf.FloorToInt(tiempo % 60f);
        return string.Format("{0:00}:{1:00}", minutos, segundos);
    }
}