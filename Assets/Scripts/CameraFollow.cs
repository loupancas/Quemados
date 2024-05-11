using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform _targetTransform;

    public void SetTarget(Transform target)
    {
        _targetTransform = target;
    }
    
    void LateUpdate()
    {
        if (!_targetTransform) return;
        
        Vector3 newPosition = transform.position;
        newPosition.x = _targetTransform.position.x;

        transform.position = newPosition;
    }
}
