using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Core
{
    using Characters;

    public enum SpellEffect
    {
        Damage,
        Heal,
        Buff,
        Debuff,
        Shield
    }

    public enum SpellTarget
    {
        Enemy,
        Ally,
        Self,
        AllEnemies,
        AllAllies
    }

    /// <summary>
    /// A spell definition with cost, effect, and check attributes.
    /// </summary>
    public class Spell
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ManaCost { get; set; }
        public int MinLevel { get; set; }
        public SpellEffect Effect { get; set; }
        public SpellTarget TargetType { get; set; }
        public DamageType DamageType { get; set; }
        public int BasePower { get; set; }
        public Attribute CheckAttr1 { get; set; }
        public Attribute CheckAttr2 { get; set; }
        public Attribute CheckAttr3 { get; set; }
        public int SkillValue { get; set; }

        // Buff/debuff parameters
        public int BuffAmount { get; set; }
        public int BuffDuration { get; set; }

        public Spell(string id, string name, string desc, int manaCost, int minLevel,
            SpellEffect effect, SpellTarget target, DamageType dmgType, int power,
            Attribute a1, Attribute a2, Attribute a3)
        {
            Id = id;
            Name = name;
            Description = desc;
            ManaCost = manaCost;
            MinLevel = minLevel;
            Effect = effect;
            TargetType = target;
            DamageType = dmgType;
            BasePower = power;
            CheckAttr1 = a1;
            CheckAttr2 = a2;
            CheckAttr3 = a3;
        }
    }

    /// <summary>
    /// Registry of all available spells.
    /// </summary>
    public static class SpellRegistry
    {
        private static readonly Dictionary<string, Spell> _spells = new();
        public static IReadOnlyDictionary<string, Spell> All => _spells;

        static SpellRegistry()
        {
            // Offensive spells
            Register("feuerball", "Feuerball", "Schleudert eine Kugel aus Feuer auf den Gegner.",
                8, 1, SpellEffect.Damage, SpellTarget.Enemy, DamageType.Fire, 12,
                Attribute.Intelligenz, Attribute.Willenskraft, Attribute.Wahrnehmung);

            Register("eislanze", "Eislanze", "Eine Lanze aus Eis durchbohrt den Gegner.",
                10, 2, SpellEffect.Damage, SpellTarget.Enemy, DamageType.Cold, 16,
                Attribute.Intelligenz, Attribute.Willenskraft, Attribute.Gewandtheit);

            Register("arkaner_blitz", "Arkaner Blitz", "Ein Blitz aus arkaner Energie trifft den Gegner.",
                6, 1, SpellEffect.Damage, SpellTarget.Enemy, DamageType.Arcane, 8,
                Attribute.Intelligenz, Attribute.Intelligenz, Attribute.Willenskraft);

            Register("giftwolke", "Giftwolke", "Eine Wolke aus Gift schadet allen Gegnern.",
                15, 3, SpellEffect.Damage, SpellTarget.AllEnemies, DamageType.Poison, 10,
                Attribute.Intelligenz, Attribute.Willenskraft, Attribute.Konstitution);

            Register("saeurestrahl", "Saeurestrahl", "Ein Strahl aus Saeure aetzt den Gegner.",
                12, 3, SpellEffect.Damage, SpellTarget.Enemy, DamageType.Poison, 18,
                Attribute.Intelligenz, Attribute.Wahrnehmung, Attribute.Willenskraft);

            // Healing spells
            Register("heilung", "Heilung", "Stellt Lebenspunkte eines Verbündeten wieder her.",
                8, 1, SpellEffect.Heal, SpellTarget.Ally, DamageType.Magical, 15,
                Attribute.Willenskraft, Attribute.Intelligenz, Attribute.Charisma);

            Register("groesse_heilung", "Grosse Heilung", "Stellt viele Lebenspunkte wieder her.",
                16, 3, SpellEffect.Heal, SpellTarget.Ally, DamageType.Magical, 30,
                Attribute.Willenskraft, Attribute.Willenskraft, Attribute.Charisma);

            Register("gruppe_heilen", "Gruppenheilung", "Heilt alle Verbündeten.",
                20, 4, SpellEffect.Heal, SpellTarget.AllAllies, DamageType.Magical, 12,
                Attribute.Willenskraft, Attribute.Charisma, Attribute.Willenskraft);

            // Buffs
            Register("kraft_boost", "Staerkung", "Erhoeht die Kraft eines Verbündeten.",
                6, 2, SpellEffect.Buff, SpellTarget.Ally, DamageType.Magical, 0,
                Attribute.Willenskraft, Attribute.Intelligenz, Attribute.Charisma,
                buffAmount: 3, buffDuration: 3);

            Register("schild", "Magischer Schild", "Erhoeht die Verteidigung eines Verbündeten.",
                8, 2, SpellEffect.Shield, SpellTarget.Ally, DamageType.Magical, 0,
                Attribute.Willenskraft, Attribute.Intelligenz, Attribute.Wahrnehmung,
                buffAmount: 4, buffDuration: 3);

            // Debuff
            Register("schwaeche", "Schwaechung", "Verringert die Angriffskraft des Gegners.",
                7, 2, SpellEffect.Debuff, SpellTarget.Enemy, DamageType.Magical, 0,
                Attribute.Willenskraft, Attribute.Intelligenz, Attribute.Charisma,
                buffAmount: -3, buffDuration: 3);

            // Extended spells
            Register("blizzard", "Blizzard", "Ein gewaltiger Eissturm trifft alle Gegner.",
                25, 5, SpellEffect.Damage, SpellTarget.AllEnemies, DamageType.Cold, 20,
                Attribute.Intelligenz, Attribute.Willenskraft, Attribute.Konstitution);

            Register("meteor", "Meteor", "Ein brennender Meteor schlaegt auf alle Gegner ein.",
                30, 6, SpellEffect.Damage, SpellTarget.AllEnemies, DamageType.Fire, 25,
                Attribute.Intelligenz, Attribute.Willenskraft, Attribute.Konstitution);

            Register("lebensraub", "Lebensraub", "Entzieht dem Gegner Leben und heilt den Zauberer.",
                12, 3, SpellEffect.Damage, SpellTarget.Enemy, DamageType.Magical, 14,
                Attribute.Willenskraft, Attribute.Intelligenz, Attribute.Charisma);

            Register("manaschild", "Manaschild", "Schuetzt vor Schaden auf Kosten von Mana.",
                10, 3, SpellEffect.Shield, SpellTarget.Self, DamageType.Magical, 0,
                Attribute.Willenskraft, Attribute.Intelligenz, Attribute.Willenskraft,
                buffAmount: 6, buffDuration: 4);

            Register("fluch", "Verfluchen", "Verringert Verteidigung und Schaden des Gegners.",
                14, 4, SpellEffect.Debuff, SpellTarget.Enemy, DamageType.Magical, 0,
                Attribute.Willenskraft, Attribute.Intelligenz, Attribute.Charisma,
                buffAmount: -5, buffDuration: 4);

            Register("segen", "Segen", "Erhoeht alle Werte eines Verbündeten.",
                18, 5, SpellEffect.Buff, SpellTarget.Ally, DamageType.Magical, 0,
                Attribute.Willenskraft, Attribute.Charisma, Attribute.Willenskraft,
                buffAmount: 4, buffDuration: 4);

            Register("kettenblitz", "Kettenblitz", "Ein Blitz springt zwischen Gegnern hin und her.",
                20, 5, SpellEffect.Damage, SpellTarget.AllEnemies, DamageType.Arcane, 15,
                Attribute.Intelligenz, Attribute.Willenskraft, Attribute.Wahrnehmung);

            Register("steinkinetic", "Steinwand", "Beschwoert eine Wand aus Stein zum Schutz.",
                12, 3, SpellEffect.Shield, SpellTarget.Ally, DamageType.Magical, 0,
                Attribute.Willenskraft, Attribute.Konstitution, Attribute.Wahrnehmung,
                buffAmount: 5, buffDuration: 3);

            Register("wiederbeleben", "Wiederbeleben", "Weckt einen gefallenen Verbündeten wieder auf.",
                30, 6, SpellEffect.Heal, SpellTarget.Ally, DamageType.Magical, 50,
                Attribute.Willenskraft, Attribute.Willenskraft, Attribute.Charisma);

            // Extended high-level spells
            Register("feuersturm", "Feuersturm", "Ein Sturm aus Feuer verwuestet alle Gegner.",
                35, 7, SpellEffect.Damage, SpellTarget.AllEnemies, DamageType.Fire, 30,
                Attribute.Intelligenz, Attribute.Willenskraft, Attribute.Wahrnehmung);

            Register("eisgefängnis", "Eisgefängnis", "Einschliessung eines Gegners in Eis.",
                18, 5, SpellEffect.Debuff, SpellTarget.Enemy, DamageType.Cold, 10,
                Attribute.Intelligenz, Attribute.Willenskraft, Attribute.Wahrnehmung,
                buffAmount: 3, buffDuration: 3);

            Register("heilige_aura", "Heilige Aura", "Heilt die gesamte Gruppe leicht.",
                25, 6, SpellEffect.Heal, SpellTarget.AllAllies, DamageType.Magical, 20,
                Attribute.Willenskraft, Attribute.Charisma, Attribute.Wahrnehmung);

            Register("schattenklinge", "Schattenklinge", "Verdunkelt die Waffe eines Verbündeten mit Schatten.",
                15, 4, SpellEffect.Buff, SpellTarget.Ally, DamageType.Magical, 0,
                Attribute.Intelligenz, Attribute.Willenskraft, Attribute.Charisma,
                buffAmount: 6, buffDuration: 5);

            Register("vergeltung", "Vergeltung", "Schaden wird auf den Angreifer zurueckgeworfen.",
                20, 5, SpellEffect.Shield, SpellTarget.Ally, DamageType.Magical, 0,
                Attribute.Willenskraft, Attribute.Konstitution, Attribute.Charisma,
                buffAmount: 5, buffDuration: 3);

            Register("apokalypse", "Apokalypse", "Verursacht massiven Schaden an allen Gegnern.",
                50, 8, SpellEffect.Damage, SpellTarget.AllEnemies, DamageType.Fire, 50,
                Attribute.Intelligenz, Attribute.Willenskraft, Attribute.Charisma);

            Register("gottesschild", "Gottesschild", "Macht einen Verbündeten kurzzeitig unverwundbar.",
                40, 7, SpellEffect.Shield, SpellTarget.Ally, DamageType.Magical, 0,
                Attribute.Willenskraft, Attribute.Charisma, Attribute.Charisma,
                buffAmount: 10, buffDuration: 2);

            Register("mana_explosion", "Manaexplosion", "Opfert Mana fuer massiven Schaden.",
                45, 7, SpellEffect.Damage, SpellTarget.AllEnemies, DamageType.Arcane, 40,
                Attribute.Intelligenz, Attribute.Willenskraft, Attribute.Konstitution);

            Register("verwandlung", "Verwandlung", "Verwandelt einen Gegner in einen Frosch.",
                22, 5, SpellEffect.Debuff, SpellTarget.Enemy, DamageType.Magical, 0,
                Attribute.Intelligenz, Attribute.Charisma, Attribute.Wahrnehmung,
                buffAmount: 5, buffDuration: 2);
        }

        private static void Register(string id, string name, string desc, int mana, int lvl,
            SpellEffect effect, SpellTarget target, DamageType dmgType, int power,
            Attribute a1, Attribute a2, Attribute a3,
            int buffAmount = 0, int buffDuration = 0)
        {
            var spell = new Spell(id, name, desc, mana, lvl, effect, target, dmgType, power, a1, a2, a3)
            {
                BuffAmount = buffAmount,
                BuffDuration = buffDuration
            };
            _spells[id] = spell;
        }

        public static Spell Get(string id) =>
            _spells.TryGetValue(id, out var s) ? s : null;

        public static IEnumerable<Spell> GetAvailableForLevel(int level)
        {
            foreach (var s in _spells.Values)
                if (s.MinLevel <= level)
                    yield return s;
        }
    }

    /// <summary>
    /// Result of casting a spell.
    /// </summary>
    public class SpellCastResult
    {
        public bool Success { get; set; }
        public string CasterName { get; set; }
        public string TargetName { get; set; }
        public string SpellName { get; set; }
        public int EffectAmount { get; set; }
        public int ManaUsed { get; set; }
        public string Message { get; set; }
        public CheckResult CheckResult { get; set; }
    }

    /// <summary>
    /// Active buff/debuff on a character or enemy.
    /// </summary>
    public class ActiveBuff
    {
        public string Name { get; set; }
        public SpellEffect Effect { get; set; }
        public int Amount { get; set; }
        public int RemainingTurns { get; set; }
    }

    /// <summary>
    /// Manages spell casting logic.
    /// </summary>
    public static class SpellSystem
    {
        /// <summary>
        /// Casts a spell from a character onto a target.
        /// </summary>
        public static SpellCastResult CastSpell(Character caster, Spell spell, object target)
        {
            var result = new SpellCastResult
            {
                CasterName = caster.Name,
                SpellName = spell.Name,
                ManaUsed = spell.ManaCost
            };

            // Check mana
            if (caster.CombatStats.CurrentMana < spell.ManaCost)
            {
                result.Success = false;
                result.Message = $"{caster.Name} hat nicht genug Mana fuer {spell.Name}.";
                return result;
            }

            // Spend mana
            caster.CombatStats.CurrentMana -= spell.ManaCost;

            // Perform skill check (3d20 against spell attributes)
            int skillValue = caster.Skills["magiekunde"];
            var checkResult = CheckSystem.SkillCheck(
                caster.Attributes[spell.CheckAttr1],
                caster.Attributes[spell.CheckAttr2],
                caster.Attributes[spell.CheckAttr3],
                skillValue
            );
            result.CheckResult = checkResult;

            if (!checkResult.Success)
            {
                result.Success = false;
                result.Message = $"{caster.Name} wirkt {spell.Name}, aber die Magie versagt!";
                return result;
            }

            // Calculate effect based on quality level
            float qlMultiplier = 1.0f + (checkResult.QualityLevel - 1) * 0.2f;
            int effectivePower = Mathf.RoundToInt(spell.BasePower * qlMultiplier);
            result.EffectAmount = effectivePower;

            // Apply effect
            switch (spell.Effect)
            {
                case SpellEffect.Damage:
                    if (target is Enemy enemy)
                    {
                        enemy.TakeDamage(effectivePower);
                        result.TargetName = enemy.Name;
                        result.Message = $"{caster.Name} wirkt {spell.Name} auf {enemy.Name} und verursacht {effectivePower} Schaden!";
                    }
                    break;

                case SpellEffect.Heal:
                    if (target is Character ally)
                    {
                        ally.Heal(effectivePower);
                        result.TargetName = ally.Name;
                        result.Message = $"{caster.Name} heilt {ally.Name} um {effectivePower} Lebenspunkte.";
                    }
                    break;

                case SpellEffect.Buff:
                case SpellEffect.Shield:
                case SpellEffect.Debuff:
                    result.Message = $"{caster.Name} wirkt {spell.Name}. (Effekt: {spell.BuffAmount} fuer {spell.BuffDuration} Runden)";
                    result.TargetName = target is Character c ? c.Name : target is Enemy e ? e.Name : "?";
                    // TODO: Apply buff to target's stat tracking
                    break;
            }

            result.Success = true;
            return result;
        }

        /// <summary>
        /// Checks which spells a character can cast (enough mana + level).
        /// </summary>
        public static IEnumerable<Spell> GetCastableSpells(Character character)
        {
            foreach (var spell in SpellRegistry.GetAvailableForLevel(character.Level))
            {
                if (character.CombatStats.CurrentMana >= spell.ManaCost)
                    yield return spell;
            }
        }
    }
}
