﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shotgunControl : EquipmentController
{
    public int bullets;
    public Task fireOneShot;

    public override void Awake()
    {
        base.Awake();
        equipType = (int)Equipment.shotgun;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //equip
    public override void onKeyDown()
    {
        if (bullets > 0 && (fireOneShot == null || !fireOneShot.Running))
        {
            fireOneShot = new Task(pControl.attackPatterns.shotgun(pControl.gun, pControl.bullet, 1, 1f));
            --bullets;
            pControl.myAudioSource.PlayOneShot(pControl.audioClips[3]);
        }
    }

    public override void onCollide(Collision2D collision)
    {
    }
}
