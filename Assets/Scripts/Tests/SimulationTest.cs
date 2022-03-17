using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.AI;

public class SimulationTest : MonoBehaviour
{
    /*
    private Channel clientChannel;
    private Channel serverChannel;

    private Channel channel4;
    private Channel channel5;
    

    [SerializeField] private Rigidbody cubeRigidBody;
    [SerializeField] private Transform clientCubeTransform;
    
    LinkedList<Snapshot> snapshots = new LinkedList<Snapshot>();

   
    private float accum;

    public int fps = 10;
    private float sendTime;

    private float counter = 0;
    private float clientTimer = 0;

    public int requiredSnaps = 3;

    private int clientID = 1;

    // Start is called before the first frame update
    void Start() {
        
        //UNIR CANALES DESPUES
        channel4 = new Channel(9020); //
        channel5 = new Channel(9021); //
        
        clientChannel = new Channel(9100); //
        serverChannel = new Channel(9200); //
        
    }

    private void OnDestroy() {
        clientChannel.Disconnect();
        serverChannel.Disconnect();
    }

    // Update is called once per frame
    void Update()
    {
        //
        //
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("client sended event to server");
            sendRealiablePacket(0);
          //  mSendRealiablePacket(serverChannel, 9200, 0);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
          //  Debug.Log("server sended event to client");
          //  mSendRealiablePacket(clientChannel, 9100, 0);
        }
        
        checkreliableACK();
        agingReliablePackets();
        getReliablePackets();
        //
        //
        
        //client
        if (clientPlaying)
        {
            clientTimer += Time.deltaTime;
        }
        updateInputList(); 
        
        //server

        //server time
        accum += Time.deltaTime;
        sendTime = 1f / fps;
        
        if (accum >= sendTime)
        {
            mSend(clientChannel, 9100);
            accum = 0;
        }
        
        
        //
        //
        //client 2.0
        manageReceivedPackages(clientChannel);
        //sv 2.0
        manageReceivedPackages(serverChannel);
        
    }
   
    
    //types 
    //1 recibo snapshot
    //2 recibo input
    //3 recibo input ack
    //4 recibo evento
    //5 recibo evento ack
    
    void manageReceivedPackages(Channel channel)
    {
        Packet packet = null;
        Packet pack;
        while ((pack = channel.GetPacket()) != null)
        {
            packet = pack;
        }
        if (packet == null) {
            return;
        }

        int type = packet.buffer.GetInt();
       
        switch (type)
        {
            case 1:
                mUpdateClient(packet); //client
                break;
            case 2:
                mGetInputList(packet); //server
                break;
            case 3:
                mCheckACKInp(packet); //client
                break;
            case 4:
                break;
            case 5:
                break;
        }
        


    }
    
    
    
    
    //client
    private bool clientPlaying = false;
    Snapshot previous = new Snapshot();
    Snapshot next = new Snapshot();
    private float nextt = 0;
    private float prevt = 0;
     private void mUpdateClient(Packet pack) {
        
            var buffer = pack.buffer;
            Snapshot snap = new Snapshot();  
            snap.set(buffer);
            snapshots.AddLast(snap);
        
      

        if (snapshots.Count >= requiredSnaps && !clientPlaying)
        {
            clientPlaying = true;
         previous = snapshots.First.Value;
         next = snapshots.First.Next.Value;
         
         prevt =  previous.time * (1f / fps);
         nextt =  next.time * (1f / fps);

         float aux = nextt - prevt;
         prevt = clientTimer - aux;
         nextt = clientTimer + aux;
         
         
        
        }
        if(snapshots.Count <= 1)
        {
            clientPlaying = false;
        }
        if (clientPlaying)
        {
            
            
            if (snapshots.Count > requiredSnaps)
            {
                snapshots.RemoveFirst();
            }
            
            if (clientTimer > nextt )
            {
                if (snapshots.Count > 2)
                {
                    snapshots.RemoveFirst();
                    previous = next;
                    next = snapshots.First.Next.Value;
             
                    prevt =  previous.time * (1f / fps);
                    nextt =  next.time * (1f / fps);
             
                    float aux = nextt - prevt;
                    prevt = clientTimer - aux;
                    nextt = clientTimer + aux;
                }
                else
                {    
                    snapshots.RemoveFirst();
                    previous = next;
                    clientPlaying = false;
                }
               
            }
            //
            float t = (clientTimer - prevt) / (nextt - prevt);
            
            Snapshot snapInterpolated = interpolate(previous, next, t);
            clientCubeTransform.transform.position = snapInterpolated.position;
            clientCubeTransform.transform.rotation = snapInterpolated.rotation;
        }
        
     
   
 
    }
    
     
    Snapshot interpolate(Snapshot previous, Snapshot next, float t)
    {
        Snapshot snap = new Snapshot();
        Vector3 pos;
        Quaternion rot;
        pos = Vector3.Lerp(previous.getPosition(), next.getPosition(), t);
        rot = Quaternion.Lerp(previous.getRotation(), next.getRotation(), t);
        snap.set(pos, rot);
        
        return snap;
    }

    
    //armo paquete de inputs acumulados {type, id, packnumber, cantidadinputs, inputs, packnumber,.. , .., 0} 
    void mSendInputs(Channel channel, int channelPort)
    {
        var packet = Packet.Obtain();
        BitBuffer buffer = packet.buffer;
        
        //packet type 2
        buffer.PutInt(2);
        buffer.PutInt(clientID);
        
        foreach (InputPacket inp in inputPackets)
        {
            buffer.PutFloat(inp.packetNumber);
            buffer.PutInt(inp.inputs.Count);
            foreach (int i in inp.inputs)
            {
                buffer.PutInt(i);
            }
        }
        buffer.PutInt(0);
        packet.buffer.Flush();
        string serverIP = "127.0.0.1";
        int port = channelPort;
        var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);
        channel.Send(packet, remoteEp);
        packet.Free();
    }
    
    //
    private float packN = 0;
    ArrayList inputPackets = new ArrayList();
    
    void updateInputList()
    {
        packN += 0.1f;
        ArrayList keys = new ArrayList();
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            keys.Add(1);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            keys.Add(2);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            keys.Add(3);
        }

        if (keys.Count > 0)
        {
            InputPacket inp = new InputPacket(packN, keys);
            inputPackets.Add(inp);
            mSendInputs(serverChannel, 9200);
        }
    }
    
    
    void mCheckACKInp(Packet pack)
    {
        float ackNumber = pack.buffer.GetFloat();
        deleteInputPackets(ackNumber);
    }
  
    

    void deleteInputPackets(float until)
    {
        int lastpos = 0; 
            
        foreach (InputPacket inp in inputPackets)
        {
            if (inp.packetNumber == until)
            {
                lastpos = inputPackets.IndexOf(inp);
                Debug.Log("lastpos: " + lastpos);
                Debug.Log("size: " + inputPackets.Count);
            }
        }
        
      
        inputPackets.RemoveRange(0, lastpos);
    }


//
//
//
//    
    ArrayList eventPackets = new ArrayList();
    public float packetAge = 5.0f;
    private float rpackn = 0f;
  
    void mSendRealiablePacket(Channel channel, int channelPort, int msg)
    {
        Debug.Log("sending ");
             
        rpackn += 0.1f;  
        Packet packet = Packet.Obtain();
        BitBuffer buffer = packet.buffer;
        
        //packet type 4
        buffer.PutInt(4);
        buffer.PutFloat(rpackn);
        buffer.PutInt(msg);
             
        EventPacket pack = new EventPacket(rpackn, packetAge, packet);
        eventPackets.Add(pack);
             
        packet.buffer.Flush();
        string serverIP = "127.0.0.1";
        int port = channelPort;
        var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);
        channel.Send(packet, remoteEp);
        packet.Free();
    }
    
    void mReSendRealiablePacket(Channel channel, int channelPort, Packet pack)
    {
        Debug.Log("resending package..");
        string serverIP = "127.0.0.1";
        int port = channelPort;
        var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);
        channel.Send(pack, remoteEp);
        pack.Free();
    }
    
    
    void sendRealiablePacket(int msg)
         {
             Debug.Log("sending ");
             
             rpackn += 0.1f;  
             Packet packet = Packet.Obtain();
             BitBuffer buffer = packet.buffer;
             buffer.PutFloat(rpackn);
             buffer.PutInt(msg);
             
             EventPacket pack = new EventPacket(rpackn, packetAge, packet);
             eventPackets.Add(pack);
             
             packet.buffer.Flush();
             string serverIP = "127.0.0.1";
             int port = 9020;
             var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);
             channel4.Send(packet, remoteEp);
             packet.Free();
         }
    
    
    void reSendRealiablePacket(Packet pack)
    {
        Debug.Log("resending ");
        string serverIP = "127.0.0.1";
        int port = 9020;
        var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);
        channel4.Send(pack, remoteEp);
        pack.Free();
    }


    void agingReliablePackets()
    {
        foreach (EventPacket ep in eventPackets)
        {
            ep.age -= 0.01f;

            if (ep.age <= 0)
            {
                ep.age = packetAge;
                reSendRealiablePacket(ep.pack);
            }
            
        }
    }
    
    void checkreliableACK()
    {
        Packet packet;
        float ackNumber = 0;
        while ( (packet = channel5.GetPacket()) != null)
        {
            ackNumber = packet.buffer.GetFloat();
        }
        deleteEventPacket(ackNumber);
    }
    
    void deleteEventPacket(float ackNumber)
    {
        foreach (EventPacket ep in eventPackets)
        {
            if (ep.packetNumber == ackNumber)
            {
                eventPackets.Remove(ep);
                return;
            }
        }
    }

    
    
    void getReliablePackets()
    {
        Packet packet = null;
        Packet pack;
        while ((pack = channel4.GetPacket()) != null)
        {
            packet = pack;
        }
        if (packet == null) {
            return;
        }
        var buffer = packet.buffer;

        float rpackn = buffer.GetFloat();
        sendReliableACK(rpackn);
        float msg = buffer.GetFloat();
    }
    
    void sendReliableACK(float n)
    {
        Debug.Log("send ack: " + n);
        var packet = Packet.Obtain();
        
        packet.buffer.PutFloat(n);
        packet.buffer.Flush();
        
        string serverIP = "127.0.0.1";
        int port = 9021;
        var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);
        channel5.Send(packet, remoteEp);
        packet.Free();
    }
    
    //
    //
    //
    //server    
    
    //send snapshot
    void mSend(Channel clientChannel, int channelPort)
    {

        counter += 0.1f;
        //serialize
        var packet = Packet.Obtain();
        //packet type 1
        packet.buffer.PutInt(1);
        CubeEntity.Serialize(cubeRigidBody, packet.buffer, counter);
        packet.buffer.Flush();

        string serverIP = "127.0.0.1";
        int port = channelPort;
        var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);

        //emulate error sending packet
        if (!Input.GetKey(KeyCode.G))
        {
            clientChannel.Send(packet, remoteEp);
        }
        else
        {
            Debug.Log("sv error sending snap to client");
        }
        packet.Free();
    }

       
    private float lastInpN = 0f;
    void mGetInputList(Packet pack)
    {
        var buffer = pack.buffer;

        int clientID = buffer.GetInt();
        float inpN = buffer.GetFloat();

        //consumo inputs ya leidos
        while (lastInpN >= inpN)
        {
            int size = buffer.GetInt();
            while (size != 0)
            {
                int consume = buffer.GetInt();
                size--;
            }
            inpN = buffer.GetFloat();
        }

        lastInpN = inpN;

        while (inpN != 0)
        {
            int size = buffer.GetInt();
            while (size != 0)
            {
                pressKey(buffer.GetInt());
                size--;
            }
            inpN = buffer.GetFloat();
            if (inpN != 0)
                lastInpN = inpN;
        }
        //sendACK2(lastInpN);
        mSendACKInp(clientChannel, 9100, lastInpN );
    }
    
    
    
    
    
    //
    //
    
    void pressKey(int key)
    {
        //Debug.Log("KEY " + key);
        
        if (key == 1) {  //space
            cubeRigidBody.AddForceAtPosition(Vector3.up * 5, Vector3.zero, ForceMode.Impulse);
        }
        else if (key == 2)
        {
            cubeRigidBody.AddForceAtPosition(Vector3.left * 5, Vector3.zero, ForceMode.Impulse);
        }
        else if (key == 3)
        {
            cubeRigidBody.AddForceAtPosition(Vector3.left * -5, Vector3.zero, ForceMode.Impulse);
        }
    }
    
 

 
    void mSendACKInp(Channel channel, int channelPort, float n)
    {
        Debug.Log("send ack inp: " + n);
        var packet = Packet.Obtain();
        
        //packet type 3
        packet.buffer.PutInt(3);
        packet.buffer.PutFloat(n);
        packet.buffer.Flush();
        
        string serverIP = "127.0.0.1";
        int port = channelPort;
        var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);
        channel.Send(packet, remoteEp);
        packet.Free();
    }

*/
}


/*
public class Snapshot
{
    public Vector3 position;
    public Quaternion rotation;
    public float time;
    
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
}

public class InputPacket
{
    public float packetNumber = 0;
    public ArrayList inputs = new ArrayList();

    public InputPacket(float packn, ArrayList inputs)
    {
        this.packetNumber = packn;
        this.inputs = inputs;
    }
    
}

public class EventPacket
{
    public float packetNumber = 0;
    public float age = 0;
    public Packet pack;

    public EventPacket(float packn, float age, Packet pack)
    {
        this.age = age;
        this.packetNumber = packn;
        this.pack = pack;
    }
    
}*/

