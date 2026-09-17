using UnityEngine;

public class SpawnObject : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject prefabEsfera;
    [SerializeField] private Transform puntoDeOrigen;
    [SerializeField] private float TimeRock;
    private float timer = 0.0f;
    void Start()
    {
        
    }

    // Update is called once per frame
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
