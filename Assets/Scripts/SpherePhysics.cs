using UnityEngine;

public class SpherePhysics : MonoBehaviour, ICollidable
{
    // coefficient of restitution, 1 = perfect bounce
    public float CoR = 0.8f;
    public float friction = 1.5f;
    public Vector3 velocity;

    public SpherePhysics lastTouched;

    static Vector3 gravity = new Vector3(0, -9.81f, 0);

    // last frame, for the time of impact
    Vector3 oldPosition;
    Vector3 oldVelocity;

    public float Radius
    {
        get { return transform.localScale.x / 2f; }
    }

    void Awake()
    {
        oldPosition = transform.position;
    }

    void Update()
    {
        oldPosition = transform.position;
        oldVelocity = velocity;

        velocity += gravity * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
    }

    float DistanceToPlane(Vector3 position, PlanePhysics plane)
    {
        return Vector3.Dot(position - plane.transform.position, plane.Normal) - Radius;
    }

    float DistanceToSphere(Vector3 position, Vector3 otherPosition, SpherePhysics other)
    {
        return Vector3.Distance(position, otherPosition) - (Radius + other.Radius);
    }

    float ImpactTime(float d0, float d1)
    {
        return Mathf.Clamp(d0 / (d0 - d1), 0f, 1f) * Time.deltaTime;
    }

    public bool CollidingWith(ICollidable other)
    {
        if (other is PlanePhysics)
        {
            PlanePhysics plane = (PlanePhysics)other;
            return DistanceToPlane(transform.position, plane) < 0 && Vector3.Dot(velocity, plane.Normal) < 0;
        }

        if (other is SpherePhysics)
        {
            SpherePhysics ball = (SpherePhysics)other;
            return DistanceToSphere(transform.position, ball.transform.position, ball) < 0;
        }

        return false;
    }

    public void ResolveCollision(ICollidable other)
    {
        if (other is PlanePhysics)
        {
            PlanePhysics plane = (PlanePhysics)other;
            Vector3 n = plane.Normal;

            // rewind to the moment of impact
            float d0 = DistanceToPlane(oldPosition, plane);
            float d1 = DistanceToPlane(transform.position, plane);
            float t = ImpactTime(d0, d1);
            Vector3 p = oldPosition + oldVelocity * t;
            Vector3 v = oldVelocity + gravity * t;

            v -= (1 + CoR) * Vector3.Dot(v, n) * n;

            Vector3 sliding = v - Vector3.Dot(v, n) * n;
            v -= sliding * Mathf.Min(1f, friction * Time.deltaTime);

            FinishFrame(p, v, t);

            // push out so resting balls don't sink
            float d = DistanceToPlane(transform.position, plane);
            if (d < 0)
                transform.position -= d * n;
        }

        if (other is SpherePhysics)
        {
            SpherePhysics ball = (SpherePhysics)other;
            Vector3 between = ball.transform.position - transform.position;

            if (Vector3.Dot(velocity - ball.velocity, between) > 0)
                Bounce(ball);

            // push apart so balls don't get stuck
            float overlap = -DistanceToSphere(transform.position, ball.transform.position, ball);
            if (overlap > 0)
            {
                Vector3 n = (ball.transform.position - transform.position).normalized;
                transform.position -= n * overlap / 2f;
                ball.transform.position += n * overlap / 2f;
            }
        }
    }

    void Bounce(SpherePhysics ball)
    {
        float d0 = DistanceToSphere(oldPosition, ball.oldPosition, ball);
        float d1 = DistanceToSphere(transform.position, ball.transform.position, ball);
        float t = ImpactTime(d0, d1);
        Vector3 p1 = oldPosition + oldVelocity * t;
        Vector3 p2 = ball.oldPosition + ball.oldVelocity * t;
        Vector3 v1 = oldVelocity + gravity * t;
        Vector3 v2 = ball.oldVelocity + gravity * t;

        // same mass, so they swap speed along the line between them
        Vector3 n = (p2 - p1).normalized;
        float push = (1 + CoR) / 2f * Vector3.Dot(v1 - v2, n);
        v1 -= push * n;
        v2 += push * n;

        FinishFrame(p1, v1, t);
        ball.FinishFrame(p2, v2, t);

        lastTouched = ball;
        ball.lastTouched = this;
    }

    void FinishFrame(Vector3 impactPosition, Vector3 impactVelocity, float impactTime)
    {
        float timeLeft = Time.deltaTime - impactTime;
        velocity = impactVelocity + gravity * timeLeft;
        transform.position = impactPosition + velocity * timeLeft;

        // so another collision this frame doesn't undo the bounce
        oldVelocity = impactVelocity - gravity * impactTime;
        oldPosition = impactPosition - oldVelocity * impactTime;
    }
}
