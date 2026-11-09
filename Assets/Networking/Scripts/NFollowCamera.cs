using UnityEngine;

public class NFollowCamera : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2.5f, -5f);

    Vector3 velocity;

    void Update()
    {
        if (target == null) return;

        Vector3 cameraPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, cameraPosition, ref velocity, 0.25f);

        Vector3 chest = target.position + Vector3.up * 1.4f;
        transform.LookAt(chest);
    }
}