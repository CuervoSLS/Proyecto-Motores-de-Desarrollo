using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public Transform spawn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Respawn()
    {
        CharacterController controller = GetComponent<CharacterController>();
        controller.enabled = false;
        transform.position = spawn.position;
        controller.enabled = true;
    }
    public void NewSpawn(Transform newSpawn)
    {
        spawn = newSpawn;
    }
}
