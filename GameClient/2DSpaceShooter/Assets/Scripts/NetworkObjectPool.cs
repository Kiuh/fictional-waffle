using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class NetworkObjectPool : MonoBehaviour
{
    [SerializeField]
    private List<PoolConfigObject> PooledPrefabsList;
    private HashSet<GameObject> prefabs = new();
    private Dictionary<GameObject, Queue<NetworkObject>> pooledObjects = new();

    public void Awake()
    {
        InitializePool();
    }

    public NetworkObject GetNetworkObject(GameObject prefab)
    {
        return GetNetworkObjectInternal(prefab, Vector3.zero, Quaternion.identity);
    }

    public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return GetNetworkObjectInternal(prefab, position, rotation);
    }

    public void ReturnNetworkObject(NetworkObject networkObject, GameObject prefab)
    {
        GameObject go = networkObject.gameObject;

        // In this simple example pool we just disable objects while they are in the pool. But we could call a function on the object here for more flexibility.
        go.SetActive(false);
        go.transform.SetParent(transform);
        pooledObjects[prefab].Enqueue(networkObject);
    }

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

        _ = NetworkManager.Singleton.PrefabHandler.AddHandler(
            prefab,
            new DummyPrefabInstanceHandler(prefab, this)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private GameObject CreateInstance(GameObject prefab)
    {
        return Instantiate(prefab);
    }

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
