using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventPacket
{
    
        public float packetNumber = 0;
        public float age = 0;
        public ArrayList msg;

        public EventPacket(float packn, float age, ArrayList msg)
        {
            this.age = age;
            this.packetNumber = packn;
            this.msg = msg;
        }
    
}


