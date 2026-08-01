using System.Collections.Generic;

namespace Schicksalswurf.Characters
{
    public enum DamageType
    {
        Physical,
        Magical,
        Fire,
        Cold,
        Poison,
        Arcane
    }

    public class Weapon
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int BaseDamage { get; set; }
        public int AttackModifier { get; set; }
        public DamageType DamageType { get; set; } = DamageType.Physical;
        public bool IsRanged { get; set; } = false;
        public int Value { get; set; }

        public Weapon(string id, string name, int dmg, int atkMod, int value = 0,
            bool ranged = false, DamageType dmgType = DamageType.Physical)
        {
            Id = id;
            Name = name;
            BaseDamage = dmg;
            AttackModifier = atkMod;
            Value = value;
            IsRanged = ranged;
            DamageType = dmgType;
        }

        // Preset weapons
        public static readonly Weapon Dagger = new("dagger", "Dolch", 4, 1, 15);
        public static readonly Weapon ShortSword = new("short_sword", "Kurzschwert", 6, 1, 30);
        public static readonly Weapon LongSword = new("long_sword", "Langschwert", 8, 2, 60);
        public static readonly Weapon BattleAxe = new("battle_axe", "Kampfaxt", 10, 0, 80);
        public static readonly Weapon Staff = new("staff", "Kampfstab", 3, 1, 10);
        public static readonly Weapon ShortBow = new("short_bow", "Kurzbogen", 5, 2, 40, true);
        public static readonly Weapon LongBow = new("long_bow", "Langbogen", 7, 3, 70, true);
        public static readonly Weapon Crossbow = new("crossbow", "Armbrust", 9, 1, 100, true);

        public static readonly List<Weapon> All = new()
        {
            Dagger, ShortSword, LongSword, BattleAxe, Staff,
            ShortBow, LongBow, Crossbow
        };
    }

    public class Armor
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Soak { get; set; }
        public int DefenseModifier { get; set; }
        public int Value { get; set; }

        public Armor(string id, string name, int soak, int defMod, int value = 0)
        {
            Id = id;
            Name = name;
            Soak = soak;
            DefenseModifier = defMod;
            Value = value;
        }

        public static readonly Armor Cloth = new("cloth", "Stoffgewand", 0, 0, 5);
        public static readonly Armor Leather = new("leather", "Lederrüstung", 1, 1, 25);
        public static readonly Armor Chain = new("chain", "Kettenhemd", 2, 2, 60);
        public static readonly Armor Plate = new("plate", "Plattenrüstung", 4, 1, 150);

        public static readonly List<Armor> All = new()
        {
            Cloth, Leather, Chain, Plate
        };
    }

    public class Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Value { get; set; }
        public int StackSize { get; set; } = 1;
        public bool IsConsumable { get; set; } = false;
        public string Note { get; set; } = "";

        public Item(string id, string name, string desc, int value, bool consumable = false)
        {
            Id = id;
            Name = name;
            Description = desc;
            Value = value;
            IsConsumable = consumable;
        }

        // Consumables
        public static readonly Item HealthPotion = new("health_potion", "Heiltrank", "Stellt 15 Lebenspunkte wieder her", 20, true);
        public static readonly Item GreaterHealingPotion = new("greater_healing_potion", "Grosser Heiltrank", "Stellt 40 Lebenspunkte wieder her", 50, true);
        public static readonly Item ManaPotion = new("mana_potion", "Manatrank", "Stellt 10 Manapunkte wieder her", 15, true);
        public static readonly Item StaminaPotion = new("stamina_potion", "Ausdauertrank", "Stellt 15 Ausdauerpunkte wieder her", 12, true);
        public static readonly Item PoisonPotion = new("poison_potion", "Gifttrank", "Kann auf Waffen aufgetragen werden", 30, true);
        public static readonly Item Torch = new("torch", "Fackel", "Spendet Licht in dunklen Dungeons", 2);
        public static readonly Item Lockpick = new("lockpick", "Dietrich", "Werkzeug zum Oeffnen von Schloessern", 5);
        public static readonly Item Rope = new("rope", "Seil", "10 Schritt lang, nuetzlich zum Klettern", 3);

        // Crafting ingredients
        public static readonly Item Heilwurzel = new("heilwurzel", "Heilwurzel", "Heilende Wurzel, Zutat fuer Traenke", 5);
        public static readonly Item Manakristall = new("manakristall", "Manakristall", "Enthaelt magische Energie", 8);
        public static readonly Item Mohn = new("mohn", "Mohnblume", "Zutat fuer starke Traenke", 6);
        public static readonly Item Eisenbarren = new("eisenbarren", "Eisenbarren", "Roheisen fuer Handwerk", 10);
        public static readonly Item Giftbeutel = new("giftbeutel", "Giftbeutel", "Gift einer Kreatur", 7);

        // Food and water
        public static readonly Item Proviant = new("proviant", "Proviant", "Reisproviant, stillt Hunger", 3, true);
        public static readonly Item Fleisch = new("fleisch", "Geroechertes Fleisch", "Stillt Hunger", 4, true);
        public static readonly Item Brot = new("brot", "Brot", "Frisches Brot, stillt Hunger", 2, true);
        public static readonly Item Wasserflasche = new("wasserflasche", "Wasserflasche", "Stillt Durst", 2, true);
    }
}
