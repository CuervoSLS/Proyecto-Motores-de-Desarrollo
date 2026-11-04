using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSprint : MonoBehaviour
{
    [Header("Correr automatico (Shift para caminar)")]
    [SerializeField] private bool _bloquearSprintEnAire = true;
    [SerializeField] private float _deceleracionAire = 4f;

    private PlayerMovement _p;
    private bool _caminarPresionado;

    public bool Sprinting { get; set; }
    public bool BloquearEnAire => _bloquearSprintEnAire;
    public float DeceleracionAire => _deceleracionAire;

    public void Init(PlayerMovement p)
    {
        _p = p;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.started || context.performed) _caminarPresionado = true;
        else if (context.canceled) _caminarPresionado = false;
    }

    public void Detectar()
    {
        if (_caminarPresionado || _p.Liana.EnLiana || _p.Move.y <= 0.1f)
        {
            Sprinting = false;
            return;
        }

        if (Sprinting) return;

        if (!_p.Slide.Sliding && (!_bloquearSprintEnAire || _p.Ground.Grounded))
        {
            Sprinting = true;
        }
    }
}