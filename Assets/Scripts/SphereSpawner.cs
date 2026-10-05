using UnityEngine;
using UnityEngine.InputSystem;

public class SphereSpawner : MonoBehaviour
{
    public SpherePhysics spherePrefab;
    public PhysicsManager manager;
    public float halfDistance = 6f;
    public float speedX = 4f;
    public float speedY = 6f;
    public float lifeTime = 10f;

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Spawn(transform.position + Vector3.left * halfDistance, new Vector3(speedX, speedY, 0));
            Spawn(transform.position + Vector3.right * halfDistance, new Vector3(-speedX, speedY, 0));
        }
    }

    void Spawn(Vector3 position, Vector3 velocity)
    {
        SpherePhysics sphere = Instantiate(spherePrefab, position, Quaternion.identity);
        sphere.Radius = Random.Range(0.3f, 0.8f);
        sphere.mass = Mathf.Pow(sphere.Radius, 3);
        sphere.UpdatePositionandVelocity(position, velocity);
        manager.Register(sphere);
        Destroy(sphere.gameObject, lifeTime);
    }
}