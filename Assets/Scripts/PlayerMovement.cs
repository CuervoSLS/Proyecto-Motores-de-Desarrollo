using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float _normalSpeed = 7.5f;
    [SerializeField] private float _sprintSpeed = 15f;


    [SerializeField] private float _lianeSpeed = 5f;
    [SerializeField] private float _slideSpeed = 17f;

    [Header("Rotacion")]

    [SerializeField] private float _rotateSpeed = 100f;

    [Header("Salto y gravedad")]
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _gravity = -9.8f;

    [SerializeField] private float _gravityLiane = -9.8f;

    [Header("Carama (modo hijo")]

    [SerializeField] private Transform _camaraTransform;
    [Header("Objeto a instanciar")]
    [SerializeField] GameObject GameObjectPrefab;

    [SerializeField] private bool _liane = false;
    public CharacterController _controller;
    [SerializeField] private float _speed;
    private Vector2 _move;
    private float _rotate;
    private Vector2 _look;
    private float _verticalVelocity;
    private float _pitch;
    private bool _sprinting = false;
    private bool _sliding = false;

    private void Awake()
    {
        _controller = GetComponent <CharacterController>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _speed = _normalSpeed;
    }

    // Update is called once per frame
    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleLook();
    }
    private void FixedUpdate()
    {
        
    }
    private void HandleMovement()
    {
        if (_liane == true) //comprueba si esta en liana
        {
           if (Keyboard.current.shiftKey.isPressed == false) //comprueba el shift ya que con eso podes moverte libremente por la liana
            {
                _verticalVelocity = 0;
                _speed = _lianeSpeed;
                Vector3 movement = transform.right * _move.x;

                if (Keyboard.current.wKey.isPressed)
                {
                    movement = Vector3.up;
                }
                else if (Keyboard.current.sKey.isPressed)
                {
                    movement = Vector3.down;
                }

                movement = movement.normalized * _speed;

                _controller.Move(movement * Time.deltaTime);
                

                return;
            }
           
        }
        Vector3 move = transform.forward * _move.y + transform.right * _move.x; //el movimiento normal, no se pone dentro de un else porque no afecta a nada.

        move = move.normalized * _speed;

        if(_liane == true) //comprueba si esta en liana para asi hacer que caiga mas lento si esta en esta
        {
            _verticalVelocity += _gravityLiane * Time.deltaTime;
        }
        else
        {
            _verticalVelocity += _gravity * Time.deltaTime;
        }
        move.y = _verticalVelocity;
        _controller.Move(move * Time.deltaTime);
    }
    private void HandleRotation()
    {
        float rotation = _rotate * _rotateSpeed * Time.deltaTime;
        transform.Rotate(0, rotation, 0);
    }
    private void HandleLook()
    {
        float mouseX = _look.x * _rotateSpeed * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);
    }
    
    public void OnMovement(InputAction.CallbackContext context)
    {
        {
            _move = context.ReadValue<Vector2>();
        }
         
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        _look = context.ReadValue<Vector2>();
    }
    public void OnRotate(InputAction.CallbackContext context)
    {
        _rotate = context.ReadValue<float>();
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_liane == false)
            {
                _speed = _sprintSpeed;
                _sprinting = true;
            }
            else
            {
                _sprinting = true;
                _speed = _normalSpeed;
            }
        }
        else if (context.canceled)
            {
                _sprinting = false;
                if (_sliding == false)
                {
                    _speed = _normalSpeed;
                }
                
            }
    }
    public void OnSlide(InputAction.CallbackContext context)
    {
        print("te detecte");
        if (context.performed)
        {
            print("JIJI DE VUELTA");
            if(_sprinting == true && _controller.isGrounded)
            {
                print("estoy slidenado");
                _speed = _slideSpeed;
                _sliding = true;
                _sprinting = false;
                Invoke("CancelSlide",1);
            }
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed && _controller.isGrounded || context.performed && _liane == true)
        {

            _verticalVelocity = Mathf.Sqrt(_jumpForce * -2f * _gravity);
            if (_sliding == true)
            {
                _sliding = false;
                CancelInvoke("CancelSlide");
                _speed = _sprintSpeed;
                _sprinting = true;
            }
        }
    }
    public void OnLiane()
    {
       
        print("hola de vuelta");
        _liane = true;
    }
    public void OffLiane()
    {
        print("chau de vuelta");
        _liane = false;
        if(_sprinting == true)
        {
            _speed = _sprintSpeed;
        }
    }
    public void CancelSlide()
    {
        print("ahora no");

        _sliding = false;
        if (_sprinting == true)
        {
            _speed = _sprintSpeed;
        }
        else
        {
            _speed = _normalSpeed;
        }
    }
}

