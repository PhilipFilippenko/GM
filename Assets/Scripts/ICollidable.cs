using UnityEngine;

public interface ICollidable
{
    //Vector3 Position { get; set; }
    //Vector3 Velocity { get; set; }
    //Vector3 Acceleration { get; }
    //Vector3 OldPosition { get; }
    //Vector3 OldVelocity { get; }
    //float Radius { get; }
    //float Mass { get; }

    bool CollidingWith(ICollidable collidable);
    void ResolveCollision(ICollidable collidable, ref Vector3 updatedPosition, ref Vector3 updatedVelocity);
}