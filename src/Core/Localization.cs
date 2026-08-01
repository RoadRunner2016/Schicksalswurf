using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Core
{
    /// <summary>
    /// Simple localization system supporting German and English.
    /// </summary>
    public static class Localization
    {
        public enum Language
        {
            German,
            English
        }

        public static Language CurrentLanguage { get; set; } = Language.German;

        private static Dictionary<string, string> _german = new();
        private static Dictionary<string, string> _english = new();

        static Localization()
        {
            // UI strings
            _german["new_game"] = "Neues Spiel";
            _english["new_game"] = "New Game";

            _german["load_game"] = "Spiel laden";
            _english["load_game"] = "Load Game";

            _german["quit"] = "Beenden";
            _english["quit"] = "Quit";

            _german["settings"] = "Einstellungen";
            _english["settings"] = "Settings";

            _german["inventory"] = "Inventar";
            _english["inventory"] = "Inventory";

            _german["character"] = "Charakter";
            _english["character"] = "Character";

            _german["help"] = "Hilfe";
            _english["help"] = "Help";

            _german["save"] = "Speichern";
            _english["save"] = "Save";

            _german["load"] = "Laden";
            _english["load"] = "Load";

            _german["rest"] = "Rasten";
            _english["rest"] = "Rest";

            _german["town"] = "Stadt";
            _english["town"] = "Town";

            _german["enter_dungeon"] = "Dungeon betreten";
            _english["enter_dungeon"] = "Enter Dungeon";

            _german["combat"] = "Kampf";
            _english["combat"] = "Combat";

            _german["attack"] = "Angriff";
            _english["attack"] = "Attack";

            _german["defend"] = "Verteidigen";
            _english["defend"] = "Defend";

            _german["cast_spell"] = "Zauber wirken";
            _english["cast_spell"] = "Cast Spell";

            _german["flee"] = "Fliehen";
            _english["flee"] = "Flee";

            _german["use_item"] = "Item verwenden";
            _english["use_item"] = "Use Item";

            _german["game_over"] = "Game Over";
            _english["game_over"] = "Game Over";

            _german["victory"] = "Sieg!";
            _english["victory"] = "Victory!";

            _german["level"] = "Ebene";
            _english["level"] = "Level";

            _german["gold"] = "Gold";
            _english["gold"] = "Gold";

            _german["health"] = "Leben";
            _english["health"] = "Health";

            _german["mana"] = "Mana";
            _english["mana"] = "Mana";

            _german["stamina"] = "Ausdauer";
            _english["stamina"] = "Stamina";

            _german["experience"] = "Erfahrung";
            _english["experience"] = "Experience";

            _german["quests"] = "Quests";
            _english["quests"] = "Quests";

            _german["achievements"] = "Erfolge";
            _english["achievements"] = "Achievements";

            _german["crafting"] = "Handwerk";
            _english["crafting"] = "Crafting";

            _german["close"] = "Schliessen";
            _english["close"] = "Close";

            // Combat messages
            _german["hit"] = "trifft";
            _english["hit"] = "hits";

            _german["miss"] = "verfehlt";
            _english["miss"] = "misses";

            _german["critical"] = "kritisch";
            _english["critical"] = "critical";

            _german["damage"] = "Schaden";
            _english["damage"] = "damage";
        }

        public static string Get(string key)
        {
            var dict = CurrentLanguage == Language.English ? _english : _german;
            return dict.TryGetValue(key, out var value) ? value : key;
        }

        public static void SetLanguage(Language lang)
        {
            CurrentLanguage = lang;
        }

        public static void ToggleLanguage()
        {
            CurrentLanguage = CurrentLanguage == Language.German ? Language.English : Language.German;
        }
    }
}
