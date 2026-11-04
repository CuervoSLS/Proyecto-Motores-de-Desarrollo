using UnityEngine;

public class Rain : MonoBehaviour
{
    /// Referencias de la lluvia
    [SerializeField] private GameObject psRain;
    [SerializeField] private AudioSource rain;            // Ambiente de lluvia
    [SerializeField] private AudioClip[] sonidosRain;     // Sonidos posibles de la lluvia

    /// Referencias del ambiente normal
    [SerializeField] private AudioSource ambienteNormal;  // Ambiente común (cuando NO llueve)

    /// Referencias para el clima
    [SerializeField] private Material skyboxLluvia;       // Skybox cuando llueve
    [SerializeField] private Material skyboxNormal;       // Skybox cuando NO llueve
    [SerializeField] private Light luzDireccional;        // Directional Light de la escena

    // Guarda la intensidad original de la luz para poder restaurarla
    private float intensidadNormal;

    // Índice del último sonido de lluvia reproducido (-1 = ninguno todavía)
    private int ultimoSonidoRain = -1;

    void Start()
    {
        // Si no se asignó la luz en el inspector, usa la luz "Sun" de la escena
        if (luzDireccional == null) luzDireccional = RenderSettings.sun;

        // Guarda la intensidad original antes de modificarla
        if (luzDireccional != null) intensidadNormal = luzDireccional.intensity;

        // Aplica el clima al iniciar
        ActualizarClima();
    }

    /// Se puede llamar manualmente (desde otro script o un UnityEvent) para refrescar el clima
    public void ActualizarClima()
    {
        // Se lee una sola vez el valor guardado
        bool llueve = PlayerPrefs.GetInt("rain") == 1;

        // Partículas de lluvia
        psRain.SetActive(llueve);

        // Audios: uno suena y el otro se detiene (se evita reiniciarlos si ya están en su estado)
        CambiarAudio(rain, llueve);
        CambiarAudio(ambienteNormal, !llueve);

        // Skybox según el clima
        Material skyboxElegido = llueve ? skyboxLluvia : skyboxNormal;
        if (skyboxElegido != null && RenderSettings.skybox != skyboxElegido)
        {
            RenderSettings.skybox = skyboxElegido;
            DynamicGI.UpdateEnvironment(); // Actualiza la luz ambiental con el skybox nuevo
        }

        // Intensidad de la luz: 0 si llueve, la original si no
        if (luzDireccional != null)
            luzDireccional.intensity = llueve ? 0f : intensidadNormal;
    }

    /// Reproduce o detiene un AudioSource solo si hace falta
    private void CambiarAudio(AudioSource audio, bool reproducir)
    {
        if (audio == null) return;

        if (reproducir && !audio.isPlaying)
        {
            // Si es el audio de la lluvia, se elige un sonido nuevo antes de reproducir
            if (audio == rain) ElegirSonidoRain();
            audio.Play();
        }
        else if (!reproducir && audio.isPlaying) audio.Stop();
    }

    /// Asigna al AudioSource de la lluvia un clip al azar, distinto del anterior
    private void ElegirSonidoRain()
    {
        if (sonidosRain == null || sonidosRain.Length == 0) return;

        int nuevo = 0;

        if (sonidosRain.Length > 1)
        {
            // Se repite el sorteo hasta que salga uno distinto al último
            do
            {
                nuevo = Random.Range(0, sonidosRain.Length);
            }
            while (nuevo == ultimoSonidoRain);
        }

        ultimoSonidoRain = nuevo;
        rain.clip = sonidosRain[nuevo];
    }
}