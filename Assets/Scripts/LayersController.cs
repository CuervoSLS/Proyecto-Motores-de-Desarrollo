using UnityEngine;

public class LayersController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        switch (other.gameObject.name)
        {
            case "Layer_1":
                Establecer(0.5f);
                break;
            case "Layer_2":
                Establecer(0.75f);
                break;
            case "Layer_3":
                Establecer(1f);
                break;
        }
    }

    private void Establecer(float intensidad)
    {
        if (MusicaPorVida.Instance != null) MusicaPorVida.Instance.EstablecerIntensidadZona(intensidad);
        else if (LayeredMusic.Instance != null) LayeredMusic.Instance.intensity = intensidad;
    }
}