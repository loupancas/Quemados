using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPower : NetworkBehaviour
{
    public float modifier = 5f;
    public string pickUpName = "Speed";
    public float fadeTime = 5;

    public bool isEnablePower = true;

    public void GetPower()
    {
        if(isEnablePower)
        {
            StartCoroutine(EnablePower());
            
            foreach(Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }

            isEnablePower = false;
        }
    }

    public IEnumerator EnablePower()
    {
        yield return new WaitForSeconds(fadeTime);

        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        isEnablePower = true;
    }
}
