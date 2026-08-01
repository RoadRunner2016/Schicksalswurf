using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Core
{
    using Characters;

    /// <summary>
    /// Perishable item system: food and some potions decay over time.
    /// </summary>
    public static class PerishableSystem
    {
        private class PerishableEntry
        {
            public Item Item { get; set; }
            public int TurnsRemaining { get; set; }
            public int MaxTurns { get; set; }
        }

        private static List<PerishableEntry> _trackedItems = new();
        private static int _globalTurnCounter = 0;

        public static bool IsPerishable(Item item)
        {
            return item.Id == "fleisch" || item.Id == "brot" || item.Id == "proviant";
        }

        public static int GetShelfLife(Item item) => item.Id switch
        {
            "fleisch" => 50,    // 50 steps
            "brot" => 80,
            "proviant" => 100,
            _ => -1 // non-perishable
        };

        public static void TrackItem(Item item)
        {
            if (!IsPerishable(item)) return;
            int shelfLife = GetShelfLife(item);
            if (shelfLife > 0)
                _trackedItems.Add(new PerishableEntry { Item = item, TurnsRemaining = shelfLife, MaxTurns = shelfLife });
        }

        public static void OnStep()
        {
            _globalTurnCounter++;
            var expired = new List<PerishableEntry>();
            foreach (var entry in _trackedItems)
            {
                entry.TurnsRemaining--;
                if (entry.TurnsRemaining <= 0)
                    expired.Add(entry);
            }
            foreach (var entry in expired)
            {
                _trackedItems.Remove(entry);
                // Mark item as spoiled by changing its name
                entry.Item.Name = "Verdorben: " + entry.Item.Name;
                entry.Item.Description = "Dieses Item ist verdorben und sollte nicht mehr verwendet werden.";
                entry.Item.Value = 0;
            }
        }

        public static bool IsSpoiled(Item item)
        {
            return item.Name.StartsWith("Verdorben:");
        }

        public static string GetFreshnessLabel(Item item)
        {
            if (!IsPerishable(item)) return "";
            if (IsSpoiled(item)) return "[VERDORBEN]";
            var entry = _trackedItems.Find(e => e.Item == item);
            if (entry == null) return "";
            float pct = (float)entry.TurnsRemaining / entry.MaxTurns;
            if (pct > 0.7f) return "[Frisch]";
            if (pct > 0.3f) return "[Alt]";
            return "[Bald verdorben]";
        }
    }
}
