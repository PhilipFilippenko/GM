using UnityEngine;

public class VesselBuiltInIK : MonoBehaviour
{
    Animator animator;
    float footWeight;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        bool standing = animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && !animator.IsInTransition(0);

        float targetWeight = 0f;
        if (standing) targetWeight = 1f;
        footWeight = Mathf.MoveTowards(footWeight, targetWeight, 5f * Time.deltaTime);

        float leftShift = PlaceFoot(AvatarIKGoal.LeftFoot);
        float rightShift = PlaceFoot(AvatarIKGoal.RightFoot);

        float lowestShift = Mathf.Min(leftShift, rightShift);
        if (lowestShift < 0f)
            animator.bodyPosition += Vector3.up * lowestShift * footWeight;
    }

    float PlaceFoot(AvatarIKGoal foot)
    {
        if (footWeight <= 0f)
        {
            animator.SetIKPositionWeight(foot, 0f);
            animator.SetIKRotationWeight(foot, 0f);
            return 0f;
        }

        Vector3 footPosition = animator.GetIKPosition(foot);
        Vector3 rayStart = footPosition + Vector3.up * 0.6f;
        RaycastHit hit;
        if (!Physics.Raycast(rayStart, Vector3.down, out hit, 1.2f))
        {
            animator.SetIKPositionWeight(foot, 0f);
            animator.SetIKRotationWeight(foot, 0f);
            return 0f;
        }

        float ankleHeight = footPosition.y - transform.position.y;
        if (ankleHeight < 0.1f) ankleHeight = 0.1f;

        Vector3 newFootPosition = hit.point + hit.normal * ankleHeight;
        Quaternion slope = Quaternion.FromToRotation(Vector3.up, hit.normal);
        Quaternion newFootRotation = slope * animator.GetIKRotation(foot);

        animator.SetIKPositionWeight(foot, footWeight);
        animator.SetIKRotationWeight(foot, footWeight);
        animator.SetIKPosition(foot, newFootPosition);
        animator.SetIKRotation(foot, newFootRotation);

        return newFootPosition.y - footPosition.y;
    }
}
