using UnityEngine;

public class Texto_Escena : MonoBehaviour
{
    public Transform player;
    public enum Eje { X, Y, Z }
    public Eje ejeACopiar = Eje.Y; // la rotación horizontal en Unity es el eje Y

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 miRot = transform.eulerAngles;
        Vector3 rotPlayer = player.eulerAngles;

        switch (ejeACopiar)
        {
            case Eje.X: miRot.x = rotPlayer.x; break;
            case Eje.Y: miRot.y = rotPlayer.y; break;
            case Eje.Z: miRot.z = rotPlayer.z; break;
        }

        transform.eulerAngles = miRot;
    }
}