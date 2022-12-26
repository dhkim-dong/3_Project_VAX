using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
public class PlayeritemSpawn : MonoBehaviour
{
    public GameObject[] items;
    public float maxdistance = 5f;

    public float timeBetSpawnMAX = 7f;
    public float timeBetSpawnMIN = 2f;

    float timeBetSpawn;
    float lastSpawnTime;
   
    private void Start()
    {

        timeBetSpawn = Random.Range(timeBetSpawnMIN, timeBetSpawnMAX);
        lastSpawnTime = 0;
    }
    private void Update()
    {
        if(!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        if(Time.time>= lastSpawnTime+ timeBetSpawn)
        {
            lastSpawnTime = Time.time;
            timeBetSpawn = Random.Range(timeBetSpawnMIN, timeBetSpawnMAX);
            Spawn();
          

        }
    }
  void Spawn()
    {
        string path = "Item/";

        Vector3 spawnPosition = GetRandomPointOnNavmesh(transform.position, maxdistance);
        spawnPosition += Vector3.up * 0.5f;

        GameObject selectiTem = items[Random.Range(0, items.Length)];
        path+=selectiTem.name;
        GameObject item = PhotonNetwork.Instantiate(path, spawnPosition, Quaternion.identity);

    }
    private Vector3 GetRandomPointOnNavmesh(Vector3 center, float distance)
    {
        Vector3 randomPos = Random.insideUnitSphere * distance + center;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomPos, out hit, distance, NavMesh.AllAreas);

        return hit.position;
    }

}
