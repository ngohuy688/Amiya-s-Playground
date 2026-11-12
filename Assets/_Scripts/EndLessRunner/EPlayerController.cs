using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EPlayerController : MonoBehaviour
{
     public static EPlayerController Instance;
    [SerializeField] private float jumpVelocity = 10f;
    [SerializeField] private float groundNormalThreshold = 0.5f;
    [SerializeField] private SkeletonAnimation spineAnimation;
    [SerializeField] private string runAnimation = "run";
    [SerializeField] private string jumpAnimation = "jump";
    public float AnimationSpeed = 1f;

    private Rigidbody2D body;
    private readonly HashSet<Collider2D> groundColliders = new HashSet<Collider2D>();
    private bool canPlayRunAnimation;
    private bool canPlayJumpAnimation;
    private string currentAnimation;
    private bool wasGrounded;

    private bool IsGrounded
    {
        get { return groundColliders.Count > 0; }
    }

    public float ElapsedTime { get; private set; }
    public int Score
    {
        get { return Mathf.FloorToInt(ElapsedTime); }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        body = GetComponent<Rigidbody2D>();
        if (spineAnimation == null)
            spineAnimation = GetComponentInChildren<SkeletonAnimation>();
    }

    private void Start()
    {
        if (spineAnimation == null)
        {
            Debug.LogError("EPlayerController requires a Spine SkeletonAnimation component.", this);
            return;
        }

        if (spineAnimation.Skeleton == null)
        {
            Debug.LogError("The Spine SkeletonAnimation has not been initialized.", this);
            return;
        }

        canPlayRunAnimation = HasAnimation(runAnimation);
        canPlayJumpAnimation = HasAnimation(jumpAnimation);

        if (!canPlayRunAnimation)
            Debug.LogError("Spine run animation not found: " + runAnimation, this);
        if (!canPlayJumpAnimation)
            Debug.LogError("Spine jump animation not found: " + jumpAnimation, this);

        PlayAnimation(runAnimation, true, canPlayRunAnimation);
    }

    private void Update()
    {
        ElapsedTime += Time.deltaTime;
        if (MinigameManager.Instance != null && !MinigameManager.Instance.IsRunning)
            return;

        bool isGrounded = IsGrounded;
        if (wasGrounded && !isGrounded)
            PlayAnimation(jumpAnimation, false, canPlayJumpAnimation);
        else if (!wasGrounded && isGrounded)
            PlayAnimation(runAnimation, true, canPlayRunAnimation);
        wasGrounded = isGrounded;

        Jump();
    }

    private bool JumpPressed()
    {
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.UpArrow) ||
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetMouseButtonDown(0))
            return true;

        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Began)
                return true;
        }

        return false;
    }

    private void Jump()
    {
        Vector2 velocity = body.velocity;
        if(JumpPressed() && IsGrounded)
        {
            velocity.y = jumpVelocity;
            groundColliders.Clear();
            PlayAnimation(jumpAnimation, false, canPlayJumpAnimation);
        }
        if(Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W) || Input.GetMouseButtonUp(0) && velocity.y > 0)
        {
            velocity.y *= 0.5f;
        }
        body.velocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateGroundContact(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateGroundContact(collision);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        groundColliders.Remove(collision.collider);
    }

    private void UpdateGroundContact(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y >= groundNormalThreshold)
            {
                groundColliders.Add(collision.collider);
                PlayAnimation(runAnimation, true, canPlayRunAnimation);
                return;
            }
        }

        groundColliders.Remove(collision.collider);
    }

    private bool HasAnimation(string animationName)
    {
        return !string.IsNullOrEmpty(animationName) &&
            spineAnimation.Skeleton.Data.FindAnimation(animationName) != null;
    }

    private void PlayAnimation(string animationName, bool loop, bool isAvailable)
    {
        if (!isAvailable || currentAnimation == animationName)
            return;

        Spine.TrackEntry entry = spineAnimation.AnimationState.SetAnimation(0, animationName, loop);

        entry.TimeScale = Mathf.Min(AnimationSpeed, 2.5f);
        currentAnimation = animationName;
    }
    
}
