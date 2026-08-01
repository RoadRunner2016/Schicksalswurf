using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Core
{
    using Characters;

    /// <summary>
    /// Status effects that can be applied during combat.
    /// </summary>
    public enum StatusEffectType
    {
        Poison,
        Stun,
        Burn,
        Bleed,
        Fear,
        Confusion
    }

    /// <summary>
    /// An active status effect on a combatant.
    /// </summary>
    public class StatusEffect
    {
        public StatusEffectType Type { get; set; }
        public int DamagePerRound { get; set; }
        public int RemainingTurns { get; set; }
        public string SourceName { get; set; }

        public string DisplayName => Type switch
        {
            StatusEffectType.Poison => "Gift",
            StatusEffectType.Stun => "Betaeubt",
            StatusEffectType.Burn => "Brand",
            StatusEffectType.Bleed => "Blutung",
            StatusEffectType.Fear => "Furcht",
            StatusEffectType.Confusion => "Verwirrung",
            _ => Type.ToString()
        };

        public Color DisplayColor => Type switch
        {
            StatusEffectType.Poison => new Color(0.3f, 0.8f, 0.2f),
            StatusEffectType.Stun => new Color(0.9f, 0.9f, 0.3f),
            StatusEffectType.Burn => new Color(1.0f, 0.4f, 0.1f),
            StatusEffectType.Bleed => new Color(0.8f, 0.1f, 0.1f),
            StatusEffectType.Fear => new Color(0.5f, 0.3f, 0.8f),
            StatusEffectType.Confusion => new Color(0.3f, 0.6f, 0.9f),
            _ => Colors.White
        };

        public bool IsDisabling => Type == StatusEffectType.Stun ||
            Type == StatusEffectType.Fear || Type == StatusEffectType.Confusion;
    }

    /// <summary>
    /// Manages status effects and buffs for combatants.
    /// </summary>
    public static class StatusSystem
    {
        /// <summary>
        /// Applies a status effect to a character.
        /// </summary>
        public static void ApplyStatus(Character target, StatusEffectType type, int dmgPerRound, int duration, string source = "")
        {
            target.ActiveBuffs.Add(new ActiveBuff
            {
                Name = type.ToString(),
                Effect = type == StatusEffectType.Poison ? SpellEffect.Debuff : SpellEffect.Debuff,
                Amount = dmgPerRound,
                RemainingTurns = duration
            });

            // Also add to a status effect list if we track one
            // For now, we use a simple approach via ActiveBuffs
        }

        /// <summary>
        /// Applies a status effect to an enemy.
        /// </summary>
        public static void ApplyStatus(Enemy target, StatusEffectType type, int dmgPerRound, int duration, string source = "")
        {
            target.StatusEffects.Add(new StatusEffect
            {
                Type = type,
                DamagePerRound = dmgPerRound,
                RemainingTurns = duration,
                SourceName = source
            });
        }

        /// <summary>
        /// Processes all status effects on an enemy at the start of their turn.
        /// Returns log messages.
        /// </summary>
        public static List<string> ProcessEnemyStatus(Enemy enemy)
        {
            var messages = new List<string>();

            for (int i = enemy.StatusEffects.Count - 1; i >= 0; i--)
            {
                var effect = enemy.StatusEffects[i];

                // Apply damage
                if (effect.DamagePerRound > 0)
                {
                    enemy.TakeDamage(effect.DamagePerRound);
                    messages.Add($"{enemy.Name} leidet unter {effect.DisplayName} (-{effect.DamagePerRound} HP).");
                }

                effect.RemainingTurns--;
                if (effect.RemainingTurns <= 0)
                {
                    enemy.StatusEffects.RemoveAt(i);
                    messages.Add($"{effect.DisplayName} auf {enemy.Name} wirkt nicht mehr.");
                }
            }

            return messages;
        }

        /// <summary>
        /// Checks if an enemy is disabled (can't act this turn).
        /// </summary>
        public static bool IsEnemyDisabled(Enemy enemy)
        {
            foreach (var effect in enemy.StatusEffects)
            {
                if (effect.IsDisabling && effect.RemainingTurns > 0)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Processes status effects on a character at the start of their turn.
        /// </summary>
        public static List<string> ProcessCharacterStatus(Character character)
        {
            var messages = new List<string>();

            for (int i = character.ActiveBuffs.Count - 1; i >= 0; i--)
            {
                var buff = character.ActiveBuffs[i];

                // Damage over time effects (poison, burn, bleed)
                if (buff.Amount > 0 && (buff.Name == "Poison" || buff.Name == "Burn" || buff.Name == "Bleed"))
                {
                    character.TakeDamage(buff.Amount);
                    messages.Add($"{character.Name} leidet unter {buff.Name} (-{buff.Amount} HP).");
                }

                buff.RemainingTurns--;
                if (buff.RemainingTurns <= 0)
                {
                    character.ActiveBuffs.RemoveAt(i);
                    messages.Add($"{buff.Name} auf {character.Name} wirkt nicht mehr.");
                }
            }

            return messages;
        }

        /// <summary>
        /// Checks if a character is disabled.
        /// </summary>
        public static bool IsCharacterDisabled(Character character)
        {
            foreach (var buff in character.ActiveBuffs)
            {
                if (buff.Name == "Stun" || buff.Name == "Fear" || buff.Name == "Confusion")
                    if (buff.RemainingTurns > 0)
                        return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the effective attack modifier from buffs/debuffs.
        /// </summary>
        public static int GetAttackModifier(Character character)
        {
            int mod = 0;
            foreach (var buff in character.ActiveBuffs)
            {
                if (buff.Name == "schwaeche" && buff.RemainingTurns > 0)
                    mod += buff.Amount; // negative
                if (buff.Name == "kraft_boost" && buff.RemainingTurns > 0)
                    mod += buff.Amount; // positive
            }
            return mod;
        }

        /// <summary>
        /// Gets the effective defense modifier from buffs/debuffs.
        /// </summary>
        public static int GetDefenseModifier(Character character)
        {
            int mod = 0;
            foreach (var buff in character.ActiveBuffs)
            {
                if (buff.Name == "schild" && buff.RemainingTurns > 0)
                    mod += buff.Amount;
            }
            return mod;
        }
    }
}
