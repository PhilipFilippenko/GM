using UnityEngine;

public interface ICollidable
{
    bool CollidingWith(ICollidable collidable);
    void ResolveCollision(ICollidable collidable, ref Vector3 updatedPosition, ref Vector3 updatedVelocity);
    void UpdatePositionandVelocity(Vector3 newPosition, Vector3 newVelocity);
}