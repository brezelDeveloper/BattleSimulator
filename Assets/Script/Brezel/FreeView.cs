using UnityEngine;

public class FreeView : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 3f;

    float yaw;
    float pitch;

    void Start()
    {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        if (pitch > 180f) pitch -= 360f;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * lookSpeed;
            pitch -= Input.GetAxis("Mouse Y") * lookSpeed;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }

        Vector3 move = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
        if (Input.GetKey(KeyCode.Space)) move += Vector3.up;
        if (Input.GetKey(KeyCode.LeftControl)) move += Vector3.down;

        transform.position += move * moveSpeed * Time.deltaTime;
    }
}