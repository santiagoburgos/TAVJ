using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.TestTools;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

public class SimulationClient : MonoBehaviour
{
    
    public Channel clientChannel;
    
  
    
    public GameObject clientGM;
    private ClientGameManager clGMScr;
    
    LinkedList<Snapshot> snapshots = new LinkedList<Snapshot>();
    
    public int pps = 60;
    public int requiredSnaps = 3;

    private float clientTimer = 0;

    [NonSerialized]
    public int clientID = -1;

    private int cliPort;
    public int servPort = 9200;

    private bool gameJoined = false;
    private bool join = false;
    [NonSerialized]
    public bool lag = false;

    private float accum;
    private float sendTime;
    
    void Start()
    {
        Debug.Log("client initialized");
        clGMScr = clientGM.GetComponent<ClientGameManager>();
        cliPort = UnityEngine.Random.Range(9000, 9199);
        clientChannel = new Channel(cliPort);
      
    }
    
    private void OnDestroy()
    {
        ArrayList msg = new ArrayList();
        msg.Add(2);
        msg.Add(clientID);
        clientChannel.Disconnect();
    }
    
    void Update()
    {
        if (!gameJoined)
        {
            if (!join)
            {
                join = true;
                joinGame(clientChannel,servPort,cliPort);
            }
            serverJoinConfirmed();
        }
        
        
        if (Input.GetKeyDown(KeyCode.L)) {
            if (!lag)
            {
                lag = true;
            }
            else
            {
                lag = false;
            }
               
        }
        
        
    }


    public void _FixedUpdate()
    {
        
        if (gameJoined)
        {
            manageReceivedPackages(clientChannel);
            agingReliablePackets();

            if (clientPlaying)
            {
                clientTimer += Time.deltaTime;
            }
        }
    }


    public void _FixedUpdateSend()
    {
        if (gameJoined)
        {
            accum += Time.deltaTime;
            sendTime = 1f / 60;
            if (accum >= sendTime)
            {
                sendInputs();
                accum = 0;
            }
        }
    }


  
    void manageReceivedPackages(Channel channel)
    {
        Packet packet = null;

        packet = channel.GetPacket();
        while (packet != null)
        {
            int type = packet.buffer.GetInt();

            switch (type)
            {
                case 1:
                    updateClient(packet);
                    break;
                case 4:
                    getReliablePackets(packet);
                    break;
                case 5:
                    reliableACK(packet);
                    break;
                case 7:
                    getACKInp(packet);
                    break;

            }
            packet.Free();
            packet = channel.GetPacket();
        }
    }
    
    
    
    
    
    private bool clientPlaying = false;
    Snapshot previous = new Snapshot();
    Snapshot next = new Snapshot();
    private float nextt = 0;
    private float prevt = 0;
      private void updateClient(Packet pack) {
        
            var buffer = pack.buffer;
            Snapshot snap = new Snapshot();  
            //
            snap.time =buffer.GetFloat();

            int id = buffer.GetInt();
            while (id != 0)
            {
                float inputplayer = buffer.GetFloat();
                Vector3 position = PlayerEntity.DeserializePos(buffer);
                Quaternion rotation = PlayerEntity.DeserializeRot(buffer);
                Quaternion cameraRotation = PlayerEntity.DeserializeRot(buffer);
                bool jump = buffer.GetBit();
                int life = buffer.GetInt();
                bool walking = buffer.GetBit();
                bool shooting = buffer.GetBit();
                bool reloading = buffer.GetBit();

                GamePlayer player = new GamePlayer(id, inputplayer, position,rotation,cameraRotation, jump, life, walking, shooting, reloading);
                
                snap.players.Add(player);
                id = buffer.GetInt();
            }
            snapshots.AddLast(snap);

            if (snapshots.Count >= requiredSnaps && !clientPlaying)
        {
            clientPlaying = true;
         previous = snapshots.First.Value;
         next = snapshots.First.Next.Value;
         
         prevt =  previous.time * (1f / pps);
         nextt =  next.time * (1f / pps);

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
             
                    prevt =  previous.time * (1f / pps);
                    nextt =  next.time * (1f / pps);
             
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
            clGMScr.applySnapshot(snapInterpolated);
        }
      }
      
      
      
      
      Snapshot interpolate(Snapshot previous, Snapshot next, float t)
      {
          Snapshot snap = new Snapshot();

          foreach (GamePlayer playerprev in previous.players)
          {
              foreach (GamePlayer playernext in next.players)
              {
                  if (playerprev.id == playernext.id)
                  {
                      int id = playerprev.id;
                      Vector3 position;
                      Quaternion rotation;
                      Quaternion cameraRotation;

                      if (id != clGMScr.p1id)
                      {
                          position = Vector3.Lerp(playerprev.getPosition(), playernext.getPosition(), t);
                          rotation = Quaternion.Lerp(playerprev.getRotation(), playernext.getRotation(), t);
                          cameraRotation = Quaternion.Lerp(playerprev.getCameraRotation(), playernext.getCameraRotation(), t);
                      }
                      else
                      {
                          position = playernext.getPosition();
                          rotation = playernext.getRotation();
                          cameraRotation = playernext.getCameraRotation();
                      }
                     

                      GamePlayer player = new GamePlayer(id,playernext.inputplayer, position,rotation, cameraRotation, playernext.m_jump, playernext.life, playernext.walking, playernext.shooting, playernext.reloading);
                      snap.players.Add(player);
                  }
              }
          }
          return snap;
      }

    
    
    ArrayList eventPackets = new ArrayList();
    public float packetAge = 5.0f;
    private float rpackn = 0f;
  
    //packn, type, intmsg
    public void sendRealiablePacket(Channel channel, int channelPort, ArrayList msg)
    {
        rpackn += 0.1f;  
        Packet packet = Packet.Obtain();
        BitBuffer buffer = packet.buffer;
        Debug.Log("cli send pck " + rpackn);
        //packet type 4
        buffer.PutInt(4);
        buffer.PutFloat(rpackn);
        buffer.PutInt(clientID);

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
        buffer.PutInt(clientID);
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
                reSendRealiablePacket(clientChannel, servPort, ep.msg, ep.packetNumber); //tosv
            }
            
        }
    }
    
    void reliableACK(Packet pack)
    {
       
        float ackNumber = pack.buffer.GetFloat();
        Debug.Log("cli ack " + ackNumber);
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

    //packn, id, msg
    void getReliablePackets(Packet pack)
    {
       
        var buffer = pack.buffer;

        float rpackn = buffer.GetFloat();
        sendReliableACK(clientChannel, servPort ,rpackn); //tosv
        int id = buffer.GetInt();
       
        int msg = buffer.GetInt();
        
        //playerjoined
        if (msg == 1)
        {
            int playerJoinedId = buffer.GetInt();
            if (playerJoinedId != clientID)
            {
                Debug.Log("player "+ clientID +" said: PLAYER "+ playerJoinedId + " JOINED" );
                clGMScr.playerJoined(playerJoinedId);
            }
        }
        else if(msg == 3)
        {
            int playid = buffer.GetInt();
            while (playid != 0)
            {
                clGMScr.playerJoined(playid);
                playid = buffer.GetInt();
            }
        }
        

       
    }
    
    void sendReliableACK(Channel channel, int channelPort,float n)
    {
        Debug.Log("cli send ack " + n);
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

    void joinGame(Channel channel, int channelPort, int msg)
    {
        clGMScr.joinGame();

        rpackn += 0.1f;  
        Packet packet = Packet.Obtain();
        BitBuffer buffer = packet.buffer;
        Debug.Log("cli send pck " + rpackn);
        //packet type 4
        buffer.PutInt(4);
        buffer.PutFloat(rpackn);
        buffer.PutInt(-1);
        buffer.PutInt(cliPort);
        
        ArrayList msglist = new ArrayList();
        msglist.Add(msg);
        
        EventPacket pack = new EventPacket(rpackn, packetAge, msglist);
        eventPackets.Add(pack);
             
        packet.buffer.Flush();
        string serverIP = "127.0.0.1";
        int port = channelPort;
        var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);
        channel.Send(packet, remoteEp);
        packet.Free();

    }

    void serverJoinConfirmed()
    {
        Packet packet = null;

        packet = clientChannel.GetPacket();
        
        if (packet == null) {
            return;
        }

        int type = packet.buffer.GetInt();

            if (type == 4)
            {

                var buffer = packet.buffer;

                float rpackn = buffer.GetFloat();
                sendReliableACK(clientChannel, servPort, rpackn); //tosv
                int id = buffer.GetInt();
                int msg = buffer.GetInt();

                clientID = msg;
                clGMScr.p1id = msg;
                Debug.Log("client: " + clientID + " joined");
                gameJoined = true;

                //ask for context
                ArrayList mes = new ArrayList();
                mes.Add(3);
                mes.Add(clGMScr.p1id);
                sendRealiablePacket(clientChannel, servPort, mes);
                
            }
            else if (type == 5)
            {
                reliableACK(packet);
            }

        packet.Free();
    }


  
    
    private float inputnumber=0;
    public void sendInputs()
    {
        inputnumber += 0.01f;
        
        CustomCliFirstPersonController ccfpc = clGMScr.p1.GetComponent<CustomCliFirstPersonController>();

        float inputPlayer = ccfpc.inputPlayerNumber;
        float horizontal = ccfpc.horizontal;
        float vertical = ccfpc.vertical;
        float rotationy = clGMScr.p1.transform.eulerAngles.y;
        float rotationx = clGMScr.p1.transform.Find("FirstPersonCharacter").transform.eulerAngles.x;

        bool jump = clGMScr.p1.GetComponent<CustomCliFirstPersonController>().m_jump;

        bool walking = ccfpc.walking;
        bool shooting = ccfpc.shooting;
        bool reloading = ccfpc.reloading;
        
        InputPlayer inpplayer = new InputPlayer(clGMScr.p1.transform.position, clGMScr.p1.transform.rotation,
            clGMScr.p1.transform.Find("FirstPersonCharacter").transform.rotation, inputPlayer , horizontal,
            vertical, rotationy, rotationx, jump);
        
        clGMScr.inputPlayerList.Add(inpplayer);
        //
        
        var packet = Packet.Obtain();
        BitBuffer buffer = packet.buffer;
        
        InputsPacket inpp = new InputsPacket(clientID, inputnumber, inputPlayer, horizontal, vertical, rotationy, rotationx, jump, walking, shooting, reloading);
        inputsList2.Add(inpp);
        
        buffer.PutInt(6);
        buffer.PutInt(clientID);
        
        //
        buffer.PutFloat(inputnumber);
        //
       // buffer.PutFloat(clGMScr.inputplayernumber);

        foreach (InputsPacket i in inputsList2)
        {
            buffer.PutFloat(i.inputNumber);
            buffer.PutFloat(i.inputPlayer);
            buffer.PutFloat(i.horizontal);    
            buffer.PutFloat(i.vertical);
            buffer.PutFloat(i.rotationY);
            buffer.PutFloat(i.rotationX);
            buffer.PutBit(i.jump);
            buffer.PutBit(i.walking);
            buffer.PutBit(i.shooting);
            buffer.PutBit(i.reloading);
          
            
        }
        buffer.PutFloat(-1);
       
         packet.buffer.Flush();
         
         string serverIP = "127.0.0.1";
         int port = servPort;
         var remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), port);

         if (lag)
         {
             StartCoroutine(sendInputsLag(packet, remoteEp, 0.1f));
         }
         else
         {
          
             clientChannel.Send(packet, remoteEp);
             packet.Free();
         }
         

         
    }
    
    
    
    private IEnumerator sendInputsLag(Packet packet,IPEndPoint remoteEp, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        
        clientChannel.Send(packet, remoteEp);
        packet.Free();

    }
    
    

    ArrayList inputsList2 = new ArrayList();


    public void getACKInp(Packet pack)
    {
      
        float ackNumber = pack.buffer.GetFloat();

        int last = 0;

        foreach (InputsPacket inpp in inputsList2)
        {
            if (inpp.inputNumber == ackNumber)
            {
                last =  inputsList2.IndexOf(inpp);
            }
        }
        
        //a
        if(inputsList2.Count >=1)
        inputsList2.RemoveRange(0, last+1);

        //Debug.Log("inputs list size: " + inputsList2.Count);

    }


    public void shootPlayer(int id, int amount)
    {
        ArrayList msg = new ArrayList();
        //type
        msg.Add(7);
        msg.Add(id);
        msg.Add(amount);
        sendRealiablePacket(clientChannel,servPort,msg);
    }
    

}







 