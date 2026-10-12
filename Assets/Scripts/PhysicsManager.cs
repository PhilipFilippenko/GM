using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    List<ICollidable> objects = new List<ICollidable>();

    // planes go last so balls never end up inside a wall
    void Awake()
    {
        foreach (MonoBehaviour mb in FindObjectsByType<MonoBehaviour>())
            if (mb is ICollidable)
                objects.Add((ICollidable)mb);

        objects.Sort((a, b) => (a is PlanePhysics).CompareTo(b is PlanePhysics));
    }

    public void Register(ICollidable collidable)
    {
        objects.Insert(0, collidable);
    }

    void LateUpdate()
    {
        objects.RemoveAll(o => (MonoBehaviour)o == null);

        for (int i = 0; i < objects.Count - 1; i++)
            for (int j = i + 1; j < objects.Count; j++)
                if (objects[i].CollidingWith(objects[j]))
                    objects[i].ResolveCollision(objects[j]);
    }
}
