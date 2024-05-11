using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsableManager : MonoBehaviour
{
    public static UsableManager instance; //singleton
    private void Awake()
    {
        instance = this;
    }


    public Usable[] dbUsables;
    Dictionary<string, Usable> usables = new Dictionary<string, Usable>();

    private void Start()
    {
        for (int i = 0; i < dbUsables.Length; i++)
        {
            usables.Add(dbUsables[i].UsableName, dbUsables[i]);
        }
    }
    public void Use(string key)
    {
        if (usables.ContainsKey(key))
        {
            usables[key].Use();
        }

    }
}
