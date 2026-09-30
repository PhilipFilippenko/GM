using UnityEngine;

public class PlanePhysics : MonoBehaviour, ICollidable
{
    public Vector3 Normal
    {
        get { return transform.up; }
    }

    public bool CollidingWith(ICollidable collidable)
    {
        throw new System.NotImplementedException();
    }



    public void ResolveCollision(ICollidable collidable, ref Vector3 updatedPosition, ref Vector3 updatedVelocity)
    {
        throw new System.NotImplementedException();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
