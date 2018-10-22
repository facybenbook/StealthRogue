﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerControl : MonoBehaviour {

    public float defaultSpeed;
    public float sprintSpeed;
    public bool isSpotted;
    public bool invincible;
    public float dashDist;
    public float dashSpeed;

    public bool playerMoving;
    public bool isAttacking;
    public Vector2 lastMove;

    public int equip;
    public GameObject equipment;
    public bool capturedBullet;
    public GameObject gun;
    public GameObject bullet;
    public string mapLocation;
    public bool changingLocation; //coroutine play once flag
    public bool gettingCaught; //coroutine play once flag
    public int health;
    public int stamina;

    public AudioClip[] audioClips;

    Task rollOneShot;

    private Rigidbody2D myRigidbody;
    private Animator anim;
    private AudioSource myAudioSource;
    private Light playerLight;
    private Light sceneLight;
    private float moveSpeed;

	// Use this for initialization
	void Start () {
        lastMove.y = -1;
        isSpotted = false;

        myRigidbody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        myAudioSource = GetComponent<AudioSource>();

        equip = 0;
        capturedBullet = false;
        gun = transform.Find("Gun").gameObject;

        playerLight = transform.Find("Player Light").gameObject.GetComponent<Light>();
        sceneLight = GameObject.Find("Scene Light").gameObject.GetComponent<Light>();

        stamina = 100;
        moveSpeed = defaultSpeed;
    }
	
	// Update is called once per frame
	void Update () {

        if (gettingCaught)
        {
            StartCoroutine(getCaught());
            gettingCaught = false;
        }

        if (changingLocation)
        {
            StartCoroutine(adjustLight());
            changingLocation = false;
        }

        if (Input.GetMouseButtonDown(0) && (rollOneShot == null || !rollOneShot.Running))
            rollOneShot = new Task(roll());

        playerMove();

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            sprint();
        }
        else if(stamina != 100)
        {
            moveSpeed = 1;
            ++stamina;
        }
    }

    private void playerMove()
    {
        playerMoving = false;

        //moves player left and right
        if ((Input.GetAxisRaw("Horizontal") == 1 || Input.GetAxisRaw("Horizontal") == -1) && !isAttacking)
        {
            //moves player according to moveSpeed horizontally
            //transform.Translate(new Vector3(Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime, 0f, 0f));
            myRigidbody.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * moveSpeed, myRigidbody.velocity.y);

            playerMoving = true;
            lastMove = new Vector2(Input.GetAxisRaw("Horizontal"), 0f);
        }

        //otherwise don't move in x direction
        else
        {
            myRigidbody.velocity = new Vector2(0f, myRigidbody.velocity.y);
        }

        //moves player up and down
        if ((Input.GetAxisRaw("Vertical") == 1 || Input.GetAxisRaw("Vertical") == -1) && !isAttacking)
        {
            //moves player according to moveSpeed vertically
            //Transform.Translate(new Vector3(0f, Input.GetAxisRaw("Vertical") * moveSpeed * Time.deltaTime, 0f));

            myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, Input.GetAxisRaw("Vertical") * moveSpeed);

            playerMoving = true;
            lastMove = new Vector2(0f, Input.GetAxisRaw("Vertical"));
        }

        //otherwise don't move in y direction
        else
        {
            myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, 0f);
        }

        if (Input.GetKeyDown("space") && !isAttacking)
        {
            isAttacking = true;
            Invoke("setAttackBack", .5f);
        }
        if (Input.GetKeyDown("e"))
        {
            equipment.GetComponent<EquipmentController>().onKeyDown();
        }

        //assigns values to animator
        anim.SetFloat("MoveX", Input.GetAxisRaw("Horizontal"));
        anim.SetFloat("MoveY", Input.GetAxisRaw("Vertical"));
        anim.SetBool("PlayerMoving", playerMoving);
        anim.SetBool("IsAttacking", isAttacking);
        anim.SetFloat("LastMoveX", lastMove.x);
        anim.SetFloat("LastMoveY", lastMove.y);
    }

    public void sprint()
    {
        moveSpeed = sprintSpeed;
        --stamina;
        if(stamina < 0)
        {
            moveSpeed = defaultSpeed;
            ++stamina;
        }
    }

    public IEnumerator getCaught()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.5f);
        myAudioSource.PlayOneShot(audioClips[1], 1f);
        sceneLight.intensity = 2f;
        yield return new WaitForSecondsRealtime(0.5f);
        myAudioSource.PlayOneShot(audioClips[2], 1f);
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1f;
    }

    private void setAttackBack()
    {
        isAttacking = false;
    }

    private void playAttackSound()
    {
        myAudioSource.PlayOneShot(audioClips[0], 1f);
    }

    public IEnumerator roll()
    {
        Vector3 target = transform.position + new Vector3(lastMove.x, lastMove.y, 0) * dashDist;

        float rollSpeedX = lastMove.x * dashSpeed;
        float rollSpeedY = lastMove.y * dashSpeed;

        while (Vector3.Distance(transform.position, target) > .1f)
        {
            Debug.Log("yay");
            myRigidbody.velocity = new Vector3(rollSpeedX, rollSpeedY, 0);
            yield return null;
        }
    }

    public IEnumerator adjustLight()
    {
        if(mapLocation == "")
        {
            for(int i = 54; i >= 30; i-=2)
            {
                playerLight.spotAngle = i;
                yield return null;
            }
        }
        else
        {
            for (int i = 30; i <= 54; i+=2)
            {
                playerLight.spotAngle = i;
                yield return null;
            }
        }
    }

    public void OnCollisionEnter2DHurt(Collision2D collision)
    {
        if (collision.gameObject.tag == "Projectile")
        {
            if(health != 0)
                --health;
            Destroy(collision.gameObject);
        }
    }

    public void OnCollisionEnter2DKnife(Collision2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            collision.gameObject.GetComponent<EnemyControl>().messageReceiver = new Message("die");
            Debug.Log("message sent");
        }
        if(equipment != null)
        {
            equipment.GetComponent<EquipmentController>().onCollide(collision);
        }
    }
}
