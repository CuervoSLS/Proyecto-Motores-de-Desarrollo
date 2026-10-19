using UnityEngine;

public class PlayerWallRun : MonoBehaviour
{
    [Header("Wall Run")]
    [SerializeField] private LayerMask _wallMask = ~0;
    [SerializeField] private float _wallCheckDistance = 0.7f;
    [SerializeField] private float _wallRunSpeed = 8f;
    [SerializeField] private float _wallRunGravity = -1f;

    private PlayerMovement _p;
    private bool _paredBloqueada;
    private Vector3 _wallNormal;

    public bool WallRunning { get; private set; }
    public int LadoPared { get; private set; }

    public void Init(PlayerMovement p)
    {
        _p = p;
    }

    public void Manejar()
    {
        WallRunning = false;

        Vector2 move = _p.Move;
        int lado = move.x < -0.1f ? -1 : move.x > 0.1f ? 1 : 0;

        if (_p.Ground.Grounded || _p.Liana.EnLiana || lado == 0)
        {
            LadoPared = 0;
            _paredBloqueada = false;
            return;
        }

        if (LadoPared != 0 && lado != LadoPared)
        {
            LadoPared = 0;
            _paredBloqueada = true;
        }

        if (_paredBloqueada) return;

        if (Physics.Raycast(transform.position, transform.right * lado, out RaycastHit hit, _wallCheckDistance, _wallMask))
        {
            WallRunning = true;
            LadoPared = lado;
            _wallNormal = hit.normal;
            _p.Jump.JumpsRestantes = _p.Jump.MaxJumps;
        }
        else
        {
            LadoPared = 0;
        }
    }

    public void Mover()
    {
        _p.VerticalVelocity = _wallRunGravity;
        _p.Dash.LimpiarInercia();
        Vector3 wallMove = Vector3.ProjectOnPlane(transform.forward, _wallNormal).normalized * _wallRunSpeed;
        wallMove.y = _p.VerticalVelocity;
        _p._controller.Move(wallMove * Time.deltaTime);
    }
}