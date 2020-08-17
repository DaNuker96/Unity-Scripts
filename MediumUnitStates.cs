using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MediumUnitStates : MonoBehaviour
{
    public GameObject fieldOfViewObject;
    Transform player;
    [HideInInspector]
    public NavMeshAgent agent;
    FieldOfView fieldOfView;
    EnemyHealth health;

    public enum State
      {
        FindCover,
        MovingToCover,
        InCover,
        FindPlayer,
        Enraged
       }

    public State currentState;

    [Header("Cover Values")]
    public List<GameObject> cover = new List<GameObject>();
    public Vector3 coverTargetPosition;
    private float coverDistance;
    public GameObject closestCover;
    public bool coverAttack;
    public float coverTime;
    public float coverWaitTime;

    [Header("Player Interaction Values")]
    public LayerMask viewMask;
    public LayerMask characterMask;
    public float playerDistance;
    public float randomPositionRadius = 100f;

    [Header("Enemy Interaction Values")]
    public float walkingSpeed = 10f;
    public float normalSpeed = 15f;
    public float enrageSpeed = 25f;
    public float rotationSpeed = 60f;

    private float enemyAttackTime;
    public bool newRandomPosition;
    private Vector3 randomPosition;
    public float randomPositionDistance;
    private float waitTime;
    private float maxWaitTime;

    private int stateSelectChance;
    private int selectedState = 0;

    //Scale the size of the collider when in cover
    private Vector3 enemyScale = new Vector3(0.35f, 0.35f, 0.35f);
    private Vector3 coverScale = new Vector3(0.35f, 0.175f, 0.35f);

    void Start()
    {
        #region Script References
        agent = GetComponent<NavMeshAgent>();
        fieldOfView =fieldOfViewObject.GetComponent<FieldOfView>();
        health = GetComponent<EnemyHealth>();
        #endregion

        //Get an instance of the player
        player = PlayerManager.instance.player.transform;

        cover.AddRange(GameObject.FindGameObjectsWithTag("Cover"));
        agent.speed = normalSpeed;

        //selects the chance of becoming enraged or moving back to cover
        stateSelectChance = Random.Range(1, 100);

        //begin cover process on spawning in the enemy
        currentState = State.FindCover;
    }

    void Update()
    {
        //when the enemy drops below 30% health they have a 50% chance to change state
        if (health.healthPercent <= 0.3f && selectedState == 0)
        {
            SelectStateChance();
        }

        //if the cover position becomes null
        if(coverTargetPosition == null)
        {
            //if cover is avaible begin to move towards it
            if (cover.Count >= 0)
            {
                currentState = State.FindCover;
            }
            //else charge towards the player
            else
                currentState = State.FindPlayer;
        }

        #region FindCoverState
        //the enemy begins to look for the nearest cover point
        if (currentState == State.FindCover)
        {
            FindNearestCover();
            MoveToCover();
        }
        #endregion

        #region MovingToCoverState
        if (currentState == State.MovingToCover)
        {
            agent.stoppingDistance = 0;
            coverDistance = Vector3.Distance(this.transform.position, coverTargetPosition);

            Debug.DrawLine(this.transform.position, coverTargetPosition);

            //once the enemy reaches the cover switch the state to in cover
            if (coverDistance <= 1f)
            {
                currentState = State.InCover;
            }
        }
        #endregion

        #region InCoverState
        //once the enemy is in cover it will begin to hide and take aim at the player
        if (currentState == State.InCover)
        {
            agent.isStopped = true;

            //enemy attacks from the cover
            if (coverAttack)
            {
                FacePlayer();
                coverTime += Time.deltaTime;
                //this.gameObject.transform.localScale = enemyScale;

                if (coverTime >= coverWaitTime)
                {
                    coverTime = 0;
                    CoverSwitchStance();
                }            
            }

            //enemy hides begin cover
            if (!coverAttack)
            {
                coverTime += Time.deltaTime;
                //this.gameObject.transform.localScale = coverScale;

                if (coverTime >= coverWaitTime)
                {
                    coverTime = 0;
                    CoverSwitchStance();
                }
            }
        }
        #endregion

        #region FindPlayerState
        //the enemy begins to find and chase the player down
        if (currentState == State.FindPlayer)
        {
            //this.gameObject.transform.localScale = enemyScale;
            agent.isStopped = false;

            if(newRandomPosition == false)
            {
                agent.speed = normalSpeed;
                agent.SetDestination(player.position);
            }

            Debug.DrawLine(this.transform.position, player.transform.position);
            playerDistance = Vector3.Distance(this.transform.position, player.transform.position);

            //when the enemy comes close to the player, they begin to strafe around the player, making it difficult for the player to focus on 1 target
            if(playerDistance <= 75 )
            {
                FacePlayer();

                if(newRandomPosition == false)
                {
                    FindRandomPosition();
                    newRandomPosition = true;
                }
            }

            randomPositionDistance = Vector3.Distance(this.transform.position, randomPosition);

            if(randomPositionDistance <= 5f && newRandomPosition == true)
            {
                FacePlayer();
                waitTime += Time.deltaTime;               

                if(waitTime >= maxWaitTime)
                {
                    newRandomPosition = false;
                    waitTime = 0;
                } 
            }
        }
        #endregion

        #region EnragedState
        if (currentState == State.Enraged)
        {
            //this.gameObject.transform.localScale = enemyScale;
            agent.isStopped = false;

            FacePlayer();
            //increased movement speed
            agent.speed = enrageSpeed;
            //lower stopping distance for more aggresion
            agent.stoppingDistance = 8f;
            agent.SetDestination(player.position);

            Debug.DrawLine(this.transform.position, player.transform.position);
        }
        #endregion
    }

    public bool CanSeePlayer()
    {
        //player is in view distance
        if (Vector3.Distance(this.transform.position, player.position) < fieldOfView.viewRadius)
        {
            //player is not hidden behind an object
            if (!Physics.Linecast(this.transform.position, player.position, viewMask))
            {
                //player is not hidden behind another enemy
                if (!Physics.Linecast(this.transform.position, player.position, characterMask))
                {
                    Vector3 dirToPlayer = (player.position - this.transform.position).normalized;
                    float angleBetweenEnemyAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);

                    //player is in view angle
                    if (angleBetweenEnemyAndPlayer < fieldOfView.viewAngle / 2f)
                    {
                        return true;
                    }
                }
            }
        }
        //else if one check fails return false
        return false;
    }

    public void MoveToCover()
    {
        if (cover.Count > 0)
        {
            //select random cover point within the array
            agent.SetDestination(closestCover.transform.position);

            //CHANGE THE Y POSITION OF THE COVER TARGET VECTOR
            //sets the position of the cover selected
            coverTargetPosition = new Vector3(closestCover.transform.position.x, 118, closestCover.transform.position.z);

            //set the current state to moving to cover
            currentState = State.MovingToCover;
        }

        //if no avaible cover points, sets destination to players position
        else
        {
            currentState = State.FindPlayer;
        }
    }

    void FindNearestCover() 
    {
        //Set the lowest distance to infinity
        float lowestDistance = Mathf.Infinity;
        closestCover = null;

        //Cycle through each cover object and find the distance between them and the player
        for(int i  = 0; i < cover.Count; i++)
        {
            float distance = Vector3.Distance(cover[i].transform.position, transform.position);

            //The lowest distance becomes the closest cover
            if(distance < lowestDistance) 
            {
                lowestDistance = distance;
                closestCover = cover[i];
            }
        }
    }

    void FindRandomPosition()
    {
        //set enemy's new speed
        agent.speed = walkingSpeed;

        //find a random point within a radius and sets it to be the enemies new destination
        Vector3 randomDirection = Random.insideUnitSphere * randomPositionRadius;
        randomDirection += player.transform.position;
        NavMeshHit hit;
        randomPosition = Vector3.zero;

        //sets the wait time once the enemy reaches the destination
        maxWaitTime = Random.Range(2, 15);

        if (NavMesh.SamplePosition(randomDirection, out hit, randomPositionRadius, 1))
        {
            randomPosition = hit.position;
            agent.SetDestination(randomPosition);
        }
    }

    void SelectStateChance()
    {
        stateSelectChance = Random.Range(1, 100);

        //50% chance to move to cover when low on health, only if cover is avaible and not already in cover
        if (stateSelectChance <= 50 && cover.Count > 0 && currentState != State.InCover)
        {
            currentState = State.FindCover;
        }
        //else become enraged and chase the player intently
        else
            currentState = State.Enraged;

        //ensures it can only be called once
        selectedState++;
    }

    public void CoverSwitchStance()
    {
        coverWaitTime = Random.Range(3f, 6f);
        coverAttack = !coverAttack;
    }

    public void FacePlayer()
    {
        //find the direction of the player and set the rotation of the enemy towards them
        Vector3 direction = (player.position - this.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, randomPositionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, agent.stoppingDistance);
    }
}
