using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Fusion;

public class ObjectsSpawner : NetworkBehaviour
{

    
    public GameObject terrain;
    List<Vector3> existingspawnPoints = new List<Vector3>();
    public Transform spawnerParent;
    public Transform spawnerContainer;
    public static ObjectsSpawner instance;
    

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

    }

    public Vector3 GetRandomSpawnPoint(float minDistance, Vector3 playerPosition)
    {
       
        Vector3 randomSpawnPoint;
        List<Vector3> existingSpawnPoints = existingspawnPoints;
        do
        {
            float randomX = Random.Range(-spawnerContainer.localScale.x / 2, spawnerContainer.localScale.x / 2);
            float randomY = 1f;
            float randomZ = Random.Range(-spawnerContainer.localScale.z / 2, spawnerContainer.localScale.z / 2);

            randomSpawnPoint = new Vector3(randomX, randomY, randomZ);


        } while (existingSpawnPoints.Any(spawnPoint => Vector3.Distance(spawnPoint, randomSpawnPoint) < minDistance) || Vector3.Distance(randomSpawnPoint, playerPosition) < minDistance); ;

        existingSpawnPoints.Add(randomSpawnPoint);
        return randomSpawnPoint;
    }


    

}
