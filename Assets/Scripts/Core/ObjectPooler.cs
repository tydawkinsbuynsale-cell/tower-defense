using UnityEngine;
using System.Collections.Generic;

namespace RobotTD.Core
{
    /// <summary>
    /// Generic object pooling system for performance optimization.
    /// Critical for mobile performance - reuses objects instead of instantiate/destroy.
    /// </summary>
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance { get; private set; }

        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int initialSize = 10;
            public bool expandable = true;
            public int maxSize = 100;
        }

        [SerializeField] private List<Pool> pools;
        
        private Dictionary<string, Queue<GameObject>> poolDictionary;
        private Dictionary<string, Pool> poolSettings;
        private Dictionary<string, Transform> poolParents;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitializePools();
        }

        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            poolSettings = new Dictionary<string, Pool>();
            poolParents = new Dictionary<string, Transform>();

            foreach (Pool pool in pools)
            {
                CreatePool(pool);
            }
        }

        /// <summary>
        /// Create a new pool
        /// </summary>
        public void CreatePool(Pool pool)
        {
            if (poolDictionary.ContainsKey(pool.tag))
            {
                Debug.LogWarning($"Pool with tag {pool.tag} already exists!");
                return;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();
            poolSettings[pool.tag] = pool;

            // Create parent object for organization
            GameObject parent = new GameObject($"Pool_{pool.tag}");
            parent.transform.SetParent(transform);
            poolParents[pool.tag] = parent.transform;

            // Pre-instantiate objects
            for (int i = 0; i < pool.initialSize; i++)
            {
                GameObject obj = CreateNewPoolObject(pool, parent.transform);
                objectPool.Enqueue(obj);
            }

            poolDictionary[pool.tag] = objectPool;
        }

        /// <summary>
        /// Create a pool at runtime with a prefab
        /// </summary>
        public void CreatePool(string tag, GameObject prefab, int initialSize = 10, bool expandable = true, int maxSize = 100)
        {
            Pool pool = new Pool
            {
                tag = tag,
                prefab = prefab,
                initialSize = initialSize,
                expandable = expandable,
                maxSize = maxSize
            };
            CreatePool(pool);
        }

        private GameObject CreateNewPoolObject(Pool pool, Transform parent)
        {
            GameObject obj = Instantiate(pool.prefab, parent);
            obj.SetActive(false);
            
            // Add pooled object component for callbacks
            var pooledObj = obj.GetComponent<PooledObject>();
            if (pooledObj == null)
            {
                pooledObj = obj.AddComponent<PooledObject>();
            }
            pooledObj.poolTag = pool.tag;

            return obj;
        }

        /// <summary>
        /// Get an object from the pool
        /// </summary>
        public GameObject GetPooledObject(string tag)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool with tag {tag} doesn't exist!");
                return null;
            }

            Queue<GameObject> pool = poolDictionary[tag];
            Pool settings = poolSettings[tag];

            // Try to get an inactive object
            GameObject obj = null;
            int attempts = pool.Count;

            while (attempts > 0 && obj == null)
            {
                GameObject candidate = pool.Dequeue();
                pool.Enqueue(candidate);
                
                if (!candidate.activeInHierarchy)
                {
                    obj = candidate;
                }
                attempts--;
            }

            // If no inactive object found and pool is expandable
            if (obj == null && settings.expandable && pool.Count < settings.maxSize)
            {
                obj = CreateNewPoolObject(settings, poolParents[tag]);
                pool.Enqueue(obj);
            }

            if (obj != null)
            {
                obj.SetActive(true);
                var pooledObj = obj.GetComponent<PooledObject>();
                pooledObj?.OnSpawnFromPool();
            }

            return obj;
        }

        /// <summary>
        /// Get an object from the pool at a specific position
        /// </summary>
        public GameObject GetPooledObject(string tag, Vector3 position, Quaternion rotation)
        {
            GameObject obj = GetPooledObject(tag);
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            return obj;
        }

        /// <summary>
        /// Return an object to the pool (disable it)
        /// </summary>
        public void ReturnToPool(GameObject obj)
        {
            var pooledObj = obj.GetComponent<PooledObject>();
            pooledObj?.OnReturnToPool();
            obj.SetActive(false);
        }

        /// <summary>
        /// Return an object to the pool after a delay
        /// </summary>
        public void ReturnToPool(GameObject obj, float delay)
        {
            StartCoroutine(ReturnToPoolDelayed(obj, delay));
        }

        private System.Collections.IEnumerator ReturnToPoolDelayed(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnToPool(obj);
        }

        /// <summary>
        /// Get current pool size for a tag
        /// </summary>
        public int GetPoolSize(string tag)
        {
            if (poolDictionary.ContainsKey(tag))
            {
                return poolDictionary[tag].Count;
            }
            return 0;
        }

        /// <summary>
        /// Clear a specific pool
        /// </summary>
        public void ClearPool(string tag)
        {
            if (poolDictionary.ContainsKey(tag))
            {
                foreach (GameObject obj in poolDictionary[tag])
                {
                    Destroy(obj);
                }
                poolDictionary[tag].Clear();
                
                if (poolParents.ContainsKey(tag))
                {
                    Destroy(poolParents[tag].gameObject);
                    poolParents.Remove(tag);
                }
            }
        }

        /// <summary>
        /// Clear all pools
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var tag in new List<string>(poolDictionary.Keys))
            {
                ClearPool(tag);
            }
        }
    }

    /// <summary>
    /// Component added to pooled objects for callbacks
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        [HideInInspector] public string poolTag;

        public virtual void OnSpawnFromPool()
        {
            // Override in derived classes for custom spawn behavior
        }

        public virtual void OnReturnToPool()
        {
            // Override in derived classes for custom return behavior
        }

        /// <summary>
        /// Return this object to its pool
        /// </summary>
        public void ReturnToPool()
        {
            ObjectPooler.Instance?.ReturnToPool(gameObject);
        }

        /// <summary>
        /// Return this object to its pool after a delay
        /// </summary>
        public void ReturnToPool(float delay)
        {
            ObjectPooler.Instance?.ReturnToPool(gameObject, delay);
        }
    }
}
