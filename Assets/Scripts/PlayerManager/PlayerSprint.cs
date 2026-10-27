using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSprint : MonoBehaviour
{
    [Header("Sprint (doble toque de la W)")]
    [SerializeField] private float _doubleTapWindow = 0.3f;
    [SerializeField] private bool _bloquearSprintEnAire = true;
    [SerializeField] private float _deceleracionAire = 4f;

    private PlayerMovement _p;
    private float _lastWTapTime = -10f;

    public bool Sprinting { get; set; }
    public bool BloquearEnAire => _bloquearSprintEnAire;
    public float DeceleracionAire => _deceleracionAire;

    public void Init(PlayerMovement p)
    {
        _p = p;
    }

    public void Detectar()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb[Key.W].wasPressedThisFrame)
        {
            if (Time.time - _lastWTapTime <= _doubleTapWindow)
            {
                _lastWTapTime = -10f;
                if (!_p.Liana.EnLiana && !_p.Slide.Sliding && (!_bloquearSprintEnAire || _p.Ground.Grounded))
                {
                    Sprinting = true;
                }
            }
            else
            {
                _lastWTapTime = Time.time;
            }
        }

        if (Sprinting && _p.Move.y <= 0.1f)
        {
            Sprinting = false;
        }
    }
}