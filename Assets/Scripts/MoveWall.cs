using Unity.VisualScripting;
using UnityEngine;

public class MoveWall : MonoBehaviour
{
    private ActivateFinalSecuence sequence;
    private PlayerSpawn referenceReset;
    private bool move;
    [SerializeField] float movementX;
    [SerializeField] float movementY;

    [SerializeField] float movementZ;
    private Vector3 originalPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = gameObject.transform.position;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        referenceReset = player.GetComponent<PlayerSpawn>();
        referenceReset.SendRespawnee += Reset;
        GameObject reliquia = GameObject.FindGameObjectWithTag("Reliquia");
        sequence = reliquia.GetComponent<ActivateFinalSecuence>();
        sequence.IniciateSequence += StartSequence;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (move == true)
        {
            transform.Translate(movementX, movementY, movementZ);
        }
    }
    void StartSequence(bool start)
    {
        move = true;
        Invoke("StopMoving", 5);
    }
    void Reset(bool stop)
    {
        CancelInvoke("StopMoving");
        move = true;
        gameObject.transform.position = originalPosition;
        Invoke("StopMoving", 5);
    } 
    void StopMoving()
    {
        move = false;
    }
}
