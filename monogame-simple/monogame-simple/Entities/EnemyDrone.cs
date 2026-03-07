using Microsoft.Xna.Framework;

namespace monogame_simple.Entities;

public sealed class EnemyDrone
{
    private readonly float _originX;
    private float _phase;

    public EnemyDrone(Vector2 startPosition)
    {
        Position = startPosition;
        _originX = startPosition.X;
    }

    public Vector2 Position { get; private set; }

    public bool IsActive { get; set; } = true;

    public Rectangle Bounds
    {
        get
        {
            return new Rectangle(
                (int)MathF.Round(Position.X - 18f),
                (int)MathF.Round(Position.Y - 10f),
                36,
                20);
        }
    }

    public Color Color => new(220, 80, 79);

    public void Update(float deltaSeconds)
    {
        _phase += deltaSeconds * 2.4f;
        Position = new Vector2(_originX + (MathF.Sin(_phase) * 44f), Position.Y + (92f * deltaSeconds));
    }
}
