using UnityEngine;

public class Controller2D
{
    private const float MaxTimeStep = 1 / 100f;
    private const float Drag = 15f;

    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }

    public float WalkForce { get; set; }
    public float JumpForce { get; set; }

    public BoxCollider2D Collider { get; private set; }

    private IForm form;

    private Vector2 lastCollisionNormal;

    private float simulationTimer;

    public Controller2D(BoxCollider2D collider, IForm defaultForm)
    {
        this.Position = collider.transform.position;
        this.Collider = collider;
        this.form = defaultForm;
    }

    public void Simulate(float elapsedTime)
    {
        this.simulationTimer += elapsedTime;

        while (this.simulationTimer >= Controller2D.MaxTimeStep)
        {
            this.simulationTimer -= Controller2D.MaxTimeStep;

            var inputVelocity = this.form.GetWalkVelocity(this.lastCollisionNormal, this.WalkForce);
            this.Velocity += this.form.GetGravity(this.Velocity) * Controller2D.MaxTimeStep;
            var jumpVelocity = this.form.GetJumpVelocity(this.lastCollisionNormal, this.JumpForce);
            if (jumpVelocity != Vector2.zero)
            {
                this.Velocity = new Vector2(this.Velocity.x + jumpVelocity.x, jumpVelocity.y);
            }

            this.lastCollisionNormal = Vector2.zero;

            this.form.Update(Controller2D.MaxTimeStep);

            this.ApplyVelocity(this.Velocity * Controller2D.MaxTimeStep, false);
            if (inputVelocity.sqrMagnitude > 0)
            {
                this.ApplyVelocity(inputVelocity * Controller2D.MaxTimeStep, true);
            }

            // Drag
            var dragMag = Controller2D.Drag * Controller2D.MaxTimeStep;
            if (Mathf.Abs(this.Velocity.x) > dragMag)
            {
                var drag = new Vector2(-Mathf.Sign(this.Velocity.x) * dragMag, 0);
                this.Velocity += drag;
            }
            else
            {
                this.Velocity = new Vector2(0, this.Velocity.y);
            }
        }
    }

    private void ApplyVelocity(Vector2 velocity, bool isInputVelocity, int recurseDepth = 0)
    {
        var collision = new BoxProjection(this.Collider.bounds, this.Position, velocity).Project();

        var adjustmentVectors = VectorSplitter.Split(velocity, collision);

        this.Position += adjustmentVectors.First;

        if (collision)
        {
            if (this.lastCollisionNormal == Vector2.zero || collision.normal.y > this.lastCollisionNormal.y)
            {
                this.lastCollisionNormal = collision.normal;
            }

            if ((Vector2.Angle(collision.normal, Vector2.up) < 45 && adjustmentVectors.Alignment == CollisionAlignment.Opposed) ||
                collision.normal.y < 0)
            {
                // Ground / Ceiling
                if (!isInputVelocity)
                {
                    this.Velocity = Vector2.zero;
                }
            }
            else
            {
                // Anything else
                if (!isInputVelocity)
                {
                    this.Velocity = this.Velocity.Project(adjustmentVectors.Second);
                }
                if (adjustmentVectors.Second.sqrMagnitude > 0)
                {
                    // HACK: Just give up. We can't seem to get this right.
                    if (recurseDepth >= 3)
                    {
                        this.Position += adjustmentVectors.Second;
                    }
                    else
                    {
                        this.ApplyVelocity(adjustmentVectors.Second, isInputVelocity, recurseDepth + 1);
                    }
                }
            }
        }
        else if (isInputVelocity)
        {
            this.ClampToSlopes(velocity);
        }
    }

    private void ClampToSlopes(Vector2 velocity)
    {
        // If we're already jumping, don't bother
        if (this.Velocity.y > 0)
        {
            return;
        }

        var clampVector = new Vector2(0, -this.form.GetSlopeClampDistance());
        var collision = new BoxProjection(this.Collider.bounds, this.Position, clampVector).Project();
        if (collision)
        {
            if (Vector2.Angle(velocity, collision.normal) < 90)
            {
                this.Position += clampVector * collision.percentToHit;
            }
        }
    }
}
