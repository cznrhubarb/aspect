using UnityEngine;

public class Controller2D
{
    public static readonly float Gravity = -10f;
    public static readonly float WalkSpeed = 15f;
    public static readonly float JumpPower = 12f;
    private const float MaxTimeStep = 1 / 100f;
    private const float FloatTolerance = 0.0001f;

    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }

    private Vector2 inputVelocity;
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

            this.inputVelocity = new Vector2(this.WalkForce * Controller2D.WalkSpeed, 0);
            this.Velocity += new Vector2(0, Controller2D.Gravity) * Controller2D.MaxTimeStep;
            if (this.isOnGround && this.JumpForce > 0)
            {
                this.Velocity += new Vector2(0, Controller2D.JumpPower);
            }

            this.isOnGround = false;

            this.ApplyVelocity(Controller2D.MaxTimeStep);
        }
    }

    private void ApplyVelocity(float simDuration)
    {
        var appliedVelocity = (this.Velocity + this.inputVelocity) * simDuration;

        var collision = new BoxProjection(this.Collider.bounds, this.Position, appliedVelocity).Project();

        appliedVelocity *= collision.percentToHit;
        this.Position += appliedVelocity;

        if (collision)
        {
            if (collision.normal.y > Controller2D.FloatTolerance)
            {
                this.isOnGround = true;
                collision.normal = new Vector2(0, 1);
            }

            this.Velocity -= Vector2.Dot(this.Velocity, collision.normal) * collision.normal;
            this.inputVelocity -= Vector2.Dot(this.inputVelocity, collision.normal) * collision.normal;

            if (this.Velocity.magnitude > Controller2D.FloatTolerance || this.inputVelocity.magnitude > Controller2D.FloatTolerance)
            {
                this.ApplyVelocity(simDuration * (1 - collision.percentToHit));
            }
        }
    }
}
