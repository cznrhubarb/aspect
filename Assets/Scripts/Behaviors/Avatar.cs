using UnityEngine;

public class Avatar : MonoBehaviour
{
    public Controller2D Controller { get; set; }

    void Start()
    {
        this.Controller = new Controller2D(this.GetComponent<BoxCollider2D>());
    }

    void FixedUpdate()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        this.Controller.WalkForce = input.x;
        this.Controller.JumpForce = Mathf.Max(input.y, 0);

        this.Controller.Simulate(Time.deltaTime);
        this.transform.position = this.Controller.Position;
    }
}
