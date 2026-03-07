namespace monogame_simple.Core;

public static class GameConfig
{
    public const int VirtualWidth = 960;
    public const int VirtualHeight = 720;

    public const int PlayfieldMarginX = 32;
    public const int PlayfieldTop = 84;
    public const int PlayfieldBottomMargin = 28;

    public const int StartingLives = 3;

    public const float PaddleBaseWidth = 118f;
    public const float PaddleHeight = 18f;
    public const float PaddleSpeed = 560f;

    public const float BallRadius = 8f;
    public const float BallBaseSpeed = 380f;
    public const float BallSpeedStep = 28f;
    public const float BallMaxSpeed = 700f;

    public const float ExpandScale = 1.65f;
    public const float ExpandDuration = 14f;
    public const float SlowDuration = 8f;
    public const float SlowBallMultiplier = 0.72f;

    public const float EnemySpawnMinSeconds = 10f;
    public const float EnemySpawnMaxSeconds = 17f;

    public const int BossHitPoints = 26;
}
