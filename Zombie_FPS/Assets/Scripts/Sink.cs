using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sink : MonoBehaviour
{
    [SerializeField] private float delay = 10;
    private float destroyHeight;
    // Start is called before the first frame update
    void Start()
    {
        if (this.gameObject.tag == "Ragdoll")
        {
            Invoke("StartSink", 5.0f);
        }
    }

   public void StartSink()
    {
        destroyHeight = Terrain.activeTerrain.SampleHeight(this.transform.position) - 5;
        Collider[] colliders = this.transform.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            Destroy(collider);
        }
        
        InvokeRepeating("SinkIntoGround", delay, 0.1f);
    }
    void SinkIntoGround()
    {
        this.transform.Translate(0, -0.001f, 0 );
        if (this.transform.position.y < destroyHeight)
        {
            Destroy(this.gameObject);
        }
    }
}
