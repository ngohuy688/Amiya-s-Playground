using UnityEngine;

public class MergeBall : MonoBehaviour
{
    public int LevelIndex { get; private set; }
    internal MergeBallGame Game { get; private set; }

    private bool isMerging;

    internal void Initialize(MergeBallGame game, int levelIndex)
    {
        Game = game;
        LevelIndex = levelIndex;
        isMerging = false;
        Game.RegisterBall(this);
    }

    internal bool IsMerging
    {
        get { return isMerging; }
    }

    internal void MarkMerging()
    {
        isMerging = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isMerging || Game == null)
            return;

        MergeBall other = collision.collider.GetComponentInParent<MergeBall>();
        if (other != null && other != this)
            Game.TryMerge(this, other);
    }

    private void OnDestroy()
    {
        if (Game != null)
            Game.UnregisterBall(this);
    }
}
