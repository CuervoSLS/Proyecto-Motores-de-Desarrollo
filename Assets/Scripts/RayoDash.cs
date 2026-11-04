using System.Collections;
using UnityEngine;
using Benjathemaker;

[RequireComponent(typeof(Collider))]
public class RayoDash : MonoBehaviour
{
    [Header("Sonido")]
    [SerializeField] private AudioSource _fuente;
    [SerializeField] private AudioClip[] _clips;
    [SerializeField, Range(0f, 1f)] private float _volumen = 1f;

    [Header("Recarga")]
    [SerializeField] private int _dashesQueDa = 1;

    [Header("Animacion al agarrarlo")]
    [SerializeField] private float _duracionEncogido = 0.5f;

    [Header("Reaparicion")]
    [SerializeField] private bool _reaparecer = true;
    [SerializeField] private float _tiempoReaparicion = 10f;

    private Collider _collider;
    private Renderer[] _renderers;
    private SimpleGemsAnim _animacion;
    private Vector3 _escalaInicial;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _renderers = GetComponentsInChildren<Renderer>(true);
        _animacion = GetComponent<SimpleGemsAnim>();
        _escalaInicial = transform.localScale;

        if (_fuente == null) _fuente = GetComponent<AudioSource>();
        if (_fuente != null) _fuente.playOnAwake = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerDash dash = other.GetComponentInParent<PlayerDash>();
        if (dash == null || !dash.AgregarDashes(_dashesQueDa)) return;

        Sonar();
        StartCoroutine(Tomar());
    }

    private void Sonar()
    {
        if (_fuente == null || _clips == null || _clips.Length == 0) return;

        AudioClip clip = _clips.Length == 1 ? _clips[0] : _clips[Random.Range(0, _clips.Length)];
        if (clip != null) _fuente.PlayOneShot(clip, _volumen);
    }

    private IEnumerator Tomar()
    {
        _collider.enabled = false;
        if (_animacion != null) _animacion.enabled = false;

        yield return Encoger();

        MostrarRenderers(false);
        transform.localScale = _escalaInicial;

        if (!_reaparecer) yield break;

        yield return new WaitForSeconds(_tiempoReaparicion);

        MostrarRenderers(true);
        _collider.enabled = true;
        if (_animacion != null) _animacion.enabled = true;
    }

    private IEnumerator Encoger()
    {
        if (_duracionEncogido <= 0f)
        {
            transform.localScale = Vector3.zero;
            yield break;
        }

        Vector3 desde = transform.localScale;
        float tiempo = 0f;

        while (tiempo < _duracionEncogido)
        {
            tiempo += Time.deltaTime;
            transform.localScale = Vector3.Lerp(desde, Vector3.zero, tiempo / _duracionEncogido);
            yield return null;
        }

        transform.localScale = Vector3.zero;
    }

    private void MostrarRenderers(bool visible)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].enabled = visible;
        }
    }
}