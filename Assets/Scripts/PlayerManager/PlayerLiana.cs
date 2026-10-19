using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLiana : MonoBehaviour
{
    [Header("Liana")]
    [SerializeField] private float _lianeSpeed = 5f;
    [SerializeField] private float _lianePegado = 1f;
    [SerializeField] private float _bloqueoLianaTrasSalto = 0.4f;
    [SerializeField] private float _gravityLiane = -9.8f;

    [Header("Estado")]
    [SerializeField] private bool _liane = false;

    private PlayerMovement _p;
    private float _lianaBloqueo;
    private Liane _lianaActual;
    private Vector3 _lianeNormal;

    public bool EnLiana => _liane;
    public float Gravedad => _gravityLiane;
    public bool Escalando => _liane && !(Keyboard.current != null && Keyboard.current.shiftKey.isPressed);

    public void Init(PlayerMovement p)
    {
        _p = p;
    }

    public void Tick()
    {
        if (_lianaBloqueo > 0f)
        {
            _lianaBloqueo -= Time.deltaTime;
        }
    }

    public void Mover(float dt)
    {
        _p.VerticalVelocity = 0f;
        _p.Dash.LimpiarInercia();

        Vector2 move = _p.Move;
        Vector3 movement = move.y > 0.1f ? Vector3.up : move.y < -0.1f ? Vector3.down : Vector3.zero;

        _p._controller.Move((movement * _lianeSpeed - _lianeNormal * _lianePegado) * dt);
    }

    public void SoltarPorSalto()
    {
        if (_lianaActual != null) _lianaActual.Soltar();
        _liane = false;
        _lianaBloqueo = _bloqueoLianaTrasSalto;
    }

    public void OnLiane()
    {
        if (_lianaBloqueo <= 0f) _liane = true;
    }

    public void OffLiane()
    {
        _lianaActual = null;
        _liane = false;
    }

    public void RecibirImpulso(Vector3 impulso)
    {
        _p.VerticalVelocity += impulso.y;
        if (_p._controller.enabled)
        {
            _p._controller.Move(new Vector3(impulso.x, 0f, impulso.z) * Time.deltaTime);
        }
    }

    private void LateUpdate()
    {
        if (_p == null) return;

        if (_p._enLianaMovil && _p._controller.enabled)
        {
            _p._controller.Move(_p._velocidadLiana * Time.deltaTime);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (_p == null) return;

        if (hit.collider.TryGetComponent(out Liane liane))
        {
            Vector2 input = _p.InputUtil;
            if (_lianaBloqueo <= 0f && (_liane || (input.y > 0.1f && Mathf.Abs(input.x) < 0.1f)))
            {
                _lianeNormal = hit.normal;
                _lianaActual = liane;
                liane.RegistrarContacto(_p);
            }
        }
    }
}