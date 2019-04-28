using UnityEngine;

public class Controller2D
{
    public const float Gravity = -40f;
    public const float WalkSpeed = 15f;
    public const float JumpPower = 22f;
    private const float MaxTimeStep = 1 / 100f;
    private const float FloatTolerance = 0.0001f;
    private const float MaxClampDistance = -Gravity / 200;

    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }

    public float WalkForce { get; set; }
    public float JumpForce { get; set; }

    public BoxCollider2D Collider { get; private set; }

    private bool isOnGround;

    private float simulationTimer;

    public Controller2D(BoxCollider2D collider)
    {
        this.Position = collider.transform.position;
        this.Collider = collider;
    }

    public void Simulate(float elapsedTime)
    {
        this.simulationTimer += elapsedTime;

        while (this.simulationTimer >= Controller2D.MaxTimeStep)
        {
            this.simulationTimer -= Controller2D.MaxTimeStep;

            var inputVelocity = new Vector2(this.WalkForce * Controller2D.WalkSpeed, 0);
            this.Velocity += new Vector2(0, Controller2D.Gravity) * Controller2D.MaxTimeStep;
            if (this.isOnGround && this.JumpForce > 0)
            {
                this.Velocity += new Vector2(0, Controller2D.JumpPower);
            }

            this.isOnGround = false;

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
            if (Vector2.Angle(collision.normal, Vector2.up) < 45 && adjustmentVectors.Alignment == CollisionAlignment.Opposed)
            {
                this.isOnGround = true;
                if (!isInputVelocity)
                {
                    this.Velocity = Vector2.zero;
                }
            }
            else
            {
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
        var clampVector = new Vector2(0, -MaxClampDistance);
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
