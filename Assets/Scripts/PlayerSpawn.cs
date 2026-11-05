using System;
using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    public Transform spawn;
    public event Action<bool> SendRespawnee;
    private bool sequence = false;
    private bool reset = true;

    public void Respawn()
    {
        CharacterController controller = GetComponent<CharacterController>();
        controller.enabled = false;
        transform.position = spawn.position;
        controller.enabled = true;
        if(sequence == true)
        {
            SendRespawnee.Invoke(reset);
        }
    }
    public void NewSpawn(Transform newSpawn)
    {
        spawn = newSpawn;
    }

    public void StartSequence()
    {
        sequence = true;
    }
}
