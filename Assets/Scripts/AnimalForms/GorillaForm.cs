﻿using UnityEngine;

public class GorillaForm : IForm
{
    private const float JumpHeight = 4;
    private const float TimeToJumpApex = 0.4f;
    public const float Gravity = -(2 * JumpHeight) / TimeToJumpApex;
    public const float JumpPower = -Gravity * TimeToJumpApex;
    public const float WalkSpeed = 25f;
    public const float DelayBetweenJumps = 0.4f;
    public const float WalkDelayAfterJump = 0.75f;

    private float jumpDelay;
    private float walkDelay;

    public GorillaForm()
    {
        this.jumpDelay = 0;
        this.walkDelay = 0;
    }

    public Vector2 GetWalkVelocity(Vector2 lastCollisionNormal, float walkForce)
    {
        var delayRatio = 1 - (this.walkDelay / GorillaForm.WalkDelayAfterJump);
        return new Vector2(walkForce * GorillaForm.WalkSpeed * delayRatio, 0);
    }

    public Vector2 GetJumpVelocity(Vector2 pushOffNormal, float jumpForce)
    {
        var jumpVelocity = Vector2.zero;

        if (jumpForce > 0 && pushOffNormal != Vector2.zero)
        {
            if (pushOffNormal.y > Mathf.Abs(pushOffNormal.x))
            {
                this.jumpDelay = GorillaForm.DelayBetweenJumps;
                jumpVelocity = new Vector2(0, GorillaForm.JumpPower);
            }
            else if (this.jumpDelay <= 0)
            {
                this.jumpDelay = GorillaForm.DelayBetweenJumps;
                this.walkDelay = GorillaForm.WalkDelayAfterJump;
                jumpVelocity = (Vector2.up + pushOffNormal).normalized * GorillaForm.JumpPower;
            }

        }

        return jumpVelocity;
    }

    public Vector2 GetGravity(Vector2 currentVelocity, float jumpForce)
    {
        return new Vector2(0, GorillaForm.Gravity);
    }

    public float GetSlopeClampDistance()
    {
        return -GorillaForm.Gravity / 200;
    }

    public void Update(float elapsedTime)
    {
        this.jumpDelay = Mathf.Max(0, this.jumpDelay - elapsedTime);
        this.walkDelay = Mathf.Max(0, this.walkDelay - elapsedTime);
    }
}
