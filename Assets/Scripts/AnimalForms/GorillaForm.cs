using UnityEngine;

public class GorillaForm : IForm
{
    public const float Gravity = -40f;
    public const float WalkSpeed = 15f;
    public const float JumpPower = 22f;
    public const float DelayBetweenJumps = 0.4f;
    public const float WalkDelayAfterJump = 0.3f;

    private float jumpDelay;
    private float walkDelay;

    public GorillaForm()
    {
        this.jumpDelay = 0;
        this.walkDelay = 0;
    }

    public Vector2 GetWalkVelocity(Vector2 lastCollisionNormal, float walkForce)
    {
        if (this.walkDelay <= 0)
        {
            return new Vector2(walkForce * GorillaForm.WalkSpeed, 0);
        }

        return Vector2.zero;
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

    public Vector2 GetGravity(Vector2 currentVelocity)
    {
        return new Vector2(0, GorillaForm.Gravity);
    }

    public float GetSlopeClampDistance()
    {
        return -GorillaForm.Gravity / 200;
    }

    public void Update(float elapsedTime)
    {
        if (this.jumpDelay > 0)
        {
            this.jumpDelay -= elapsedTime;
        }

        if (this.walkDelay > 0)
        {
            this.walkDelay -= elapsedTime;
        }
    }
}
