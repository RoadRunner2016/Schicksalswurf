using System;
using System.Collections.Generic;

namespace Schicksalswurf.Core
{
    /// <summary>
    /// The seven core attributes of a character. Inspired by but distinct from
    /// any existing RPG system.
    /// </summary>
    public enum Attribute
    {
        Kraft,       // Physical strength, carrying capacity, melee damage
        Gewandtheit, // Agility, reflexes, dodging
        Konstitution,// Health, stamina, resistance
        Intelligenz, // Logic, memory, spellcasting
        Willenskraft,// Mental strength, resistance to fear/magic
        Wahrnehmung, // Senses, awareness, aiming
        Charisma     // Social interaction, leadership, animal handling
    }

    /// <summary>
    /// Maps attributes to their German short names and descriptions.
    /// </summary>
    public static class AttributeInfo
    {
        public static readonly Dictionary<Attribute, string> ShortNames = new()
        {
            { Attribute.Kraft, "KR" },
            { Attribute.Gewandtheit, "GE" },
            { Attribute.Konstitution, "KO" },
            { Attribute.Intelligenz, "IN" },
            { Attribute.Willenskraft, "WI" },
            { Attribute.Wahrnehmung, "WA" },
            { Attribute.Charisma, "CH" }
        };

        public static readonly Dictionary<Attribute, string> Descriptions = new()
        {
            { Attribute.Kraft, "Körperliche Stärke und rohe Muskelkraft" },
            { Attribute.Gewandtheit, "Beweglichkeit, Reflexe und Geschick" },
            { Attribute.Konstitution, "Widerstandsfähigkeit, Gesundheit und Ausdauer" },
            { Attribute.Intelligenz, "Verstand, Logik und Erinnerungsvermögen" },
            { Attribute.Willenskraft, "Mentale Stärke und Selbstbeherrschung" },
            { Attribute.Wahrnehmung, "Sinnesschärfe und Aufmerksamkeit" },
            { Attribute.Charisma, "Ausstrahlung, Überzeugungskraft und Empathie" }
        };

        public static string GetName(Attribute attr) => attr.ToString();
        public static string GetShort(Attribute attr) => ShortNames[attr];
        public static string GetDescription(Attribute attr) => Descriptions[attr];
    }

    /// <summary>
    /// Attribute values for a character. Values typically range from 5 to 20,
    /// with 8 being below average, 13 average, and 18 exceptional.
    /// </summary>
    public class AttributeSet
    {
        private readonly Dictionary<Attribute, int> _values = new();

        public int this[Attribute attr]
        {
            get => _values.GetValueOrDefault(attr, 8);
            set => _values[attr] = Math.Clamp(value, 1, 25);
        }

        public AttributeSet() : this(8, 8, 8, 8, 8, 8, 8) { }

        public AttributeSet(int kraft, int gewandtheit, int konstitution,
            int intelligenz, int willenskraft, int wahrnehmung, int charisma)
        {
            _values[Attribute.Kraft] = kraft;
            _values[Attribute.Gewandtheit] = gewandtheit;
            _values[Attribute.Konstitution] = konstitution;
            _values[Attribute.Intelligenz] = intelligenz;
            _values[Attribute.Willenskraft] = willenskraft;
            _values[Attribute.Wahrnehmung] = wahrnehmung;
            _values[Attribute.Charisma] = charisma;
        }

        public int Get(Attribute attr) => this[attr];
        public void Set(Attribute attr, int value) => this[attr] = value;

        public int Sum()
        {
            int sum = 0;
            foreach (var v in _values.Values)
                sum += v;
            return sum;
        }
    }
}
