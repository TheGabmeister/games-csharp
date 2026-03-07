using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using monogame_simple.Core;
using monogame_simple.Systems;

namespace monogame_simple;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly InputState _input = new();
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private SpriteFont _font = null!;
    private GameplaySession _session = null!;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = false;

        _graphics.PreferredBackBufferWidth = GameConfig.VirtualWidth;
        _graphics.PreferredBackBufferHeight = GameConfig.VirtualHeight;
    }

    protected override void Initialize()
    {
        var playfield = new Rectangle(
            GameConfig.PlayfieldMarginX,
            GameConfig.PlayfieldTop,
            GameConfig.VirtualWidth - (GameConfig.PlayfieldMarginX * 2),
            GameConfig.VirtualHeight - GameConfig.PlayfieldTop - GameConfig.PlayfieldBottomMargin);

        _session = new GameplaySession(playfield);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        _font = Content.Load<SpriteFont>("Fonts/GameFont");
    }

    protected override void Update(GameTime gameTime)
    {
        _input.Update();

        if (_input.IsDown(Keys.Escape))
        {
            Exit();
            return;
        }

        _session.Update(gameTime, _input);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(14, 15, 26));

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _session.Draw(_spriteBatch, _pixel, _font);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}
