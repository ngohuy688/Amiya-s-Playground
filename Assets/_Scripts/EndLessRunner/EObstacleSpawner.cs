using UnityEngine;

public class EObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] obstaclePrefabs;
    [HideInInspector] [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private MoveBackground movementSource;
    [SerializeField] private int poolCapacity = 12;
    [SerializeField] private float startInterval = 1.5f;
    [SerializeField] private float minimumInterval = 0.65f;
    [SerializeField] private float intervalDecreasePerSecond = 0.01f;

    private float spawnTimer;
    private EObstacle[] obstaclePool;
    private GameObject[] poolPrefabs;

    private GameObject[] PrefabsToSpawn
    {
        get
        {
            if (obstaclePrefabs != null && obstaclePrefabs.Length > 0)
                return obstaclePrefabs;
            return obstaclePrefab != null ? new[] { obstaclePrefab } : new GameObject[0];
        }
    }

    private void Start()
    {
        spawnTimer = Random.Range(minimumInterval, Mathf.Max(minimumInterval, startInterval));

        if (MinigameManager.Instance == null)
        {
            Debug.LogError("EObstacleSpawner requires an MinigameManager in the active scene.", this);
            enabled = false;
            return;
        }

        if (movementSource == null || spawnPoint == null || PrefabsToSpawn.Length == 0)
        {
            Debug.LogError("EObstacleSpawner requires a MoveBackground speed source, a spawn point, and at least one obstacle prefab.", this);
            enabled = false;
            return;
        }

        if (poolCapacity <= 0)
        {
            Debug.LogError("EObstacleSpawner pool capacity must be greater than zero.", this);
            enabled = false;
            return;
        }

        foreach (GameObject prefab in PrefabsToSpawn)
        {
            if (prefab == null || prefab.GetComponent<EObstacle>() == null)
            {
                Debug.LogError("Every obstacle prefab must have EObstacle on its root.", this);
                enabled = false;
                return;
            }
        }

        obstaclePool = new EObstacle[poolCapacity];
        poolPrefabs = new GameObject[poolCapacity];
    }

    private void Update()
    {
        MinigameManager gameManager = MinigameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("EObstacleSpawner requires an MinigameManager in the active scene.", this);
            enabled = false;
            return;
        }

        if (!gameManager.IsRunning)
            return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f)
            return;

        GameObject[] prefabs = PrefabsToSpawn;
        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        int poolIndex = GetAvailablePoolIndex(prefab);
        if (poolIndex >= 0)
        {
            EObstacle obstacle = obstaclePool[poolIndex];
            if (obstacle == null || poolPrefabs[poolIndex] != prefab)
            {
                if (obstacle != null)
                    Destroy(obstacle.gameObject);

                obstacle = Instantiate(prefab).GetComponent<EObstacle>();
                obstacle.InitializePoolEntry(ReleaseObstacle);
                obstaclePool[poolIndex] = obstacle;
                poolPrefabs[poolIndex] = prefab;
            }

            obstacle.Spawn(movementSource, spawnPoint.position, spawnPoint.rotation);
        }

        float interval = Mathf.Max(
            minimumInterval,
            startInterval - EPlayerController.Instance.ElapsedTime * intervalDecreasePerSecond);
        spawnTimer = Random.Range(minimumInterval, interval);
    }

    private int GetAvailablePoolIndex(GameObject prefab)
    {
        int firstInactiveIndex = -1;

        for (int i = 0; i < obstaclePool.Length; i++)
        {
            if (obstaclePool[i] == null)
                return i;

            if (!obstaclePool[i].gameObject.activeSelf)
            {
                if (poolPrefabs[i] == prefab)
                    return i;

                if (firstInactiveIndex < 0)
                    firstInactiveIndex = i;
            }
        }

        return firstInactiveIndex;
    }

    private void ReleaseObstacle(EObstacle obstacle)
    {
        obstacle.gameObject.SetActive(false);
    }
}
