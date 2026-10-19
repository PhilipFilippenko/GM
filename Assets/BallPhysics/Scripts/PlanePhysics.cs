using UnityEngine;

public class PlanePhysics : MonoBehaviour, ICollidable
{
    public Vector3 Normal
    {
        get { return transform.up; }
    }

    public bool CollidingWith(ICollidable other)
    {
        if (other is SpherePhysics)
            return other.CollidingWith(this);
        return false;
    }

    public void ResolveCollision(ICollidable other)
    {
        if (other is SpherePhysics)
            other.ResolveCollision(this);
    }
}
