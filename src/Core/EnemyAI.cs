using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Core
{
    using Characters;

    /// <summary>
    /// Enemy AI with special abilities: spell casting, poison, fleeing.
    /// </summary>
    public static class EnemyAI
    {
        /// <summary>
        /// Decides and executes an enemy's action during combat.
        /// Returns log messages from the action.
        /// </summary>
        public static List<string> ExecuteEnemyAction(Enemy enemy, List<Character> targets, int dungeonLevel)
        {
            var messages = new List<string>();

            // Process status effects first
            var statusMessages = StatusSystem.ProcessEnemyStatus(enemy);
            messages.AddRange(statusMessages);

            // Check if disabled
            if (StatusSystem.IsEnemyDisabled(enemy))
            {
                messages.Add($"{enemy.Name} ist gelähmt und kann nicht handeln.");
                return messages;
            }

            if (!enemy.IsAlive) return messages;

            // Pick a target (lowest HP)
            Character target = null;
            int lowestHp = int.MaxValue;
            foreach (var t in targets)
            {
                if (t.CombatStats.CurrentHealth > 0 && t.CombatStats.CurrentHealth < lowestHp)
                {
                    lowestHp = t.CombatStats.CurrentHealth;
                    target = t;
                }
            }

            if (target == null) return messages;

            var rng = new RandomNumberGenerator();
            rng.Randomize();

            // Decide action
            int roll = rng.RandiRange(1, 100);

            // Boss enemies use special abilities more often
            int specialChance = enemy.IsBoss ? 40 : 20;

            if (enemy.CanCastSpells && roll <= specialChance)
            {
                // Cast a spell
                int spellDmg = enemy.Damage + 3 + dungeonLevel;
                target.TakeDamage(spellDmg);
                messages.Add($"{enemy.Name} wirkt {enemy.SpecialAbilityName} auf {target.Name} (-{spellDmg} HP).");

                if (enemy.CanPoison && rng.Randf() < 0.5f)
                {
                    StatusSystem.ApplyStatus(target, StatusEffectType.Poison, 3, 3, enemy.Name);
                    messages.Add($"{target.Name} wird vergiftet!");
                }
            }
            else if (enemy.CanPoison && roll <= specialChance + 15)
            {
                // Poison attack
                int dmg = enemy.Damage;
                target.TakeDamage(dmg);
                StatusSystem.ApplyStatus(target, StatusEffectType.Poison, 3, 3, enemy.Name);
                messages.Add($"{enemy.Name} verwendet {enemy.SpecialAbilityName} auf {target.Name} (-{dmg} HP, vergiftet!)");
            }
            else if (enemy.CanFlee && enemy.Health < enemy.MaxHealth * 0.2 && rng.Randf() < 0.3f)
            {
                // Try to flee
                messages.Add($"{enemy.Name} versucht zu fliehen!");
                enemy.Health = 0; // remove from combat
                messages.Add($"{enemy.Name} ist entkommen.");
            }
            else
            {
                // Normal attack
                int dmg = enemy.Damage;
                target.TakeDamage(dmg);
                messages.Add($"{enemy.Name} greift {target.Name} an (-{dmg} HP).");
            }

            return messages;
        }
    }
}
