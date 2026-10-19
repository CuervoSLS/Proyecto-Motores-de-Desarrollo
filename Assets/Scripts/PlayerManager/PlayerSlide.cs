using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSlide : MonoBehaviour
{
    [Header("Deslizar")]
    [SerializeField] private float _slideDuration = 1f;

    private PlayerMovement _p;
    private float _slideTimer;

    public bool Sliding { get; set; }

    public void Init(PlayerMovement p)
    {
        _p = p;
    }

    public void Tick()
    {
        if (!Sliding) return;

        _slideTimer -= Time.deltaTime;
        if (_slideTimer <= 0f)
        {
            Sliding = false;
        }
    }

    public void OnSlide(InputAction.CallbackContext context)
    {
        if (!context.performed || !_p.Sprint.Sprinting || !_p.Ground.Grounded || _p.Dash.Dashing) return;

        Sliding = true;
        _p.Sprint.Sprinting = false;
        _slideTimer = _slideDuration;
    }
}