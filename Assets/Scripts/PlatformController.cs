using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    [SerializeField] private string rotationAxis;
    [SerializeField] private float rotationSpeed, peakRotation = 45;
    private float currentRotation, direction;
    private Vector3 rotationVector;

    private void Start()
    {
        rotationAxis = rotationAxis.ToLower();
        peakRotation = Mathf.Abs(peakRotation);
        if(rotationAxis == "x")
        {
            currentRotation = transform.localEulerAngles.x;
        }
        else
        {
            currentRotation = transform.localEulerAngles.z;
        }
        rotationVector = transform.localEulerAngles;
        if(currentRotation <= 0)
        {
            direction = 1;
        }
        else
        {
            direction = -1;
        }
    }

    private void Update()
    {
        currentRotation += rotationSpeed * direction * Time.deltaTime;
        currentRotation = Mathf.Clamp(currentRotation, -peakRotation, peakRotation);
        if(rotationAxis == "x") 
        {
            rotationVector.x = currentRotation;
        }
        else 
        {
            rotationVector.z = currentRotation;
        }
        if(currentRotation >= peakRotation || currentRotation <= -peakRotation)
        {
            direction *= -1;
        }
        transform.localEulerAngles = rotationVector;
    }
}
