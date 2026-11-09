using UnityEngine;

public class HeadLookAt : MonoBehaviour
{
    public Transform target;
    public float maxAngle = 70f;

    Transform head;

    void Start()
    {
        Animator animator = GetComponent<Animator>();
        head = animator.GetBoneTransform(HumanBodyBones.Head);
    }

    private void LateUpdate()
    {
        Vector3 direction = target.position - head.position;
        direction = Vector3.RotateTowards(transform.forward, direction, maxAngle * Mathf.Deg2Rad, 0f);
        head.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
