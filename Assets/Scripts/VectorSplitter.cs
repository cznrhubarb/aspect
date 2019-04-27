using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorSplitter
{
    private const float MinimumAngleOfReflection = 90;
    private const float MaximumAngleOfReflection = 180 - 45;

    public static List<Vector2> Split(Vector2 original, CollisionEvent collision)
    {
        var splitVectors = new List<Vector2>() { original * collision.percentToHit };

        if (collision)
        {
            var angle = Vector2.SignedAngle(original, collision.normal);
            if (Mathf.Abs(angle) <= MinimumAngleOfReflection)
            {
                throw new ArgumentException("Invalid Collision Normal: Collision normal is aligned with the vector to be split");
            }

            if (Mathf.Abs(angle) < MaximumAngleOfReflection)
            {
                Vector2 second = new Vector2(collision.normal.y, -collision.normal.x);
                if (angle < 0)
                {
                    second *= -1f;
                }
                second = second.normalized * original.magnitude * (1 - collision.percentToHit);
                splitVectors.Add(second);
            }
        }

        return splitVectors;
    }
}
