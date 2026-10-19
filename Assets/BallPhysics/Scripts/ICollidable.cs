public interface ICollidable
{
    bool CollidingWith(ICollidable other);
    void ResolveCollision(ICollidable other);
}
