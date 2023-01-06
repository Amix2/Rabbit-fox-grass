using UnityEngine;

public class CameraConroller : MonoBehaviour
{
    public Transform root;
    public Vector3 defaultOffsetDir = new Vector3(0f, 1f, 1f);
    public float offsetLength = 15f;
    public float maxCameraFrameMove = 1f;

    private const float minCameraDistance = 1f;

    private void Start()
    {
        transform.SetPositionAndRotation(root.position, root.rotation);
        transform.Translate(defaultOffsetDir.normalized * offsetLength, Space.World);
        transform.LookAt(root);
    }

    private void LateUpdate()
    {
        UpdateAngle();
        UpdateDistance();
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (Input.GetMouseButton(1))
        {
            // move root based on mouse movement, camera is a child of root so it will move automatically
            Vector3 mouseMove = new Vector3(Input.GetAxis("Mouse X"), 0f, Input.GetAxis("Mouse Y"));
            Vector3 cameraRootMove = mouseMove * offsetLength * Settings.Player.cameraMoveSensitivity * Time.deltaTime;
            if (cameraRootMove.sqrMagnitude > maxCameraFrameMove * maxCameraFrameMove) cameraRootMove = cameraRootMove.normalized * maxCameraFrameMove;
            root.Translate(mouseMove * offsetLength * Settings.Player.cameraMoveSensitivity * Time.deltaTime);
        }
    }

    private void UpdateDistance()
    {
        //canculate zoom based on % of distance to root, cameraScrollSensitivity and delta time
        float change = offsetLength * Settings.Player.cameraScrollSensitivity * Input.mouseScrollDelta.y * Time.deltaTime;
        float distance = (transform.position - root.position).magnitude;
        if (change > distance - minCameraDistance)
        {
            // camera cant get closer than minCameraDistance to root
            change = distance - minCameraDistance;
        }
        transform.Translate(Vector3.forward * change, Space.Self);
        offsetLength -= change;
    }

    private void UpdateAngle()
    {
        if (Input.GetMouseButton(0))
        {
            // rotate root around Up vector to keep it flat on the ground
            root.RotateAround(root.position, Vector3.up, Input.GetAxis("Mouse X") * Settings.Player.cameraRotateSensitivity * Time.deltaTime);
            // change camera vertical angle
            transform.RotateAround(root.position, root.right, Input.GetAxis("Mouse Y") * Settings.Player.cameraRotateSensitivity * Time.deltaTime);
        }
    }
}