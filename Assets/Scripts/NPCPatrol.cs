using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;


public class NPCPatrol : MonoBehaviour
{
    private float distanceToPlayer;
    public NavMeshAgent guard;
    public GameObject myGuard;
    public GameObject[] patrolPoints = new GameObject[3];
    private GameObject player;
    public int destPoint = 0;
    public NPCInteraction myNpcInteraction;
    public string chaseState = "PLAYER";
    public Animator guardController;
    bool hasInteracted;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        distanceToPlayer = UnityEngine.Vector3.Distance(player.transform.position, myGuard.transform.position);
        //guard.destination = patrolPoints[0].transform.position;
        ChasePlayer();
    }

    void ChasePlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        guard.destination = player.transform.position;
    }
    void GetNextPatrolPoint()
    {
        guard.destination = patrolPoints[destPoint].transform.position;
        if (guard.remainingDistance < 1.5f)
        {
            GotoNextPoint();
        }
    }


    void GotoNextPoint()
    {

        if (patrolPoints.Length == 0)
            return;
        
        /*float distance = Vector3.Distance(player.transform.position, guard.transform.position);
        if (distance < 10)
        {
            ChasePlayer();

        }
        else
        {
        */
            guard.destination = patrolPoints[destPoint].transform.position;
        //}
        

        destPoint = (destPoint + 1) % patrolPoints.Length;
    }


    void Update()
    {
            if (chaseState.Equals("PLAYER"))
            {
                player = GameObject.FindGameObjectWithTag("Player");
                guard.destination = player.transform.position;
            }
            else
            {
                GetNextPatrolPoint();
            }

        UpdatePatrolState();
       
    }
    private void FixedUpdate()
    {
        distanceToPlayer = UnityEngine.Vector3.Distance(player.transform.position, myGuard.transform.position);

        //Debug.Log("Distance too " + distanceToPlayer);
    }

    void UpdatePatrolState()
    {
        // float distance = Vector3.Distance(player.transform.position, guard.transform.position);
        player = GameObject.FindGameObjectWithTag("Player");
       
        if (distanceToPlayer > 6)

        {
            chaseState = "PATROL";
            //guardController.SetBool("isTalking", false);
            
        }
        else if(distanceToPlayer > 2 && distanceToPlayer < 3 && chaseState != "PLAYER")
        {

            if (chaseState != "PLAYER")
            {
                myNpcInteraction.AggroSpeech();
            }
               
            
            
            chaseState = "PLAYER";
            //guardController.SetBool("isTalking", true);
        }
        else if (distanceToPlayer < 2 && !hasInteracted)
        {
            Debug.Log("Inside Distance < 2");
            guard.speed = 0;
            myNpcInteraction.Interact();
            hasInteracted = true;
        }
        
    }
}
