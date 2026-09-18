using UnityEngine;

public class SpawnPoint : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerSpawn spawn = other.GetComponent<PlayerSpawn>();
            spawn.NewSpawn(gameObject.transform);
        }
    }
}
