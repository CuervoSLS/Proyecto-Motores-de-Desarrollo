using System.Collections;
using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager instance = null;
    [SerializeField] GameObject platformPrefab;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Instantiate(platformPrefab, new Vector3(5.5f, 15.25f, -221f), platformPrefab.transform.rotation);
        Instantiate(platformPrefab, new Vector3(-1.5f, 15.25f, -221f), platformPrefab.transform.rotation);
        Instantiate(platformPrefab, new Vector3(-8.5f, 15.25f, -221f), platformPrefab.transform.rotation);
    }

    IEnumerator SpawnPlatform(Vector3 spawnPosition)
    {
        yield return new WaitForSeconds(5f);
        Instantiate(platformPrefab, spawnPosition, platformPrefab.transform.rotation);
    }
}
