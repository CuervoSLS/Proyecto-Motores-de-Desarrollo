using UnityEngine;

public class SendToSpawn : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerSpawn spawn = other.GetComponent<PlayerSpawn>();
            spawn.Respawn();
        }
    }
}
