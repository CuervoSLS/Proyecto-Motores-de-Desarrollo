using UnityEngine;

public class SendToSpawn : MonoBehaviour
{
    public bool usarTrigger = true;

    public void OnTriggerEnter(Collider other)
    {
        if (usarTrigger)
        {
            RespawnPlayer(other);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!usarTrigger)
        {
            RespawnPlayer(collision.collider);
        }
    }

    private void RespawnPlayer(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerSpawn spawn = other.GetComponent<PlayerSpawn>();

            if (spawn != null)
            {
                spawn.Respawn();
            }
        }
    }
}
