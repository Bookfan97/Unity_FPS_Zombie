using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Spawn : MonoBehaviour
{
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private int numberToSpawn = 1;
    [SerializeField] private float spawnRadius = 2;
    // Start is called before the first frame update
    void Start()
    {
        if (spawnOnStart)
        {
            SpawnAll();
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!spawnOnStart && other.gameObject.tag == "Player")
        {
            SpawnAll();
        }
    }

    void SpawnAll()
    {
        for (int i = 0; i < numberToSpawn; i++)
        { 
            Vector3 randomPoint = this.transform.position + (Vector3) (Random.insideUnitCircle * spawnRadius); 
            NavMeshHit hit; 
            if (NavMesh.SamplePosition(randomPoint, out hit, 10.0f, NavMesh.AllAreas)) 
            { 
                Instantiate(zombiePrefab, hit.position, Quaternion.identity);                
            }
            else
            { 
                i--;
            }
        }
    }
}
