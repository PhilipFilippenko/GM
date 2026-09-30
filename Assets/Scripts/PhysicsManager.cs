using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{

    List<ICollidable> allObjects;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allObjects = new List<ICollidable>();

        // code to fill list


    }

    // Update is called once per frame
    void Update()
    {


        // check for collisions

        for (int i = 0; i < allObjects.Count-1; i++)
            for (int j =i+1; j< allObjects.Count; j++)
            { // test for collision
              if (allObjects[i].CollidingWith(allObjects[j]) )  {

                    Vector3 newPositionj = Vector3.zero, newVelocityj = Vector3.zero;
                    allObjects[i].ResolveCollision(allObjects[j], ref newPositionj, ref newVelocityj);
                    allObjects[j].UpdatePositionandVelocity(newPositionj, newVelocityj);
                }
        }
    }
}
