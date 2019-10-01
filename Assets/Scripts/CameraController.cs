using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public float height = -10f;

    private Camera camera;

    private float moveCameraRadius = 2f;
    private bool transitioning = false;
    private Vector3 velocity = Vector3.zero;
    private float smoothTime = 0.3f;

    private float targetOrtho = 5f;
    private float smoothZoomSpeed = 4f;
    private Vector3 zoomVelocity = Vector3.zero;


    private GridSystem gridSystem;

    void Start()
    {
        camera = this.GetComponent<Camera>();
        gridSystem = (GridSystem) GameObject.Find("GridSystem").GetComponent<GridSystem>();
        transform.position = new Vector3(
            player.transform.position.x,
            player.transform.position.y,
            height
        );
        Debug.Log(camera.orthographicSize);
    }

    void Update() {
        if (gridSystem.activated) { targetOrtho = 4.5f; }
        else { targetOrtho = 5f; }
    }

    void LateUpdate()
    {
        var movingRadius = moveCameraRadius;

        // smooth follow the player character in overworld
        if (transitioning) {
            // if the player breached the radius to move the camera,
            // the camera will follow until the player reached the center again
            movingRadius = 0;
        }
        if (Vector2.Distance(transform.position, player.transform.position) > movingRadius) {
            transitioning = true;
            var transition = Vector3.SmoothDamp(
                transform.position,
                player.transform.position,
                ref velocity,
                smoothTime
            );
            Debug.Log(Vector2.Distance(transform.position, player.transform.position));
            transform.position = new Vector3(transition.x, transition.y, height);
        }
        if (Vector2.Distance(transform.position, player.transform.position) < .01f) {
            // if the player is close to the center of the camera again, the camera-move radius will be used again
            transitioning = false;
        }

        // zooming when grid is activated
        if (camera.orthographicSize != targetOrtho) {
            camera.orthographicSize = Mathf.MoveTowards (camera.orthographicSize, targetOrtho, smoothZoomSpeed * Time.deltaTime);
        }
    }
}
