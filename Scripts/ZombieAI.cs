using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    [SerializeField]
    private float health = 100f;
    public float PathUpdates_ShortDelay = .1f; // 
    public float PathUpdates_LongDelay = 1f; // 
    public float ZombieMeleeRange = 1.5f;
    private float pathTimer = 0;

    // If the zombie is 25 meters away or less it will do more callculations per second
    public float RangeForFaster_NavCalculations = 25f;

    private Animator animator;
    private NavMeshAgent agent;
    private GameObject Player;
    private CharacterController m_Zombie;
    private PlayerSystems playerSystems;
    private UI_Controller ui_controller;
    private Spawner spawner;

    void Start()
    {
        spawner = FindObjectOfType<Spawner>();
        Player = GameObject.Find("Player");
        m_Zombie = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        playerSystems = FindObjectOfType<PlayerSystems>();
        ui_controller = FindObjectOfType<UI_Controller>();

        // Start checking for meleeing player after 5 seconds every 1 second
        InvokeRepeating("AttemptHitPlayer", 3.0f, .25f);

        // Set health
        health = spawner.RoundNumber + 50f;

        // Give zombies random stats
        float randStats = Random.Range(0f, 1f);
        agent.speed = agent.speed * randStats + agent.speed * .5f;
    }

    void Update()
    {
        if (agent == null || playerSystems.health <= 0)
            return;

        if(health <= 0)
        {
            KillZombie();
        }

        // TESTING AI USING SPAWNER TO OPTIMIZE AI
        pathTimer += Time.deltaTime;

        float distFromPlayer = Vector3.Distance(Player.transform.position, transform.GetChild(0).transform.position);
        
        // Check if the player is close enough have the zombie nav update faster
        if (distFromPlayer < RangeForFaster_NavCalculations)
        {
            StartCoroutine(UpdateZombiePath(PathUpdates_ShortDelay));
        
        }
        // Zombie is far so update nav less often
        else
        {
            StartCoroutine(UpdateZombiePath(PathUpdates_LongDelay));
        }
    }

    public IEnumerator UpdateZombiePath(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (agent)
        {
            agent.SetDestination(Player.transform.position);

        }
    }

    // Zombie attempts to hit player
    // TODO: Allow zombie to transfew to attack animation and connect this function to the attack
    private void AttemptHitPlayer()
    {
        // Zombie hitting player
        if (Vector3.Distance(Player.transform.position, transform.GetChild(0).transform.position) < ZombieMeleeRange)
        {
            animator.SetTrigger("Attack");
            
        }
    }
    public void MeleePlayer()
    {

        if (Vector3.Distance(Player.transform.position, transform.GetChild(0).transform.position) < ZombieMeleeRange)
        {
            playerSystems.HitPlayer(35);
        }
    }
    

    private void KillZombie()
    {
        // Get rid of nav mesh stuff
        Destroy(agent);
        //Destroy(animator);
        animator.SetTrigger("FallBackwards");
        spawner.RemoveZombie(gameObject);
        Destroy(this);
        Destroy(gameObject, 10f);
    }

    public void DamageZombie(float dmgTaken)
    {
        pathTimer = 0;
        health -= dmgTaken;

        // Hit Marker effect
        ui_controller.CrosshairHitmarker();
    }
}
