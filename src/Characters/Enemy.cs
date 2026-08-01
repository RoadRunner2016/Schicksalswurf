using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Characters
{
    using Core;

    /// <summary>
    /// Enemy definition for combat encounters.
    /// </summary>
    public class Enemy
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Damage { get; set; }
        public int Soak { get; set; }
        public int Initiative { get; set; }
        public int ExperienceReward { get; set; }
        public int GoldReward { get; set; }

        // Special abilities
        public bool CanCastSpells { get; set; } = false;
        public bool CanPoison { get; set; } = false;
        public bool CanFlee { get; set; } = false;
        public bool IsBoss { get; set; } = false;
        public string SpecialAbilityName { get; set; } = "";

        // Active status effects
        public List<StatusEffect> StatusEffects { get; set; } = new();

        public bool IsAlive => Health > 0;

        public Enemy(string name, int level, int hp, int atk, int def, int dmg, int soak,
            int init, int exp, int gold)
        {
            Name = name;
            Level = level;
            MaxHealth = hp;
            Health = hp;
            Attack = atk;
            Defense = def;
            Damage = dmg;
            Soak = soak;
            Initiative = init;
            ExperienceReward = exp;
            GoldReward = gold;
        }

        public void TakeDamage(int amount)
        {
            Health = Mathf.Max(0, Health - amount);
        }

        // Preset enemies
        public static readonly Enemy Goblin = new("Goblin", 1, 18, 10, 8, 5, 1, 8, 15, 8) { CanFlee = true };
        public static readonly Enemy Wolf = new("Wolf", 1, 15, 12, 6, 4, 0, 12, 12, 3) { CanPoison = true, SpecialAbilityName = "Biss" };
        public static readonly Enemy Skeleton = new("Skelett", 2, 22, 11, 9, 6, 1, 7, 20, 12);
        public static readonly Enemy Bandit = new("Bandit", 2, 25, 12, 10, 7, 1, 9, 25, 20) { CanFlee = true };
        public static readonly Enemy Orc = new("Ork", 3, 35, 14, 8, 9, 2, 8, 40, 30);
        public static readonly Enemy GiantSpider = new("Riesenspinne", 3, 28, 13, 7, 6, 0, 14, 35, 15) { CanPoison = true, SpecialAbilityName = "Giftbiss" };
        public static readonly Enemy DarkMage = new("Dunkler Magier", 4, 30, 10, 12, 12, 3, 11, 60, 50) { CanCastSpells = true, SpecialAbilityName = "Schattenblitz" };
        public static readonly Enemy Troll = new("Troll", 5, 60, 16, 6, 14, 4, 6, 100, 80) { IsBoss = true };

        // Extended enemies
        public static readonly Enemy Ghoul = new("Ghoul", 2, 20, 11, 7, 5, 1, 9, 18, 5) { CanPoison = true, SpecialAbilityName = "Paralysebiss" };
        public static readonly Enemy Harpy = new("Harpyie", 3, 22, 14, 8, 7, 0, 15, 35, 25) { CanFlee = true, SpecialAbilityName = "Sturzflug" };
        public static readonly Enemy Gargoyle = new("Gargoyle", 4, 40, 13, 14, 8, 5, 6, 55, 35);
        public static readonly Enemy Necromancer = new("Nekromant", 5, 35, 12, 13, 14, 3, 10, 80, 60) { CanCastSpells = true, CanPoison = true, SpecialAbilityName = "Todesfluch" };
        public static readonly Enemy Demon = new("Dämon", 6, 55, 17, 12, 16, 4, 9, 120, 90) { CanCastSpells = true, SpecialAbilityName = "Hoellenfeuer" };
        public static readonly Enemy Vampire = new("Vampir", 5, 45, 15, 11, 12, 3, 13, 90, 70) { CanCastSpells = true, SpecialAbilityName = "Lebensraub" };
        public static readonly Enemy Wraith = new("Geist", 4, 30, 14, 15, 10, 2, 16, 65, 40) { CanCastSpells = true, CanFlee = true, SpecialAbilityName = "Frostgriff" };
        public static readonly Enemy Slime = new("Schleim", 1, 12, 8, 5, 3, 0, 5, 8, 2) { CanPoison = true, SpecialAbilityName = "Saure Beruehrung" };

        // Boss enemies
        public static readonly Enemy DungeonBoss = new("Dungeonwaechter", 5, 80, 18, 12, 15, 4, 10, 200, 150) { IsBoss = true, CanCastSpells = true, SpecialAbilityName = "Verwuestung" };
        public static readonly Enemy Dragon = new("Drache", 8, 120, 20, 14, 20, 5, 8, 500, 300) { IsBoss = true, CanCastSpells = true, CanPoison = true, SpecialAbilityName = "Drachenodem" };
        public static readonly Enemy LichKing = new("Lichkoenig", 7, 100, 19, 15, 18, 5, 12, 400, 250) { IsBoss = true, CanCastSpells = true, CanPoison = true, SpecialAbilityName = "Seelenraub" };
        public static readonly Enemy DemonLord = new("Daemonenfuerst", 9, 150, 22, 16, 22, 6, 10, 600, 400) { IsBoss = true, CanCastSpells = true, SpecialAbilityName = "Apokalypse" };

        public static readonly List<Enemy> Bestiary = new()
        {
            Goblin, Wolf, Slime, Skeleton, Bandit, Ghoul, Orc, GiantSpider,
            Harpy, DarkMage, Gargoyle, Wraith, Necromancer, Vampire, Troll, Demon
        };

        public static readonly List<Enemy> Bosses = new()
        {
            DungeonBoss, Dragon, LichKing, DemonLord
        };

        public static Enemy Clone(Enemy template) => new(
            template.Name, template.Level, template.MaxHealth,
            template.Attack, template.Defense, template.Damage,
            template.Soak, template.Initiative,
            template.ExperienceReward, template.GoldReward
        )
        {
            CanCastSpells = template.CanCastSpells,
            CanPoison = template.CanPoison,
            CanFlee = template.CanFlee,
            IsBoss = template.IsBoss,
            SpecialAbilityName = template.SpecialAbilityName
        };
    }

    /// <summary>
    /// A combat encounter with enemies.
    /// </summary>
    public class Encounter
    {
        public List<Enemy> Enemies { get; } = new();

        public void AddEnemy(Enemy template)
        {
            Enemies.Add(Enemy.Clone(template));
        }

        public bool IsCleared => Enemies.TrueForAll(e => !e.IsAlive);

        public IEnumerable<Enemy> AliveEnemies => Enemies.FindAll(e => e.IsAlive);
    }
}
