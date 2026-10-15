using UnityEngine;

public class SpawnObject : MonoBehaviour
{
    [SerializeField] private GameObject prefabEsfera;
    [SerializeField] private Transform puntoDeOrigen;
    [SerializeField] private float TimeRock;
    private float timer = 0.0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= TimeRock)
        {
            Instantiate(prefabEsfera, transform.position, Quaternion.identity);
            timer = 0.0f;
        }
    }
}
