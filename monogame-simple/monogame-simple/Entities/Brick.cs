using Microsoft.Xna.Framework;

namespace monogame_simple.Entities;

public enum BrickKind
{
    Normal,
    Tough,
    Steel
}

public sealed class Brick
{
    public Brick(Rectangle bounds, BrickKind kind, int hitPoints, int scoreValue)
    {
        Bounds = bounds;
        Kind = kind;
        HitPoints = hitPoints;
        MaxHitPoints = hitPoints;
        ScoreValue = scoreValue;
    }

    public Rectangle Bounds { get; }

    public BrickKind Kind { get; }

    public int HitPoints { get; private set; }

    public int MaxHitPoints { get; }

    public int ScoreValue { get; }

    public bool IsAlive { get; private set; } = true;

    public bool IsDestructible => Kind != BrickKind.Steel;

    public Color Color
    {
        get
        {
            return Kind switch
            {
                BrickKind.Steel => new Color(112, 118, 138),
                BrickKind.Tough when HitPoints == MaxHitPoints => new Color(233, 137, 52),
                BrickKind.Tough => new Color(244, 196, 95),
                _ => new Color(107, 220, 213)
            };
        }
    }

    public bool ApplyHit()
    {
        if (!IsAlive || !IsDestructible)
        {
            return false;
        }

        HitPoints--;
        if (HitPoints <= 0)
        {
            IsAlive = false;
            return true;
        }

        return false;
    }
}
