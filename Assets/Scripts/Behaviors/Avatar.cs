﻿using UnityEngine;

public class Avatar : MonoBehaviour
{
    public Controller2D Controller { get; set; }

    void Start()
    {
        this.Controller = new Controller2D(this.GetComponent<BoxCollider2D>(), new GorillaForm());
    }

    void FixedUpdate()
    {
        this.Controller.WalkForce = Input.GetAxisRaw("Horizontal");
        this.Controller.JumpForce = Input.GetKeyDown(KeyCode.UpArrow) ? 1 : 0;

        this.Controller.Simulate(Time.deltaTime);
        this.transform.position = this.Controller.Position;
    }
}
