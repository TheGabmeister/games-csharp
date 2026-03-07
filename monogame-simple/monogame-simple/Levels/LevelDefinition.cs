using System.Collections.Generic;

namespace monogame_simple.Levels;

public sealed class LevelDefinition
{
    public LevelDefinition(int stageNumber, string name, bool isBossStage, params string[] patternRows)
    {
        StageNumber = stageNumber;
        Name = name;
        IsBossStage = isBossStage;
        PatternRows = patternRows;
    }

    public int StageNumber { get; }

    public string Name { get; }

    public bool IsBossStage { get; }

    public IReadOnlyList<string> PatternRows { get; }
}
