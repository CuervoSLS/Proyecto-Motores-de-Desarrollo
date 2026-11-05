using UnityEngine.AI;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Transform target;
    private Vector3 originalPosition;
    private bool move = false;
    private NavMeshAgent agent;

    private PlayerSpawn referenceReset;
    private ActivateFinalSecuence sequence;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = gameObject.transform.position;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        target = player.gameObject.transform;
        referenceReset = player.GetComponent<PlayerSpawn>();
        referenceReset.SendRespawnee += StopMoving;
        GameObject reliquia = GameObject.FindGameObjectWithTag("Reliquia");
        sequence = reliquia.GetComponent<ActivateFinalSecuence>();
        sequence.IniciateSequence += StartSequence;
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(move == true)
        {
            agent.SetDestination(target.position);
        }
    }
    public void Move()
    {
        move = true;
    }
    public void StopMoving(bool reset)
    {
        CancelInvoke("Move");
        Debug.Log("Hola");
        move = false;
        gameObject.transform.position = originalPosition;
        agent.SetDestination(gameObject.transform.position);
        Invoke("Move", 5);
        Debug.Log("Hola2");
    }
    public void StartSequence(bool start)
    {
        Invoke("Move", 6);
    }
}
