using Unity.Netcode;
using UnityEngine;

public class ObstaclesHolder
{
    public int[] Obstacles;
}

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private NetworkObjectPool objectPool;

    [SerializeField]
    private int asteroidAmount = 4;

    [SerializeField]
    private GameObject powerupPrefab;

    [SerializeField]
    private GameObject asteroidPrefab;

    [SerializeField]
    private GameObject obstaclePrefab1;

    [SerializeField]
    private GameObject obstaclePrefab2;

    [SerializeField]
    private GameObject obstaclePrefab3;

    [SerializeField]
    private GameObject obstacleCornerPrefab;

    // to easily visualize, search for "0". Blocks are spawned as shown below, but flipped horizontally.
    private static int[] obstacles = new int[] { };

    private void Awake()
    {
        TextAsset file = Resources.Load("Map") as TextAsset;
        ObstaclesHolder list = JsonUtility.FromJson<ObstaclesHolder>(file.text);
        obstacles = list.Obstacles;
    }

    private void Start()
    {
        SpawnObstacles();
    }

    private void SpawnAsteroids()
    {
        for (int i = 0; i < asteroidAmount; i++)
        {
            GameObject go = objectPool.GetNetworkObject(asteroidPrefab).gameObject;
            go.transform.position = new Vector3(Random.Range(-40, 40), Random.Range(-40, 40));

            go.transform.localScale = new Vector3(4, 4, 4);

            Asteroid asteroid = go.GetComponent<Asteroid>();
            asteroid.Size = new NetworkVariable<int>(4);

            float dx = Random.Range(-40, 40) / 10.0f;
            float dy = Random.Range(-40, 40) / 10.0f;
            float dir = Random.Range(-40, 40);
            go.transform.rotation = Quaternion.Euler(0, 0, dir);
            Rigidbody2D rigidbody2D = go.GetComponent<Rigidbody2D>();
            rigidbody2D.angularVelocity = dir;
            rigidbody2D.velocity = new Vector2(dx, dy);
            asteroid.asteroidPrefab = asteroidPrefab;
            asteroid.NetworkObject.Spawn(true);
        }
    }

    private void SpawnObstacles()
    {
        // Obstacles are not networked we just spawn them as static objects on each peer
        int y = 0;
        int x = 0;
        for (int i = 0; i < obstacles.Length; i++)
        {
            if (obstacles[i] == 1)
            {
                _ = Instantiate(
                    obstaclePrefab1,
                    new Vector3(-40 + (x * 2), -40 + (y * 2), 0),
                    Quaternion.identity
                );
            }

            if (obstacles[i] == 2)
            {
                _ = Instantiate(
                    obstacleCornerPrefab,
                    new Vector3(-40 + (x * 2), -40 + (y * 2), 0),
                    Quaternion.identity
                );
            }

            if (obstacles[i] == 3)
            {
                _ = Instantiate(
                    obstaclePrefab2,
                    new Vector3(-40 + (x * 2), -40 + (y * 2), 0),
                    Quaternion.identity
                );
            }

            if (obstacles[i] == 4)
            {
                _ = Instantiate(
                    obstaclePrefab3,
                    new Vector3(-40 + (x * 2), -40 + (y * 2), 0),
                    Quaternion.identity
                );
            }

            if (i % 40 == 0)
            {
                y++;
                x = 0;
            }
            else
            {
                x++;
            }
        }
    }

    private void Update()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        if (Powerup.NumPowerUps < asteroidAmount * 4)
        {
            Vector3 pos = new(Random.Range(-40, 40), Random.Range(-40, 40), 0);
            Collider2D[] hits = Physics2D.OverlapCircleAll(pos, 2.0f);
            while (hits.Length > 0)
            {
                pos = new Vector3(Random.Range(-40, 40), Random.Range(-40, 40), 0);
                hits = Physics2D.OverlapCircleAll(pos, 2.0f);
            }

            GameObject powerUp = objectPool.GetNetworkObject(powerupPrefab).gameObject;
            powerUp.transform.position = pos;
            powerUp.GetComponent<NetworkObject>().Spawn(true);
            powerUp.GetComponent<Powerup>().BuffType.Value = (Buff.BuffType)
                Random.Range(0, (int)Buff.BuffType.Last);
        }

        if (Asteroid.numAsteroids == 0)
        {
            SpawnAsteroids();
        }
    }
}
