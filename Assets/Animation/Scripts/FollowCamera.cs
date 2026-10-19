using UnityEngine;

// Keeps the camera behind and above a target, smoothing position and look direction.
public class FollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 14f, -26f);
    public float lookHeight = 9f;
    public float smoothTime = 0.25f;

    Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = target.TransformPoint(offset);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
        transform.rotation = Quaternion.LookRotation(target.position + Vector3.up * lookHeight - transform.position);
    }
}
