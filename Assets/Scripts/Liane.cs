
using UnityEngine;

public class Liane : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        print("COLISIONO");
    }

    // Update is called once per frame
    void Update()
    {
       //QUE TENGO QUE HACER, HACER QUE CUANDO CHOQUE LIANE TRUE, QUE ISGROUNDED Y ON COLLISION EXT LIANE FALSE
    }
    void OnTriggerEnter(Collider other)
    {
        print("COLISIONO");
        if(other.CompareTag("Player"))
        {
            print("hola");
            PlayerMovement controller = other.gameObject.GetComponent<PlayerMovement>();
            controller.Invoke("OnLiane", 0);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            print("chau");
            PlayerMovement controller = other.gameObject.GetComponent<PlayerMovement>();
            controller.Invoke("OffLiane", 0);
        }
    }
}
