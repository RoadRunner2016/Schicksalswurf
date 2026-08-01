namespace Schicksalswurf.Core
{
    /// <summary>
    /// Combat-related derived values calculated from attributes.
    /// </summary>
    public class CombatStats
    {
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public int MaxStamina { get; set; }
        public int CurrentStamina { get; set; }
        public int MaxMana { get; set; }
        public int CurrentMana { get; set; }

        // Initiative = Gewandtheit + Wahrnehmung / 2 (rounded up)
        public int Initiative { get; set; }

        // Attack base = Kraft + Gewandtheit / 2
        public int AttackBase { get; set; }

        // Defense base = Gewandtheit + Wahrnehmung / 2
        public int DefenseBase { get; set; }

        // Ranged base = Wahrnehmung + Gewandtheit / 2
        public int RangedBase { get; set; }

        // Soak = Konstitution / 3 (rounded down)
        public int Soak { get; set; }

        // Carry capacity = Kraft * 2
        public int CarryCapacity { get; set; }

        public static CombatStats Calculate(AttributeSet attrs, int level = 1)
        {
            int maxHealth = attrs[Attribute.Konstitution] * 4 + 10 + level * 2;
            int maxStamina = attrs[Attribute.Konstitution] * 2 + attrs[Attribute.Kraft] + level;
            int maxMana = attrs[Attribute.Intelligenz] * 2 + attrs[Attribute.Willenskraft] + level;

            return new CombatStats
            {
                MaxHealth = maxHealth,
                CurrentHealth = maxHealth,
                MaxStamina = maxStamina,
                CurrentStamina = maxStamina,
                MaxMana = maxMana,
                CurrentMana = maxMana,
                Initiative = (attrs[Attribute.Gewandtheit] + attrs[Attribute.Wahrnehmung] + 1) / 2,
                AttackBase = (attrs[Attribute.Kraft] + attrs[Attribute.Gewandtheit] + 1) / 2,
                DefenseBase = (attrs[Attribute.Gewandtheit] + attrs[Attribute.Wahrnehmung] + 1) / 2,
                RangedBase = (attrs[Attribute.Wahrnehmung] + attrs[Attribute.Gewandtheit] + 1) / 2,
                Soak = attrs[Attribute.Konstitution] / 3,
                CarryCapacity = attrs[Attribute.Kraft] * 2
            };
        }

        public bool IsAlive => CurrentHealth > 0;
        public bool IsWounded => CurrentHealth < MaxHealth / 2;
        public bool IsCritical => CurrentHealth < MaxHealth / 4;
    }
}
