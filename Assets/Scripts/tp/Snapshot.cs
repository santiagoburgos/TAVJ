using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snapshot
{
   
        public Vector3 position;
        public Quaternion rotation;
        public float time;
    
        /*
        public Vector3 getPosition()
        {
            return position;
        }
        public Quaternion getRotation()
        {
            return rotation;
        }
        public void set(BitBuffer buff)
        {
            position = CubeEntity.DeserializePos(buff);
            rotation = CubeEntity.DeserializeRot(buff);
            time = CubeEntity.DeserializeTime(buff);
        }
        public void set(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
            time = -1;
        }
        */
        
        //
        //
        public List<GamePlayer> players = new List<GamePlayer>();
        
        
    }


public class GamePlayer
{
    public int id;
    public Vector3 position;
    public Quaternion rotation;
    public Quaternion cameraRotation;
    public bool m_jump;
    public float inputplayer;
    public int life;
    public bool walking;
    public bool shooting;
    public bool reloading;

    public GamePlayer(int id, float inputplayer, Vector3 pos, Quaternion rot, Quaternion camrot, bool jump, int life, bool walking, bool shooting, bool reloading)
    {
        this.id = id;
        this.inputplayer = inputplayer;
        this.position = pos;
        this.rotation = rot;
        this.cameraRotation = camrot;
        this.m_jump = jump;
        this.life = life;
        this.walking = walking;
        this.shooting = shooting;
        this.reloading = reloading;
    }

    public Quaternion getRotation()
    {
        return rotation;
    }
    
    public Vector3 getPosition()
    {
        return position;
    }
    
    public Quaternion getCameraRotation()
    {
        return cameraRotation;
    }
}