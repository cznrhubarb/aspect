using UnityEngine;

public enum CollisionAlignment
{
    NoSplit,
    Opposed,
    NonOpposed,
    Aligned
}

public class VectorSplitter
{
    private const float MinimumAngleOfReflection = 90;
    private const float MaximumAngleOfReflection = 180 - 45;

    public static SplitPair Split(Vector2 original, CollisionEvent collision)
    {
        Vector2 second = Vector2.zero;
        CollisionAlignment alignment = CollisionAlignment.NoSplit;

        if (collision)
        {
            var angle = Vector2.SignedAngle(original, collision.normal);
            if (Mathf.Abs(angle) <= MinimumAngleOfReflection)
            {
                alignment = CollisionAlignment.Aligned;
            }
            else if (Mathf.Abs(angle) < MaximumAngleOfReflection)
            {
                alignment = CollisionAlignment.NonOpposed;
            }
            else
            {
                alignment = CollisionAlignment.Opposed;
            }
            
            Vector2 normPerp = Vector2.Perpendicular(collision.normal).normalized;
            if (angle < 0)
            {
                normPerp *= -1f;
            }
            second = (original * (1 - collision.percentToHit)).Project(normPerp);
        }

        return new SplitPair(original * collision.percentToHit, second, alignment);
    }

    public class SplitPair
    {
        public Vector2 First { get; private set; }
        public Vector2 Second { get; private set; }
        public CollisionAlignment Alignment { get; private set; }

        public SplitPair(Vector2 first, Vector2 second, CollisionAlignment alignment)
        {
            this.First = first;
            this.Second = second;
            this.Alignment = alignment;
        }
    }
}
