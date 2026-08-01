using System;
using System.Collections.Generic;

namespace Schicksalswurf.Core
{
    /// <summary>
    /// Central dice roller for the game. All randomness goes through here.
    /// </summary>
    public static class DiceRoller
    {
        private static Random _rng = new();

        public static void SetSeed(int seed) => _rng = new Random(seed);

        public static int Roll(int sides) => _rng.Next(1, sides + 1);

        public static int RollD20() => Roll(20);

        public static int RollD6() => Roll(6);

        public static int RollD3() => Roll(3);

        public static int RollMultiple(int count, int sides)
        {
            int sum = 0;
            for (int i = 0; i < count; i++)
                sum += Roll(sides);
            return sum;
        }

        /// <summary>
        /// Rolls 3d20 and returns the individual results.
        /// </summary>
        public static (int d1, int d2, int d3) Roll3D20()
        {
            return (RollD20(), RollD20(), RollD20());
        }
    }

    /// <summary>
    /// Result of a skill check.
    /// </summary>
    public class CheckResult
    {
        public bool Success { get; set; }
        public int QualityLevel { get; set; }
        public int[] Rolls { get; set; }
        public int[] AttributeValues { get; set; }
        public int SkillPointsUsed { get; set; }
        public bool SpectacularSuccess { get; set; }
        public bool SpectacularFailure { get; set; }

        public override string ToString()
        {
            string rollsStr = Rolls != null ? string.Join(", ", Rolls) : "";
            return Success
                ? $"Erfolg (QS {QualityLevel}) - Würfe: [{rollsStr}]"
                : $"Fehlschlag - Würfe: [{rollsStr}]";
        }
    }

    /// <summary>
    /// Performs attribute checks and skill checks using a 3d20 roll-under system.
    /// Each character has 7 attributes. A skill check rolls 3d20 against 3 specified
    /// attributes. The skill value can be used to compensate for rolls that exceed
    /// the attribute values. Remaining skill points determine the quality level.
    /// </summary>
    public static class CheckSystem
    {
        /// <summary>
        /// Performs a simple attribute check (single d20 roll under the attribute value).
        /// </summary>
        public static bool AttributeCheck(int attributeValue, int modifier = 0)
        {
            int roll = DiceRoller.RollD20();
            return roll <= attributeValue + modifier;
        }

        /// <summary>
        /// Performs a full skill check: 3d20 against three attribute values.
        /// The skill points compensate for over-rolls. Remaining points = quality level.
        /// </summary>
        public static CheckResult SkillCheck(
            int attr1, int attr2, int attr3,
            int skillValue, int modifier = 0)
        {
            var (d1, d2, d3) = DiceRoller.Roll3D20();
            int[] rolls = { d1, d2, d3 };
            int[] attrs = { attr1, attr2, attr3 };

            // Spectacular success: all three rolls are 1
            bool specSuccess = d1 == 1 && d2 == 1 && d3 == 1;
            // Spectacular failure: all three rolls are 20
            bool specFailure = d1 == 20 && d2 == 20 && d3 == 20;

            if (specSuccess)
            {
                return new CheckResult
                {
                    Success = true,
                    QualityLevel = 6,
                    Rolls = rolls,
                    AttributeValues = attrs,
                    SkillPointsUsed = 0,
                    SpectacularSuccess = true
                };
            }

            if (specFailure)
            {
                return new CheckResult
                {
                    Success = false,
                    QualityLevel = 0,
                    Rolls = rolls,
                    AttributeValues = attrs,
                    SkillPointsUsed = 0,
                    SpectacularFailure = true
                };
            }

            // Calculate how many skill points are needed to compensate
            int neededPoints = 0;
            for (int i = 0; i < 3; i++)
            {
                int effectiveAttr = attrs[i] + modifier;
                if (rolls[i] > effectiveAttr)
                    neededPoints += rolls[i] - effectiveAttr;
            }

            int remainingPoints = skillValue - neededPoints;

            bool success = remainingPoints >= 0;
            int qualityLevel = 0;

            if (success)
            {
                // Quality level based on remaining skill points
                // QS1: 0-3, QS2: 4-6, QS3: 7-9, QS4: 10-12, QS5: 13-15, QS6: 16+
                if (remainingPoints >= 16) qualityLevel = 6;
                else if (remainingPoints >= 13) qualityLevel = 5;
                else if (remainingPoints >= 10) qualityLevel = 4;
                else if (remainingPoints >= 7) qualityLevel = 3;
                else if (remainingPoints >= 4) qualityLevel = 2;
                else qualityLevel = 1;
            }

            return new CheckResult
            {
                Success = success,
                QualityLevel = qualityLevel,
                Rolls = rolls,
                AttributeValues = attrs,
                SkillPointsUsed = neededPoints,
                SpectacularSuccess = false,
                SpectacularFailure = false
            };
        }

        /// <summary>
        /// Performs an opposed check between two characters.
        /// Returns positive if attacker wins, negative if defender wins, 0 for tie.
        /// </summary>
        public static int OpposedCheck(int attackerValue, int defenderValue)
        {
            int attackerRoll = DiceRoller.RollD20();
            int defenderRoll = DiceRoller.RollD20();

            int attackerMargin = attackerValue - attackerRoll;
            int defenderMargin = defenderValue - defenderRoll;

            return attackerMargin - defenderMargin;
        }
    }
}
