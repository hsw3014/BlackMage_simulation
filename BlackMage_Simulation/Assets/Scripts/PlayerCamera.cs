using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform player;

    public float speed;
    public Vector2 offset;

    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    private float cameraHalfWidth, cameraHalfHeight;

    // Start is called before the first frame update
    void Start()
    {
        speed = 10;
        cameraHalfWidth = Camera.main.aspect * Camera.main.orthographicSize;
        cameraHalfHeight = Camera.main.orthographicSize;
    }

    private void LateUpdate()
    {
        Vector3 desiredPosition = new Vector3(
            Mathf.Clamp(player.position.x + offset.x, minX + cameraHalfWidth, maxX - cameraHalfWidth),
            Mathf.Clamp(player.position.y + offset.y, minY + cameraHalfHeight, maxY - cameraHalfHeight),
            -10);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * speed);
    }
}
