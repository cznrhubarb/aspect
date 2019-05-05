using UnityEngine;

public class Controller2D
{
    private const float MaxTimeStep = 1 / 100f;

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
        }
    }

    private void ApplyVelocity(Vector2 velocity, bool isInputVelocity)
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

            if (Vector2.Angle(collision.normal, Vector2.up) < 45 && adjustmentVectors.Alignment == CollisionAlignment.Opposed)
            {
                // Ground
                if (!isInputVelocity)
                {
                    this.Velocity = Vector2.zero;
                }
            }
            else if (collision.normal.y < 0)
            {
                // Ceiling
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
                    this.Position += adjustmentVectors.Second;
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
