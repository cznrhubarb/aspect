using UnityEngine;

public class CollisionEvent
{
    public Vector2 normal = Vector2.up;
    public float percentToHit = 1f;

    public static bool operator true(CollisionEvent e) => e.percentToHit < 1;
    public static bool operator false(CollisionEvent e) => e.percentToHit >= 1;
}
