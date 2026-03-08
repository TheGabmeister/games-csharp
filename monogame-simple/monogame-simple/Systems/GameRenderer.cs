using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using monogame_simple.Core;
using monogame_simple.Entities;

namespace monogame_simple.Systems;

internal sealed class GameRenderer
{
    private readonly GameplaySession _session;

    public GameRenderer(GameplaySession session) => _session = session;

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
    {
        DrawRect(spriteBatch, pixel, _session.Playfield, new Color(19, 22, 37));
        DrawRectOutline(spriteBatch, pixel, _session.Playfield, new Color(72, 83, 128), 3);

        DrawBricks(spriteBatch, pixel);
        DrawBoss(spriteBatch, pixel);
        DrawEnemies(spriteBatch, pixel);
        DrawCapsules(spriteBatch, pixel, font);
        DrawPaddle(spriteBatch, pixel);
        DrawBall(spriteBatch, pixel);
        DrawHud(spriteBatch, font);
        DrawOverlay(spriteBatch, font);
    }

    private void DrawBricks(SpriteBatch spriteBatch, Texture2D pixel)
    {
        foreach (var brick in _session.Bricks)
        {
            if (!brick.IsAlive)
            {
                continue;
            }

            DrawRect(spriteBatch, pixel, brick.Bounds, brick.Color);
            DrawRectOutline(spriteBatch, pixel, brick.Bounds, new Color(25, 27, 40), 2);
        }
    }

    private void DrawBoss(SpriteBatch spriteBatch, Texture2D pixel)
    {
        var boss = _session.Boss;
        if (boss == null)
        {
            return;
        }

        DrawRect(spriteBatch, pixel, boss.Bounds, new Color(177, 86, 82));
        DrawRectOutline(spriteBatch, pixel, boss.Bounds, new Color(62, 22, 22), 3);
        DrawRect(spriteBatch, pixel, boss.CoreBounds, new Color(246, 214, 105));

        var healthWidth = 250;
        var filledWidth = (int)MathF.Round((boss.HitPoints / (float)GameConfig.BossHitPoints) * healthWidth);
        var healthBack = new Rectangle(_session.Playfield.Center.X - (healthWidth / 2), _session.Playfield.Top + 8, healthWidth, 12);
        var healthFront = new Rectangle(healthBack.X, healthBack.Y, Math.Max(0, filledWidth), healthBack.Height);

        DrawRect(spriteBatch, pixel, healthBack, new Color(48, 22, 22));
        DrawRect(spriteBatch, pixel, healthFront, new Color(236, 92, 84));
        DrawRectOutline(spriteBatch, pixel, healthBack, Color.Black, 1);
    }

    private void DrawEnemies(SpriteBatch spriteBatch, Texture2D pixel)
    {
        foreach (var enemy in _session.Enemies)
        {
            DrawRect(spriteBatch, pixel, enemy.Bounds, enemy.Color);
            DrawRectOutline(spriteBatch, pixel, enemy.Bounds, new Color(67, 18, 18), 2);
        }
    }

    private void DrawCapsules(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
    {
        foreach (var capsule in _session.Capsules)
        {
            DrawRect(spriteBatch, pixel, capsule.Bounds, capsule.Color);
            DrawRectOutline(spriteBatch, pixel, capsule.Bounds, new Color(30, 34, 45), 1);

            var size = font.MeasureString(capsule.Label);
            var textPosition = new Vector2(
                capsule.Bounds.Center.X - (size.X / 2f),
                capsule.Bounds.Center.Y - (size.Y / 2f) - 1f);
            spriteBatch.DrawString(font, capsule.Label, textPosition, Color.Black, 0f, Vector2.Zero, 0.65f, SpriteEffects.None, 0f);
        }
    }

    private void DrawPaddle(SpriteBatch spriteBatch, Texture2D pixel)
    {
        var body = _session.Paddle.Bounds;
        DrawRect(spriteBatch, pixel, body, new Color(118, 227, 228));
        DrawRectOutline(spriteBatch, pixel, body, new Color(20, 44, 67), 2);
    }

    private void DrawBall(SpriteBatch spriteBatch, Texture2D pixel)
    {
        DrawRect(spriteBatch, pixel, _session.Ball.Bounds, new Color(250, 246, 230));
        DrawRectOutline(spriteBatch, pixel, _session.Ball.Bounds, new Color(104, 99, 88), 1);
    }

    private void DrawHud(SpriteBatch spriteBatch, SpriteFont font)
    {
        var stageText = _session.State == GameState.Title ? "--" : _session.CurrentStageNumber.ToString();
        var hudY = 16f;
        spriteBatch.DrawString(font, $"SCORE {_session.Score:0000000}", new Vector2(_session.Playfield.Left, hudY), Color.White);
        spriteBatch.DrawString(font, $"LIVES {_session.Lives}", new Vector2(_session.Playfield.Center.X - 64, hudY), Color.White);
        spriteBatch.DrawString(font, $"STAGE {stageText}", new Vector2(_session.Playfield.Right - 190, hudY), Color.White);

        if (_session.FlashTimer > 0f && !string.IsNullOrWhiteSpace(_session.FlashText))
        {
            DrawCenteredText(spriteBatch, font, _session.FlashText, 52f, new Color(255, 238, 130), 0.85f);
        }
    }

    private void DrawOverlay(SpriteBatch spriteBatch, SpriteFont font)
    {
        if (_session.State == GameState.Title)
        {
            DrawCenteredText(spriteBatch, font, "ARKANOID", 250f, new Color(143, 235, 255), 1.55f);
            DrawCenteredText(spriteBatch, font, "Move: A/D or Left/Right", 320f, Color.White, 0.92f);
            DrawCenteredText(spriteBatch, font, "Launch: Space", 352f, Color.White, 0.92f);
            DrawCenteredText(spriteBatch, font, "Press Enter to Begin", 406f, new Color(255, 238, 130), 1f);
            return;
        }

        if (_session.State == GameState.Serve)
        {
            DrawCenteredText(spriteBatch, font, _session.BannerText, _session.Playfield.Bottom - 42f, new Color(238, 227, 132), 0.8f);
            return;
        }

        if (_session.State is GameState.LevelTransition or GameState.LifeLost)
        {
            DrawCenteredText(spriteBatch, font, _session.BannerText, _session.Playfield.Center.Y - 14, new Color(238, 227, 132), 1f);
            return;
        }

        if (_session.State == GameState.Defeat)
        {
            DrawCenteredText(spriteBatch, font, "GAME OVER", 272f, new Color(238, 106, 106), 1.35f);
            DrawCenteredText(spriteBatch, font, "The Arkanoid drifts in silence.", 334f, Color.White, 0.86f);
            DrawCenteredText(spriteBatch, font, "Press Enter to Restart", 382f, new Color(255, 238, 130), 0.94f);
            return;
        }

        if (_session.State == GameState.Victory)
        {
            DrawCenteredText(spriteBatch, font, "DOH FALLS", 238f, new Color(129, 252, 150), 1.3f);
            DrawCenteredText(spriteBatch, font, "Time reverses. Vaus escapes back to Arkanoid.", 300f, Color.White, 0.79f);
            DrawCenteredText(spriteBatch, font, "The journey has only started. DOH will return.", 340f, Color.White, 0.79f);
            DrawCenteredText(spriteBatch, font, "Press Enter to Play Again", 404f, new Color(255, 238, 130), 0.95f);
        }
    }

    private static void DrawRect(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rectangle, Color color)
    {
        spriteBatch.Draw(pixel, rectangle, color);
    }

    private static void DrawRectOutline(SpriteBatch spriteBatch, Texture2D pixel, Rectangle rectangle, Color color, int thickness)
    {
        var top = new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, thickness);
        var bottom = new Rectangle(rectangle.Left, rectangle.Bottom - thickness, rectangle.Width, thickness);
        var left = new Rectangle(rectangle.Left, rectangle.Top, thickness, rectangle.Height);
        var right = new Rectangle(rectangle.Right - thickness, rectangle.Top, thickness, rectangle.Height);

        DrawRect(spriteBatch, pixel, top, color);
        DrawRect(spriteBatch, pixel, bottom, color);
        DrawRect(spriteBatch, pixel, left, color);
        DrawRect(spriteBatch, pixel, right, color);
    }

    private static void DrawCenteredText(SpriteBatch spriteBatch, SpriteFont font, string text, float y, Color color, float scale)
    {
        var size = font.MeasureString(text) * scale;
        var x = (GameConfig.VirtualWidth - size.X) / 2f;
        spriteBatch.DrawString(font, text, new Vector2(x, y), color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }
}
