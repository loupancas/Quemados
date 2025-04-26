using Fusion;
using UnityEngine;

public class BallSpawner : NetworkBehaviour
{
    public GameObject ballPrefab;
    public float spawnInterval = 5f;
    private float timer;
    public Vector3 spawnPosition;
    BallPickUp ballPickUp;

    public override void Spawned()
    {
        base.Spawned();
        
        //InitializeBallPickUp();
    }

    public override void FixedUpdateNetwork()
    {
        //if (ballPickUp.IsPickedUp)
        //{
        //    timer += Runner.DeltaTime;
        //    if (timer >= spawnInterval)
        //    {
        //        timer = 0f;
        //        SpawnBall();
        //    }
        //}

        if (ballPrefab == null) return;

        BallPickUp ballPickUp = ballPrefab.GetComponentInChildren<BallPickUp>();
        if (ballPickUp != null && ballPickUp.IsPickedUp)
        {
            timer += Runner.DeltaTime;
            if (timer >= spawnInterval)
            {
                timer = 0f;
                SpawnBall();
            }
        }
    }

    private void InitializeBallPickUp()
    {
        if (ballPrefab != null)
        {
            ballPickUp = ballPrefab.GetComponentInChildren<BallPickUp>();
        }
    }

    private void SpawnBall()
    {
        spawnPosition = new Vector3(0, 1.28f, 0);
        if (ballPrefab != null)
        {
           
            GameObject newBall = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
            Runner.Spawn(newBall, spawnPosition, Quaternion.identity);
        }
    }
}
