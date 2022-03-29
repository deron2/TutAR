// This complete script can be attached to a camera to make it
// continuously point at another object.

// The target variable shows up as a property in the inspector.
// Drag another object onto it to make the camera look at it.
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{

    void Update()
    {
        // Rotate the object every frame so it keeps looking at the camera (user wearing HMD)
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180f, 0);
        }

    }
}