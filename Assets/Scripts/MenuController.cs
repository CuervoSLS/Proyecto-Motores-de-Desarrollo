using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    /// Referencias de los botones
    [SerializeField] private GameObject rainOff;
    [SerializeField] private GameObject rainOn;

    /// Referencia al script de clima (el que tiene ActualizarClima)
    [SerializeField] private Rain rainClima;

    // El nombre de un nivel siempre deberá estar escrito como: "Nivel ". De lo contrario no coincidirá!
    public void OpenLevel(int levelId)
    {
        string levelName = "Nivel " + levelId;
        SceneManager.LoadScene(levelName);
    }
    public void Start()
    {
        Check();
    }
    // Check para activar el botón "On" u "Off"
    public void Check()
    {
        bool llueve = PlayerPrefs.GetInt("rain") == 1;

        rainOff.SetActive(!llueve);
        rainOn.SetActive(llueve);
    }

    // Activación y desactivación de la opción "Rain", usando "PlayerPrefs"
    public void ActivateRain()
    {
        CambiarRain(1);
    }

    public void DeactivateRain()
    {
        CambiarRain(0);
    }

    /// Guarda el valor y actualiza el clima al instante
    private void CambiarRain(int valor)
    {
        PlayerPrefs.SetInt("rain", valor);

        // Llama al clima para que se refresque con el nuevo valor
        if (rainClima != null) rainClima.ActualizarClima();
    }
}