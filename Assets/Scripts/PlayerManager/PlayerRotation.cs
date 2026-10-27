using Unity.Cinemachine;
using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    [Header("Rotacion")]
    [SerializeField] private float _rotateSpeed = 100f;
    [SerializeField] private Transform _camara;
    [SerializeField] private bool _alinearConCamara = true;
    [SerializeField] private float _velocidadAlinearCamara = 540f;
    [SerializeField] private float _umbralLateral = 0.1f;

    [Header("Cuerda")]
    [SerializeField] private Transform _objetoInclinacion;
    [SerializeField] private Vector3 _posicionEnCuerda = new Vector3(0f, -2f, -0.4f);
    [SerializeField] private bool _copiarRotacionX = true;
    [SerializeField] private bool _copiarRotacionZ = true;
    [SerializeField] private float _anguloAdelanteAtras = 15f;
    [SerializeField] private float _anguloLateral = 15f;
    [SerializeField] private float _velocidadInclinacion = 10f;
    [SerializeField] private float _velocidadRestaurar = 8f;

    private PlayerMovement _p;
    private Transform _padreOriginal;
    private Vector3 _posLocalInicial;
    private Quaternion _rotLocalInicial = Quaternion.identity;
    private Cuerda _cuerda;
    private bool _enganchado;
    private bool _restaurando;

    public void Init(PlayerMovement p)
    {
        _p = p;
        ResolverCamara();

        if (_objetoInclinacion == null) return;

        _padreOriginal = _objetoInclinacion.parent;
        _posLocalInicial = _objetoInclinacion.localPosition;
        _rotLocalInicial = _objetoInclinacion.localRotation;
    }

    public void Manejar()
    {
        if (_p.Rope.EnCuerda)
        {
            if (_objetoInclinacion != null) SeguirCuerda();
            return;
        }

        if (_enganchado) DesengancharObjeto();
        if (_restaurando) RestaurarObjeto();

        if (_p.Liana.EnLiana || _p.Dash.Dashing) return;

        Vector2 move = _p.Move;
        if (move.sqrMagnitude < 0.01f) return;

        float rotation = (_p.Rotate + _p.Look.x) * _rotateSpeed * Time.deltaTime;
        if (rotation != 0f)
        {
            transform.Rotate(0f, rotation, 0f);
        }

        if (!_alinearConCamara || _p.WallRun.WallRunning || _p.Slide.Sliding) return;
        if (!MoviendoseSoloAdelanteAtras(move)) return;

        if (_camara == null) ResolverCamara();
        if (_camara == null) return;

        float diferencia = Mathf.DeltaAngle(transform.eulerAngles.y, _camara.eulerAngles.y);
        float maxPaso = _velocidadAlinearCamara * Time.deltaTime;
        transform.Rotate(0f, Mathf.Clamp(diferencia, -maxPaso, maxPaso), 0f);
    }

    private void SeguirCuerda()
    {
        float yaw = transform.eulerAngles.y;

        if (!_enganchado)
        {
            _cuerda = BuscarCuerdaAgarrada();
            if (_cuerda == null) return;

            _cuerda.ConfigurarEjes(_copiarRotacionX, _copiarRotacionZ, yaw);
            _objetoInclinacion.SetParent(_cuerda.PuntoAgarre, true);
            _enganchado = true;
            _restaurando = false;
        }

        _cuerda.ConfigurarEjes(_copiarRotacionX, _copiarRotacionZ, yaw);

        Vector2 input = _p.Move;
        Quaternion inclinacionInput = Quaternion.Euler(input.y * _anguloAdelanteAtras, 0f, -input.x * _anguloLateral);

        Quaternion baseMundo = _padreOriginal != null ? _padreOriginal.rotation : Quaternion.identity;
        Quaternion baseLocal = Quaternion.Inverse(transform.rotation) * baseMundo * _rotLocalInicial;
        Quaternion destino = inclinacionInput * baseLocal;

        float paso = 1f - Mathf.Exp(-_velocidadInclinacion * Time.deltaTime);
        _objetoInclinacion.localRotation = Quaternion.Slerp(_objetoInclinacion.localRotation, destino, paso);
        _objetoInclinacion.localPosition = Vector3.Lerp(_objetoInclinacion.localPosition, _posicionEnCuerda, paso);
    }

    private void DesengancharObjeto()
    {
        _objetoInclinacion.SetParent(_padreOriginal, true);
        _cuerda = null;
        _enganchado = false;
        _restaurando = true;
    }

    private void RestaurarObjeto()
    {
        float paso = 1f - Mathf.Exp(-_velocidadRestaurar * Time.deltaTime);

        Vector3 pos = Vector3.Lerp(_objetoInclinacion.localPosition, _posLocalInicial, paso);
        Quaternion rot = Quaternion.Slerp(_objetoInclinacion.localRotation, _rotLocalInicial, paso);

        bool llego = (pos - _posLocalInicial).sqrMagnitude < 0.0001f && Quaternion.Angle(rot, _rotLocalInicial) < 0.1f;
        if (llego)
        {
            pos = _posLocalInicial;
            rot = _rotLocalInicial;
            _restaurando = false;
        }

        _objetoInclinacion.localPosition = pos;
        _objetoInclinacion.localRotation = rot;
    }

    private Cuerda BuscarCuerdaAgarrada()
    {
        for (int i = 0; i < Cuerda.Todas.Count; i++)
        {
            if (Cuerda.Todas[i].Agarrada) return Cuerda.Todas[i];
        }

        return null;
    }

    private bool MoviendoseSoloAdelanteAtras(Vector2 move)
    {
        return Mathf.Abs(move.y) > 0.1f && Mathf.Abs(move.x) < _umbralLateral;
    }

    private void ResolverCamara()
    {
        Transform candidata = _camara;
        if (candidata == null && Camera.main != null) candidata = Camera.main.transform;

        if (candidata == null) return;

        if (candidata.IsChildOf(transform))
        {
            _camara = null;
            return;
        }

        _camara = candidata;
    }
}