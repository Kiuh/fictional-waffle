using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NetworkManager))]
public class RandomPositionPlayerSpawner : MonoBehaviour
{
    private int m_RoundRobinIndex = 0;

    [SerializeField]
    private SpawnMethod m_SpawnMethod;

    [SerializeField]
    private List<Vector3> m_SpawnPositions = new() { Vector3.zero };

    /// <summary>
    /// Get a spawn position for a spawned object based on the spawn method.
    /// </summary>
    /// <returns>?The spawn position.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public Vector3 GetNextSpawnPosition()
    {
        switch (m_SpawnMethod)
        {
            case SpawnMethod.Random:
                int index = Random.Range(0, m_SpawnPositions.Count);
                return m_SpawnPositions[index];
            case SpawnMethod.RoundRobin:
                m_RoundRobinIndex = (m_RoundRobinIndex + 1) % m_SpawnPositions.Count;
                return m_SpawnPositions[m_RoundRobinIndex];
            default:
                throw new NotImplementedException();
        }
    }

    private void Awake()
    {
        NetworkManager networkManager = gameObject.GetComponent<NetworkManager>();
        networkManager.ConnectionApprovalCallback += ConnectionApprovalWithRandomSpawnPos;
    }

    private void ConnectionApprovalWithRandomSpawnPos(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response
    )
    {
        // Here we are only using ConnectionApproval to set the player's spawn position. Connections are always approved.
        response.CreatePlayerObject = true;
        response.Position = GetNextSpawnPosition();
        response.Rotation = Quaternion.identity;
        response.Approved = true;
    }
}

internal enum SpawnMethod
{
    Random = 0,
    RoundRobin = 1,
}
