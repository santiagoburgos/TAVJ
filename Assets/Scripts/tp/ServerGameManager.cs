using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class ServerGameManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject playerPrefab;

    public GameObject serverGO;

    public GameObject p1Spawner;
    [NonSerialized]
    public GameObject p1;
    public int p1id = 1;
    public bool p1playing = false;
    [NonSerialized]
    public int p1port;
    [NonSerialized]
    public int p1life;
    
    public GameObject p2Spawner;
    [NonSerialized]
    public GameObject p2;
    public int p2id = 2;
    public bool p2playing = false;
    [NonSerialized]
    public int p2port;
    [NonSerialized]
    public int p2life;
    
    public GameObject p3Spawner;
    [NonSerialized]
    public GameObject p3;
    public int p3id = 3;
    public bool p3playing = false;
    [NonSerialized]
    public int p3port;
    [NonSerialized]
    public int p3life;
    
    public GameObject p4Spawner;
    [NonSerialized]
    public GameObject p4;
    public int p4id = 4;
    public bool p4playing = false;
    [NonSerialized]
    public int p4port;
    [NonSerialized]
    public int p4life;

    public SimulationServer SSScr = null;
   
    public int startingLife = 100;
    
    private int players = 0;
    
    
 
    void Start()
    {
        SSScr = serverGO.GetComponent<SimulationServer>();
    }

    // Update is called once per frame
 
    void FixedUpdate()
    {

        //recibo paquetes
        SSScr._FixedUpdate();
        
        //aplico inputs
        if(p1playing)
            p1.transform.Find("FPSController").transform.GetComponent<CustomFirstPersonController>()._FixedUpdate();
        if(p2playing)
            p2.transform.Find("FPSController").transform.GetComponent<CustomFirstPersonController>()._FixedUpdate();
        if(p3playing)
            p3.transform.Find("FPSController").transform.GetComponent<CustomFirstPersonController>()._FixedUpdate();
        if(p4playing)
            p4.transform.Find("FPSController").transform.GetComponent<CustomFirstPersonController>()._FixedUpdate();
        
        //envio snapshots
        SSScr._FixedUpdateSend();
        
        
        if(p1playing)
            p1.transform.Find("FPSController").transform.GetComponent<CustomFirstPersonController>().m_Jump = false;
        if(p2playing)
            p2.transform.Find("FPSController").transform.GetComponent<CustomFirstPersonController>().m_Jump = false;
        if(p3playing)
            p3.transform.Find("FPSController").transform.GetComponent<CustomFirstPersonController>().m_Jump = false;
        if(p4playing)
            p4.transform.Find("FPSController").transform.GetComponent<CustomFirstPersonController>().m_Jump = false;
        
        
        
    }


    public int NewPlayer()
    {
        if (!p1playing)
        {
            p1 = Instantiate(playerPrefab, p1Spawner.transform);
            p1.transform.Find("FPSController").GetComponent<CustomFirstPersonController>().id = p1id;
            players += 1;
            p1playing = true;
            p1life = startingLife;
            return p1id;
        }
        else if (!p2playing)
        {
            p2 = Instantiate(playerPrefab, p2Spawner.transform);
            p2.transform.Find("FPSController").GetComponent<CustomFirstPersonController>().id = p2id;
            players += 1;
            p2playing = true;
            p2life = startingLife;
            return p2id;
        }
        else if (!p3playing)
        {
            p3 = Instantiate(playerPrefab, p3Spawner.transform);
            p3.transform.Find("FPSController").GetComponent<CustomFirstPersonController>().id = p3id;
            players += 1;
            p3playing = true;
            p3life = startingLife;
            return p3id;
        }
        else if (!p4playing)
        {
            p4 = Instantiate(playerPrefab, p4Spawner.transform);
            p4.transform.Find("FPSController").GetComponent<CustomFirstPersonController>().id = p4id;
            players += 1;
            p4playing = true;
            p4life = startingLife;
            return p4id;
        }

        return -1;
    }

    public int getPlayersCount()
    {
        return this.players;
    }

    public int getPlayerPort(int id)
    {
        switch (id)
        {
            case 1:
                return p1port;
            case 2:
                return p2port;
            case 3:
                return p3port;
            case 4:
                return p4port;
            
        }

        return 0;
    }
    
    public List<int> getPlayersPorts()
    {
        List<int> ports = new List<int>();
        
        if(p1playing)
            ports.Add(p1port);
        if(p2playing)
            ports.Add(p2port);
        if(p3playing)
            ports.Add(p3port);
        if(p4playing)
            ports.Add(p4port);

        return ports;
    }

    public void playerLeft(int id)
    {
        if (id == p1id)
        {
            players -= 1;
            Destroy(p1);
            p1playing = false;
        } else if (id == p2id)
        {
            players -= 1;
            Destroy(p2);
            p2playing = false;
        } else if (id == p3id)
        {
            players -= 1;
            Destroy(p3);
            p3playing = false;
        } else if (id == p4id)
        {
            players -= 1;
            Destroy(p4);
            p4playing = false;
        }
    }
    
    public GameObject getPlayer(int id)
    {
        if (id == p1id)
        {
            return p1;
        } else if (id == p2id)
        {
            return p2;
        } else if (id == p3id)
        {
            return p3;
        } else if (id == p4id)
        {
            return p4;
        }

        return null;
    }
    
    
    public int getPlayerId(GameObject player)
    {
        if (player == p1)
        {
            return p1id;
        } else if (player == p2)
        {
            return p2id;
        } else if (player == p3)
        {
            return p3id;
        } else if (player == p4)
        {
            return p4id;
        }

        return -1;
    }
    
    public void takePlayerLife(int id,int amount)
    {
        if (id == p1id)
        {
            int aux = p1life;
            aux -= amount;
            if (aux <= 0)
            {
                p1life = 0;
                playerDead(p1id);
            }
            else
            {
                p1life = aux;
            }

        } else if (id == p2id)
        {
            int aux = p2life;
            aux -= amount;
            if (aux <= 0)
            {
                p2life = 0;
                playerDead(p2id);
            }
            else
            {
                p2life = aux;
            }
        } else if (id == p3id)
        {
            int aux = p3life;
            aux -= amount;
            if (aux <= 0)
            {
                p3life = 0;
                playerDead(p3id);
            }
            else
            {
                p3life = aux;
            }
        } else if (id == p4id)
        {
            int aux = p4life;
            aux -= amount;
            if (aux <= 0)
            {
                p4life = 0;
                playerDead(p4id);
            }
            else
            {
                p4life = aux;
            }
        }
        
    }





    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            playerDead(2);
        }
    }

    public void playerDead(int id)
    {
        StartCoroutine("Respawn", id);


    }
    
    
    IEnumerator Respawn(int id)
    {
        yield return new WaitForSeconds(3f);
        
        GameObject player = getPlayer(id);
        if (id == p1id)
        {
            p1life = startingLife;
            p1.transform.Find("FPSController").localPosition = new Vector3(0f,0f,0f);
        } else if (id == p2id)
        {
            p2life = startingLife;
            p2.transform.Find("FPSController").localPosition = new Vector3(0f,0f,0f);
        } else if (id == p3id)
        {
            p3life = startingLife;
            p3.transform.Find("FPSController").localPosition = new Vector3(0f,0f,0f);
        } else if (id == p4id)
        {
            p4life = startingLife;
            p4.transform.Find("FPSController").localPosition = new Vector3(0f,0f,0f);
        }
       
        

    }
    
    
}
