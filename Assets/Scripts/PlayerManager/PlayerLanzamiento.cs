using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLanzamiento : MonoBehaviour
{
    [Header("Modo lanzamiento")]
    [SerializeField] private bool _requiereGanchoEnMano = true;
    [SerializeField] private float _delayVolverCamara = 0.8f;

    private PlayerMovement _p;
    private AgarrarGancho _agarrar;
    private bool _presionado;
    private bool _activo;
    private bool _camaraActiva;
    private float _camaraHasta;

    public bool Activo => _activo;
    public bool CamaraActiva => _camaraActiva;

    public event Action<bool> CambioCamara;

    public void Init(PlayerMovement p)
    {
        _p = p;
        _agarrar = GetComponent<AgarrarGancho>();
    }

    public void OnLanzarGancho(InputAction.CallbackContext context)
    {
        if (context.started || context.performed) _presionado = true;
        else if (context.canceled) _presionado = false;
    }

    public void Actualizar()
    {
        bool activo = _presionado
            && !_p.Rope.EnCuerda
            && !_p.Liana.EnLiana
            && !_p.Dash.Dashing
            && !_p.Slide.Sliding
            && !_p.WallRun.WallRunning
            && (!_requiereGanchoEnMano || GanchoEnMano());

        _activo = activo;

        bool camara = _activo || Time.time < _camaraHasta;
        if (camara == _camaraActiva) return;

        _camaraActiva = camara;
        CambioCamara?.Invoke(_camaraActiva);
    }

    public void AlLanzar()
    {
        _camaraHasta = Time.time + _delayVolverCamara;
    }

    private bool GanchoEnMano()
    {
        if (_agarrar == null) return false;

        GanchoConCuerda gancho = _agarrar.Sostenido;
        return gancho != null && gancho.Agarrado;
    }
}