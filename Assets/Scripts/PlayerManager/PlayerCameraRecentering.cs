using Unity.Cinemachine;
using UnityEngine;

public class PlayerCameraRecentering : MonoBehaviour
{
    [Header("Recentering de camara")]
    [SerializeField] private CinemachineOrbitalFollow _orbital;
    [SerializeField] private bool _recentrarVertical = false;

    private PlayerMovement _p;
    private bool _recenteringActivo = true;

    public void Init(PlayerMovement p)
    {
        _p = p;
        if (_orbital == null) _orbital = FindFirstObjectByType<CinemachineOrbitalFollow>();
    }

    public void Actualizar()
    {
        if (_orbital == null) return;

        Vector2 move = _p.Move;
        bool activar = !_p.Rope.EnCuerda && !_p.Liana.EnLiana && Mathf.Abs(move.y) > 0.1f && Mathf.Abs(move.x) < 0.1f;
        if (activar == _recenteringActivo) return;
        _recenteringActivo = activar;

        InputAxis horizontal = _orbital.HorizontalAxis;
        horizontal.Recentering.Enabled = activar;
        _orbital.HorizontalAxis = horizontal;

        if (_recentrarVertical)
        {
            InputAxis vertical = _orbital.VerticalAxis;
            vertical.Recentering.Enabled = activar;
            _orbital.VerticalAxis = vertical;
        }
    }
}