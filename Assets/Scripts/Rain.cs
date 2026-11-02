using UnityEngine;

public class Rain : MonoBehaviour
{
    //Referencias
    [SerializeField] private GameObject psRain;
    [SerializeField] private AudioSource rain;

    //Check para activar o desactivar la lluvia
    void Start()
    {
        if (PlayerPrefs.GetInt("rain") == 1)
        {
            rain.Play();
            psRain.SetActive(true);
        }
        else
        {
            rain.Stop();
            psRain.SetActive(false);
        }
    }
}
