using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Rota el objeto en el eje Y según el movimiento horizontal del mouse,
/// solo mientras se mantiene apretado el click izquierdo.
/// </summary>
public class RotacionMouseY : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private string nombreAccion = "Look";

    [Header("Rotación")]
    [SerializeField] private float sensibilidad = 0.1f;
    [SerializeField] private bool invertir = false;

    [Header("Condición")]
    [Tooltip("Si se asigna, la rotación solo funciona mientras este objeto esté activo en la escena. Si está vacío, no se aplica esta condición.")]
    [SerializeField] private GameObject objetoActivador;

    private InputAction accionLook;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = GetComponentInParent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("RotacionMouseY: no se encontró un PlayerInput.", this);
            enabled = false;
            return;
        }

        accionLook = playerInput.actions[nombreAccion];
    }

    private void Update()
    {
        // Si hay un objeto activador asignado, solo funciona mientras esté activo
        if (objetoActivador != null && !objetoActivador.activeInHierarchy) return;

        // Solo rota mientras el click izquierdo está apretado
        if (Mouse.current == null || !Mouse.current.leftButton.isPressed) return;

        float deltaX = accionLook.ReadValue<Vector2>().x;
        if (invertir) deltaX = -deltaX;

        transform.Rotate(0f, deltaX * sensibilidad, 0f, Space.World);
    }
}