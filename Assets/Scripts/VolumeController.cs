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
    [SerializeField] private Slider vozSlider;

    //Rango del mixer para SFX y Music (en dB)
    private const float SFX_MIN_DB = -80f;
    private const float SFX_MAX_DB = 10f;
    private const float MUSIC_MIN_DB = -80f;
    private const float MUSIC_MAX_DB = 10f;

    //Check para las "PlayerPrefs"
    private void Start()
    {
        if (PlayerPrefs.HasKey("musicVolume") || PlayerPrefs.HasKey("sfxVolume") || PlayerPrefs.HasKey("ambVolume") || PlayerPrefs.HasKey("vozVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSFXVolume();
            SetAmbVolume();
            SetVozVolume();
        }
    }

    //Set del volumen de cada canal ("Music", "SFX", "Ambience", "Voz")
    public void SetMusicVolume()
    {
        float music = musicSlider.value;

        // Slider (min a max) -> de -80 dB a +10 dB
        float t = Mathf.InverseLerp(musicSlider.minValue, musicSlider.maxValue, music);
        float musicDb = Mathf.Lerp(MUSIC_MIN_DB, MUSIC_MAX_DB, t);

        myMixer.SetFloat("music", musicDb);
        PlayerPrefs.SetFloat("musicVolume", music);
    }

    public void SetSFXVolume()
    {
        float sfx = sfxSlider.value;

        // Slider (min a max) -> de -80 dB a +10 dB
        float t = Mathf.InverseLerp(sfxSlider.minValue, sfxSlider.maxValue, sfx);
        float sfxDb = Mathf.Lerp(SFX_MIN_DB, SFX_MAX_DB, t);

        myMixer.SetFloat("sfx", sfxDb);
        PlayerPrefs.SetFloat("sfxVolume", sfx);
    }

    public void SetAmbVolume()
    {
        float amb = ambSlider.value;
        myMixer.SetFloat("amb", Mathf.Log10(amb) * 20);
        PlayerPrefs.SetFloat("ambVolume", amb);
    }

    public void SetVozVolume()
    {
        float voz = vozSlider.value;
        myMixer.SetFloat("voz", Mathf.Log10(voz) * 20);
        PlayerPrefs.SetFloat("vozVolume", voz);
    }

    //Carga para el ajuste guardado del volumen, usando "PlayerPrefs"
    private void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume", musicSlider.value);
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume", sfxSlider.value);
        ambSlider.value = PlayerPrefs.GetFloat("ambVolume", ambSlider.value);
        vozSlider.value = PlayerPrefs.GetFloat("vozVolume", vozSlider.value);

        SetMusicVolume();
        SetSFXVolume();
        SetAmbVolume();
        SetVozVolume();
    }
}