using UnityEngine;

public class GorillaForm
{
    public Vector2 GetJumpVelocity(Vector2 pushOffNormal)
    {
        if (pushOffNormal == Vector2.zero)
        {
            return Vector2.zero;
        }
        else if (pushOffNormal.y > Mathf.Abs(pushOffNormal.x))
        {
            return Vector2.up;
        }
        else
        {
            return (pushOffNormal + Vector2.up).normalized;
        }
    }
}
