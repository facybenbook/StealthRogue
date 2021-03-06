﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeHitBox : MonoBehaviour {
    private PlayerControl pControl;

    void Awake()
    {
        pControl = transform.parent.gameObject.GetComponent<PlayerControl>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        pControl.OnCollisionEnter2DKnife(collision);
    }
}
