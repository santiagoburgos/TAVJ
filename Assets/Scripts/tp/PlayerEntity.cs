using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEntity
{
    
    public static void Serialize(Transform player, Transform camerarotation, BitBuffer buffer) {
        var transform = player.transform;
        var position = player.position;
        
        buffer.PutFloat(position.x);
        buffer.PutFloat(position.y);
        buffer.PutFloat(position.z);
        
        Quaternion rotation = transform.rotation;
        buffer.PutFloat(rotation.eulerAngles.x);
        buffer.PutFloat(rotation.eulerAngles.y);
        buffer.PutFloat(rotation.eulerAngles.z);

        Quaternion camerarot = camerarotation.transform.rotation;
        buffer.PutFloat(camerarot.eulerAngles.x);
        buffer.PutFloat(camerarot.eulerAngles.y);
        buffer.PutFloat(camerarot.eulerAngles.z);


    }
    
    public static Vector3 DeserializePos(BitBuffer buffer) {
        var position = new Vector3();
        position.x = buffer.GetFloat();
        position.y = buffer.GetFloat();
        position.z = buffer.GetFloat();

        return position;
    }
    
    public static Quaternion DeserializeRot(BitBuffer buffer) {
        var rotationv = new Vector3(buffer.GetFloat(), buffer.GetFloat(), buffer.GetFloat());
        Quaternion rotation = new Quaternion();
        rotation.eulerAngles = rotationv;
        
        return rotation;
    }

   

}

public class Snaplayer
{/*
    private int id;
    private Vector3 plaposition;
    private Quaternion plarotation;
    private Quaternion camrotation;
    

    public Snaplayer(BitBuffer buffer)
    {
        id = buffer.GetInt();
        plaposition = new Vector3(buffer.GetFloat(), buffer.GetFloat(), buffer.GetFloat());
        camrotation.eulerAngles = new Vector3(buffer.GetFloat(), 0f, 0f);
        plarotation.eulerAngles = new Vector3(0f, buffer.GetFloat(), 0f);
    }*/

}




