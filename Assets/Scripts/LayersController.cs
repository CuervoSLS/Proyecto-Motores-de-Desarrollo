using UnityEngine;

public class LayersController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Layer_1")
        {
            LayeredMusic.Instance.intensity = 0f;
            Debug.Log("Entered Layer_1");
        }

        else if (other.gameObject.name == "Layer_2")
        {
            LayeredMusic.Instance.intensity = 0.74f;
            Debug.Log("Entered Layer_2");
        }

        else if (other.gameObject.name == "Layer_3")
        {
            LayeredMusic.Instance.intensity = 1f;
            Debug.Log("Entered Layer_3");
        }
    }
}
