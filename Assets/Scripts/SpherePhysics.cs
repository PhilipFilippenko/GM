using System;
using UnityEngine;

public class SpherePhysics : MonoBehaviour, ICollidable
{
    public PlanePhysics thePlane;
    float d0;
    float CoR = 0.8f;

    public float Radius
    { 
        get { return transform.localScale.x / 2f; } 
        set { transform.localScale = Vector3.one * value; }
    }

    Vector3 velocity = Vector3.zero;
    Vector3 acceleration = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        print(transform.position);
        d0 = DistanceToPlane(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        float T = Time.deltaTime;
        acceleration = new Vector3(0, -9.81f, 0);

        Vector3 oldVelocity = velocity;
        Vector3 oldPosition = transform.position;

        velocity += acceleration * T;
        transform.position += velocity * T;

        float d1 = DistanceToPlane(transform.position);

        if (d1 < 0 && d0 > 0)
        {
            Vector3 n = thePlane.Normal.normalized;

            float vDrop = (d1 - d0) / T;
            float tImpact = -d0 / vDrop;

            Vector3 vImpact = oldVelocity + acceleration * tImpact;
            Vector3 pImpact = oldPosition + velocity * tImpact;

            Vector3 vParallel = Vector3.Dot(velocity, n) * n;
            Vector3 vPerp = velocity - vParallel;
            Vector3 vImpactOut = vPerp - CoR * vParallel;

            float tRemaining = T - tImpact;
            velocity = vImpactOut + acceleration * tRemaining;
            transform.position = pImpact + velocity * tRemaining;
        }


        d0 = DistanceToPlane(transform.position);
    }

    /// <summary>
    /// Returns the magnitude of the parallel component of vector v parallel to n 
    /// </summary>
    /// <param name="v">Vector to be decomposed</param>
    /// <param name="n">Unit vector parallel to above component</param>
    /// <returns></returns>

    float DistanceToPlane(Vector3 pos)
    {
        return parallel_Distance(pos - thePlane.transform.position, thePlane.Normal) - Radius;
    }

    float parallel_Distance(Vector3 v, Vector3 n)
    {
        return Vector3.Dot(v, n.normalized);
    }

    public bool CollidingWith(ICollidable collidable)
    {
        if (collidable is PlanePhysics)
        { // code for sphere plane collision}
            PlanePhysics physics = (PlanePhysics)collidable;

        }

        if (collidable is SpherePhysics)

        {
            // code for sphere / sphere
            SpherePhysics physics = (SpherePhysics)collidable;

        }


    }



    public void ResolveCollision(ICollidable collidable, ref Vector3 updatedPosition, ref Vector3 updatedVelocity)
    {
        throw new NotImplementedException();
    }
}
