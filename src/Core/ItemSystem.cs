using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Core
{
    using Characters;

    /// <summary>
    /// Item identification, equipment durability, enchantments, and rarity system.
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum EnchantmentType
    {
        None,
        FireDamage,
        ColdDamage,
        PoisonDamage,
        LifeSteal,
        ManaRegen,
        ExtraArmor,
        ExtraDamage,
        Speed,
        Luck
    }

    public static class ItemSystem
    {
        // Item identification
        public static bool IsIdentified(Item item)
        {
            return !item.Id.StartsWith("unidentified_");
        }

        public static (bool success, string message) IdentifyItem(Character identifier, Item item)
        {
            if (IsIdentified(item))
                return (true, "Item ist bereits identifiziert.");

            var result = identifier.PerformSkillCheck("wissen", 12);
            if (result.Success)
            {
                // In a full implementation, this would map to the real item
                return (true, $"{identifier.Name} identifiziert das Item: {item.Name}.");
            }
            else
            {
                return (false, $"{identifier.Name} kann das Item nicht identifizieren.");
            }
        }

        // Equipment durability
        public static int GetDurability(Item item)
        {
            // Durability stored in item's Note field as "dur:N"
            if (string.IsNullOrEmpty(item.Note)) return 100;
            if (item.Note.StartsWith("dur:"))
                return int.Parse(item.Note.Substring(4));
            return 100;
        }

        public static void SetDurability(Item item, int durability)
        {
            item.Note = $"dur:{Mathf.Clamp(durability, 0, 100)}";
        }

        public static void ReduceDurability(Item item, int amount = 1)
        {
            int dur = GetDurability(item);
            SetDurability(item, dur - amount);
        }

        public static bool IsBroken(Item item) => GetDurability(item) <= 0;

        public static (bool success, string message) RepairItem(Character repairer, Item item, int cost)
        {
            if (GetDurability(item) >= 100)
                return (true, "Item ist bereits vollstaendig repariert.");

            var result = repairer.PerformSkillCheck("handwerk", 10);
            if (result.Success)
            {
                SetDurability(item, 100);
                return (true, $"{repairer.Name} repariert {item.Name} erfolgreich. (-{cost} Gold)");
            }
            else
            {
                return (false, $"{repairer.Name} scheitert bei der Reparatur von {item.Name}.");
            }
        }

        // Enchantments
        public static EnchantmentType GetEnchantment(Item item)
        {
            if (string.IsNullOrEmpty(item.Note)) return EnchantmentType.None;
            if (item.Note.StartsWith("ench:"))
            {
                if (System.Enum.TryParse<EnchantmentType>(item.Note.Substring(5), out var ench))
                    return ench;
            }
            return EnchantmentType.None;
        }

        public static Item Enchant(Item item, EnchantmentType enchantment)
        {
            item.Note = $"ench:{enchantment}";
            return item;
        }

        public static string GetEnchantmentName(EnchantmentType ench) => ench switch
        {
            EnchantmentType.FireDamage => "Feuerschaden",
            EnchantmentType.ColdDamage => "Frostschaden",
            EnchantmentType.PoisonDamage => "Giftschaden",
            EnchantmentType.LifeSteal => "Lebensraub",
            EnchantmentType.ManaRegen => "Manaregeneration",
            EnchantmentType.ExtraArmor => "Zusaetzliche Ruestung",
            EnchantmentType.ExtraDamage => "Zusaetzlicher Schaden",
            EnchantmentType.Speed => "Schnelligkeit",
            EnchantmentType.Luck => "Glueck",
            _ => "Keine"
        };

        public static int GetEnchantmentBonus(EnchantmentType ench) => ench switch
        {
            EnchantmentType.FireDamage => 2,
            EnchantmentType.ColdDamage => 2,
            EnchantmentType.PoisonDamage => 2,
            EnchantmentType.LifeSteal => 1,
            EnchantmentType.ManaRegen => 2,
            EnchantmentType.ExtraArmor => 3,
            EnchantmentType.ExtraDamage => 2,
            EnchantmentType.Speed => 1,
            EnchantmentType.Luck => 1,
            _ => 0
        };

        // Rarity
        public static ItemRarity GetRarity(Item item)
        {
            if (item.Value >= 500) return ItemRarity.Legendary;
            if (item.Value >= 200) return ItemRarity.Epic;
            if (item.Value >= 100) return ItemRarity.Rare;
            if (item.Value >= 50) return ItemRarity.Uncommon;
            return ItemRarity.Common;
        }

        public static Color GetRarityColor(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Common => new Color(0.8f, 0.8f, 0.8f),
            ItemRarity.Uncommon => new Color(0.3f, 0.9f, 0.3f),
            ItemRarity.Rare => new Color(0.3f, 0.5f, 1.0f),
            ItemRarity.Epic => new Color(0.7f, 0.3f, 0.9f),
            ItemRarity.Legendary => new Color(1.0f, 0.7f, 0.1f),
            _ => new Color(0.8f, 0.8f, 0.8f)
        };

        public static string GetRarityName(ItemRarity rarity) => rarity switch
        {
            ItemRarity.Common => "Gewoehnlich",
            ItemRarity.Uncommon => "Ungewoehnlich",
            ItemRarity.Rare => "Selten",
            ItemRarity.Epic => "Episch",
            ItemRarity.Legendary => "Legendär",
            _ => "Gewoehnlich"
        };

        // Equipment set bonuses
        public static string GetItemSet(Item item)
        {
            if (item.Id.StartsWith("wolf_")) return "wolf";
            if (item.Id.StartsWith("magier_")) return "magier";
            if (item.Id.StartsWith("schurke_")) return "schurke";
            if (item.Id.StartsWith("krieger_")) return "krieger";
            return null;
        }

        public static int CountSetPieces(Character character, string setId)
        {
            int count = 0;
            foreach (var item in character.Inventory)
            {
                if (GetItemSet(item) == setId)
                    count++;
            }
            return count;
        }

        public static string GetSetBonusName(string setId, int pieces) => (setId, pieces) switch
        {
            ("wolf", 2) => "+5% Ausweichen",
            ("wolf", 4) => "+10% Kritischer Treffer",
            ("magier", 2) => "+5 Mana",
            ("magier", 4) => "+15% Zauberschaden",
            ("schurke", 2) => "+5% Schleichen",
            ("schurke", 4) => "+1 Angriff pro Runde",
            ("krieger", 2) => "+5 Ruestung",
            ("krieger", 4) => "+10% Schaden",
            _ => null
        };
    }
}
