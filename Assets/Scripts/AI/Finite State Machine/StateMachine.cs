﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine{

    EnemyControl owner;
    public State currentState;
    public State prevState;
    public State globalState;

    //constructor for state machine
    public StateMachine(EnemyControl currOwner)
    {
        owner = currOwner;
        currentState = null;
        prevState = null;
        globalState = null;
    }

    public void changeState(State newState)
    {
        //save current state to previous state
        prevState = currentState;

        //changes to new state
        currentState.Exit(owner);
        currentState = newState;
        currentState.Enter(owner);
    }

    public void revertToPrevState()
    {
        changeState(prevState);
    }

    public void stateUpdate()
    {
        if(currentState != null)
            currentState.Execute(owner);
        if (globalState != null)
            globalState.Execute(owner);
    }
}