using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class ObjectDragDrop : MonoBehaviour
{
    private Plane movePlane;
    private float fixedDistance = 2f;
    private float hitDist, t;
    private Ray camRay;
    private Vector3 startPos, point, corPoint;
    private bool top = true;
    private bool dragging = false;

    int speed = 12;
    float friction = 0.5f;
    float lerpSpeed = 1.5f;
    float xDeg;
    float yDeg;
    Quaternion fromRotation;
    Quaternion toRotation;

    public Camera bodyCamera;
    public Button perspectiveButton;


    private void Start()
    {
        perspectiveButton.onClick.AddListener(perspectiveChanged);

    }
    void perspectiveChanged()
    {
        top = !top;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            dragging = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            dragging = false;
        }

        if (dragging)
        {
            yourOnMouseDragRight();
        }
    }

    private void yourOnMouseDragRight()
    {
        xDeg -= Input.GetAxis("Mouse X") * speed * friction;
        yDeg += Input.GetAxis("Mouse Y") * speed * friction;
        fromRotation = transform.rotation;
        toRotation = Quaternion.Euler(yDeg, xDeg, 0);
        transform.rotation = Quaternion.Lerp(fromRotation, toRotation, Time.deltaTime * lerpSpeed);

    }

    void OnMouseDown()
    {

        startPos = transform.position; // save position in case draged to invalid place
        movePlane = new Plane(-bodyCamera.transform.forward, transform.position); // find a parallel plane to the camera based on obj start pos;
    }

    void OnMouseDrag()
    {
        camRay = bodyCamera.ScreenPointToRay(Input.mousePosition); // shoot a ray at the obj from mouse screen point

        if (movePlane.Raycast(camRay, out hitDist))
        { // finde the collision on movePlane
            if (top)
            {
                fixedDistance = gameObject.transform.position.y;
                point = camRay.GetPoint(hitDist); // define the point on movePlane
                t = -(fixedDistance - camRay.origin.y) / (camRay.origin.y - point.y); // the x,y or z plane you want to be fixed to
                corPoint.x = camRay.origin.x + (point.x - camRay.origin.x) * t; // calculate the new point t futher along the ray
                corPoint.y = fixedDistance;
                corPoint.z = camRay.origin.z + (point.z - camRay.origin.z) * t;
                transform.position = corPoint;

            }
            else
            {
                fixedDistance = gameObject.transform.position.x;
                point = camRay.GetPoint(hitDist); // define the point on movePlane
                t = -(fixedDistance - camRay.origin.x) / (camRay.origin.x - point.x); // the x,y or z plane you want to be fixed to
                corPoint.x = fixedDistance; // calculate the new point t futher along the ray
                corPoint.y = camRay.origin.y + (point.y - camRay.origin.y) * t;
                corPoint.z = camRay.origin.z + (point.z - camRay.origin.z) * t;
                transform.position = corPoint;
            }

        }
    }
}