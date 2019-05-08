using UnityEngine;

public interface IForm
{
    Vector2 GetWalkVelocity(Vector2 lastCollisionNormal, float walkForce);
    Vector2 GetJumpVelocity(Vector2 lastCollisionNormal, float jumpForce);
    Vector2 GetGravity(Vector2 currentVelocity, float jumpForce);
    float GetSlopeClampDistance();
    void Update(float elapsedTime);
}