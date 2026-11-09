using UnityEngine;

public class VesselAnimationEvents : MonoBehaviour
{
    public void FootStep(AnimationEvent e)
    {
        if (e.animatorClipInfo.weight < 0.5f) return;
        Debug.Log("Step: " + e.animatorClipInfo.clip.name);
    }

    public void Hit()
    {
        Debug.Log("Hit");
    }
}
