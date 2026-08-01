using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Core
{
    using Characters;

    /// <summary>
    /// Save/Load system using Godot ConfigFile.
    /// </summary>
    public static class SaveSystem
    {
        private const string SavePath = "user://save.cfg";

        public static bool HasSaveFile()
        {
            return FileAccess.FileExists(SavePath);
        }

        public static void SaveGame(Party party, int dungeonLevel, Vector2I playerPos, int facing, int gold)
        {
            var config = new ConfigFile();

            config.SetValue("game", "dungeon_level", dungeonLevel);
            config.SetValue("game", "player_x", playerPos.X);
            config.SetValue("game", "player_y", playerPos.Y);
            config.SetValue("game", "facing", facing);
            config.SetValue("game", "gold", gold);
            config.SetValue("game", "member_count", party.Members.Count);

            for (int i = 0; i < party.Members.Count; i++)
            {
                var m = party.Members[i];
                string section = $"member_{i}";
                config.SetValue(section, "name", m.Name);
                config.SetValue(section, "archetype", (int)m.Archetype);
                config.SetValue(section, "level", m.Level);
                config.SetValue(section, "experience", m.Experience);
                config.SetValue(section, "current_hp", m.CombatStats.CurrentHealth);
                config.SetValue(section, "current_mp", m.CombatStats.CurrentMana);
                config.SetValue(section, "current_stamina", m.CombatStats.CurrentStamina);
                config.SetValue(section, "unspent_attr", m.UnspentAttributePoints);
                config.SetValue(section, "unspent_skill", m.UnspentSkillPoints);

                // Save attributes
                foreach (Attribute attr in System.Enum.GetValues(typeof(Attribute)))
                    config.SetValue(section, $"attr_{attr}", m.Attributes[attr]);

                // Save known spells
                int spellIdx = 0;
                foreach (var spellId in m.KnownSpells)
                {
                    config.SetValue(section, $"spell_{spellIdx}", spellId);
                    spellIdx++;
                }
                config.SetValue(section, "spell_count", spellIdx);
            }

            config.Save(SavePath);
        }

        public static (Party party, int dungeonLevel, Vector2I playerPos, int facing, int gold) LoadGame()
        {
            var config = new ConfigFile();
            var err = config.Load(SavePath);
            if (err != Error.Ok)
                return (null, 1, Vector2I.Zero, 0, 0);

            int dungeonLevel = (int)config.GetValue("game", "dungeon_level", 1);
            int px = (int)config.GetValue("game", "player_x", 0);
            int py = (int)config.GetValue("game", "player_y", 0);
            int facing = (int)config.GetValue("game", "facing", 0);
            int gold = (int)config.GetValue("game", "gold", 0);
            int memberCount = (int)config.GetValue("game", "member_count", 0);

            var party = new Party { Gold = gold };

            for (int i = 0; i < memberCount; i++)
            {
                string section = $"member_{i}";
                string name = (string)config.GetValue(section, "name", $"Held {i + 1}");
                var archetype = (Archetype)(int)config.GetValue(section, "archetype", 0);
                int level = (int)config.GetValue(section, "level", 1);
                int exp = (int)config.GetValue(section, "experience", 0);
                int hp = (int)config.GetValue(section, "current_hp", 30);
                int mp = (int)config.GetValue(section, "current_mp", 10);
                int stamina = (int)config.GetValue(section, "current_stamina", 30);
                int unspentAttr = (int)config.GetValue(section, "unspent_attr", 0);
                int unspentSkill = (int)config.GetValue(section, "unspent_skill", 0);

                var member = new Character(name, archetype);
                member.Level = level;
                member.Experience = exp;
                member.CombatStats.CurrentHealth = hp;
                member.CombatStats.CurrentMana = mp;
                member.CombatStats.CurrentStamina = stamina;
                member.UnspentAttributePoints = unspentAttr;
                member.UnspentSkillPoints = unspentSkill;

                // Restore attributes
                foreach (Attribute attr in System.Enum.GetValues(typeof(Attribute)))
                {
                    int val = (int)config.GetValue(section, $"attr_{attr}", member.Attributes[attr]);
                    member.Attributes[attr] = val;
                }
                member.CombatStats = CombatStats.Calculate(member.Attributes, member.Level);
                member.CombatStats.CurrentHealth = hp;
                member.CombatStats.CurrentMana = mp;
                member.CombatStats.CurrentStamina = stamina;

                // Restore spells
                int spellCount = (int)config.GetValue(section, "spell_count", 0);
                member.KnownSpells.Clear();
                for (int s = 0; s < spellCount; s++)
                {
                    string spellId = (string)config.GetValue(section, $"spell_{s}", "");
                    if (!string.IsNullOrEmpty(spellId))
                        member.KnownSpells.Add(spellId);
                }

                party.AddMember(member);
            }

            return (party, dungeonLevel, new Vector2I(px, py), facing, gold);
        }

        public static void DeleteSave()
        {
            DirAccess.RemoveAbsolute(SavePath);
        }
    }
}
