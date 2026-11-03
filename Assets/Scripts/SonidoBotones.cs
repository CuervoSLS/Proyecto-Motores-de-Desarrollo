using UnityEngine;

public class SonidoBotones : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private AudioSource audioSource;

    [Header("Sonido")]
    [SerializeField] private AudioClip sonido;

    [Header("Pitch aleatorio")]
    [SerializeField] private float pitchMinimo = 0.85f;
    [SerializeField] private float pitchMaximo = 1.15f;

    private void Awake()
    {
        // Si no se asignó en el inspector, intenta tomar el del mismo objeto
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    // Llamar desde el OnClick del botón
    public void ReproducirSonido()
    {
        if (audioSource == null || sonido == null)
        {
            Debug.LogWarning("SonidoBotones: falta el AudioSource o el AudioClip.");
            return;
        }

        audioSource.pitch = Random.Range(pitchMinimo, pitchMaximo);
        audioSource.PlayOneShot(sonido);
    }
}