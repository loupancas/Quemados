using Fusion;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    [SerializeField] private GameObject _ballPrefab;
    [SerializeField] private Transform _firingPositionTransform;
    //[SerializeField] private ParticleSystem _shootingParticles;

    [Networked]
    NetworkBool _spawnedBall { get; set; }

    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
    }

    public override void Render()
    {
        if (_changeDetector == null)
        {
            Debug.Log("ChangeDetector is not initialized.");
            return;
        }
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(_spawnedBall):
                    RemoteParticles();
                    break;
            }
        }
    }

    public void Fire()
    {
        if (_ballPrefab == null || _firingPositionTransform == null)
        {
            Debug.Log("Ball prefab or firing position transform is not set.");
            return;
        }
        Runner.Spawn(_ballPrefab, _firingPositionTransform.position, transform.rotation);
        _spawnedBall = !_spawnedBall;
    }

    void RemoteParticles()
    {
        //_shootingParticles.Play();
    }
}