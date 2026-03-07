using Microsoft.Xna.Framework;

namespace monogame_simple.Systems;

public static class CollisionHelper
{
    public static bool CircleIntersectsRectangle(Vector2 center, float radius, Rectangle rectangle)
    {
        var nearestX = MathHelper.Clamp(center.X, rectangle.Left, rectangle.Right);
        var nearestY = MathHelper.Clamp(center.Y, rectangle.Top, rectangle.Bottom);
        var dx = center.X - nearestX;
        var dy = center.Y - nearestY;
        return (dx * dx) + (dy * dy) <= radius * radius;
    }

    public static Vector2 GetBounceNormal(Vector2 circleCenter, float radius, Rectangle rectangle)
    {
        var rectCenter = new Vector2(rectangle.Center.X, rectangle.Center.Y);
        var delta = circleCenter - rectCenter;
        var overlapX = (rectangle.Width / 2f) + radius - MathF.Abs(delta.X);
        var overlapY = (rectangle.Height / 2f) + radius - MathF.Abs(delta.Y);

        if (overlapX < overlapY)
        {
            return new Vector2(delta.X >= 0f ? 1f : -1f, 0f);
        }

        return new Vector2(0f, delta.Y >= 0f ? 1f : -1f);
    }

    public static Vector2 Reflect(Vector2 velocity, Vector2 normal)
    {
        var reflected = velocity;
        if (normal.X != 0f)
        {
            reflected.X = -reflected.X;
        }

        if (normal.Y != 0f)
        {
            reflected.Y = -reflected.Y;
        }

        return reflected;
    }
}
