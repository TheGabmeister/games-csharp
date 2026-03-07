using System.Collections.Generic;

namespace monogame_simple.Levels;

public static class LevelFactory
{
    public static IReadOnlyList<LevelDefinition> CreateCampaign()
    {
        return
        [
            new LevelDefinition(
                stageNumber: 1,
                name: "Departure",
                isBossStage: false,
                "NNNNNNNNNNNN",
                "NTNTNTNTNTNT",
                "NNNNNNNNNNNN",
                ".NNNNNNNNNN.",
                "..NNNNNNNN.."),

            new LevelDefinition(
                stageNumber: 5,
                name: "Outer Shell",
                isBossStage: false,
                "SSNNNNNNNNSS",
                "SNTTTTTTTTNS",
                "SNNNNNNNNNNS",
                ".SNNNNNNNNS.",
                "..SS....SS.."),

            new LevelDefinition(
                stageNumber: 12,
                name: "Crossfire",
                isBossStage: false,
                "NNSSNNNNSSNN",
                "NNTTNNNNTTNN",
                "NNNNSSSSNNNN",
                ".TTNNNNNNTT.",
                "..NNTTTTNN.."),

            new LevelDefinition(
                stageNumber: 21,
                name: "Steel Prison",
                isBossStage: false,
                "SSSSSSSSSSSS",
                "SNNNNNNNNNNS",
                "SNNTTTTTTNNS",
                "SNNSSNNSSNNS",
                ".SNNNNNNNNS."),

            new LevelDefinition(
                stageNumber: 32,
                name: "Final Wall",
                isBossStage: false,
                "TTTTTTTTTTTT",
                "TNNNNSSNNNNT",
                "TNTNTNTNTNTT",
                "TNNNTTTTNNNT",
                ".TTNNNNNNTT."),

            new LevelDefinition(
                stageNumber: 33,
                name: "DOH",
                isBossStage: true)
        ];
    }
}
