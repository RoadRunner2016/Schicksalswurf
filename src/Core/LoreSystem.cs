using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Core
{
    /// <summary>
    /// Lore entries and story system for narrative depth.
    /// </summary>
    public class LoreEntry
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Category { get; set; } // "world", "npc", "item", "location"
        public bool Discovered { get; set; } = false;
    }

    public static class LoreSystem
    {
        private static Dictionary<string, LoreEntry> _entries = new();

        public static IReadOnlyDictionary<string, LoreEntry> Entries => _entries;
        public static List<LoreEntry> DiscoveredEntries
        {
            get
            {
                var result = new List<LoreEntry>();
                foreach (var e in _entries.Values)
                    if (e.Discovered) result.Add(e);
                return result;
            }
        }

        public static void Initialize()
        {
            _entries.Clear();

            Add("world_intro", "Das Schicksalswurf",
                "Vor Jahrhunderten versank die alte Zivilisation in Dunkelheit. Nur die Tapfersten wagen sich in die tiefen Dungeons, auf der Suche nach Reichtum und dem Schicksalswurf - einem Artefakt, das das Schicksal der Welt besiegeln soll.",
                "world");

            Add("world_dungeon", "Die Dungeons",
                "Die Dungeons erstrecken sich ueber zehn Ebenen unter der Erde. Jede Ebene ist gefaehrlicher als die vorherige. Auf Ebene 3 haust der Dungeonwaechter, und in den tiefsten Schachten wartet der Drache.",
                "world");

            Add("world_town", "Die Stadt",
                "Die Stadt am Fusse des Dungeons ist der letzte sichere Hafen. Hier koennen Abenteurer rasten, handelteln und sich heilen lassen, bevor sie sich erneut in die Tiefe wagen.",
                "world");

            Add("npc_merchant", "Der wandernde Haendler",
                "Einige Haendler wagen sich in die oberen Ebenen des Dungeons, um ihre Waren anzubieten. Sie verkaufen Traenke, Dietriche und Ausruestung zu hoeheren Preisen als in der Stadt.",
                "npc");

            Add("npc_healer", "Die Heilerin",
                "Heilerinnen der alten Ordnung patrouillieren die Dungeons, um erschöpfte Abenteurer gegen Gold zu heilen. Ihre Kraefte sind begrenzt, aber ihr Preis ist fair.",
                "npc");

            Add("npc_questgiver", "Der Questgeber",
                "Bewohner der Stadt suchen hauefig nach tapferen Abenteurern, die ihre Auftraege erfuellen. Ob es um das Toeten von Monstern oder das Erreichen tiefer Ebenen geht - die Belohnungen sind oft wertvoll.",
                "npc");

            Add("item_heilwurzel", "Heilwurzel",
                "Die Heilwurzel waechst in den feuchten Gängen der oberen Dungeon-Ebenen. Alchimisten koennen daraus Heiltraenke brauen.",
                "item");

            Add("item_manakristall", "Manakristall",
                "Manakristalle sind Ueberreste der alten Zivilisation. Sie enthalten magische Energie, die fuer das Brauen von Manatraenken verwendet wird.",
                "item");

            Add("item_schicksalswurf", "Der Schicksalswurf",
                "Das legendaere Artefakt, das das Schicksal der Welt besiegeln soll. Es soll in den tiefsten Schachten des Dungeons liegen, bewacht von einem Daemonenfuersten.",
                "item");

            Add("location_ebene1", "Ebene 1 - Die Katakomben",
                "Die oberste Ebene besteht aus alten Katakomben. Goblins und Schleime patrouillieren die gaenge.",
                "location");

            Add("location_ebene3", "Ebene 3 - Die Verliese",
                "Die Verliese sind die Heimat des Dungeonwaechters, eines gewaltigen Konstrukts aus Stein und Magie.",
                "location");

            Add("location_ebene5", "Ebene 5 - Die Hoehle",
                "Eine riesige natuerliche Hoehle, in der Orks und Riesenspinnen ihr Unwesen treiben.",
                "location");

            Add("location_ebene10", "Ebene 10 - Die Unterwelt",
                "Die tiefste Ebene ist ein Vorposten der Hoelle. Daemonen und der Daemonenfuerst herrschen hier. Der Schicksalswurf soll hier verborgen sein.",
                "location");

            Add("enemy_dragon", "Der Drache",
                "Ein uralter roter Drache, der in den tiefen Hoehlen haust. Sein Odem verbrennt alles, sein Schatz ist legendaer.",
                "npc");

            Add("enemy_lich", "Der Lichkoenig",
                "Ein untoter Magier von immenser Macht. Er war einst der Erzmagier der alten Zivilisation, bevor er dem Wahnsinn verfiel.",
                "npc");
        }

        private static void Add(string id, string title, string text, string category)
        {
            _entries[id] = new LoreEntry { Id = id, Title = title, Text = text, Category = category };
        }

        public static void Discover(string id)
        {
            if (_entries.TryGetValue(id, out var entry))
                entry.Discovered = true;
        }

        public static void DiscoverByLocation(int dungeonLevel)
        {
            switch (dungeonLevel)
            {
                case 1: Discover("location_ebene1"); Discover("world_dungeon"); break;
                case 3: Discover("location_ebene3"); break;
                case 5: Discover("location_ebene5"); break;
                case 10: Discover("location_ebene10"); Discover("item_schicksalswurf"); break;
            }
        }
    }
}
