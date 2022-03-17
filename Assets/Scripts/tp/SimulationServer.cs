using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class SimulationServer : MonoBehaviour
{
    
    private Channel serverChannel;
    
    private float accum;
    
    public int pps = 60;
    private float sendTime;
    
    private float counter = 0;

    public GameObject serverGM;
    private ServerGameManager svGMScr;
    
    private GameObject player1;
    private GameObject player2;
    private GameObject player3;
    private GameObject player4;


    private bool lag = false;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("server initialized");
        serverChannel = new Channel(9200);
        
        
        svGMScr = serverGM.GetComponent<ServerGameManager>();
        svGMScr.SSScr = this;
    }
    
    private void OnDestroy() {
        serverChannel.Disconnect();
    }
    // Update is called once per frame
  
    void Update()
    {

        
           
        
            

    }

    public void _FixedUpdate()
    {
        //
        
        manageReceivedPackages(serverChannel);
            
        if (Input.GetKeyDown(KeyCode.L))
        {
            ArrayList msg = new ArrayList();
            msg.Add(9);
            broadcast(serverChannel,  msg);
        }
        
        agingReliablePackets();
        
        //
        //
        //
    }


    public void _FixedUpdateSend()
    {
        accum += Time.deltaTime;
        sendTime = 1f / pps;
        
        if (accum >= sendTime)
        {
            counter += 0.1f;
            foreach (int port in svGMScr.getPlayersPorts())
            {
                send(serverChannel, port);
            }
             
            
            accum = 0;
        }
    }

    

    void manageReceivedPackages(Channel channel)
    {
        Packet packet = null;

        packet = channel.GetPacket();
        while (packet != null) {
            int type = packet.buffer.GetInt();
      
            switch (type)
            {
                case 4:
                    getReliablePackets(packet);
                    break;
                case 5:
                    reliableACK(packet);
                    break;
                case 6:
                    getInputs(packet);
                    break;
            }
            packet.Free();
            packet = channel.GetPacket();
        }
      
        
    }
    
    
    
    // type, counter, id, transform id transform, ... , 0
    void send(Channel clientChannel, int port)
    {

     

        var packet = Packet.Obtain();
        //packet type 1
        packet.buffer.PutInt(1);
     
        packet.buffer.PutFloat(counter);
        if (svGMScr.p1playing)
        { 
           packet.buffer.PutInt(svGMScr.p1id);
           
           
           CustomFirstPersonController playerCFPSC = player1.transform.Find("FPSController").GetComponent<CustomFirstPersonController>();
           
           packet.buffer.PutFloat(playerCFPSC.inputPlayerNumber);
           
           PlayerEntity.Serialize(player1.transform.Find("FPSController"),player1.transform.Find("FPSController").transform.Find("FirstPersonCharacter"), packet.buffer);
           packet.buffer.PutBit(playerCFPSC.m_Jump);
           packet.buffer.PutInt(svGMScr.p1life);
           packet.buffer.PutBit(playerCFPSC.walking);
           packet.buffer.PutBit(playerCFPSC.shooting);
           packet.buffer.PutBit(playerCFPSC.reloading);
           
        }
        if (svGMScr.p2playing)
        {
            packet.buffer.PutInt(svGMScr.p2id);
       
            CustomFirstPersonController playerCFPSC = player2.transform.Find("FPSController").GetComponent<CustomFirstPersonController>();
            packet.buffer.PutFloat(playerCFPSC.inputPlayerNumber);

            PlayerEntity.Serialize(player2.transform.Find("FPSController"),player2.transform.Find("FPSController").transform.Find("FirstPersonCharacter"), packet.buffer);
            packet.buffer.PutBit(playerCFPSC.m_Jump);
            packet.buffer.PutInt(svGMScr.p2life);
            packet.buffer.PutBit(playerCFPSC.walking);
            packet.buffer.PutBit(playerCFPSC.shooting);
            packet.buffer.PutBit(playerCFPSC.reloading);
        }
        if (svGMScr.p3playing)
        {
            packet.buffer.PutInt(svGMScr.p3id);
            CustomFirstPersonController playerCFPSC = player3.transform.Find("FPSController").GetComponent<CustomFirstPersonController>();
            packet.buffer.PutFloat(playerCFPSC.inputPlayerNumber);
           
            PlayerEntity.Serialize(player3.transform.Find("FPSController"),player3.transform.Find("FPSController").transform.Find("FirstPersonCharacter"), packet.buffer);
            packet.buffer.PutBit(playerCFPSC.m_Jump);
            packet.buffer.PutInt(svGMScr.p3life);
            packet.buffer.PutBit(playerCFPSC.walking);
            packet.buffer.PutBit(playerCFPSC.shooting);
            packet.buffer.PutBit(playerCFPSC.reloading);
        }
        if (svGMScr.p4playing)
        {
            packet.buffer.PutInt(svGMScr.p4id);
            
            CustomFirstPersonController playerCFPSC = player4.transform.Find("FPSController").GetComponent<CustomFirstPersonController>();
            packet.buffer.PutFloat(playerCFPSC.inputPlayerNumber);
           
            PlayerEntity.Serialize(player4.transform.Find("FPSController"),player4.transform.Find("FPSController").transform.Find("FirstPersonCharacter"), packet.buffer);
            packet.buffer.PutBit(playerCFPSC.m_Jump);
            packet.buffer.PutInt(svGMScr.p4life);
            packet.buffer.PutBit(playerCFPSC.walking);
            packet.buffer.PutBit(playerCFPSC.shooting);
            packet.buffer.PutBit(playerCFPSC.reloading);
        }
        packet.buffer.PutInt(0);
        packet.buffer.Flush();
        string serverIP = "127.0.0.1";
        
       
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
    
    
    ArrayList eventPackets = new ArrayList();
    public float packetAge = 5.0f;
    private float rpackn = 0f;

    void broadcast(Channel channel, ArrayList msg)
    {
        foreach( int port in svGMScr.getPlayersPorts())
        {
            sendRealiablePacket(channel, port , msg);
        }
    }
  
    void sendRealiablePacket(Channel channel, int channelPort, ArrayList msg)
    {
         
        rpackn += 0.1f;  
        Packet packet = Packet.Obtain();
        BitBuffer buffer = packet.buffer;
        Debug.Log("sv send " + rpackn);

        //packet type 4
        buffer.PutInt(4);
        buffer.PutFloat(rpackn);
        buffer.PutInt(-1);
        foreach (int i in msg)
        {
            buffer.PutInt(i);
        }
        buffer.PutInt(0);
             
        EventPacket pack = new EventPacket(rpackn, packetAge, msg);
        eventPackets.Add(pack);
             
        packet.buffer.Flush();
        string serverIP = "127.0.0.1";
        int port = channelPort;
        var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);
        channel.Send(packet, remoteEp);
        packet.Free();
    }
    
    
    void reSendRealiablePacket(Channel channel, int channelPort, ArrayList msg, float packn)
    {
        Debug.Log("resending event package..");
        Packet packet = Packet.Obtain();
        BitBuffer buffer = packet.buffer;
        
        //packet type 4
        buffer.PutInt(4);
        buffer.PutFloat(packn);
        buffer.PutInt(-1);
        foreach (int i in msg)
        {
            buffer.PutInt(i);
        }
        buffer.PutInt(0);
        
             
        packet.buffer.Flush();
        string serverIP = "127.0.0.1";
        int port = channelPort;
        var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);
        channel.Send(packet, remoteEp);
        packet.Free();
    }

    void agingReliablePackets()
    {
        foreach (EventPacket ep in eventPackets)
        {
            ep.age -= 0.01f;

            if (ep.age <= 0)
            {
                ep.age = packetAge;
                reSendRealiablePacket(serverChannel, 9100, ep.msg, ep.packetNumber); //tocli
            }
            
        }
    }
    
    void reliableACK(Packet pack)
    {
        float ackNumber = pack.buffer.GetFloat();
        Debug.Log("sv ack " + ackNumber);   
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

    
    void getReliablePackets(Packet pack)
    {
       
        var buffer = pack.buffer;

        float rpackn = buffer.GetFloat();
       
        int id = buffer.GetInt();
        int msg = buffer.GetInt();

        if (id == -1)
        {
            Debug.Log("server received a new player " + msg);
            joinPlayer(msg);
            sendReliableACK(serverChannel, msg ,rpackn); //tocli
            return;
        }
        
        sendReliableACK(serverChannel, svGMScr.getPlayerPort(id) ,rpackn); //tocli
        
        Debug.Log("event from: " + id);
        //player left
        if (msg == 2)
        {
            int playerLeftId = buffer.GetInt();
            Debug.Log("PLAYER " + playerLeftId + " LEFT");
            svGMScr.playerLeft(playerLeftId);
        } else if (msg == 3)
        {
            int playerid = buffer.GetInt();
            ArrayList playersContext = new ArrayList();
            var packet = Packet.Obtain();
            
            //message type
            playersContext.Add(3);
            
            if (svGMScr.p1playing && svGMScr.p1id != playerid)
            {
                Debug.Log("PLAYER: " + playerid + " ADDED " + svGMScr.p1id);
                playersContext.Add(svGMScr.p1id);
            }
            if (svGMScr.p2playing && svGMScr.p2id != playerid)
            {
                Debug.Log("PLAYER: " + playerid + " ADDED " + svGMScr.p3id);
                playersContext.Add(svGMScr.p2id);
            }
            if (svGMScr.p3playing && svGMScr.p3id != playerid)
            {
                Debug.Log("PLAYER: " + playerid + " ADDED " + svGMScr.p3id);
                playersContext.Add(svGMScr.p3id);
            }
            if (svGMScr.p4playing && svGMScr.p4id != playerid)
            {
                Debug.Log("PLAYER: " + playerid + " ADDED " + svGMScr.p4id);
                playersContext.Add(svGMScr.p4id);
            }
            playersContext.Add(0);
            sendRealiablePacket(serverChannel,svGMScr.getPlayerPort(playerid),playersContext);
        }
        //shoot
        else if (msg == 7)
        {
            shootPlayer(buffer);
        }
        
    }
    
    void sendReliableACK(Channel channel, int channelPort,float n)
    {
        var packet = Packet.Obtain();
       
        //packet type 5
        packet.buffer.PutInt(5);
        packet.buffer.PutFloat(n);
        packet.buffer.Flush();
        
        string serverIP = "127.0.0.1";
        int port = channelPort;
        var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);
        channel.Send(packet, remoteEp);
        packet.Free();
    }
    

    void joinPlayer(int playerPort)
    {
        int id = -1;
        
        
        if (svGMScr.getPlayersCount() == 0)
        {
            svGMScr.p1port = playerPort;
            id = svGMScr.NewPlayer();
            player1 = svGMScr.p1;
        } else if (svGMScr.getPlayersCount() == 1)
        {
            svGMScr.p2port = playerPort;
            id = svGMScr.NewPlayer();
            player2 = svGMScr.p2;
        } else if (svGMScr.getPlayersCount() == 2)
        {
            svGMScr.p3port = playerPort;
            id = svGMScr.NewPlayer();
            player3 = svGMScr.p3;
        } else if (svGMScr.getPlayersCount() == 3)
        {
            svGMScr.p4port = playerPort;
            id = svGMScr.NewPlayer();
            player4 = svGMScr.p4;
        }
        
        ArrayList msg = new ArrayList();
        msg.Add(id);
        
        sendRealiablePacket(serverChannel, playerPort, msg);
        
        
        ArrayList playerjoined = new ArrayList();
        playerjoined.Add(1);
        playerjoined.Add(id);
        broadcast(serverChannel, playerjoined);
    }



    
    
    //
    private float inputNumberP1 = 0;
    private float inputNumberP2 = 0;
    private float inputNumberP3 = 0;
    private float inputNumberP4 = 0;
    public void getInputs(Packet packet)
    {

        BitBuffer buffer = packet.buffer;
        int id = buffer.GetInt(); 
        
        float inputnumber = buffer.GetFloat();
      
        
        sendACKInp(serverChannel, svGMScr.getPlayerPort(id), inputnumber);
        
        float lastInp = 0;
        if (id == 1)
        {
            lastInp = inputNumberP1;
        }
        else  if (id == 2)
        {
            lastInp = inputNumberP2;
        }
        else  if (id == 3)
        {
            lastInp = inputNumberP3;
        }
        else  if (id == 4)
        {
            lastInp = inputNumberP4;
        }

        float inputNumber = buffer.GetFloat();
        //consume already readed inputs
        while (lastInp >= inputNumber)
        {
            float inpp = buffer.GetFloat();
            float hor = buffer.GetFloat();
          float ver = buffer.GetFloat();
          float roty = buffer.GetFloat();
          float rotx = buffer.GetFloat();
          bool ju =  buffer.GetBit();
          bool wa =  buffer.GetBit();
          bool sho =  buffer.GetBit();
          bool rel =  buffer.GetBit();

          inputNumber = buffer.GetFloat();
        }

        float lastInputNumber = inputNumber;

        while (inputNumber != -1)
        {
            float inputPlayer = buffer.GetFloat();
            float horizontal = buffer.GetFloat();
            float vertical = buffer.GetFloat();
            float rotationy = buffer.GetFloat();
            float rotationx = buffer.GetFloat();
            bool jump = buffer.GetBit();
            bool walking = buffer.GetBit();
            bool shooting = buffer.GetBit();
            bool reloading = buffer.GetBit();
            
            GameObject player = svGMScr.getPlayer(id);
            CustomFirstPersonController playerCFPSC = player.transform.Find("FPSController").GetComponent<CustomFirstPersonController>();

            playerCFPSC.inputPlayerNumber = inputPlayer;
            
            Quaternion rotY = Quaternion.Euler(0f, rotationy, 0f);
            player.transform.Find("FPSController").transform.localRotation = rotY;

            Quaternion rotX = Quaternion.Euler(rotationx, 0f, 0f);
            player.transform.Find("FPSController").transform.Find("FirstPersonCharacter").localRotation = rotX;
            
        
            
            
            playerCFPSC.horizontal = horizontal;
            playerCFPSC.vertical = vertical;
            playerCFPSC.m_Jump = jump;
            playerCFPSC.walking = walking;
            playerCFPSC.shooting = shooting;
            playerCFPSC.reloading = reloading;
         
            lastInputNumber = inputNumber;
            inputNumber = buffer.GetFloat();
        }

        if (lastInputNumber != -1)
        {
            if (id == 1)
            {
                inputNumberP1 = lastInputNumber;
            }
            else  if (id == 2)
            {
                inputNumberP2 = lastInputNumber;
            }
            else  if (id == 3)
            {
                inputNumberP3 = lastInputNumber;
            }
            else  if (id == 4)
            {
                inputNumberP4 = lastInputNumber;
            }
        }
        

    }



    void sendACKInp(Channel channel, int channelPort, float n)
    {
       // Debug.Log("send ack inp: " + n);
        var packet = Packet.Obtain();
        
        //packet type 3
        packet.buffer.PutInt(7);
        packet.buffer.PutFloat(n);
        packet.buffer.Flush();
        
        string serverIP = "127.0.0.1";
        int port = channelPort;
        var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);
        channel.Send(packet, remoteEp);
        packet.Free();
    }


    public void shootPlayer(BitBuffer buff)
    {
        int id = buff.GetInt();
        int amount = buff.GetInt();

        svGMScr.takePlayerLife(id, amount);

    }
    
   
}
