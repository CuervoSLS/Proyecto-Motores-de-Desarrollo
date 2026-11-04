using UnityEngine;

public class FundidoAparicion : MonoBehaviour
{
    private static readonly int ColorBase = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorClasico = Shader.PropertyToID("_Color");

    private Renderer[] _renderers;
    private int[] _ids;
    private Color[] _colores;
    private MaterialPropertyBlock _bloque;
    private float _duracion;
    private float _tiempo;

    private void Awake()
    {
        Renderer[] todos = GetComponentsInChildren<Renderer>(true);
        int[] ids = new int[todos.Length];
        Color[] colores = new Color[todos.Length];
        Renderer[] validos = new Renderer[todos.Length];
        int cantidad = 0;

        for (int i = 0; i < todos.Length; i++)
        {
            Material material = todos[i].sharedMaterial;
            if (material == null) continue;

            int id;
            if (material.HasProperty(ColorBase)) id = ColorBase;
            else if (material.HasProperty(ColorClasico)) id = ColorClasico;
            else continue;

            validos[cantidad] = todos[i];
            ids[cantidad] = id;
            colores[cantidad] = material.GetColor(id);
            cantidad++;
        }

        _renderers = new Renderer[cantidad];
        _ids = new int[cantidad];
        _colores = new Color[cantidad];
        System.Array.Copy(validos, _renderers, cantidad);
        System.Array.Copy(ids, _ids, cantidad);
        System.Array.Copy(colores, _colores, cantidad);

        _bloque = new MaterialPropertyBlock();
        enabled = false;
    }

    public void Iniciar(float duracion)
    {
        if (_renderers.Length == 0) return;

        _duracion = Mathf.Max(0.01f, duracion);
        _tiempo = 0f;
        Aplicar(0f);
        enabled = true;
    }

    private void Update()
    {
        _tiempo += Time.deltaTime;
        float avance = Mathf.Clamp01(_tiempo / _duracion);

        if (avance >= 1f)
        {
            Limpiar();
            enabled = false;
            return;
        }

        Aplicar(avance);
    }

    private void Aplicar(float avance)
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            Color color = _colores[i];
            color.a *= avance;

            _renderers[i].GetPropertyBlock(_bloque);
            _bloque.SetColor(_ids[i], color);
            _renderers[i].SetPropertyBlock(_bloque);
        }
    }

    private void Limpiar()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            _renderers[i].SetPropertyBlock(null);
        }
    }
}