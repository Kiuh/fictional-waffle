﻿using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class Asteroid : NetworkBehaviour
{
    private static string s_ObjectPoolTag = "ObjectPool";

    public static int numAsteroids = 0;
    private NetworkObjectPool m_ObjectPool;

    public NetworkVariable<int> Size = new(4);

    [SerializeField]
    private int m_NumCreates = 3;

    [HideInInspector]
    public GameObject asteroidPrefab;

    private void Awake()
    {
        m_ObjectPool = GameObject.FindWithTag(s_ObjectPoolTag).GetComponent<NetworkObjectPool>();
        Assert.IsNotNull(
            m_ObjectPool,
            $"{nameof(NetworkObjectPool)} not found in scene. Did you apply the {s_ObjectPoolTag} to the GameObject?"
        );
    }

    private void Start()
    {
        numAsteroids += 1;
    }

    public override void OnNetworkSpawn()
    {
        int size = Size.Value;
        transform.localScale = new Vector3(size, size, size);
    }

    public void Explode()
    {
        if (NetworkObject.IsSpawned == false)
        {
            return;
        }
        Assert.IsTrue(NetworkManager.IsServer);

        numAsteroids -= 1;

        int newSize = Size.Value - 1;

        if (newSize > 0)
        {
            int num = Random.Range(1, m_NumCreates + 1);

            for (int i = 0; i < num; i++)
            {
                int dx = Random.Range(0, 4) - 2;
                int dy = Random.Range(0, 4) - 2;
                Vector3 diff = new(dx * 0.3f, dy * 0.3f, 0);

                NetworkObject go = m_ObjectPool.GetNetworkObject(
                    asteroidPrefab,
                    transform.position + diff,
                    Quaternion.identity
                );

                Asteroid asteroid = go.GetComponent<Asteroid>();
                asteroid.Size = new NetworkVariable<int>(newSize);
                asteroid.asteroidPrefab = asteroidPrefab;
                go.GetComponent<NetworkObject>().Spawn();
                go.GetComponent<Rigidbody2D>().AddForce(diff * 10, ForceMode2D.Impulse);
            }
        }

        NetworkObject.Despawn(true);
    }
}
