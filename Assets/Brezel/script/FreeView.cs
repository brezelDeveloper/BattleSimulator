using UnityEngine;

public class FreeView : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float turnSpeed = 90f;
    public float zoomSpeed = 1f;

    void Update()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;

        Vector3 move = transform.right * Input.GetAxis("Horizontal") + forward.normalized * Input.GetAxis("Vertical");
        transform.position += move * moveSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Q)) transform.Rotate(0, -turnSpeed * Time.deltaTime, 0, Space.World);
        if (Input.GetKey(KeyCode.E)) transform.Rotate(0, turnSpeed * Time.deltaTime, 0, Space.World);

        transform.position += transform.forward * Input.mouseScrollDelta.y * zoomSpeed;
    }
}