using UnityEngine;
using TMPro;

public class MostrarRecord : MonoBehaviour
{
    [Header("Nivel (1 o 2)")]
    public int nivel = 1;

    [Header("UI")]
    public TextMeshProUGUI textoRecord;

    private void Start()
    {
        float record = NivelTerminado.ObtenerRecord(nivel);

        // Solo se muestra el tiempo (MM:SS). Si todavía no hay record, queda 00:00
        textoRecord.text = NivelTerminado.FormatearTiempo(Mathf.Max(0f, record));
    }
}