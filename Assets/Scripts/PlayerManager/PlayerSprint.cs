using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSprint : MonoBehaviour
{
    [Header("Sprint (mantener Shift)")]
    [SerializeField] private bool _bloquearSprintEnAire = true;
    [SerializeField] private float _deceleracionAire = 4f;

    private PlayerMovement _p;
    private bool _teclaPresionada;

    public bool Sprinting { get; set; }
    public bool BloquearEnAire => _bloquearSprintEnAire;
    public float DeceleracionAire => _deceleracionAire;

    public void Init(PlayerMovement p)
    {
        _p = p;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started || context.performed) _teclaPresionada = true;
        else if (context.canceled) _teclaPresionada = false;
    }

    public void Detectar()
    {
        if (!_teclaPresionada || _p.Liana.EnLiana)
        {
            Sprinting = false;
            return;
        }

        if (Sprinting)
        {
            if (_p.Move.y <= 0.1f) Sprinting = false;
            return;
        }

        if (_p.Move.y > 0.1f && !_p.Slide.Sliding && (!_bloquearSprintEnAire || _p.Ground.Grounded))
        {
            Sprinting = true;
        }
    }
}