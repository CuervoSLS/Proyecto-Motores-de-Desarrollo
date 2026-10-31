using UnityEngine;
using UnityEngine.Audio;

public class LayeredMusic : MonoBehaviour
{
    public static LayeredMusic Instance { get; private set; }

    public AudioClip kick;
    public AudioClip shaker;
    public AudioClip perc;
    public AudioClip snare;
    public AudioClip bass;

    public AudioMixerGroup exit;

    [Range(0f, 1f)] public float intensity;

    private AudioSource layerKick;
    private AudioSource layerShaker;
    private AudioSource layerPerc;
    private AudioSource layerSnare;
    private AudioSource layerBass;

    public LayeredMusic()
    {
        Instance = this;
    }

    private AudioSource CreateSource(AudioClip clip)
    {
        AudioSource x = gameObject.AddComponent<AudioSource>();
        x.playOnAwake = false;
        x.loop = true;
        x.spatialBlend = 0f;
        x.clip = clip;
        x.outputAudioMixerGroup = exit;
        x.volume = 0f;
        return x;
    }

    private void OnEnable()
    {
        if(kick == null || shaker == null || perc == null || snare == null || bass == null)
        {
            Debug.LogWarning("AudioClip missing!", this);
            return;
        }

        if (layerKick == null) layerKick = CreateSource(kick);
        if (layerShaker == null) layerShaker = CreateSource(shaker);
        if (layerPerc == null) layerPerc = CreateSource(perc);
        if (layerSnare == null) layerSnare = CreateSource(snare);
        if (layerBass == null) layerBass = CreateSource(bass);

        layerKick.clip = kick;
        layerShaker.clip = shaker;
        layerPerc.clip = perc;
        layerSnare.clip = snare;
        layerBass.clip = bass;
        layerKick.outputAudioMixerGroup = exit;
        layerShaker.outputAudioMixerGroup = exit;
        layerPerc.outputAudioMixerGroup = exit;
        layerSnare.outputAudioMixerGroup = exit;
        layerBass.outputAudioMixerGroup = exit;

        double start = AudioSettings.dspTime + 0.2f;
        layerKick.PlayScheduled(start);
        layerShaker.PlayScheduled(start);
        layerPerc.PlayScheduled(start);
        layerSnare.PlayScheduled(start);
        layerBass.PlayScheduled(start);
    }

    private void OnDisable()
    {
        if (layerKick != null) layerKick.Stop();
        if (layerShaker != null) layerShaker.Stop();
        if (layerPerc != null) layerPerc.Stop();
        if (layerSnare != null) layerSnare.Stop();
        if (layerBass != null) layerBass.Stop();
    }

    private void Update()
    {
        if (layerKick != null) layerKick.volume = 0.3f;
        if (layerShaker != null && intensity >= 0.25f) layerShaker.volume = Mathf.MoveTowards(layerShaker.volume, 0.3f * intensity, Time.unscaledDeltaTime);
        if (layerPerc != null && intensity >= 0.5f) layerPerc.volume = Mathf.MoveTowards(layerPerc.volume, 0.3f * intensity, Time.unscaledDeltaTime);
        if (layerSnare != null && intensity >= 0.75f) layerSnare.volume = Mathf.MoveTowards(layerSnare.volume, 0.3f * intensity, Time.unscaledDeltaTime);
        if (layerBass != null && intensity >= 1f) layerBass.volume = Mathf.MoveTowards(layerBass.volume, 0.3f * intensity, Time.unscaledDeltaTime);
        if (layerShaker != null && intensity < 0.25f) layerShaker.volume = 0f;
        if (layerPerc != null && intensity < 0.5f) layerPerc.volume = 0f;
        if (layerSnare != null && intensity < 0.75f) layerSnare.volume = 0f;
        if (layerBass != null && intensity < 1f) layerBass.volume = 0f;
    }
}
