using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputPlayer
{
    public float inputplayern;

    public float horizontal;
    public float vertical;
    public float rotationy;
    public float rotationx;
    public bool jump;
    
    public Vector3 position;
    public Quaternion rotation;
    public Quaternion camrotation;

    public InputPlayer(Vector3 position, Quaternion rotation, Quaternion camrotation, float inputnumber, float horizontal, float vertical, float rotationy, float rotationx, bool jump)
    {
        this.inputplayern = inputnumber;

        this.position = position;
        this.rotation = rotation;
        this.camrotation = camrotation;
        
        this.horizontal = horizontal;
        this.vertical = vertical;
        this.rotationy = rotationy;
        this.rotationx = rotationx;
        this.jump = jump;
    }
}