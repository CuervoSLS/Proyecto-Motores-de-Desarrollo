using UnityEngine;
using System;

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
        if (!other.CompareTag("Player")) return;

        PlayerSpawn spawn = other.GetComponentInParent<PlayerSpawn>();
        if (spawn == null) return;

        spawn.Respawn();

        PlayerVoice voz = spawn.GetComponent<PlayerVoice>();
        if (voz != null) voz.SonarMuerte();
    }
}