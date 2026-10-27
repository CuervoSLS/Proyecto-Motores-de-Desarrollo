using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float _normalSpeed = 7.5f;
    [SerializeField] private float _backSpeed = 5.5f;
    [SerializeField] private float _sprintSpeed = 15f;
    [SerializeField] private float _slideSpeed = 17f;
    [SerializeField] private float _velocidadMovimientoEnElAire = 5f;

    [Header("Gravedad")]
    [SerializeField] private float _gravity = -9.8f;
    [SerializeField] private float _groundedStickVelocity = -2f;
    [SerializeField] private float _multiplicadorCaida = 1.5f;

    private PlayerMovement _p;
    private float _velocidadActual;

    public float Gravity => _gravity;
    public float GroundedStickVelocity => _groundedStickVelocity;
    public float MultiplicadorCaida => _multiplicadorCaida;

    public void Init(PlayerMovement p)
    {
        _p = p;
        _velocidadActual = _normalSpeed;
    }

    public void Manejar()
    {
        float dt = Time.deltaTime;

        if (_p.Liana.Escalando)
        {
            _p.Liana.Mover(dt);
            return;
        }

        if (_p.WallRun.WallRunning)
        {
            _p.WallRun.Mover();
            return;
        }

        if (_p.Dash.Dashing)
        {
            _p.Dash.Mover(dt);
            return;
        }

        Vector2 input = _p.Move;
        Vector3 move = (transform.forward * input.y + transform.right * input.x).normalized * ObtenerVelocidad(dt);

        if (_p.Ground.Grounded && _p.VerticalVelocity < 0f)
        {
            _p.VerticalVelocity = _groundedStickVelocity;
        }

        bool enLiana = _p.Liana.EnLiana;
        float gravedad = enLiana ? _p.Liana.Gravedad : _gravity;
        if (_p.VerticalVelocity < 0f && !enLiana)
        {
            gravedad *= _multiplicadorCaida;
        }

        _p.VerticalVelocity += gravedad * dt;

        _p.Dash.AplicarInercia(ref move, dt);
        _p.Rope.AplicarSalida(ref move, dt);

        move.y = _p.VerticalVelocity;
        _p._controller.Move(move * dt);
    }

    private float ObtenerVelocidad(float dt)
    {
        bool enLiana = _p.Liana.EnLiana;
        bool enAire = !_p.Ground.Grounded && !enLiana;
        float baseSpeed = !enLiana && _p.Move.y < -0.1f ? _backSpeed : _normalSpeed;
        float velocidadCaminar = enAire ? Mathf.Min(baseSpeed, _velocidadMovimientoEnElAire) : baseSpeed;

        if (_p.Sprint.BloquearEnAire && enAire)
        {
            _velocidadActual = Mathf.Max(velocidadCaminar, _velocidadActual - _p.Sprint.DeceleracionAire * dt);
        }
        else
        {
            _velocidadActual = _p.Slide.Sliding ? _slideSpeed : (!enLiana && _p.Sprint.Sprinting) ? _sprintSpeed : velocidadCaminar;
        }

        return _velocidadActual;
    }
}