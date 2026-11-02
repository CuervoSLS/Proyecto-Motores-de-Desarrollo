using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MenuController : MonoBehaviour
{
    //Referencias
    [SerializeField] private GameObject rainOff;
    [SerializeField] private GameObject rainOn;

    //El nombre de un nivel siempre debera de estar escrito como: "Nivel ". De lo contrario No coincidira!
    public void OpenLevel(int levelId)
    {
        string levelName = "Nivel " + levelId;
        SceneManager.LoadScene(levelName);
    }

    //Check para activar el Boton "On" u "Off"
    public void Check()
    {
        if (PlayerPrefs.GetInt("rain") == 1)
        {
            rainOff.SetActive(false);
            rainOn.SetActive(true);
        }
        else
        {
            rainOff.SetActive(true);
            rainOn.SetActive(false);
        }
    }

    //Activacion y Desactivacion de la opcion "Rain", usando "PlayerPrefs"
    public void ActivateRain()
    {
        PlayerPrefs.SetInt("rain", 1);
    }

    public void DeactivateRain()
    {
        PlayerPrefs.SetInt("rain", 0);
    }
}
