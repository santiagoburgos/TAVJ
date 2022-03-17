using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class FloorCollider : MonoBehaviour
{


    public GameObject ServerGM;

    private ServerGameManager SGMScr;
    // Start is called before the first frame update
    void Start()
    {
        SGMScr = ServerGM.GetComponent<ServerGameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "ServerPlayer")
        {
            Debug.Log("se cayo " + other.gameObject.GetComponent<CustomFirstPersonController>().id);
            
            SGMScr.playerDead(other.gameObject.GetComponent<CustomFirstPersonController>().id);
        }
    }
}
