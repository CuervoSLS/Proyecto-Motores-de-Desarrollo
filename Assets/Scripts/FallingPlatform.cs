using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    bool isFalling = false;
    float downSpeed = 0;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.name == "Player")
        {
            isFalling = true;
            Destroy(gameObject, 5);

            PlatformManager.instance.StartCoroutine("SpawnPlatform", new Vector3(transform.position.x, transform.position.y, transform.position.z));

        }
    }

    void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.name == "Player")
        {
            GetComponent<BoxCollider>().enabled = false;
        }
    }

    void Update()
    {
        if (isFalling)
        {
            downSpeed += Time.deltaTime / 50;
            transform.position = new Vector3(transform.position.x, transform.position.y - downSpeed, transform.position.z);
        }
    }
}
