using System;
using UnityEngine;

public class SpherePhysics : MonoBehaviour, ICollidable
{
    float CoR = 0.8f;
    public float mass = 1f;

    public float Radius
    {
        get { return transform.localScale.x / 2f; }
        set { transform.localScale = Vector3.one * value * 2f; }
    }

    Vector3 velocity = Vector3.zero;
    Vector3 acceleration = Vector3.zero;
    Vector3 oldVelocity;
    Vector3 oldPosition;

    public Vector3 Velocity { get { return velocity; } }

    void Awake()
    {
        oldPosition = transform.position;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        print(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        float T = Time.deltaTime;
        acceleration = new Vector3(0, -9.81f, 0);

        oldVelocity = velocity;
        oldPosition = transform.position;

        velocity += acceleration * T;
        transform.position += velocity * T;
    }

    /// <summary>
    /// Returns the magnitude of the parallel component of vector v parallel to n 
    /// </summary>
    /// <param name="v">Vector to be decomposed</param>
    /// <param name="n">Unit vector parallel to above component</param>
    /// <returns></returns>

    float DistanceToPlane(Vector3 pos, PlanePhysics plane)
    {
        return parallel_Distance(pos - plane.transform.position, plane.Normal) - Radius;
    }

    float DistanceToSphere(Vector3 pos, Vector3 otherPos, SpherePhysics other)
    {
        return Vector3.Distance(pos, otherPos) - (Radius + other.Radius);
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

            float d0 = DistanceToPlane(oldPosition, physics);
            float d1 = DistanceToPlane(transform.position, physics);

            return d1 < 0 && d0 > 0;
        }

        if (collidable is SpherePhysics)

        {
            // code for sphere / sphere
            SpherePhysics physics = (SpherePhysics)collidable;

            float d0 = DistanceToSphere(oldPosition, physics.oldPosition, physics);
            float d1 = DistanceToSphere(transform.position, physics.transform.position, physics);

            return d1 < 0 && d0 >= 0;
        }

        return false;
    }

    public void ResolveCollision(ICollidable collidable, ref Vector3 updatedPosition, ref Vector3 updatedVelocity)
    {
        float T = Time.deltaTime;

        if (collidable is PlanePhysics)
        {
            PlanePhysics physics = (PlanePhysics)collidable;
            Vector3 n = physics.Normal.normalized;

            float d0 = DistanceToPlane(oldPosition, physics);
            float d1 = DistanceToPlane(transform.position, physics);

            float vDrop = (d1 - d0) / T;
            float tImpact = -d0 / vDrop;

            Vector3 vImpact = oldVelocity + acceleration * tImpact;
            Vector3 pImpact = oldPosition + velocity * tImpact;

            Vector3 vParallel = Vector3.Dot(vImpact, n) * n;
            Vector3 vPerp = vImpact - vParallel;
            Vector3 vImpactOut = vPerp - CoR * vParallel;

            float tRemaining = T - tImpact;
            velocity = vImpactOut + acceleration * tRemaining;
            transform.position = pImpact + velocity * tRemaining;

            updatedPosition = physics.transform.position;
            updatedVelocity = Vector3.zero;
        }

        if (collidable is SpherePhysics)
        {
            SpherePhysics other = (SpherePhysics)collidable;

            float d0 = DistanceToSphere(oldPosition, other.oldPosition, other);
            float d1 = DistanceToSphere(transform.position, other.transform.position, other);

            float vDrop = (d1 - d0) / T;
            float tImpact = -d0 / vDrop;

            Vector3 p1 = oldPosition + velocity * tImpact;
            Vector3 p2 = other.oldPosition + other.velocity * tImpact;
            Vector3 v1 = oldVelocity + acceleration * tImpact;
            Vector3 v2 = other.oldVelocity + other.acceleration * tImpact;

            Vector3 n = (p2 - p1).normalized;

            float u1 = Vector3.Dot(v1, n);
            float u2 = Vector3.Dot(v2, n);
            Vector3 v1Perp = v1 - u1 * n;
            Vector3 v2Perp = v2 - u2 * n;

            float m1 = mass, m2 = other.mass;
            float u1Out = ((m1 - m2) / (m1 + m2)) * u1 + (2 * m2 / (m1 + m2)) * u2;
            float u2Out = ((m2 - m1) / (m1 + m2)) * u2 + (2 * m1 / (m1 + m2)) * u1;

            Vector3 v1Out = CoR * u1Out * n + v1Perp;
            Vector3 v2Out = CoR * u2Out * n + v2Perp;

            float tRemaining = T - tImpact;

            velocity = v1Out + acceleration * tRemaining;
            transform.position = p1 + velocity * tRemaining;

            updatedVelocity = v2Out + other.acceleration * tRemaining;
            updatedPosition = p2 + updatedVelocity * tRemaining;
        }
    }

    public void UpdatePositionandVelocity(Vector3 newPosition, Vector3 newVelocity)
    {
        transform.position = newPosition;
        velocity = newVelocity;
    }
}