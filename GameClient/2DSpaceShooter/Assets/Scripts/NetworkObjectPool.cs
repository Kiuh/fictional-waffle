using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class NetworkObjectPool : MonoBehaviour
{
    [SerializeField]
    private NetworkManager m_NetworkManager;

    [SerializeField]
    private List<PoolConfigObject> PooledPrefabsList;
    private HashSet<GameObject> prefabs = new();
    private Dictionary<GameObject, Queue<NetworkObject>> pooledObjects = new();

    public void Awake()
    {
        InitializePool();
    }

    public void OnValidate()
    {
        for (int i = 0; i < PooledPrefabsList.Count; i++)
        {
            GameObject prefab = PooledPrefabsList[i].Prefab;
            if (prefab != null)
            {
                Assert.IsNotNull(
                    prefab.GetComponent<NetworkObject>(),
                    $"{nameof(NetworkObjectPool)}: Pooled prefab \"{prefab.name}\" at index {i} has no {nameof(NetworkObject)} component."
                );
            }
        }
    }

    /// <summary>
    /// Gets an instance of the given prefab from the pool. The prefab must be registered to the pool.
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns></returns>
    public NetworkObject GetNetworkObject(GameObject prefab)
    {
        return GetNetworkObjectInternal(prefab, Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// Gets an instance of the given prefab from the pool. The prefab must be registered to the pool.
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="position">The position to spawn the object at.</param>
    /// <param name="rotation">The rotation to spawn the object with.</param>
    /// <returns></returns>
    public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return GetNetworkObjectInternal(prefab, position, rotation);
    }

    /// <summary>
    /// Return an object to the pool (and reset them).
    /// </summary>
    public void ReturnNetworkObject(NetworkObject networkObject, GameObject prefab)
    {
        GameObject go = networkObject.gameObject;

        // In this simple example pool we just disable objects while they are in the pool. But we could call a function on the object here for more flexibility.
        go.SetActive(false);
        go.transform.SetParent(transform);
        pooledObjects[prefab].Enqueue(networkObject);
    }

    /// <summary>
    /// Adds a prefab to the list of spawnable prefabs.
    /// </summary>
    /// <param name="prefab">The prefab to add.</param>
    /// <param name="prewarmCount"></param>
    public void AddPrefab(GameObject prefab, int prewarmCount = 0)
    {
        NetworkObject networkObject = prefab.GetComponent<NetworkObject>();

        Assert.IsNotNull(
            networkObject,
            $"{nameof(prefab)} must have {nameof(networkObject)} component."
        );
        Assert.IsFalse(
            prefabs.Contains(prefab),
            $"Prefab {prefab.name} is already registered in the pool."
        );

        RegisterPrefabInternal(prefab, prewarmCount);
    }

    /// <summary>
    /// Builds up the cache for a prefab.
    /// </summary>
    private void RegisterPrefabInternal(GameObject prefab, int prewarmCount)
    {
        _ = prefabs.Add(prefab);

        Queue<NetworkObject> prefabQueue = new();
        pooledObjects[prefab] = prefabQueue;

        for (int i = 0; i < prewarmCount; i++)
        {
            GameObject go = CreateInstance(prefab);
            ReturnNetworkObject(go.GetComponent<NetworkObject>(), prefab);
        }

        // Register Netcode Spawn handlers
        _ = m_NetworkManager.PrefabHandler.AddHandler(
            prefab,
            new DummyPrefabInstanceHandler(prefab, this)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private GameObject CreateInstance(GameObject prefab)
    {
        return Instantiate(prefab);
    }

    /// <summary>
    /// This matches the signature of <see cref="NetworkSpawnManager.SpawnHandlerDelegate"/>
    /// </summary>
    /// <param name="prefabHash"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    private NetworkObject GetNetworkObjectInternal(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation
    )
    {
        Queue<NetworkObject> queue = pooledObjects[prefab];

        NetworkObject networkObject;
        if (queue.Count > 0)
        {
            networkObject = queue.Dequeue();
        }
        else
        {
            networkObject = CreateInstance(prefab).GetComponent<NetworkObject>();
        }

        // Here we must reverse the logic in ReturnNetworkObject.
        GameObject go = networkObject.gameObject;
        go.transform.SetParent(null);
        go.SetActive(true);

        go.transform.position = position;
        go.transform.rotation = rotation;

        return networkObject;
    }

    /// <summary>
    /// Registers all objects in <see cref="PooledPrefabsList"/> to the cache.
    /// </summary>
    private void InitializePool()
    {
        foreach (PoolConfigObject configObject in PooledPrefabsList)
        {
            RegisterPrefabInternal(configObject.Prefab, configObject.PrewarmCount);
        }
    }
}

[Serializable]
internal struct PoolConfigObject
{
    public GameObject Prefab;
    public int PrewarmCount;
}

internal class DummyPrefabInstanceHandler : INetworkPrefabInstanceHandler
{
    private GameObject m_Prefab;
    private NetworkObjectPool m_Pool;

    public DummyPrefabInstanceHandler(GameObject prefab, NetworkObjectPool pool)
    {
        m_Prefab = prefab;
        m_Pool = pool;
    }

    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        return m_Pool.GetNetworkObject(m_Prefab, position, rotation);
    }

    public void Destroy(NetworkObject networkObject)
    {
        m_Pool.ReturnNetworkObject(networkObject, m_Prefab);
    }
}
