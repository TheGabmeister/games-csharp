using Microsoft.Xna.Framework;

namespace monogame_simple.Entities;

public sealed class DohBoss
{
    private float _velocityX = 130f;

    public DohBoss(Rectangle playfield, int hitPoints)
    {
        HitPoints = hitPoints;
        Position = new Vector2(playfield.Center.X, playfield.Top + 72f);
    }

    public Vector2 Position { get; private set; }

    public int HitPoints { get; private set; }

    public bool IsDefeated => HitPoints <= 0;

    public Rectangle Bounds
    {
        get
        {
            return new Rectangle(
                (int)MathF.Round(Position.X - 140f),
                (int)MathF.Round(Position.Y - 48f),
                280,
                96);
        }
    }

    public Rectangle CoreBounds
    {
        get
        {
            return new Rectangle(
                (int)MathF.Round(Position.X - 56f),
                (int)MathF.Round(Position.Y - 8f),
                112,
                46);
        }
    }

    public void Update(float deltaSeconds, Rectangle playfield)
    {
        Position += new Vector2(_velocityX * deltaSeconds, 0f);

        var bounds = Bounds;
        if (bounds.Left <= playfield.Left + 16)
        {
            Position = new Vector2(playfield.Left + 16 + (bounds.Width / 2f), Position.Y);
            _velocityX = MathF.Abs(_velocityX);
        }
        else if (bounds.Right >= playfield.Right - 16)
        {
            Position = new Vector2(playfield.Right - 16 - (bounds.Width / 2f), Position.Y);
            _velocityX = -MathF.Abs(_velocityX);
        }
    }

    public bool TryDamage(Vector2 hitPoint)
    {
        if (IsDefeated || !CoreBounds.Contains(hitPoint))
        {
            return false;
        }

        HitPoints--;
        return true;
    }
}
