using System.Collections.Generic;
using UnityEngine;

public class BoxProjection
{
    private const int RayCountPerSide = 4;
    private const float SkinWidth = 0.1f;
    private const float FloatTolerance = 0.0001f;

    private Bounds bounds;
    private Vector2 boxCenter;
    private Vector2 projectionVector;

    private float heightBetweenRays;
    private float widthBetweenRays;

    public BoxProjection(Bounds bounds, Vector2 boxCenter, Vector2 projectionVector)
    {
        this.bounds = bounds;
        this.boxCenter = boxCenter;
        this.projectionVector = projectionVector;

        var skinnedBounds = this.GetSkinnedBounds();
        this.heightBetweenRays = skinnedBounds.size.y / (RayCountPerSide - 1);
        this.widthBetweenRays = skinnedBounds.size.x / (RayCountPerSide - 1);
    }

    public CollisionEvent Project()
    {
        CollisionEvent collEvent = new CollisionEvent();

        var rays = this.GetRays();
        var collisionLayer = LayerMask.GetMask("SolidObstacles");
        RaycastHit2D closestHit = new RaycastHit2D { distance = float.MaxValue };
        foreach (var ray in rays)
        {
            var projectedVelocityLength = Vector2.Dot(ray.direction, this.projectionVector);
            var hit = Physics2D.Raycast(ray.origin, ray.direction, projectedVelocityLength + BoxProjection.SkinWidth, collisionLayer);
            if (hit && hit.distance < closestHit.distance)
            {
                // TODO: Two raycast algorithm was put in place because of a problem with not taking into account movement
                //  in one axis that would avoid the collision in the other
                // Originally was just trying to set the ray origin to account for this offset, but this caused a different
                //  problem if the ray origin was now inside of a collision volume and would create a false positive collision
                // Maybe the trick is to offset the ray origins, but order the ray checking someway better than by distance?
                //  Or maybe I need to test each ray origin and only offset it if it isn't in a volume?
                var displacedOrigin = ray.origin + (ray.direction.x > 0 ? new Vector2(0, this.projectionVector.y) : new Vector2(this.projectionVector.x, 0));
                var wouldStillHit = Physics2D.Raycast(displacedOrigin, ray.direction, projectedVelocityLength + BoxProjection.SkinWidth, collisionLayer);
                if (wouldStillHit)
                {
                    closestHit = wouldStillHit;
                    collEvent.normal = closestHit.normal;
                    collEvent.percentToHit = (closestHit.distance - BoxProjection.SkinWidth) / projectedVelocityLength;
                }
            }
        }

        return collEvent;
    }

    private Bounds GetSkinnedBounds()
    {
        Bounds bounds = this.bounds;
        bounds.Expand(BoxProjection.SkinWidth * -2);
        return bounds;
    }

    private List<Ray2D> GetRays()
    {
        var rayList = new List<Ray2D>();
        var skinnedBounds = this.GetSkinnedBounds();

        if (Mathf.Abs(this.projectionVector.x) > BoxProjection.FloatTolerance)
        {
            var xDirection = Mathf.Sign(this.projectionVector.x);
            for (var nRay = 0; nRay < RayCountPerSide; nRay++)
            {
                var offsetToExtent = new Vector2(skinnedBounds.extents.x * xDirection, skinnedBounds.extents.y - nRay * this.heightBetweenRays);
                rayList.Add(new Ray2D(this.boxCenter + offsetToExtent, new Vector2(this.projectionVector.x, 0)));
            }
        }

        if (Mathf.Abs(this.projectionVector.y) > BoxProjection.FloatTolerance)
        {
            var yDirection = Mathf.Sign(this.projectionVector.y);
            for (var nRay = 0; nRay < RayCountPerSide; nRay++)
            {
                var offsetToExtent = new Vector2(skinnedBounds.extents.x - nRay * this.widthBetweenRays, skinnedBounds.extents.y * yDirection);
                rayList.Add(new Ray2D(this.boxCenter + offsetToExtent, new Vector2(0, this.projectionVector.y)));
            }
        }

        return rayList;
    }
}