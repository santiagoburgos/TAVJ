using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputsPacket 
{
    public int id;
    public float inputNumber;
    public float inputPlayer;
    
    public float horizontal;
    public float vertical;
    public float rotationY;
    public float rotationX;
    public bool jump;

    public bool walking;
    public bool shooting;
    public bool reloading;

    public InputsPacket(int id, float inpn, float inpp, float hor, float ver, float rotY, float rotX, bool jump, bool walking, bool shooting, bool reloading)
    {
        this.id = id;
        inputNumber = inpn;
        inputPlayer = inpp;
        horizontal = hor;
        vertical = ver;
        rotationY = rotY;
        rotationX = rotX;
        this.jump = jump;
        this.walking = walking;
        this.shooting = shooting;
        this.reloading = reloading;
    }
}

