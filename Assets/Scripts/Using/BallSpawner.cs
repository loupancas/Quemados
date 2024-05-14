using Fusion;
using UnityEngine;

public class BallSpawner : NetworkBehaviour
{
    public GameObject ballPrefab;
    public float spawnInterval = 5f;
    private float timer;

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            timer += Runner.DeltaTime;
            if (timer >= spawnInterval)
            {
                timer = 0f;
                SpawnBall();
            }
        }
    }

    private void SpawnBall()
    {
        Vector3 spawnPosition = new Vector3(Random.Range(-10f, 10f), 1f, Random.Range(-10f, 10f));
        Runner.Spawn(ballPrefab, spawnPosition, Quaternion.identity);
    }
}
