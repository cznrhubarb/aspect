using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow
{
    private GameObject following;
    public Vector2 Position { get; private set; }

    public CameraFollow(GameObject go)
    {
        this.following = go;
        this.Position = go.transform.position;
    }

    public void Update(float elapsedTime)
    {
        this.Position = this.following.transform.position;
    }
}
