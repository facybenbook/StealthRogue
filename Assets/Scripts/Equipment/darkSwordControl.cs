﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class darkSwordControl : EquipmentController {

    public override void Awake()
    {
        base.Awake();
        equipType = (int)Equipment.darkSword;
    }

    //equip
    public override void onKeyDown()
    {
        if(pControl.capturedBullet)
        {
            pControl.gun.GetComponent<GunControl>().Fire(pControl.bullet, 0);
            pControl.capturedBullet = false;
        }
    }

    public override void onCollide(Collision2D collision)
    {
        if (collision.gameObject.tag == "Projectile")
        {
            Destroy(collision.gameObject);
            pControl.capturedBullet = true;
        }
    }
}
