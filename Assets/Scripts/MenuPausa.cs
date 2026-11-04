using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInput playerInput;   // El PlayerInput con la action "Pausa"
    [SerializeField] private GameObject panelPausa;     // El panel/menú de pausa (hijo, NO el objeto de este script)
    [SerializeField] private BlurTransition blur;       // El script BlurTransition del Volume (se activa solo en pausa)
    [SerializeField] private GameObject objetoBloqueo;
    [SerializeField] private GameObject objetoBloqueo1; // Si está activo, NO se puede despausar hasta que se desactive

    [Header("Opciones")]
    [SerializeField] private bool controlarCursor = true;

    private InputAction accionPausa;
    private bool enPausa = false;

    private void Awake()
    {
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();

        accionPausa = playerInput.actions["Pausa"];
    }

    private void OnEnable()
    {
        accionPausa.performed += AlPresionarPausa;
    }

    private void OnDisable()
    {
        accionPausa.performed -= AlPresionarPausa;
    }

    private void Start()
    {
        // Arranca el juego con el menú cerrado
        AplicarPausa(false);
    }

    private void AlPresionarPausa(InputAction.CallbackContext context)
    {
        // Con la action se activa o desactiva el menú
        if (enPausa && DespausaBloqueada()) return;

        AplicarPausa(!enPausa);
    }

    private bool DespausaBloqueada()
    {
        return objetoBloqueo != null && objetoBloqueo.activeInHierarchy;
        return objetoBloqueo1 != null && objetoBloqueo.activeInHierarchy;
    }

    private void AplicarPausa(bool pausar)
    {
        enPausa = pausar;
        panelPausa.SetActive(pausar);

        if (blur != null)
            blur.Borroso = pausar;

        Time.timeScale = pausar ? 0f : 1f;

        if (controlarCursor)
        {
            Cursor.visible = pausar;
            Cursor.lockState = pausar ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    // ---------- Funciones para los botones ----------

    // Botón "Reanudar" (también sirve como botón para salir del menú)
    public void Reanudar()
    {
        if (DespausaBloqueada()) return;

        AplicarPausa(false);
    }

    // Botón "Reiniciar"
    public void ReiniciarEscena()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Botón "Menú principal" (escena 0)
    public void IrAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}