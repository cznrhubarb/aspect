using UnityEngine;

public class HumanForm : IForm
{
    public const float MaxJumpHeight = 4;
    public const float TimeToMaxJumpApex = 0.4f;
    public const float MinJumpHeight = 1;
    public const float TimeToMinJumpApex = 0.05f;
    public const float TimeToFall = 0.1f;
    public const float MaxJumpGravity = -(2 * MaxJumpHeight) / TimeToMaxJumpApex;
    public const float MinJumpGravity = -(2 * MinJumpHeight) / TimeToMinJumpApex;
    public const float FallGravity = -(2 * MaxJumpHeight) / TimeToFall;
    public const float JumpPower = -MaxJumpGravity * TimeToMaxJumpApex;
    //public const float Gravity = -40f;
    //public const float JumpPower = 22f;
    public const float WalkSpeed = 15f;

    public Vector2 GetWalkVelocity(Vector2 lastCollisionNormal, float walkForce)
    {
        return new Vector2(walkForce * HumanForm.WalkSpeed, 0);
    }

    public Vector2 GetJumpVelocity(Vector2 lastCollisionNormal, float jumpForce)
    {
        if (jumpForce > 0 && lastCollisionNormal.y > Mathf.Abs(lastCollisionNormal.x))
        {
            return new Vector2(0, HumanForm.JumpPower);
        }

        return Vector2.zero;
    }

    public Vector2 GetGravity(Vector2 currentVelocity, float jumpForce)
    {
        if (currentVelocity.y <= 0)
        {
            return new Vector2(0, HumanForm.FallGravity);
        }
        else if (jumpForce == 0)
        {
            return new Vector2(0, HumanForm.MinJumpGravity);
        }
        else
        {
            return new Vector2(0, HumanForm.MaxJumpGravity);
        }
    }

    public float GetSlopeClampDistance()
    {
        return -HumanForm.MaxJumpGravity / 200;
    }

    public void Update(float elapsedTime)
    {

    }
}