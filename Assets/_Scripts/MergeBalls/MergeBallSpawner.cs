using UnityEngine;
using UnityEngine.EventSystems;

public class MergeBallSpawner : MonoBehaviour
{
    [SerializeField] private MergeBallGame game;
    [SerializeField] private float leftLimitX;
    [SerializeField] private float rightLimitX = 5f;
    [SerializeField, Min(0.1f)] private float moveSpeed = 2f;
    [SerializeField, Min(0)] private int maxSpawnLevelIndex;

    private float spawnY;
    private MergeBall previewBall;
    private int previewLevelIndex;

    private void Start()
    {
        spawnY = transform.position.y;

        if (game == null)
        {
            Debug.LogError("Assign a MergeBallGame to MergeBallSpawner.", this);
            enabled = false;
            return;
        }

        if (leftLimitX >= rightLimitX)
        {
            Debug.LogError("MergeBallSpawner requires leftLimitX to be less than rightLimitX.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (game == null || !game.IsConfigured ||
            MinigameManager.Instance == null || !MinigameManager.Instance.IsRunning)
        {
            return;
        }

        float normalizedPosition = Mathf.PingPong(Time.time * moveSpeed, 1f);
        transform.position = new Vector3(
            Mathf.Lerp(leftLimitX, rightLimitX, normalizedPosition),
            spawnY,
            transform.position.z);

        if (previewBall == null)
        {
            previewLevelIndex = Random.Range(0, Mathf.Clamp(maxSpawnLevelIndex, 0, game.LevelCount - 1) + 1);
            if (!game.TryCreatePreviewBall(new Vector3(transform.position.x, transform.position.y-2f, transform.position.z), previewLevelIndex, out previewBall))
                return;
        }

        previewBall.transform.position = transform.position;
        if (DropPressed())
        {
            if (game.TryDropPreviewBall(previewBall, previewLevelIndex))
                previewBall = null;
        }
    }

    private bool DropPressed()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            return touch.phase == TouchPhase.Began &&
                   (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject(touch.fingerId));
        }

        return Input.GetMouseButtonDown(0) &&
               (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject());
    }

    private void OnDrawGizmos()
    {
        if (game == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(leftLimitX, game.OverflowLineY, transform.position.z),
            new Vector3(rightLimitX, game.OverflowLineY, transform.position.z));
    }
}
