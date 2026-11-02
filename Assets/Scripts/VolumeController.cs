using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    //Referencias
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider ambSlider;

    //Check para las "PlayerPrefs"
    private void Start()
    {
        if (PlayerPrefs.HasKey("musicVolume") || PlayerPrefs.HasKey("sfxVolume") || PlayerPrefs.HasKey("ambVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSFXVolume();
            SetAmbVolume();
        }
    }

    //Set del volumen de cada canal ("Music", "SFX", "Ambience")
    public void SetMusicVolume()
    {
        float music = musicSlider.value;
        myMixer.SetFloat("music", Mathf.Log10(music) * 20);
        PlayerPrefs.SetFloat("musicVolume", music);
    }

    public void SetSFXVolume()
    {
        float sfx = sfxSlider.value;
        myMixer.SetFloat("sfx", Mathf.Log10(sfx) * 20);
        PlayerPrefs.SetFloat("sfxVolume", sfx);
    }

    public void SetAmbVolume()
    {
        float amb = ambSlider.value;
        myMixer.SetFloat("amb", Mathf.Log10(amb) * 20);
        PlayerPrefs.SetFloat("ambVolume", amb);
    }

    //Carga para el ajuste guardado del volumen, usando "PlayerPrefs"
    private void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
        ambSlider.value = PlayerPrefs.GetFloat("ambVolume");

        SetMusicVolume();
        SetSFXVolume();
        SetAmbVolume();
    }
}
