using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using monogame_simple.Core;
using monogame_simple.Entities;
using monogame_simple.Levels;

namespace monogame_simple.Systems;

public sealed class GameplaySession
{
    private readonly Rectangle _playfield;
    private readonly Random _random = new();
    private readonly IReadOnlyList<LevelDefinition> _campaign;
    private readonly Paddle _paddle;
    private readonly Ball _ball;
    private readonly List<Brick> _bricks = [];
    private readonly List<Capsule> _capsules = [];
    private readonly List<EnemyDrone> _enemies = [];
    private readonly GameRenderer _renderer;
    private DohBoss _boss;

    private int _levelIndex;
    private int _score;
    private int _lives = GameConfig.StartingLives;

    private float _activeBallSpeed = GameConfig.BallBaseSpeed;
    private float _slowBallTimer;
    private float _stateTimer;
    private float _enemySpawnTimer;
    private float _bossDamageCooldown;
    private float _flashTimer;
    private string _flashText = string.Empty;
    private string _bannerText = "PRESS ENTER TO START";

    public GameplaySession(Rectangle playfield)
    {
        _playfield = playfield;
        _campaign = LevelFactory.CreateCampaign();

        var paddleY = playfield.Bottom - 36f;
        _paddle = new Paddle(new Vector2(playfield.Center.X, paddleY));
        _ball = new Ball(GameConfig.BallRadius);
        _ball.AttachToPaddle(_paddle);

        _renderer = new GameRenderer(this);
        ScheduleEnemySpawn();
    }

    public GameState State { get; private set; } = GameState.Title;

    // Internal properties read by GameRenderer
    internal Rectangle Playfield               => _playfield;
    internal IReadOnlyList<Brick> Bricks       => _bricks;
    internal DohBoss Boss                      => _boss;
    internal IReadOnlyList<EnemyDrone> Enemies => _enemies;
    internal IReadOnlyList<Capsule> Capsules   => _capsules;
    internal Paddle Paddle                     => _paddle;
    internal Ball Ball                         => _ball;
    internal int Score                         => _score;
    internal int Lives                         => _lives;
    internal float FlashTimer                  => _flashTimer;
    internal string FlashText                  => _flashText;
    internal string BannerText                 => _bannerText;
    internal int CurrentStageNumber            => CurrentLevel.StageNumber;

    private LevelDefinition CurrentLevel => _campaign[Math.Clamp(_levelIndex, 0, _campaign.Count - 1)];

    public void Update(GameTime gameTime, InputState input)
    {
        var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (deltaSeconds <= 0f)
        {
            return;
        }

        UpdateTransientTimers(deltaSeconds);

        if (State == GameState.Title)
        {
            if (input.IsNewPress(Keys.Enter) || input.IsNewPress(Keys.Space))
            {
                StartNewGame();
            }

            return;
        }

        if (State is GameState.Victory or GameState.Defeat)
        {
            if (input.IsNewPress(Keys.Enter))
            {
                ResetToTitle();
            }

            return;
        }

        if (State == GameState.LifeLost)
        {
            _stateTimer -= deltaSeconds;
            if (_stateTimer <= 0f)
            {
                if (_lives <= 0)
                {
                    State = GameState.Defeat;
                    _bannerText = CurrentLevel.IsBossStage
                        ? "FINAL BATTLE LOST. PRESS ENTER"
                        : "GAME OVER. PRESS ENTER";
                }
                else
                {
                    PrepareServe("PRESS SPACE TO LAUNCH");
                }
            }

            return;
        }

        if (State == GameState.LevelTransition)
        {
            _stateTimer -= deltaSeconds;
            if (_stateTimer <= 0f)
            {
                _levelIndex++;
                if (_levelIndex >= _campaign.Count)
                {
                    State = GameState.Victory;
                    _bannerText = "JOURNEY COMPLETE. PRESS ENTER";
                    return;
                }

                LoadCurrentLevel();
                PrepareServe($"STAGE {CurrentLevel.StageNumber} - {CurrentLevel.Name}");
            }

            return;
        }

        _paddle.Update(deltaSeconds, input, _playfield);
        _ball.Update(deltaSeconds, _paddle);

        if (State == GameState.Serve)
        {
            if (input.IsNewPress(Keys.Space) || input.IsNewPress(Keys.Up))
            {
                LaunchBall();
            }

            return;
        }

        if (State != GameState.Playing)
        {
            return;
        }

        if (CurrentLevel.IsBossStage && _boss is not null)
        {
            _boss.Update(deltaSeconds, _playfield);
        }
        else
        {
            UpdateEnemies(deltaSeconds);
        }

        UpdateCapsules(deltaSeconds);
        ResolveWallCollision();
        ResolvePaddleCollision();
        ResolveBrickCollisions();
        ResolveEnemyCollisions();
        ResolveBossCollision();
        ResolveBallLost();

        if (State == GameState.Playing && !CurrentLevel.IsBossStage && !HasDestructibleBricksRemaining())
        {
            BeginLevelClear();
        }
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel, SpriteFont font)
        => _renderer.Draw(spriteBatch, pixel, font);

    private void StartNewGame()
    {
        _score = 0;
        _lives = GameConfig.StartingLives;
        _levelIndex = 0;
        _slowBallTimer = 0f;
        _bossDamageCooldown = 0f;
        _flashText = string.Empty;
        _flashTimer = 0f;

        LoadCurrentLevel();
        PrepareServe($"STAGE {CurrentLevel.StageNumber} - {CurrentLevel.Name}");
    }

    private void ResetToTitle()
    {
        State = GameState.Title;
        _bannerText = "PRESS ENTER TO START";
        _levelIndex = 0;

        _bricks.Clear();
        _capsules.Clear();
        _enemies.Clear();
        _boss = null;

        _paddle.Reset(new Vector2(_playfield.Center.X, _playfield.Bottom - 36f));
        _ball.AttachToPaddle(_paddle);
    }

    private void LoadCurrentLevel()
    {
        _bricks.Clear();
        _capsules.Clear();
        _enemies.Clear();
        _boss = null;

        _paddle.Reset(new Vector2(_playfield.Center.X, _playfield.Bottom - 36f));
        _ball.AttachToPaddle(_paddle);

        if (CurrentLevel.IsBossStage)
        {
            _boss = new DohBoss(_playfield, GameConfig.BossHitPoints);
        }
        else
        {
            BuildBricks(CurrentLevel.PatternRows);
        }

        RecalculateBallSpeed();
        ScheduleEnemySpawn();
    }

    private void BuildBricks(IReadOnlyList<string> patternRows)
    {
        if (patternRows.Count == 0)
        {
            return;
        }

        var columns = patternRows.Max(r => r.Length);
        const int gap = 4;
        const int sidePadding = 26;
        const int brickHeight = 22;

        var usableWidth = _playfield.Width - (sidePadding * 2) - ((columns - 1) * gap);
        var brickWidth = (int)MathF.Floor(usableWidth / (float)columns);
        var startX = _playfield.Left + sidePadding;
        var startY = _playfield.Top + 46;

        for (var row = 0; row < patternRows.Count; row++)
        {
            var rowPattern = patternRows[row];
            for (var column = 0; column < rowPattern.Length; column++)
            {
                var marker = rowPattern[column];
                if (marker == '.')
                {
                    continue;
                }

                var brickBounds = new Rectangle(
                    startX + (column * (brickWidth + gap)),
                    startY + (row * (brickHeight + gap)),
                    brickWidth,
                    brickHeight);

                _bricks.Add(marker switch
                {
                    'T' => new Brick(brickBounds, BrickKind.Tough, hitPoints: 2, scoreValue: 120),
                    'S' => new Brick(brickBounds, BrickKind.Steel, hitPoints: 1, scoreValue: 0),
                    _ => new Brick(brickBounds, BrickKind.Normal, hitPoints: 1, scoreValue: 60)
                });
            }
        }
    }

    private void PrepareServe(string text)
    {
        State = GameState.Serve;
        _bannerText = text;
        _ball.AttachToPaddle(_paddle);
    }

    private void LaunchBall()
    {
        var x = (float)(_random.NextDouble() * 1.2 - 0.6);
        var direction = new Vector2(x, -1f);
        direction.Normalize();

        _ball.Launch(direction, _activeBallSpeed);
        State = GameState.Playing;
        _bannerText = string.Empty;
    }

    private void UpdateTransientTimers(float deltaSeconds)
    {
        if (_flashTimer > 0f)
        {
            _flashTimer -= deltaSeconds;
            if (_flashTimer <= 0f)
            {
                _flashText = string.Empty;
            }
        }

        if (_slowBallTimer > 0f)
        {
            _slowBallTimer -= deltaSeconds;
            if (_slowBallTimer <= 0f)
            {
                _slowBallTimer = 0f;
                RecalculateBallSpeed();
                ShowFlash("BALL SPEED RESTORED");
            }
        }

        if (_bossDamageCooldown > 0f)
        {
            _bossDamageCooldown -= deltaSeconds;
        }
    }

    private void RecalculateBallSpeed()
    {
        var speed = MathF.Min(GameConfig.BallBaseSpeed + (_levelIndex * GameConfig.BallSpeedStep), GameConfig.BallMaxSpeed);
        if (_slowBallTimer > 0f)
        {
            speed *= GameConfig.SlowBallMultiplier;
        }

        _activeBallSpeed = speed;
        _ball.SetSpeed(_activeBallSpeed);
    }

    private void ScheduleEnemySpawn()
    {
        _enemySpawnTimer = MathHelper.Lerp(
            GameConfig.EnemySpawnMinSeconds,
            GameConfig.EnemySpawnMaxSeconds,
            (float)_random.NextDouble());
    }

    private void UpdateEnemies(float deltaSeconds)
    {
        _enemySpawnTimer -= deltaSeconds;
        if (_enemySpawnTimer <= 0f)
        {
            var x = MathHelper.Lerp(_playfield.Left + 40, _playfield.Right - 40, (float)_random.NextDouble());
            _enemies.Add(new EnemyDrone(new Vector2(x, _playfield.Top + 22f)));
            ScheduleEnemySpawn();
        }

        for (var i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];
            enemy.Update(deltaSeconds);

            if (!enemy.IsActive || enemy.Bounds.Top > _playfield.Bottom + 32)
            {
                _enemies.RemoveAt(i);
            }
        }
    }

    private void UpdateCapsules(float deltaSeconds)
    {
        for (var i = _capsules.Count - 1; i >= 0; i--)
        {
            var capsule = _capsules[i];
            capsule.Update(deltaSeconds);

            if (capsule.Bounds.Intersects(_paddle.Bounds))
            {
                ApplyCapsule(capsule.Type);
                capsule.IsActive = false;
                _score += 100;
            }

            if (!capsule.IsActive || capsule.Bounds.Top > _playfield.Bottom + 18)
            {
                _capsules.RemoveAt(i);
            }
        }
    }

    private void ApplyCapsule(CapsuleType type)
    {
        switch (type)
        {
            case CapsuleType.Expand:
                _paddle.ApplyExpansion(GameConfig.ExpandDuration, GameConfig.ExpandScale);
                ShowFlash("CAPSULE: EXPAND");
                break;

            case CapsuleType.Slow:
                _slowBallTimer = MathF.Max(_slowBallTimer, GameConfig.SlowDuration);
                RecalculateBallSpeed();
                ShowFlash("CAPSULE: SLOW");
                break;

            case CapsuleType.ExtraLife:
                _lives = Math.Min(_lives + 1, 9);
                ShowFlash("CAPSULE: EXTRA LIFE");
                break;
        }
    }

    private void ResolveWallCollision()
    {
        var position = _ball.Position;
        var velocity = _ball.Velocity;
        var radius = _ball.Radius;
        var collided = false;

        if (position.X - radius < _playfield.Left)
        {
            position.X = _playfield.Left + radius;
            velocity.X = MathF.Abs(velocity.X);
            collided = true;
        }
        else if (position.X + radius > _playfield.Right)
        {
            position.X = _playfield.Right - radius;
            velocity.X = -MathF.Abs(velocity.X);
            collided = true;
        }

        if (position.Y - radius < _playfield.Top)
        {
            position.Y = _playfield.Top + radius;
            velocity.Y = MathF.Abs(velocity.Y);
            collided = true;
        }

        if (collided)
        {
            _ball.Position = position;
            _ball.Velocity = EnsureSpeed(velocity);
        }
    }

    private void ResolvePaddleCollision()
    {
        if (_ball.Velocity.Y <= 0f)
        {
            return;
        }

        if (!CollisionHelper.CircleIntersectsRectangle(_ball.Position, _ball.Radius, _paddle.Bounds))
        {
            return;
        }

        var relativeHit = (_ball.Position.X - _paddle.Position.X) / (_paddle.Width / 2f);
        relativeHit = MathHelper.Clamp(relativeHit, -1f, 1f);

        var bounceDirection = new Vector2(relativeHit * 0.95f, -1.12f);
        bounceDirection.Normalize();

        _ball.Velocity = bounceDirection * _activeBallSpeed;
        _ball.Position = new Vector2(_ball.Position.X, _paddle.Bounds.Top - _ball.Radius - 1f);
    }

    private void ResolveBrickCollisions()
    {
        for (var i = 0; i < _bricks.Count; i++)
        {
            var brick = _bricks[i];
            if (!brick.IsAlive)
            {
                continue;
            }

            if (!CollisionHelper.CircleIntersectsRectangle(_ball.Position, _ball.Radius, brick.Bounds))
            {
                continue;
            }

            var normal = CollisionHelper.GetBounceNormal(_ball.Position, _ball.Radius, brick.Bounds);
            _ball.Velocity = EnsureSpeed(CollisionHelper.Reflect(_ball.Velocity, normal));
            PushBallOut(normal, brick.Bounds);

            if (brick.ApplyHit())
            {
                _score += brick.ScoreValue;
                TrySpawnCapsule(brick.Bounds);
            }
            else if (brick.Kind == BrickKind.Tough)
            {
                _score += 25;
            }

            break;
        }

        _bricks.RemoveAll(brick => !brick.IsAlive);
    }

    private void ResolveEnemyCollisions()
    {
        for (var i = _enemies.Count - 1; i >= 0; i--)
        {
            var enemy = _enemies[i];
            if (enemy.Bounds.Intersects(_paddle.Bounds))
            {
                enemy.IsActive = false;
                _score = Math.Max(0, _score - 120);
                ShowFlash("ENEMY IMPACT");
            }

            if (CollisionHelper.CircleIntersectsRectangle(_ball.Position, _ball.Radius, enemy.Bounds))
            {
                var normal = CollisionHelper.GetBounceNormal(_ball.Position, _ball.Radius, enemy.Bounds);
                _ball.Velocity = EnsureSpeed(CollisionHelper.Reflect(_ball.Velocity, normal));
                PushBallOut(normal, enemy.Bounds);
                enemy.IsActive = false;
                _score += 280;
                ShowFlash("ENEMY DESTROYED");
            }

            if (!enemy.IsActive)
            {
                _enemies.RemoveAt(i);
            }
        }
    }

    private void ResolveBossCollision()
    {
        if (_boss is null)
        {
            return;
        }

        if (!CollisionHelper.CircleIntersectsRectangle(_ball.Position, _ball.Radius, _boss.Bounds))
        {
            return;
        }

        var normal = CollisionHelper.GetBounceNormal(_ball.Position, _ball.Radius, _boss.Bounds);
        _ball.Velocity = EnsureSpeed(CollisionHelper.Reflect(_ball.Velocity, normal));
        PushBallOut(normal, _boss.Bounds);

        if (_bossDamageCooldown > 0f)
        {
            return;
        }

        if (!_boss.TryDamage(_ball.Position))
        {
            return;
        }

        _score += 500;
        _bossDamageCooldown = 0.12f;
        ShowFlash($"DOH CORE: {_boss.HitPoints}");

        if (!_boss.IsDefeated)
        {
            return;
        }

        State = GameState.Victory;
        _bannerText = "DOH DEFEATED. PRESS ENTER";
    }

    private void ResolveBallLost()
    {
        if (_ball.Position.Y - _ball.Radius <= _playfield.Bottom)
        {
            return;
        }

        _lives--;
        _capsules.Clear();
        _enemies.Clear();

        if (_lives <= 0)
        {
            State = GameState.Defeat;
            _bannerText = CurrentLevel.IsBossStage
                ? "DEFEATED BY DOH. PRESS ENTER"
                : "GAME OVER. PRESS ENTER";
            return;
        }

        _ball.AttachToPaddle(_paddle);
        State = GameState.LifeLost;
        _stateTimer = 0.8f;
        _bannerText = "VAUS DESTROYED";
    }

    private void BeginLevelClear()
    {
        _score += 600;
        State = GameState.LevelTransition;
        _stateTimer = 1.5f;
        _bannerText = $"STAGE {CurrentLevel.StageNumber} CLEARED";
    }

    private bool HasDestructibleBricksRemaining()
    {
        return _bricks.Any(brick => brick.IsDestructible && brick.IsAlive);
    }

    private void TrySpawnCapsule(Rectangle sourceBounds)
    {
        if (_random.NextDouble() > 0.27)
        {
            return;
        }

        var type = _random.Next(0, 3) switch
        {
            0 => CapsuleType.Expand,
            1 => CapsuleType.Slow,
            _ => CapsuleType.ExtraLife
        };

        _capsules.Add(new Capsule(new Vector2(sourceBounds.Center.X, sourceBounds.Center.Y), type));
    }

    private Vector2 EnsureSpeed(Vector2 velocity)
    {
        if (velocity == Vector2.Zero)
        {
            return new Vector2(0f, -_activeBallSpeed);
        }

        velocity.Normalize();
        return velocity * _activeBallSpeed;
    }

    private void PushBallOut(Vector2 normal, Rectangle rectangle)
    {
        var position = _ball.Position;
        if (normal.X > 0f)
        {
            position.X = rectangle.Right + _ball.Radius + 1f;
        }
        else if (normal.X < 0f)
        {
            position.X = rectangle.Left - _ball.Radius - 1f;
        }

        if (normal.Y > 0f)
        {
            position.Y = rectangle.Bottom + _ball.Radius + 1f;
        }
        else if (normal.Y < 0f)
        {
            position.Y = rectangle.Top - _ball.Radius - 1f;
        }

        _ball.Position = position;
    }

    private void ShowFlash(string text)
    {
        _flashText = text;
        _flashTimer = 1.1f;
    }
}
