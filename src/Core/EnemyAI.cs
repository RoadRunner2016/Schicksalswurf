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
                // Cast a spell — uses opposed check
                int spellAttack = enemy.Attack + 2;
                int targetDefense = target.GetDefenseValue();
                int margin = CheckSystem.OpposedCheck(spellAttack, targetDefense);

                if (margin > 0)
                {
                    int spellDmg = enemy.Damage + 3 + dungeonLevel / 2;
                    bool crit = DiceRoller.RollD20() == 1;
                    if (crit) spellDmg *= 2;
                    int netDmg = Mathf.Max(1, spellDmg - target.GetSoak() / 2);
                    target.TakeDamage(netDmg);
                    messages.Add(crit
                        ? $"{enemy.Name} trifft {target.Name} kritisch mit {enemy.SpecialAbilityName} (-{netDmg} HP)!"
                        : $"{enemy.Name} wirkt {enemy.SpecialAbilityName} auf {target.Name} (-{netDmg} HP).");

                    if (enemy.CanPoison && rng.Randf() < 0.5f)
                    {
                        StatusSystem.ApplyStatus(target, StatusEffectType.Poison, 3, 3, enemy.Name);
                        messages.Add($"{target.Name} wird vergiftet!");
                    }
                }
                else
                {
                    messages.Add($"{enemy.Name} wirkt {enemy.SpecialAbilityName}, aber {target.Name} widersteht.");
                }
            }
            else if (enemy.CanPoison && roll <= specialChance + 15)
            {
                // Poison attack — uses opposed check
                int attackVal = enemy.Attack;
                int targetDef = target.GetDefenseValue();
                int margin = CheckSystem.OpposedCheck(attackVal, targetDef);

                if (margin > 0)
                {
                    int dmg = Mathf.Max(1, enemy.Damage - target.GetSoak() / 2);
                    target.TakeDamage(dmg);
                    StatusSystem.ApplyStatus(target, StatusEffectType.Poison, 3, 3, enemy.Name);
                    messages.Add($"{enemy.Name} verwendet {enemy.SpecialAbilityName} auf {target.Name} (-{dmg} HP, vergiftet!)");
                }
                else
                {
                    StatusSystem.ApplyStatus(target, StatusEffectType.Poison, 2, 2, enemy.Name);
                    messages.Add($"{enemy.Name} verfehlt {target.Name}, aber Spritzer vergiften leicht (-2 HP/Runde).");
                }
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
                // Normal attack — uses D20 opposed check
                int attackVal = enemy.Attack;
                int targetDef = target.GetDefenseValue();
                int margin = CheckSystem.OpposedCheck(attackVal, targetDef);

                if (margin > 0)
                {
                    bool crit = DiceRoller.RollD20() == 1;
                    int dmg = enemy.Damage;
                    if (crit) dmg *= 2;
                    int netDmg = Mathf.Max(1, dmg - target.GetSoak());
                    target.TakeDamage(netDmg);
                    messages.Add(crit
                        ? $"{enemy.Name} trifft {target.Name} kritisch (-{netDmg} HP)!"
                        : $"{enemy.Name} greift {target.Name} an (-{netDmg} HP).");
                }
                else
                {
                    messages.Add($"{enemy.Name} verfehlt {target.Name}.");
                }
            }

            return messages;
        }
    }
}
