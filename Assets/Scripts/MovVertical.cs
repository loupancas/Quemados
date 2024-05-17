using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovVertical : MonoBehaviour
{
    

    public float speed = 10f;
    public float max = 10f;
    public float min = -10f;
    public Vector3 axis = Vector3.up;
    void Update()
    {
        transform.Translate(axis * speed * Time.deltaTime);
        if (transform.position.y >= max)
        {
            speed = -speed;
        }
        if (transform.position.y <= min)
        {
            speed = -speed;
        }
    }
}
