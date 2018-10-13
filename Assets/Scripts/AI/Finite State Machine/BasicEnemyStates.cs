﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace BasicEnemyState
{
    public class PatrolWaypoint : State
    {
        //singleton of state
        private static PatrolWaypoint instance = null;

        public override void Enter(EnemyControl owner)
        {
            //turn on FOV visualization
            owner.viewMeshFilter.SetActive(true);

            //turn on all waypoints
            owner.reenableWaypoints();

            //fire coroutine first time
            new Task(owner.moveTowardsNext());
        }

        public override void Execute(EnemyControl owner)
        {
            //check if player is spotted every udpate
            owner.playerSpotted = owner.enemyVision.checkVision();
            if (owner.playerSpotted)
                owner.targetControl.isSpotted = true;

            //changes to attack state if enemy spots player
            if (owner.targetControl.isSpotted)
                owner.FSM.changeState(AttackPlayer.Instance);

            //changes to new state when enemy receives message
            if (owner.messageReceiver.newState != null)
                owner.FSM.changeState(owner.messageReceiver.newState);

        }

        public override void Exit(EnemyControl owner)
        {
            owner.StopAllCoroutines();
        }

        //singleton
        public static PatrolWaypoint Instance
        {
            get
            {
                if (instance == null)
                    instance = new PatrolWaypoint();

                return instance;
            }
        }
    }

    public class AttackPlayer : State
    {
        //singleton of state
        private static AttackPlayer instance = null;

        public override void Enter(EnemyControl owner)
        {
            //play gettingCaught() scene sequence
            owner.targetControl.gettingCaught = true;
            //have enemy stand in place
            owner.transform.GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0, 0);
            //turn off FOV visualization
            owner.viewMeshFilter.SetActive(false);
            //turn off all waypoints
            owner.disableWaypoints();

            //fire coroutines first time
            owner.attackOneShot = new Task(owner.attackCoroutine);
            owner.lookingAtPlayerOneShot = new Task(owner.RotateTo(owner.targetControl.transform.position, 0f));
        }

        public override void Execute(EnemyControl owner)
        {
            Debug.Log("Attacking is executing");
            owner.GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0, 0);

            //only fires coroutines if current one is not running
            if (!owner.attackOneShot.Running) {
                owner.attackStates();
                owner.attackOneShot = new Task(owner.attackCoroutine);
            }
            Debug.Log(owner.lookingAtPlayerOneShot.Running);
            if (!owner.lookingAtPlayerOneShot.Running)
            {
                owner.lookingAtPlayerOneShot = new Task(owner.RotateTo(owner.targetControl.transform.position, 0f));
            }

            //change to waypoint state if player is no longer spotted
            if (!owner.targetControl.isSpotted)
            {
                owner.FSM.changeState(PatrolWaypoint.Instance);
            }
        }

        public override void Exit(EnemyControl owner)
        {
            //stop both coroutines
            owner.attackOneShot.Stop();
            owner.lookingAtPlayerOneShot.Stop();
            owner.attackOneShot = null;
            owner.lookingAtPlayerOneShot = null;
        }

        //singleton
        public static AttackPlayer Instance
        {
            get
            {
                if (instance == null)
                    instance = new AttackPlayer();

                return instance;
            }
        }
    }

    public class LookAtMe: State
    {
        //singleton of state
        private static LookAtMe instance = null;

        //coroutines in execute()
        

        public override void Enter(EnemyControl owner)
        {
            Vector3 bombPosition = owner.messageReceiver.senderPosition;
            owner.lookAtMeOneShot = new Task(owner.RotateTo(bombPosition, 5f));

            owner.messageReceiver = new Message(Vector3.zero, null);
        }

        public override void Execute(EnemyControl owner)
        {
            owner.GetComponent<Rigidbody2D>().velocity = new Vector3(0, 0, 0);

            //check if player is spotted every udpate
            owner.playerSpotted = owner.enemyVision.checkVision();
            if (owner.playerSpotted)
                owner.targetControl.isSpotted = true;

            //Change to attack state if player is spotted
            if (owner.targetControl.isSpotted)
                owner.FSM.changeState(AttackPlayer.Instance);
            //Reverts back to previous state after coroutine is done running
            if (!owner.lookAtMeOneShot.Running)
                owner.FSM.changeState(PatrolWaypoint.Instance);
            if (owner.messageReceiver.newState != null)
                owner.FSM.changeState(owner.messageReceiver.newState);

        }

        public override void Exit(EnemyControl owner)
        {
        }

        //singleton
        public static LookAtMe Instance
        {
            get
            {
                if (instance == null)
                    instance = new LookAtMe();

                return instance;
            }
        }
    }

    public class Die : State
    {
        //singleton of state
        private static Die instance = null;

        public override void Enter(EnemyControl owner)
        {
        }

        public override void Execute(EnemyControl owner)
        {
            GameObject.Destroy(owner.gameObject);
        }

        public override void Exit(EnemyControl owner)
        {
        }

        //singleton
        public static Die Instance
        {
            get
            {
                if (instance == null)
                    instance = new Die();

                return instance;
            }
        }
    }

    public class GlobalState : State
    {
        //singleton of state
        private static GlobalState instance = null;

        public override void Enter(EnemyControl owner)
        {
        }

        public override void Execute(EnemyControl owner)
        {
            if (owner.messageReceiver.newState is Die)
                owner.FSM.changeState(Die.Instance);
        }

        public override void Exit(EnemyControl owner)
        {
        }

        //singleton
        public static GlobalState Instance
        {
            get
            {
                if (instance == null)
                    instance = new GlobalState();

                return instance;
            }
        }
    }
}

