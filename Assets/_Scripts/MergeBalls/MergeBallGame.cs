using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MergeBallGame : MonoBehaviour
{
    [Header("Ball levels (list index is the level)")]
    [SerializeField] private List<GameObject> ballPrefabs = new List<GameObject>();

    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField, Min(1)] private int pointsPerLevel = 10;

    [Header("Game over")]
    [SerializeField] private float overflowLineY;
    [SerializeField, Min(0.1f)] private float overflowDuration = 2f;

    private readonly HashSet<MergeBall> activeBalls = new HashSet<MergeBall>();
    private int score;
    private float overflowTimer;
    private bool hasEnded;

    public int LevelCount
    {
        get { return ballPrefabs.Count; }
    }

    public float OverflowLineY
    {
        get { return overflowLineY; }
    }

    public bool IsConfigured { get; private set; }

    private bool IsGameRunning
    {
        get
        {
            return IsConfigured &&
                   !hasEnded &&
                   MinigameManager.Instance != null &&
                   MinigameManager.Instance.IsRunning;
        }
    }

    private void Start()
    {
        if (MinigameManager.Instance == null)
        {
            Debug.LogError("MergeBallGame requires a MinigameManager in the scene.", this);
            return;
        }

        if (scoreText == null)
        {
            Debug.LogError("Assign the in-game score TMP_Text to MergeBallGame.", this);
            return;
        }

        if (ballPrefabs == null || ballPrefabs.Count < 2)
        {
            Debug.LogError("Assign at least two ball prefabs, ordered from lowest to highest level.", this);
            return;
        }

        for (int i = 0; i < ballPrefabs.Count; i++)
        {
            GameObject prefab = ballPrefabs[i];
            if (prefab == null ||
                prefab.GetComponent<MergeBall>() == null ||
                prefab.GetComponent<Rigidbody2D>() == null ||
                prefab.GetComponent<Collider2D>() == null)
            {
                Debug.LogError(
                    "Ball level " + i + " must have a prefab with MergeBall, Rigidbody2D, and Collider2D.",
                    this);
                return;
            }
        }

        score = 0;
        scoreText.text = "Score: 0";
        IsConfigured = true;
    }

    private void Update()
    {
        if (!IsGameRunning)
            return;

        bool ballAboveLimit = false;
        foreach (MergeBall ball in activeBalls)
        {
            if (ball == null || !ball.isActiveAndEnabled)
                continue;

            Collider2D ballCollider = ball.GetComponent<Collider2D>();
            if (ballCollider != null && ballCollider.bounds.max.y > overflowLineY)
            {
                ballAboveLimit = true;
                break;
            }
        }

        if (!ballAboveLimit)
        {
            overflowTimer = 0f;
            return;
        }

        overflowTimer += Time.deltaTime;
        if (overflowTimer >= overflowDuration)
        {
            hasEnded = true;
            MinigameManager.Instance.GameOver(score);
        }
    }

    internal void RegisterBall(MergeBall ball)
    {
        if (ball != null)
            activeBalls.Add(ball);
    }

    internal void UnregisterBall(MergeBall ball)
    {
        if (ball != null)
            activeBalls.Remove(ball);
    }

    internal bool TryMerge(MergeBall first, MergeBall second)
    {
        if (!IsGameRunning ||
            first == null ||
            second == null ||
            first == second ||
            first.Game != this ||
            second.Game != this ||
            first.IsMerging ||
            second.IsMerging ||
            first.LevelIndex != second.LevelIndex)
        {
            return false;
        }

        int nextLevel = first.LevelIndex + 1;
        if (nextLevel >= ballPrefabs.Count)
            return false;

        GameObject nextPrefab = ballPrefabs[nextLevel];
        if (nextPrefab == null || nextPrefab.GetComponent<MergeBall>() == null)
        {
            Debug.LogError("The prefab for ball level " + nextLevel + " is not configured.", this);
            return false;
        }

        first.MarkMerging();
        second.MarkMerging();

        Vector3 mergePosition = (first.transform.position + second.transform.position) * 0.5f;
        first.gameObject.SetActive(false);
        second.gameObject.SetActive(false);

        GameObject mergedObject = Instantiate(nextPrefab, mergePosition, Quaternion.identity);
        MergeBall mergedBall = mergedObject.GetComponent<MergeBall>();
        mergedBall.Initialize(this, nextLevel);

        score += pointsPerLevel * (first.LevelIndex + 1);
        scoreText.text = "Score: " + score;
        return true;
    }

    public bool TrySpawnBall(Vector3 position, int levelIndex)
    {
        if (!IsGameRunning)
            return false;

        if (levelIndex < 0 || levelIndex >= ballPrefabs.Count)
        {
            Debug.LogError("Cannot spawn a ball: level index is out of range.", this);
            return false;
        }

        GameObject spawnedObject = Instantiate(ballPrefabs[levelIndex], position, Quaternion.identity);
        MergeBall ball = spawnedObject.GetComponent<MergeBall>();
        if (ball == null)
        {
            Debug.LogError("The spawned ball prefab is missing MergeBall.", spawnedObject);
            Destroy(spawnedObject);
            return false;
        }

        ball.Initialize(this, levelIndex);
        return true;
    }

    internal bool TryCreatePreviewBall(Vector3 position, int levelIndex, out MergeBall previewBall)
    {
        previewBall = null;
        if (!IsGameRunning)
            return false;

        if (levelIndex < 0 || levelIndex >= ballPrefabs.Count)
        {
            Debug.LogError("Cannot create a preview ball: level index is out of range.", this);
            return false;
        }

        GameObject previewObject = Instantiate(ballPrefabs[levelIndex], position, Quaternion.identity);
        previewBall = previewObject.GetComponent<MergeBall>();
        Rigidbody2D body = previewObject.GetComponent<Rigidbody2D>();
        Collider2D ballCollider = previewObject.GetComponent<Collider2D>();

        if (previewBall == null || body == null || ballCollider == null)
        {
            Debug.LogError("The preview ball prefab is missing a required component.", previewObject);
            Destroy(previewObject);
            previewBall = null;
            return false;
        }

        body.simulated = false;
        ballCollider.enabled = false;
        return true;
    }

    public bool TryDropPreviewBall(MergeBall previewBall, int levelIndex)
    {
        if (!IsGameRunning || previewBall == null || previewBall.Game != null)
            return false;

        Rigidbody2D body = previewBall.GetComponent<Rigidbody2D>();
        Collider2D ballCollider = previewBall.GetComponent<Collider2D>();
        if (body == null || ballCollider == null)
        {
            Debug.LogError("The preview ball is missing a Rigidbody2D or Collider2D.", previewBall);
            return false;
        }

        ballCollider.enabled = true;
        body.simulated = true;
        previewBall.Initialize(this, levelIndex);
        return true;
    }
}
