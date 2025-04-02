using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    private static object _lock = new object();
    private static bool applicationQuit;

    public static T Instance
    {
        get
        {
            if (applicationQuit)
                return null;

            lock(_lock)
            {
                if(instance == null)
                {
                    GameObject singleton = new GameObject(typeof(T).ToString());
                    instance = singleton.AddComponent<T>();
                }
                return instance;
            }
        }
        set
        {
            if(instance == null)
                instance = value;
        }
    }

    private void OnDestroy()
    {
        applicationQuit = true;
    }

}
