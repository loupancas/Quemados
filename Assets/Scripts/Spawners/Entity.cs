using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


public class Entity : NetworkBehaviour
{
    protected T Spawn<T>(T prefab, Vector3 position, NetworkRunner.OnBeforeSpawned onBeforeSpawned = null)
        where T : SimulationBehaviour =>
        Spawn(Runner, prefab, position, onBeforeSpawned);

    public static T Spawn<T>(NetworkRunner runner, T prefab, Vector3 position, NetworkRunner.OnBeforeSpawned onBeforeSpawned = null)
        where T : SimulationBehaviour
    {
        return runner.Spawn(prefab, position, onBeforeSpawned: OnBeforeSpawnedMethod);

        void OnBeforeSpawnedMethod(NetworkRunner runner, NetworkObject obj)
        {
            obj.transform.SetParent(GameManager.instance.EntityHolder);
            onBeforeSpawned?.Invoke(runner, obj);
        }
    }
}
