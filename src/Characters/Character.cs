using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Characters
{
    using Core;

    /// <summary>
    /// Character archetypes available at character creation.
    /// Each archetype provides starting attribute modifiers and skill bonuses.
    /// </summary>
    public enum Archetype
    {
        Krieger,    // Warrior - high KR, KO, GE
        Schurke,    // Rogue - high GE, WA, CH
        Gelehrter,  // Scholar - high IN, WI, WA
        Magier,     // Mage - high IN, WI, CH
        Heiler,     // Healer - high KO, WI, CH
        Jaeger      // Ranger - high GE, WA, KO
    }

    public static class ArchetypeData
    {
        public static readonly Dictionary<Archetype, (string desc, AttributeSet attrs, Dictionary<string, int> skills)> Data = new()
        {
            [Archetype.Krieger] = (
                "Ein kampferprobter Krieger, der Feinde mit Waffengewalt bezwingt.",
                new AttributeSet(15, 12, 14, 9, 10, 10, 9),
                new() { { "koerperbeherrschung", 5 }, { "kriegskunst", 3 }, { "klettern", 2 } }
            ),
            [Archetype.Schurke] = (
                "Ein gewandter Schurke, der sich lautlos bewegt und Schlösser knackt.",
                new AttributeSet(10, 15, 11, 11, 9, 13, 11),
                new() { { "schleichen", 5 }, { "ueberreden", 3 }, { "akrobatik", 3 } }
            ),
            [Archetype.Gelehrter] = (
                "Ein wissender Gelehrter, der die Geheimnisse der Welt erforscht.",
                new AttributeSet(8, 9, 10, 16, 13, 13, 11),
                new() { { "geschichte", 5 }, { "magiekunde", 3 }, { "alchimie", 3 } }
            ),
            [Archetype.Magier] = (
                "Ein begabter Magier, der die Kräfte des Arkanen beherrscht.",
                new AttributeSet(8, 10, 10, 15, 15, 11, 11),
                new() { { "magiekunde", 5 }, { "alchimie", 3 }, { "geschichte", 2 } }
            ),
            [Archetype.Heiler] = (
                "Ein mitfühlender Heiler, der Wunden lindert und Leben rettet.",
                new AttributeSet(9, 10, 14, 12, 13, 11, 13),
                new() { { "heilkunde", 5 }, { "pflanzenkunde", 3 }, { "etikette", 2 } }
            ),
            [Archetype.Jaeger] = (
                "Ein erfahrener Jäger, der in der Wildnis zuhause ist.",
                new AttributeSet(12, 13, 13, 10, 10, 15, 9),
                new() { { "faehrtensuche", 5 }, { "wildnisleben", 3 }, { "tierkunde", 3 } }
            )
        };

        public static string GetName(Archetype a) => a.ToString();
    }

    /// <summary>
    /// A complete character with attributes, skills, combat stats, and equipment.
    /// </summary>
    public class Character
    {
        public string Name { get; set; }
        public Archetype Archetype { get; set; }
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;

        public AttributeSet Attributes { get; set; }
        public SkillSet Skills { get; set; } = new();
        public CombatStats CombatStats { get; set; }

        // Equipment
        public Weapon EquippedWeapon { get; set; }
        public Armor EquippedArmor { get; set; }

        // Inventory
        public List<Item> Inventory { get; set; } = new();

        // Learned spells
        public HashSet<string> KnownSpells { get; set; } = new();

        // Active buffs/debuffs
        public List<ActiveBuff> ActiveBuffs { get; set; } = new();

        // Position in party (0-3, front row = 0,1; back row = 2,3)
        public int PartySlot { get; set; } = 0;

        public Character(string name, Archetype archetype)
        {
            Name = name;
            Archetype = archetype;

            var data = ArchetypeData.Data[archetype];
            Attributes = data.attrs;

            foreach (var kv in data.skills)
                Skills[kv.Key] = kv.Value;

            CombatStats = Core.CombatStats.Calculate(Attributes, Level);

            // Give starting weapon based on archetype
            EquippedWeapon = archetype switch
            {
                Archetype.Krieger => Weapon.LongSword,
                Archetype.Schurke => Weapon.Dagger,
                Archetype.Jaeger => Weapon.ShortBow,
                _ => Weapon.Staff
            };

            EquippedArmor = archetype == Archetype.Krieger ? Armor.Leather : Armor.Cloth;

            // Learn starting spells based on archetype
            if (archetype == Archetype.Magier || archetype == Archetype.Gelehrter)
            {
                KnownSpells.Add("feuerball");
                KnownSpells.Add("arkaner_blitz");
                KnownSpells.Add("heilung");
            }
            else if (archetype == Archetype.Heiler)
            {
                KnownSpells.Add("heilung");
                KnownSpells.Add("schild");
                KnownSpells.Add("kraft_boost");
            }
            else
            {
                KnownSpells.Add("heilung"); // everyone gets basic healing
            }
        }

        public int UnspentAttributePoints { get; set; } = 0;
        public int UnspentSkillPoints { get; set; } = 0;

        public static readonly int[] ExpForLevel = new int[]
        {
            0, 100, 250, 500, 900, 1400, 2000, 2800, 3800, 5000, 6500
        };

        public void GainExperience(int amount)
        {
            Experience += amount;
            while (Level < ExpForLevel.Length - 1 && Experience >= ExpForLevel[Level + 1])
            {
                LevelUp();
            }
        }

        public void LevelUp()
        {
            Level++;
            int oldMaxHP = CombatStats.MaxHealth;
            int oldMaxMP = CombatStats.MaxMana;
            CombatStats = Core.CombatStats.Calculate(Attributes, Level);
            CombatStats.CurrentHealth += CombatStats.MaxHealth - oldMaxHP;
            CombatStats.CurrentMana += CombatStats.MaxMana - oldMaxMP;

            UnspentAttributePoints += 2;
            UnspentSkillPoints += 3;
        }

        public void IncreaseAttribute(Attribute attr)
        {
            if (UnspentAttributePoints <= 0) return;
            Attributes[attr] = Attributes[attr] + 1;
            UnspentAttributePoints--;
            int oldMaxHP = CombatStats.MaxHealth;
            CombatStats = Core.CombatStats.Calculate(Attributes, Level);
            CombatStats.CurrentHealth += CombatStats.MaxHealth - oldMaxHP;
        }

        public void IncreaseSkill(string skillId)
        {
            if (UnspentSkillPoints <= 0) return;
            Skills[skillId] = Skills[skillId] + 1;
            UnspentSkillPoints--;
        }

        public void LearnSpell(string spellId)
        {
            KnownSpells.Add(spellId);
        }

        public void Rest()
        {
            CombatStats.CurrentHealth = CombatStats.MaxHealth;
            CombatStats.CurrentStamina = CombatStats.MaxStamina;
            CombatStats.CurrentMana = CombatStats.MaxMana;
        }

        public bool UseHealthPotion()
        {
            var potion = Inventory.Find(i => i.Id == "health_potion");
            if (potion == null) return false;
            Heal(15);
            Inventory.Remove(potion);
            return true;
        }

        public bool UseManaPotion()
        {
            var potion = Inventory.Find(i => i.Id == "mana_potion");
            if (potion == null) return false;
            CombatStats.CurrentMana = System.Math.Min(CombatStats.MaxMana, CombatStats.CurrentMana + 10);
            Inventory.Remove(potion);
            return true;
        }

        public CheckResult PerformSkillCheck(string skillId, int modifier = 0)
        {
            var skill = SkillRegistry.Get(skillId);
            if (skill == null)
                return new CheckResult { Success = false, QualityLevel = 0 };

            int skillValue = Skills[skillId];
            return CheckSystem.SkillCheck(
                Attributes[skill.CheckAttr1],
                Attributes[skill.CheckAttr2],
                Attributes[skill.CheckAttr3],
                skillValue,
                modifier
            );
        }

        public int GetAttackValue()
        {
            int baseVal = CombatStats.AttackBase;
            if (EquippedWeapon != null)
                baseVal += EquippedWeapon.AttackModifier;
            return baseVal;
        }

        public int GetDefenseValue()
        {
            int baseVal = CombatStats.DefenseBase;
            if (EquippedArmor != null)
                baseVal += EquippedArmor.DefenseModifier;
            return baseVal;
        }

        public int GetDamage()
        {
            int dmg = EquippedWeapon?.BaseDamage ?? 1;
            dmg += Attributes[Attribute.Kraft] / 5;
            return dmg;
        }

        public int GetSoak()
        {
            int soak = CombatStats.Soak;
            if (EquippedArmor != null)
                soak += EquippedArmor.Soak;
            return soak;
        }

        public void TakeDamage(int amount)
        {
            CombatStats.CurrentHealth = Mathf.Max(0, CombatStats.CurrentHealth - amount);
        }

        public void Heal(int amount)
        {
            CombatStats.CurrentHealth = Mathf.Min(CombatStats.MaxHealth, CombatStats.CurrentHealth + amount);
        }

        public bool IsAlive => CombatStats.IsAlive;

        public override string ToString() => $"{Name} (Lvl {Level} {Archetype})";
    }

    /// <summary>
    /// Manages a party of up to 4 characters.
    /// </summary>
    public class Party
    {
        public List<Character> Members { get; } = new();
        public int Gold { get; set; } = 50;
        public List<Item> SharedInventory { get; } = new();

        public void AddMember(Character c)
        {
            if (Members.Count >= 4)
                return;
            c.PartySlot = Members.Count;
            Members.Add(c);
        }

        public void RemoveMember(Character c)
        {
            Members.Remove(c);
            for (int i = 0; i < Members.Count; i++)
                Members[i].PartySlot = i;
        }

        public Character GetMember(int slot) =>
            slot >= 0 && slot < Members.Count ? Members[slot] : null;

        public IEnumerable<Character> AliveMembers => Members.FindAll(m => m.IsAlive);
        public IEnumerable<Character> FrontRow => Members.FindAll(m => m.PartySlot < 2 && m.IsAlive);
        public IEnumerable<Character> BackRow => Members.FindAll(m => m.PartySlot >= 2 && m.IsAlive);

        public bool IsDefeated => Members.TrueForAll(m => !m.IsAlive);

        public void Rest()
        {
            foreach (var m in Members)
            {
                m.CombatStats.CurrentHealth = m.CombatStats.MaxHealth;
                m.CombatStats.CurrentStamina = m.CombatStats.MaxStamina;
                m.CombatStats.CurrentMana = m.CombatStats.MaxMana;
            }
        }
    }
}
