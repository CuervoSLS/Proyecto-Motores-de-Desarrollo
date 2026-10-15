using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public Transform spawn;

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
