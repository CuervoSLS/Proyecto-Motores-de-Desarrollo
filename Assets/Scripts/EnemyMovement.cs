using UnityEngine.AI;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform spawn;

    private Animator anim;
    private bool move = false;
    private NavMeshAgent agent;

    private PlayerSpawn referenceReset;
    private ActivateFinalSecuence sequence;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();
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
            anim.SetBool("Moving", true);
            agent.SetDestination(target.position);
        }
    }
    public void Move()
    {
        move = true;
        transform.position = spawn.position;
    }
    public void StopMoving(bool reset)
    {
        anim.SetBool("Moving", false);

        CancelInvoke("Move");
        move = false;

        agent.ResetPath();
        agent.Warp(spawn.position);

        Invoke("Move", 5);

    }
    public void StartSequence(bool start)
    {
        Invoke("Move", 6);
    }
}
