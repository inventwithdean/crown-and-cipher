using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCWander : MonoBehaviour
{
    public float wanderRadius = 20f;
    public float wanderTimer = 6f;

    private NavMeshAgent agent;
    private Animator animator;
    private float timer;

    void OnEnable()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        timer = wanderTimer;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= wanderTimer && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, agent.areaMask);
            agent.SetDestination(newPos);
            timer = 0f;
        }
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(randDirection, out navHit, dist, layermask))
        {
            return navHit.position;
        }
        return origin;
    }

    public void SetWanderState(bool isWandering)
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = !isWandering;
        }

        if (!isWandering)
        {
            GetComponent<Animator>().SetFloat("Speed", 0f);
        }
        
        enabled = isWandering;
    }
}
