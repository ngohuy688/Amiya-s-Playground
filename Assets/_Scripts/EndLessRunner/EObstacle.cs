using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class EObstacle : MonoBehaviour
{
    [SerializeField] private float despawnX = -15f;
    [SerializeField] private SkeletonAnimation spineAnimation;
    [SerializeField] private string[] randomAnimations;

    private MoveBackground movementSource;
    private Action<EObstacle> releaseToPool;
    private readonly List<string> availableAnimations = new List<string>();
    private bool animationsValidated;

    private void Awake()
    {
        if (spineAnimation == null)
            spineAnimation = GetComponentInChildren<SkeletonAnimation>();
    }

    public void InitializePoolEntry(Action<EObstacle> releaseCallback)
    {
        releaseToPool = releaseCallback;
    }

    public void Spawn(MoveBackground source, Vector3 position, Quaternion rotation)
    {
        movementSource = source;
        transform.SetPositionAndRotation(position, rotation);
        gameObject.SetActive(true);
        PlayRandomAnimation();
    }

    private void Update()
    {
        MinigameManager gameManager = MinigameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("EObstacle requires an MinigameManager in the active scene.", this);
            enabled = false;
            return;
        }

        if (!gameManager.IsRunning)
            return;

        if (movementSource == null)
        {
            Debug.LogError("EObstacle requires a MoveBackground speed source.", this);
            enabled = false;
            return;
        }

        transform.position += Vector3.right * movementSource.speed * Time.deltaTime;

        if (transform.position.x <= despawnX)
            ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (releaseToPool == null)
        {
            Debug.LogError("EObstacle was not initialized by an EObstacleSpawner pool.", this);
            Destroy(gameObject);
            return;
        }

        releaseToPool(this);
    }

    private void PlayRandomAnimation()
    {
        if (randomAnimations == null || randomAnimations.Length == 0)
            return;

        if (spineAnimation == null)
        {
            Debug.LogError("Random Spine animations are configured, but no SkeletonAnimation was found.", this);
            return;
        }

        if (spineAnimation.Skeleton == null)
            spineAnimation.Initialize(false);

        if (spineAnimation.Skeleton == null)
        {
            Debug.LogError("The obstacle SkeletonAnimation could not be initialized.", this);
            return;
        }

        if (!animationsValidated)
        {
            foreach (string animationName in randomAnimations)
            {
                if (!string.IsNullOrEmpty(animationName) &&
                    spineAnimation.Skeleton.Data.FindAnimation(animationName) != null)
                {
                    availableAnimations.Add(animationName);
                }
                else
                {
                    Debug.LogError("Spine animation not found: " + animationName, this);
                }
            }

            animationsValidated = true;
        }

        if (availableAnimations.Count == 0)
        {
            Debug.LogError("EObstacle has no valid Spine animations to play.", this);
            return;
        }

        string selectedAnimation = availableAnimations[UnityEngine.Random.Range(0, availableAnimations.Count)];
        spineAnimation.AnimationState.SetAnimation(0, selectedAnimation, true);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckPlayerCollision(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckPlayerCollision(other);
    }

    private void CheckPlayerCollision(Collider2D other)
    {
        EPlayerController player = other.GetComponentInParent<EPlayerController>();
        if (player == null)
            return;

        MinigameManager gameManager = MinigameManager.Instance;
        if (gameManager != null)
            gameManager.GameOver(EPlayerController.Instance.Score);
    }
}
