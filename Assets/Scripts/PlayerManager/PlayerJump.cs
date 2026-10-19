using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [Header("Salto")]
    [SerializeField] private float _jumpForce = 5f;

    [Header("Doble salto")]
    [SerializeField] private int _maxJumps = 2;

    private PlayerMovement _p;

    public int MaxJumps => _maxJumps;
    public int JumpsRestantes { get; set; }

    public void Init(PlayerMovement p)
    {
        _p = p;
        JumpsRestantes = _maxJumps;
    }

    public void Tick()
    {
        if (_p.Ground.Grounded && !_p.Liana.EnLiana)
        {
            JumpsRestantes = _maxJumps;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (_p.Rope.EnCuerda)
        {
            _p.Rope.Soltar();
            _p.VerticalVelocity += _p.Rope.ImpulsoSalto;
            JumpsRestantes = _maxJumps - 1;
            return;
        }

        if (_p.Ground.Grounded || _p.Liana.EnLiana)
        {
            if (_p.Liana.EnLiana)
            {
                _p.Liana.SoltarPorSalto();
            }

            _p.VerticalVelocity = JumpVelocity();
            JumpsRestantes = _maxJumps - 1;

            if (_p.Slide.Sliding)
            {
                _p.Slide.Sliding = false;
                _p.Sprint.Sprinting = true;
            }
            return;
        }

        if (JumpsRestantes > 0)
        {
            _p.VerticalVelocity = JumpVelocity();
            JumpsRestantes--;
        }
    }

    private float JumpVelocity()
    {
        return Mathf.Sqrt(_jumpForce * -2f * _p.Locomotion.Gravity);
    }
}