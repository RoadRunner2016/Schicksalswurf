using System.Collections.Generic;

namespace Schicksalswurf.Core
{
    /// <summary>
    /// Skill categories for organizing talents.
    /// </summary>
    public enum SkillCategory
    {
        Koerperlich,    // Physical skills
        Gesellschaftlich, // Social skills
        Natur,          // Nature skills
        Wissen,         // Knowledge skills
        Handwerk        // Crafting skills
    }

    /// <summary>
    /// A skill/talent definition. Each skill is checked against three attributes.
    /// The skill value represents how trained the character is (0 = untrained).
    /// </summary>
    public class Skill
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SkillCategory Category { get; set; }
        public Attribute CheckAttr1 { get; set; }
        public Attribute CheckAttr2 { get; set; }
        public Attribute CheckAttr3 { get; set; }
        public int DefaultValue { get; set; } = 0;

        public Skill(string id, string name, string description, SkillCategory category,
            Attribute a1, Attribute a2, Attribute a3)
        {
            Id = id;
            Name = name;
            Description = description;
            Category = category;
            CheckAttr1 = a1;
            CheckAttr2 = a2;
            CheckAttr3 = a3;
        }
    }

    /// <summary>
    /// Registry of all skills available in the game.
    /// Add new skills here.
    /// </summary>
    public static class SkillRegistry
    {
        private static readonly Dictionary<string, Skill> _skills = new();

        public static IReadOnlyDictionary<string, Skill> All => _skills;

        static SkillRegistry()
        {
            // Physical skills
            Register("klettern", "Klettern", "Erklimmen von Wänden, Felsen und Klippen",
                SkillCategory.Koerperlich, Attribute.Kraft, Attribute.Gewandtheit, Attribute.Konstitution);
            Register("schleichen", "Schleichen", "Sich lautlos bewegen und verbergen",
                SkillCategory.Koerperlich, Attribute.Gewandtheit, Attribute.Gewandtheit, Attribute.Wahrnehmung);
            Register("schwimmen", "Schwimmen", "Fortbewegung im Wasser und Tauchen",
                SkillCategory.Koerperlich, Attribute.Konstitution, Attribute.Gewandtheit, Attribute.Konstitution);
            Register("akrobatik", "Akrobatik", "Sprünge, Balance und Körperkontrolle",
                SkillCategory.Koerperlich, Attribute.Gewandtheit, Attribute.Gewandtheit, Attribute.Konstitution);
            Register("koerperbeherrschung", "Körperbeherrschung", "Schmerzresistenz und körperliche Disziplin",
                SkillCategory.Koerperlich, Attribute.Konstitution, Attribute.Willenskraft, Attribute.Konstitution);

            // Social skills
            Register("ueberreden", "Überreden", "Jemanden mit Argumenten umstimmen",
                SkillCategory.Gesellschaftlich, Attribute.Charisma, Attribute.Charisma, Attribute.Intelligenz);
            Register("menschenkenntnis", "Menschenkenntnis", "Absichten und Gemütszustand anderer erkennen",
                SkillCategory.Gesellschaftlich, Attribute.Wahrnehmung, Attribute.Charisma, Attribute.Intelligenz);
            Register("etikette", "Etikette", "Verhalten in gesellschaftlichen Situationen",
                SkillCategory.Gesellschaftlich, Attribute.Charisma, Attribute.Intelligenz, Attribute.Willenskraft);
            Register("handel", "Handel", "Feilschen und Preisverhandlungen",
                SkillCategory.Gesellschaftlich, Attribute.Charisma, Attribute.Intelligenz, Attribute.Charisma);

            // Nature skills
            Register("faehrtensuche", "Fährtensuche", "Spuren lesen und verfolgen",
                SkillCategory.Natur, Attribute.Wahrnehmung, Attribute.Wahrnehmung, Attribute.Intelligenz);
            Register("wildnisleben", "Wildnisleben", "Überleben in der Natur, Lagerbau",
                SkillCategory.Natur, Attribute.Konstitution, Attribute.Wahrnehmung, Attribute.Willenskraft);
            Register("pflanzenkunde", "Pflanzenkunde", "Kräuter und Pflanzen bestimmen",
                SkillCategory.Natur, Attribute.Intelligenz, Attribute.Wahrnehmung, Attribute.Intelligenz);
            Register("tierkunde", "Tierkunde", "Verhalten und Eigenschaften von Tieren",
                SkillCategory.Natur, Attribute.Intelligenz, Attribute.Wahrnehmung, Attribute.Charisma);

            // Knowledge skills
            Register("magiekunde", "Magiekunde", "Wissen über Zauber und magische Phänomene",
                SkillCategory.Wissen, Attribute.Intelligenz, Attribute.Intelligenz, Attribute.Willenskraft);
            Register("alchimie", "Alchimie", "Tränke brauen und Substanzen analysieren",
                SkillCategory.Wissen, Attribute.Intelligenz, Attribute.Intelligenz, Attribute.Wahrnehmung);
            Register("geschichte", "Geschichte", "Wissen über vergangene Epochen und Ereignisse",
                SkillCategory.Wissen, Attribute.Intelligenz, Attribute.Intelligenz, Attribute.Wahrnehmung);
            Register("kriegskunst", "Kriegskunst", "Taktik und militärische Strategie",
                SkillCategory.Wissen, Attribute.Intelligenz, Attribute.Wahrnehmung, Attribute.Willenskraft);

            // Crafting skills
            Register("schmieden", "Schmieden", "Waffen und Rüstungen herstellen",
                SkillCategory.Handwerk, Attribute.Kraft, Attribute.Wahrnehmung, Attribute.Intelligenz);
            Register("heilkunde", "Heilkunde", "Wunden behandeln und Krankheiten heilen",
                SkillCategory.Handwerk, Attribute.Intelligenz, Attribute.Wahrnehmung, Attribute.Konstitution);
            Register("kochen", "Kochen", "Mahlzeiten zubereiten und Zutaten veredeln",
                SkillCategory.Handwerk, Attribute.Wahrnehmung, Attribute.Konstitution, Attribute.Charisma);
            Register("holzbearbeitung", "Holzbearbeitung", "Gegenstände aus Holz fertigen",
                SkillCategory.Handwerk, Attribute.Kraft, Attribute.Gewandtheit, Attribute.Wahrnehmung);
        }

        private static void Register(string id, string name, string desc,
            SkillCategory cat, Attribute a1, Attribute a2, Attribute a3)
        {
            _skills[id] = new Skill(id, name, desc, cat, a1, a2, a3);
        }

        public static Skill Get(string id) =>
            _skills.TryGetValue(id, out var skill) ? skill : null;

        public static IEnumerable<Skill> GetByCategory(SkillCategory cat)
        {
            foreach (var s in _skills.Values)
                if (s.Category == cat)
                    yield return s;
        }
    }

    /// <summary>
    /// A character's skill values, keyed by skill ID.
    /// </summary>
    public class SkillSet
    {
        private readonly Dictionary<string, int> _values = new();

        public int this[string skillId]
        {
            get => _values.GetValueOrDefault(skillId, 0);
            set => _values[skillId] = System.Math.Clamp(value, 0, 25);
        }

        public int Get(string skillId) => this[skillId];
        public void Set(string skillId, int value) => this[skillId] = value;

        public bool HasSkill(string skillId) => _values.ContainsKey(skillId) && _values[skillId] > 0;
    }
}
