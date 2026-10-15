using UnityEngine;

public class SendToSpawn : MonoBehaviour
{
<<<<<<< Updated upstream
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter(Collider other)
=======
    public bool usarTrigger = true;

    private void OnTriggerEnter(Collider other)
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
>>>>>>> Stashed changes
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