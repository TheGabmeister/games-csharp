using Microsoft.Xna.Framework;

namespace monogame_simple.Entities;

public sealed class Ball
{
    private float _attachOffsetX;

    public Ball(float radius)
    {
        Radius = radius;
    }

    public Vector2 Position { get; set; }

    public Vector2 Velocity { get; set; }

    public float Radius { get; }

    public bool IsAttachedToPaddle { get; private set; } = true;

    public Rectangle Bounds
    {
        get
        {
            var diameter = (int)MathF.Round(Radius * 2f);
            return new Rectangle(
                (int)MathF.Round(Position.X - Radius),
                (int)MathF.Round(Position.Y - Radius),
                diameter,
                diameter);
        }
    }

    public void AttachToPaddle(Paddle paddle, float offsetX = 0f)
    {
        IsAttachedToPaddle = true;
        _attachOffsetX = offsetX;
        Velocity = Vector2.Zero;
        Position = new Vector2(paddle.Position.X + _attachOffsetX, paddle.Bounds.Top - Radius - 2f);
    }

    public void Launch(Vector2 direction, float speed)
    {
        if (direction == Vector2.Zero)
        {
            direction = new Vector2(0f, -1f);
        }

        direction.Normalize();
        Velocity = direction * speed;
        IsAttachedToPaddle = false;
    }

    public void SetSpeed(float speed)
    {
        if (IsAttachedToPaddle || Velocity == Vector2.Zero)
        {
            return;
        }

        var dir = Velocity;
        dir.Normalize();
        Velocity = dir * speed;
    }

    public void Update(float deltaSeconds, Paddle paddle)
    {
        if (IsAttachedToPaddle)
        {
            Position = new Vector2(paddle.Position.X + _attachOffsetX, paddle.Bounds.Top - Radius - 2f);
            return;
        }

        Position += Velocity * deltaSeconds;
    }
}
