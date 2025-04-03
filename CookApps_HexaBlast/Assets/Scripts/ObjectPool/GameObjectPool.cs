using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool
{
    private record InternalPool
    {
        private GameObject source;
        public GameObject Source => source;
        private Queue<GameObject> pool;

        private GameObject rootObject;
        public GameObject RootObject => rootObject;

        public InternalPool(GameObject source, int capacity)
        {
            this.source = source;
            pool = new(capacity);

            rootObject = new GameObject(source.name);
            rootObject.SetActive(false);
            GameObject.DontDestroyOnLoad(rootObject);
        }

        public GameObject Get()
        {
            GameObject retInstance = null;
            if(pool.Count > 0)
            {
                retInstance = pool.Dequeue();
            }
            else
            {
                retInstance = GameObject.Instantiate(source);
                GameObject.DontDestroyOnLoad(retInstance);
            }

            MoveToRoot(retInstance);
            return retInstance;
        }

        public void Return(GameObject instance)
        {
            if (!instance) return;
            pool.Enqueue(instance);
            MoveToRoot(instance);
        }

        public void Clear()
        {
            var enumerator = pool.GetEnumerator();
            while(enumerator.MoveNext())
            {
                var element = enumerator.Current;
                GameObject.Destroy(element);
            }
            pool.Clear();
        }

        ~InternalPool()
        {
            if (pool.Count > 0)
                Clear();

            source = null;
            pool = null;
        }

        private void MoveToRoot(GameObject instance)
        {
            if (!instance) return;
            instance.transform.SetParent(rootObject.transform);
        }
    }

    private static readonly int POOL_CAPACITY = 10;
    private Dictionary<object, InternalPool> dicPool;
    private Dictionary<int, object> dicInstanceKey;

    private GameObject rootObject;
    public GameObject RootObject => rootObject;
    private string name;

    public GameObjectPool(string name)
    {
        this.name = name;
        dicPool = new();
        dicInstanceKey = new();
    }

    ~GameObjectPool()
    {
        GameObject.Destroy(rootObject);
        rootObject = null;
        dicInstanceKey = null;
        dicPool = null;
    }

    public GameObject Get(string key, bool active = true)
    {
        if(!rootObject)
        {
            rootObject = new GameObject($"{name}");
            GameObject.DontDestroyOnLoad(rootObject);
        }

        if (key == null) return null;
        if (key is string textKey && string.IsNullOrEmpty(textKey)) return null;
        if(!dicPool.TryGetValue(key, out var pool))
        {
            var source = Resources.Load<GameObject>(key);
            if (!source) return null;
            var newPool = new InternalPool(source, POOL_CAPACITY);
            dicPool.Add(key, newPool);
            newPool.RootObject.transform.SetParent(RootObject.transform);
            pool = newPool;
        }

        var instance = pool.Get();
        if(instance)
        {
            instance.SetActive(active);
            if (!dicInstanceKey.TryAdd(instance.GetInstanceID(), key))
                throw new System.Exception("Error");
        }
        return instance;
    }

    public bool TryGet(string key, out GameObject outInstance, bool active = true)
    {
        outInstance = Get(key, active);
        return outInstance;
    }

    public void Return(GameObject instance)
    {
        if (!instance) return;
        if(!dicInstanceKey.TryGetValue(instance.GetInstanceID(), out var key))
        {
            GameObject.Destroy(instance);
            Debug.LogError("Not Found Key");
            return;
        }
        dicInstanceKey.Remove(instance.GetInstanceID());
        if(!dicPool.TryGetValue(key, out var pool))
        {
            GameObject.Destroy(instance);
            Debug.LogError("Not Found Pool");
            return;
        }

        instance.SetActive(false);
        pool.Return(instance);
    }

    public void ReleaseAll()
    {
        var enumerator = dicPool.GetEnumerator();
        while(enumerator.MoveNext())
        {
            var element = enumerator.Current;
            var pool = element.Value;
            pool.Clear();
        }
        dicPool.Clear();
        dicInstanceKey.Clear();
    }
}
