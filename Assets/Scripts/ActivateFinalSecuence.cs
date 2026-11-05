using System;
using UnityEngine;

public class ActivateFinalSecuence : MonoBehaviour
{
    public event Action<bool> IniciateSequence;

    private bool activate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.GetComponent<PlayerSpawn>().StartSequence();
            IniciateSequence.Invoke(activate);
            Destroy(gameObject);
        }       
    }
}
