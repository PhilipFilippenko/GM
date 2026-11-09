using UnityEngine;
using UnityEngine.InputSystem;

public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2.5f, -5f);

    Vector3 velocity;
    float angle;

    void LateUpdate()
    {
        if (Mouse.current.rightButton.isPressed)
            angle += Mouse.current.delta.ReadValue().x * 0.2f;

        Vector3 rotatedOffset = Quaternion.Euler(0f, angle, 0f) * offset;
        Vector3 cameraPosition = target.position + rotatedOffset;
        transform.position = Vector3.SmoothDamp(transform.position, cameraPosition, ref velocity, 0.25f);

        Vector3 chest = target.position + Vector3.up * 1.4f;
        transform.LookAt(chest);
    }
}
