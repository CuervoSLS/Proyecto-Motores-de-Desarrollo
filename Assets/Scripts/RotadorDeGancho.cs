using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ProBuilder.AutoUnwrapSettings;

public class RotadorDeGancho : MonoBehaviour
{
    public Transform playerTransform;

    [SerializeField]private bool rotar = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerTransform = player.transform;
        rotar = true;
        Invoke("StopRotar", 1);
    }

    // Update is called once per frame
    void Update()
    {
        if(rotar == true)
        {
            Vector3 direccion = playerTransform.position - gameObject.transform.position;

            gameObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, direccion);
        }
    }
    void StopRotar()
    {
        rotar = false;
    }
}
