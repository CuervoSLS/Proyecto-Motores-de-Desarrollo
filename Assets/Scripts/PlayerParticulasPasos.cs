using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerParticulasPasos : MonoBehaviour
{
    [Header("Particulas en el piso")]
    [SerializeField] private GameObject[] _particulas;
    [SerializeField] private float _umbralMovimiento = 0.01f;

    private PlayerMovement _p;
    private bool _visibles = true;

    private void Awake()
    {
        _p = GetComponent<PlayerMovement>();
        Mostrar(false);
    }

    private void LateUpdate()
    {
        bool mostrar = _p.Ground.Grounded
            && _p.Move.sqrMagnitude > _umbralMovimiento
            && !_p.Liana.EnLiana
            && !_p.Rope.EnCuerda
            && !_p.WallRun.WallRunning
            && !_p.Dash.Dashing
            && !_p.Slide.Sliding;

        Mostrar(mostrar);
    }

    private void Mostrar(bool visible)
    {
        if (_visibles == visible || _particulas == null) return;

        _visibles = visible;

        for (int i = 0; i < _particulas.Length; i++)
        {
            GameObject particula = _particulas[i];
            if (particula != null) particula.SetActive(visible);
        }
    }
}