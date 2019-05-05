using UnityEngine;

public class HumanForm : IForm
{
    public const float Gravity = -40f;
    public const float WalkSpeed = 15f;
    public const float JumpPower = 22f;

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

    public Vector2 GetGravity(Vector2 currentVelocity)
    {
        return new Vector2(0, HumanForm.Gravity);
    }

    public float GetSlopeClampDistance()
    {
        return -HumanForm.Gravity / 200;
    }

    public void Update(float elapsedTime)
    {

    }
}