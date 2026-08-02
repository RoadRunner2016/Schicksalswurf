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
        // Extended weapons
        public static readonly Weapon WarHammer = new("war_hammer", "Kriegshammer", 12, 1, 120);
        public static readonly Weapon Halberd = new("halberd", "Hellebarde", 11, 2, 110);
        public static readonly Weapon MagicSword = new("magic_sword", "Verzaubertes Schwert", 10, 3, 200, false, DamageType.Arcane);
        public static readonly Weapon FireStaff = new("fire_staff", "Feuerstab", 6, 2, 150, false, DamageType.Fire);
        public static readonly Weapon IceDagger = new("ice_dagger", "Eisdolch", 5, 2, 90, false, DamageType.Cold);
        public static readonly Weapon PoisonSword = new("poison_sword", "Giftklinge", 7, 2, 130, false, DamageType.Poison);
        public static readonly Weapon DragonSlayer = new("dragon_slayer", "Drachenschlacht", 15, 4, 500, false, DamageType.Physical);
        public static readonly Weapon DemonBow = new("demon_bow", "Dämonenbogen", 12, 4, 350, true, DamageType.Fire);

        public static readonly List<Weapon> All = new()
        {
            Dagger, ShortSword, LongSword, BattleAxe, Staff,
            ShortBow, LongBow, Crossbow,
            WarHammer, Halberd, MagicSword, FireStaff, IceDagger, PoisonSword, DragonSlayer, DemonBow
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
        // Extended armor
        public static readonly Armor DragonScale = new("dragon_scale", "Drachenschuppenrüstung", 6, 3, 400);
        public static readonly Armor MageRobe = new("mage_robe", "Magiergewand", 1, 3, 80);
        public static readonly Armor ShadowCloak = new("shadow_cloak", "Schattenumhang", 2, 4, 200);
        public static readonly Armor DemonArmor = new("demon_armor", "Dämonenrüstung", 7, 2, 500);

        public static readonly List<Armor> All = new()
        {
            Cloth, Leather, Chain, Plate, DragonScale, MageRobe, ShadowCloak, DemonArmor
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
        // Extended consumables
        public static readonly Item ElixirOfLife = new("elixir_of_life", "Elixier des Lebens", "Stellt 80 HP wieder her", 120, true);
        public static readonly Item ManaElixir = new("mana_elixir", "Manaelixir", "Stellt 40 MP wieder her", 80, true);
        public static readonly Item Antidote = new("antidote", "Gegengift", "Heilt Vergiftung", 25, true);
        public static readonly Item Bomb = new("bomb", "Sprengsatz", "Verursacht 20 Schaden an allen Gegnern", 50, true);
        public static readonly Item ScrollOfEscape = new("scroll_escape", "Schriftrolle der Flucht", "Teleportiert aus dem Dungeon", 100, true);
        // Extended crafting ingredients
        public static readonly Item Drachenschuppe = new("drachenschuppe", "Drachenschuppe", "Seltenes Material", 50);
        public static readonly Item Dämonenblut = new("dämonenblut", "Dämonenblut", "Mächtige Zutat für dunkle Tränke", 40);
        public static readonly Item Goldbarren = new("goldbarren", "Goldbarren", "Wertvolles Metall", 100);
        public static readonly Item Edelstein = new("edelstein", "Edelstein", "Wertvoller Stein", 75);
        // Treasure
        public static readonly Item Silbermünze = new("silbermünze", "Silbermünze", "Kleine Münze", 1);
        public static readonly Item Goldmünze = new("goldmünze", "Goldmünze", "Münze aus reinem Gold", 10);
        public static readonly Item Juwel = new("juwel", "Juwel", "Funkelnder Edelstein", 200);
    }
}
