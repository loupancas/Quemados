using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Fusion;

public class ObjectsSpawner : NetworkBehaviour
{
    [SerializeField] public GameObject powerUpPrefab;
    [SerializeField] public GameObject ballPrefab;
    [SerializeField] public int numberOfPowerUps;

    private Vector3 spawnAreaCenter;
    private Vector3 spawnAreaSize;
    public GameObject terrain;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            spawnBall();
            SpawnPowerUps();
        }
    }

    void Start()
    {
        //Instantiate(ballPrefab, Vector3.up, Quaternion.identity);
        //SpawnPowerUps();
    }

    void SpawnPowerUps()
    {
        List<Vector3> spawnPoints = Enumerable.Range(0, numberOfPowerUps)
            .Select(num => GetRandomSpawnPoint())
            .ToList();

        spawnPoints.ForEach(spawnPoint =>
        {
            Instantiate(powerUpPrefab, spawnPoint, Quaternion.identity);
        });
    }

    void spawnBall()
    {
        Instantiate(ballPrefab, Vector3.up, Quaternion.identity);
    }

    Vector3 GetRandomSpawnPoint()
    {
       
        spawnAreaCenter = new Vector3(terrain.transform.position.x, terrain.transform.position.y+1, terrain.transform.position.z);
        spawnAreaSize = new Vector3(terrain.transform.localScale.x, 1, terrain.transform.localScale.z);

        float randomX = Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2, spawnAreaCenter.x + spawnAreaSize.x / 2);
        float randomY = Random.Range(spawnAreaCenter.y - spawnAreaSize.y / 2, spawnAreaCenter.y + spawnAreaSize.y / 2);
        float randomZ = Random.Range(spawnAreaCenter.z - spawnAreaSize.z / 2, spawnAreaCenter.z + spawnAreaSize.z / 2);

        return new Vector3(randomX, randomY, randomZ);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
    }

    public void PlayerJoined(PlayerRef player)
    {
        throw new System.NotImplementedException();
    }
}
