using UnityEngine;

public class PlanePhysics : MonoBehaviour, ICollidable
{
    public Vector3 Normal
    {
        get { return transform.up; }
    }

    public bool CollidingWith(ICollidable collidable)
    {
        if (collidable is SpherePhysics)
            return collidable.CollidingWith(this);

        return false;
    }

    public void ResolveCollision(ICollidable collidable, ref Vector3 updatedPosition, ref Vector3 updatedVelocity)
    {
        if (collidable is SpherePhysics)
        {
            SpherePhysics sphere = (SpherePhysics)collidable;
            Vector3 p = Vector3.zero, v = Vector3.zero;
            sphere.ResolveCollision(this, ref p, ref v);

            updatedPosition = sphere.transform.position;
            updatedVelocity = sphere.Velocity;
        }
    }

    public void UpdatePositionandVelocity(Vector3 newPosition, Vector3 newVelocity)
    {
    }
}