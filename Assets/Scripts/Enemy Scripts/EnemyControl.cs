﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BasicEnemyState;
using BasicEnemyAttackState;
using Pathfinding;


//EnemyControl
//class to control enemy behavior
public class EnemyControl : MonoBehaviour {

    //enemy properties
    public float detectionAngle;
    public float detectionDistance;
    public float moveSpeed;
    public float rotateSpeed;
    public float fovResolution;
    public float distToFire;
    public bool playerSpotted;
    public int currAmmo;
    public int maxAmmo;
    public float reloadTime;

    //objects associated with enemy
    public GameObject bullet;
    public GameObject waypointControl;
    public Transform[] wayPoints; //waypoints[1] is waypointControl
    public GameObject texture;
    public GameObject viewMeshFilter;
    public PlayerControl targetControl;
    public GameObject gun;

    public EnemyVision enemyVision;
    public AttackPatterns attackPatterns;
    public AIPath pathFinder;

    //Finite State Machines
    public StateMachine mainFSM;
    public StateMachine attackFSM;

    //coroutines in attackPlayer state
    public Task attackOneShot;
    public Task lookingAtPlayerOneShot;
    public Task reloadOneShot;
    public Task lookAtMeOneShot;

    //Receives messages
    public Message messageReceiver = new Message(Vector3.zero, null);

    //helper variables
    public GameObject target;
    public Animator anim;
    public int nextWayPoint;
    public IEnumerator attackCoroutine;
    public bool patrolDirection; //false when going backwards
    public Quaternion up; //to keep texture upright

    //animDirection
    //Struct for keeping track of directions for animator
    public struct animDirection
    {
        public static float x;
        public static float y;
    }

    //Awake
    //on start script
    void Awake()
    {
        target = GameObject.FindWithTag("Player");
        targetControl = target.GetComponent<PlayerControl>();
        gun = transform.Find("Gun").gameObject;
        attackPatterns = new AttackPatterns();
        wayPoints = waypointControl.GetComponentsInChildren<Transform>();
        patrolDirection = true;
        nextWayPoint = 2; //set to two to navigate towards first waypoint that is not an enpoint
        anim = texture.GetComponent<Animator>();
        enemyVision = new EnemyVision(this);
        up = transform.rotation;

        //initialize pathFinder
        pathFinder = GetComponent<AIPath>();

        //initialize state machine and enter first state
        mainFSM = new StateMachine(this);
        mainFSM.changeState(PatrolWaypoint.Instance);
        mainFSM.changeGlobalState(BasicEnemyGlobal.Instance);

        attackFSM = new StateMachine(this);
    }

    //Update
    //called once per frame
    void Update()
    {
        updateAnim();
        mainFSM.stateUpdate();
    }

    //LateUpdate
    //draws enemies fieldOfView for player
    private void LateUpdate()
    {
        enemyVision.drawFOV();
    }

    //updateVision
    //update animator with direction state
    public void updateAnim()
    {
        Vector3 directionVector = gun.transform.position - transform.position;

        if (directionVector.x > 0.01f)
        {
            animDirection.x = 1;
        }
        else if (directionVector.x < -.01f)
        {
            animDirection.x = -1;
        }
        else
            animDirection.x = 0;


        if (directionVector.y > .01f)
        {
            animDirection.y = 1;
        }
        else if (directionVector.y < -.01f)
        {
            animDirection.y = -1;
        }
        else
        {
            animDirection.y = 0;
        }

        anim.SetFloat("MoveX", animDirection.x);
        anim.SetFloat("MoveY", animDirection.y);

        //keeps animation texture upright
        if(texture.transform.rotation != up)
            texture.transform.rotation = up;
        

        //determines whether or not to play idle animation
        if (transform.GetComponent<Rigidbody2D>().velocity == new Vector2(0,0))
            anim.SetBool("isMoving", false);
        else
            anim.SetBool("isMoving", true);
    }

    public IEnumerator Reload(float delay)
    {
        yield return new WaitForSeconds(delay);
    }

    //RotateTo
    //rotates enemy to face target
    //coroutine stops when enemy is facing target
    public IEnumerator RotateTo(Vector3 targ, float delayAfter)
    {
        Quaternion lookDirection = Quaternion.LookRotation(Vector3.forward, (targ - transform.position).normalized);
        while (Quaternion.Angle(transform.rotation, lookDirection) > .1f)
        {
            Debug.DrawRay(transform.position, (targ - transform.position), Color.red);
            Debug.Log("Enemy rotation: " + transform.rotation);
            Debug.Log("Target rotation: " + lookDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookDirection, rotateSpeed * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(delayAfter);
    }

    //attackStates
    //randomly choose attackPattern
    public void attackStates()
    {
        int randomState = UnityEngine.Random.Range(0, 3);
        switch (randomState)
        {
            case 0:
                attackCoroutine = attackPatterns.shootThree(gun, bullet, 5, .5f);
                break;
            case 1:
                attackCoroutine = attackPatterns.shootStraight(gun, bullet, 5, .5f);
                break;
            case 2:
                attackCoroutine = attackPatterns.shootWave(gun, bullet, 10, 1f);
                break;
            default:
                Debug.Log("attack coroutine random range is out of bounds");
                break;

        }
    }
    
    //moveTowardsNext
    //move towards next waypoint in wayPoints
    public IEnumerator moveTowardsNext()
    {
        float waitTime = wayPoints[nextWayPoint].gameObject.GetComponent<WaypointControl>().waitTime;
        float waitToRotate = wayPoints[nextWayPoint].gameObject.GetComponent<WaypointControl>().waitToRotate;
        transform.GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0, 0);
        if (waitTime > 0)
            yield return new WaitForSeconds(waitTime);
        yield return RotateTo(wayPoints[nextWayPoint].transform.position, 0);
        if (waitToRotate > 0)
            yield return new WaitForSeconds(waitToRotate);
        transform.GetComponent<Rigidbody2D>().velocity = ((wayPoints[nextWayPoint].position - transform.position).normalized * moveSpeed);
    }

    //disableWaypoints
    //disables all waypoints
    public void disableWaypoints()
    {
        foreach (Transform waypoint in wayPoints)
        {
            waypoint.gameObject.SetActive(false);
        }
    }

    //reenableWaypoints
    //reenables all waypoints at end of chain
    public void reenableWaypoints()
    {
        foreach(Transform waypoint in wayPoints)
        {
            waypoint.gameObject.SetActive(true);
        }
    }

    //OnTriggerEnter2D
    //determines whether waypoint is waypoint or endpoint
    //reverses direction on collision with endpoint
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.tag == "Waypoint")
        {
            if(GameObject.ReferenceEquals(other.transform.gameObject, wayPoints[nextWayPoint].gameObject))
            {
                other.transform.gameObject.SetActive(false);
                if (patrolDirection)
                    ++nextWayPoint;
                else
                    --nextWayPoint;
                StartCoroutine(moveTowardsNext());
            }
        }
        else if(other.transform.tag == "Endpoint")
        {
            Debug.Log("hits endpoint");
            if (GameObject.ReferenceEquals(other.transform.gameObject, wayPoints[nextWayPoint].gameObject))
            {
                reenableWaypoints();
                patrolDirection = !patrolDirection;
                if (patrolDirection)
                    ++nextWayPoint;
                else
                    --nextWayPoint;
                StartCoroutine(moveTowardsNext());
            }
        }
    }
}