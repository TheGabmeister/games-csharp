using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using monogame_simple.Core;

namespace monogame_simple.Entities;

public sealed class Paddle
{
    private float _width;
    private float _expandTimer;

    public Paddle(Vector2 startPosition)
    {
        Position = startPosition;
        _width = GameConfig.PaddleBaseWidth;
    }

    public Vector2 Position { get; private set; }

    public float Width => _width;

    public float Height => GameConfig.PaddleHeight;

    public Rectangle Bounds
    {
        get
        {
            var left = (int)MathF.Round(Position.X - (_width / 2f));
            var top = (int)MathF.Round(Position.Y - (Height / 2f));
            return new Rectangle(left, top, (int)MathF.Round(_width), (int)MathF.Round(Height));
        }
    }

    public void Reset(Vector2 newPosition)
    {
        Position = newPosition;
        _width = GameConfig.PaddleBaseWidth;
        _expandTimer = 0f;
    }

    public void ApplyExpansion(float durationSeconds, float widthScale)
    {
        _expandTimer = MathF.Max(_expandTimer, durationSeconds);
        _width = GameConfig.PaddleBaseWidth * widthScale;
    }

    public void Update(float deltaSeconds, InputState input, Rectangle playfield)
    {
        var direction = 0f;
        if (input.IsDown(Keys.Left) || input.IsDown(Keys.A))
        {
            direction -= 1f;
        }

        if (input.IsDown(Keys.Right) || input.IsDown(Keys.D))
        {
            direction += 1f;
        }

        Position += new Vector2(direction * GameConfig.PaddleSpeed * deltaSeconds, 0f);
        ClampToPlayfield(playfield);

        if (_expandTimer <= 0f)
        {
            return;
        }

        _expandTimer -= deltaSeconds;
        if (_expandTimer <= 0f)
        {
            _width = GameConfig.PaddleBaseWidth;
            ClampToPlayfield(playfield);
        }
    }

    private void ClampToPlayfield(Rectangle playfield)
    {
        var halfWidth = _width / 2f;
        var clampedX = MathHelper.Clamp(Position.X, playfield.Left + halfWidth, playfield.Right - halfWidth);
        Position = new Vector2(clampedX, Position.Y);
    }
}
