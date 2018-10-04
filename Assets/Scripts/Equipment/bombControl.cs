﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bombControl : EquipmentController {
    public int bombCount;
    public GameObject bomb;

    void Awake()
    {
        equipType = (int)Equipment.bomb;
        player = GameObject.FindWithTag("Player");
        pControl = player.GetComponent<PlayerControl>();
        bombCount = 3;
    }

    //equip
    public override void onKeyDown()
    {
        if(bombCount > 0)
        {
            GameObject projectile = (GameObject)Instantiate(bomb, player.transform.position, player.transform.rotation);
            --bombCount;
        }
    }

    public override void onCollide(Collision2D collision)
    {

    }
}
