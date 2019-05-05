using UnityEngine;

public class PlayerFollowCamera : MonoBehaviour
{
    [SerializeField]
    private GameObject playerAvatar;
    private CameraFollow follower;

    void Start()
    {
        this.follower = new CameraFollow(this.playerAvatar);
    }

    void Update()
    {
        this.follower.Update(Time.deltaTime);
        //this.transform.position = new Vector3(this.follower.Position.x, this.follower.Position.y, -10);
    }
}
