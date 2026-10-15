using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerSpawn spawn = other.GetComponent<PlayerSpawn>();
            spawn.NewSpawn(gameObject.transform);
        }
    }
}
