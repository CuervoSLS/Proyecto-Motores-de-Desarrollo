using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Objeto a desactivar y sonido")]
    public AudioSource audioSource;

    private bool yaActivado = false;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (yaActivado) return;

        if (other.CompareTag("Player"))
        {
            yaActivado = true;

            PlayerSpawn spawn = other.GetComponent<PlayerSpawn>();
            spawn.NewSpawn(gameObject.transform);

            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }
}