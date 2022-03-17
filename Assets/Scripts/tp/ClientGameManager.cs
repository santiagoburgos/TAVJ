using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class ClientGameManager : MonoBehaviour
{

    public GameObject playerReplicaPrefab;
    public GameObject playerClientPrefab;

    public GameObject clientGO;
    
    public GameObject HealthBar;
    public Text lifeText;
    private Slider healthSlider;
    public Text ammoText;
    
    [NonSerialized]    
    private CustomCliFirstPersonController FPSScr = null;
    [NonSerialized]  
    public SimulationClient SCScr = null;
    
    [NonSerialized]  
    public GameObject p1;
    public int p1id ;
    public bool p1playing = false;
    public int p1life = 100;
    private RaycastShoot raycastShoot;
    private AnimatorEvents animatorEvents;
    
    
    [NonSerialized]  
    public GameObject p2;
    public int p2id ;
    public bool p2playing = false;
    private Animator p2anim = null;
    
    [NonSerialized]  
    public GameObject p3;
    public int p3id ;
    public bool p3playing = false;
    private Animator p3anim = null;
    
    [NonSerialized]  
    public GameObject p4;
    public int p4id ;
    public bool p4playing = false;
    private Animator p4anim = null;
    
   
    private float timer = 0;
    private int fups = 0;
    public Text updfix;
    
    public Text lagText;

    private Animator anim = null;
    
    
    void Awake () {
        QualitySettings.vSyncCount = 0; 
        Application.targetFrameRate = 60;
    }
    
    
    
    void Start()
    {
        SCScr = clientGO.GetComponent<SimulationClient>();
        healthSlider = HealthBar.GetComponent<Slider>();
    }

   
    void Update()
    {

        if (p1playing)
        {
            lifeText.text = p1life.ToString();
            ammoText.text = raycastShoot.bullets.ToString();
            healthSlider.value = p1life;

            if (SCScr.lag)
            {
                lagText.color = Color.red;
            }
            else
            {
                lagText.color = Color.green;
            }
            
        }

        
        
        timer += (Time.unscaledDeltaTime - timer) * 0.1f;
        float fps = 1.0f / timer ;
        updfix.text = fps.ToString();
            
            
               
            if (p1playing)
            {
                if (Input.GetKeyDown(KeyCode.R) && (raycastShoot.bullets != raycastShoot.maxBullets) )
                {
                    anim.SetTrigger("reloading");
                    FPSScr.reloading = true;
                }

                if (animatorEvents.reload)
                {
                    raycastShoot.bullets = 30;
                    FPSScr.reloading = false;
                    animatorEvents.reload = false;
                }
                
                

                if (raycastShoot.shooting && p1life > 0)
                {
                    FPSScr.shooting = true;
                
                    anim.SetBool("shooting", true);
                
                    if (raycastShoot.hitted)
                    {
                        int id = raycastShoot.id;
                        Debug.Log("PEW PEW TO " + raycastShoot.id);
                        shootPlayer(id, 10);
                        raycastShoot.hitted = false;
                    
                        getPlayer(id).transform.Find("Terraformer").GetComponent<Animator>().SetTrigger("receiveshoot");
                    }
                
                
                }
                else
                {
                    anim.SetBool("shooting", false);
                }

            }

        

        
        
    }

    void FixedUpdate()
    {
        if (p1playing)
        {
            //receive packets
            SCScr._FixedUpdate();
            
            if (conc)
            {
                conciliate(play);
                conc = false;
            }

            //apply inputs
            FPSScr._FixedUpdate();

            //send inputs
            SCScr._FixedUpdateSend();
            if (!raycastShoot.shooting)
           {
               FPSScr.shooting = false;
           }
        }
    }
    

   
    private bool conc = false;
    private GamePlayer play = null;
    public void applySnapshot(Snapshot snap)
    {
        foreach (GamePlayer player in snap.players)
        {
            GameObject pla = getPlayer(player.id);
            if (pla != null)
            {
                if (player.id == p1id)
                {
                    p1life = player.life;
               
                    conc = true;
                    play = player;
                }
                else
                {
                    //REPLICAS
                    pla.transform.position = player.position;
                    pla.transform.localRotation = player.rotation ;

                    Quaternion rota = Quaternion.Euler(player.cameraRotation.eulerAngles.x, 0f, 0f);
                    // pla.transform.Find("FirstPersonCharacter").transform.localRotation = player.cameraRotation; 
                  pla.transform.Find("FirstPersonCharacter").transform.localRotation = rota;
                  pla.transform.Find("Terraformer").GetComponent<Animator>().SetBool("walking", player.walking);
                  
                 
                  if (player.shooting)
                  {
                      pla.transform.Find("Terraformer").GetComponent<Animator>().SetTrigger("shoot");
                      pla.transform.Find("Replica").GetComponent<ReplicaId>().muzzleEffect.Play();
                      pla.transform.Find("FirstPersonCharacter").GetComponent<ReplicaFireSound>().shoot = true;
                  }
                  else
                  {
                      pla.transform.Find("FirstPersonCharacter").GetComponent<ReplicaFireSound>().shoot = false;
                  }
    
                  pla.transform.Find("Terraformer").GetComponent<Animator>().SetBool("reloading", player.reloading);
                  
                     
                  
                }
            }
        }
    }


    public void conciliate(GamePlayer player)
    {
        int index = 0;
          // Debug.Log("--START ITERATION-- " + player.id );
          //    Debug.Log("size i: " + inputPlayerList.Count );

        bool matched = false;
        foreach ( InputPlayer ip in inputPlayerList)
        {
              //     Debug.Log("CURRENT N:" + ip.inputplayern);


                if (ip.inputplayern < player.inputplayer)
                {
                    matched = true;
                    index = inputPlayerList.IndexOf(ip);
                }

                if (ip.inputplayern == player.inputplayer)
            {
                //
                //
                
                  //     Debug.Log("client me: " + "position " + p1.transform.position + "rotation " + p1.transform.rotation + FPSScr.m_jump + " N:" + FPSScr.inputPlayerNumber );
                
                   //     Debug.Log("server me: " + "position " + player.position + "rotation " + player.rotation + " jump:" + ip.jump + " N:" + player.inputplayer  );
                
                
                //
                matched = true;
                index = inputPlayerList.IndexOf(ip);
                //

                bool fix = needFix(p1.transform.position, player.position);

               // Debug.Log(fix);
                
                if (!fix)
                {
                    break;
                }
                    p1.transform.position = player.position;
                    p1.transform.localRotation = player.rotation;

                    Quaternion rota = Quaternion.Euler(player.cameraRotation.eulerAngles.x, 0f, 0f);
                    p1.transform.Find("FirstPersonCharacter").transform.localRotation = rota;
                
                
              //  Debug.Log("inputplayer matched: " + ip.inputplayern);
                
                 //  Debug.Log("client matched: " + "position " + ip.position + "rotation " + ip.rotation + " jump:" + ip.jump + " N:" + ip.inputplayern );
                
                 //      Debug.Log("client fixed: " + "position " + p1.transform.position + "rotation " + p1.transform.rotation );

            }
            if(ip.inputplayern > player.inputplayer && matched)
            {    
                
                   //    Debug.Log("SIMULATE: " + ip.inputplayern );
                
                ApplyInput(ip);
                
                    //   Debug.Log("client me final: " + "position " + p1.transform.position + "rotation " + p1.transform.rotation );
                
            }
        }
       //   Debug.Log("--END ITERATION--" );


          if (matched)
          {
              inputPlayerList.RemoveRange(0, index+1); 
          }

       //   Debug.Log("size e: " + inputPlayerList.Count );
        
    }
    

    public bool needFix(Vector3 pa, Vector3 pb)
    {
        if(Math.Abs(pa.x - pb.x) < 0.2f && Math.Abs(pa.y - pb.y) < 0.2f && Math.Abs(pa.z - pb.z) < 0.2f)
            return false;
        
        return true;
    }
  
    public ArrayList inputPlayerList = new ArrayList();


    private bool jumpcoming = false;
    public void ApplyInput(InputPlayer player)
    {
     float horizontal = player.horizontal;
     float vertical = player.vertical;
     float rotationy = player.rotationy;
     float rotationx = player.rotationx;
     bool jump = player.jump;
     
     
     p1.transform.localRotation = Quaternion.Euler(0f, rotationy, 0f);
     p1.transform.Find("FirstPersonCharacter").localRotation = Quaternion.Euler(rotationx, 0f, 0f);
     
     FPSScr.SimulatedUpdate(horizontal, vertical, jump);
    }



    public void shootPlayer(int id, int amount)
    {
        SCScr.shootPlayer(id,amount);
    }


  
    
    
      public void joinGame()
    {
      
        p1 = Instantiate(playerClientPrefab);
        FPSScr = p1.GetComponent<CustomCliFirstPersonController>();
        raycastShoot = p1.transform.Find("FirstPersonCharacter").transform.Find("Gun").transform
            .GetComponent<RaycastShoot>();
        anim = p1.transform.Find("FirstPersonCharacter").transform.Find("TerraformerFirstPerson").transform
            .GetComponent<Animator>();
        animatorEvents = p1.transform.Find("FirstPersonCharacter").transform.Find("TerraformerFirstPerson").transform
            .GetComponent<AnimatorEvents>();
        p1playing = true;
    }

    //
    public void playerJoined(int id)
    {
        if (p1id == id || p2id == id || p3id == id || p4id == id)
        {
            return;
        }

        if (!p2playing)
        {
            p2playing = true;
            p2 = Instantiate(playerReplicaPrefab);
            p2.transform.Find("Replica").GetComponent<ReplicaId>().id = id;
            p2id = id;
            p2anim =   p2.transform.Find("Terraformer").GetComponent<Animator>();
        } else if (!p3playing)
        {
            p3playing = true;
            p3 = Instantiate(playerReplicaPrefab);
            p3.transform.Find("Replica").GetComponent<ReplicaId>().id = id;
            p3id = id;
            p3anim =   p3.transform.Find("Terraformer").GetComponent<Animator>();
        } else if (!p4playing)
        {
            p4playing = true;
            p4 = Instantiate(playerReplicaPrefab);
            p4.transform.Find("Replica").GetComponent<ReplicaId>().id = id;
            p4id = id;
            p4anim =   p4.transform.Find("Terraformer").GetComponent<Animator>();
        }
    }

    public GameObject getPlayer(int id)
    {
        if (p1id == id)
        {
            return p1;
        } else if (p2id == id)
        {
            return p2;
        } else if (p3id == id)
        {
            return p3;
        }
        else if (p4id == id)
        {
            return p4;
        }

        return null;
    }
    
    
    
}


