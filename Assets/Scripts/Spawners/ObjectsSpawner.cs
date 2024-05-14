using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsSpawner : NetworkBehaviour
{
    public List<GameObject> objectsPrefabs;
    public float spawnInterval = 5f;
    public float terrainWidth = 50f;
    public float terrainDepth = 50f;
    public float minimumSeparation = 2f;
    public int maxBallsToSpawn = 10; // Máximo número de bolas a instanciar

    private float timer;
    private List<Vector3> spawnPositions = new List<Vector3>();
    private int ballsSpawned = 0; // Contador de bolas actualmente instanciadas

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority && ballsSpawned < maxBallsToSpawn)
        {
            timer += Runner.DeltaTime;
            if (timer >= spawnInterval)
            {
                timer = 0f;
                TrySpawnBall();
            }
        }
    }

    private void TrySpawnBall()
    {
        int attempts = 10;
        while (attempts > 0)
        {
            Vector3 spawnPosition = new Vector3(
                Random.Range(-terrainWidth / 2, terrainWidth / 2),
                1f,
                Random.Range(-terrainDepth / 2, terrainDepth / 2));

            if (IsValidSpawnPosition(spawnPosition))
            {
                spawnPositions.Add(spawnPosition);
                int prefabIndex = Random.Range(0, objectsPrefabs.Count);
                Runner.Spawn(objectsPrefabs[prefabIndex], spawnPosition, Quaternion.identity);
                ballsSpawned++;
                break;
            }
            attempts--;
        }
    }

    private bool IsValidSpawnPosition(Vector3 position)
    {
        foreach (Vector3 otherPosition in spawnPositions)
        {
            if (Vector3.Distance(position, otherPosition) < minimumSeparation)
            {
                return false; // Posición no válida si está demasiado cerca de otra bola
            }
        }
        return true; // Posición válida si no está cerca de otras bolas
    }
}
