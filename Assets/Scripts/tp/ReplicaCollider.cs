using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplicaCollider : MonoBehaviour
{

    public GameObject replicaGO;
    
    [NonSerialized]
    public int id = 0;
    // Start is called before the first frame update
    void Start()
    {
        id = replicaGO.GetComponent<ReplicaId>().id;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
