using Microsoft.Xna.Framework;

namespace monogame_simple.Entities;

public enum CapsuleType
{
    Expand,
    Slow,
    ExtraLife
}

public sealed class Capsule
{
    public const int Width = 24;
    public const int Height = 14;

    public Capsule(Vector2 startPosition, CapsuleType type)
    {
        Position = startPosition;
        Type = type;
    }

    public Vector2 Position { get; private set; }

    public CapsuleType Type { get; }

    public bool IsActive { get; set; } = true;

    public Rectangle Bounds
    {
        get
        {
            return new Rectangle(
                (int)MathF.Round(Position.X - (Width / 2f)),
                (int)MathF.Round(Position.Y - (Height / 2f)),
                Width,
                Height);
        }
    }

    public Color Color
    {
        get
        {
            return Type switch
            {
                CapsuleType.Expand => new Color(89, 219, 103),
                CapsuleType.Slow => new Color(76, 152, 240),
                CapsuleType.ExtraLife => new Color(238, 106, 106),
                _ => Color.White
            };
        }
    }

    public string Label
    {
        get
        {
            return Type switch
            {
                CapsuleType.Expand => "E",
                CapsuleType.Slow => "S",
                CapsuleType.ExtraLife => "L",
                _ => "?"
            };
        }
    }

    public void Update(float deltaSeconds)
    {
        Position += new Vector2(0f, 185f * deltaSeconds);
    }
}
