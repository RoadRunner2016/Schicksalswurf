using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Core
{
    using Characters;

    /// <summary>
    /// Companion/pet system: summoned creatures and animal companions that fight alongside the party.
    /// </summary>
    public class Companion
    {
        public string Name { get; set; }
        public string Species { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Damage { get; set; }
        public int Defense { get; set; }
        public int Soak { get; set; }
        public int Initiative { get; set; }
        public int Duration { get; set; } // rounds remaining, -1 = permanent
        public bool IsActive => Health > 0 && Duration != 0;
        public string SpecialAbility { get; set; } = "";

        public Companion(string name, string species, int hp, int atk, int dmg, int def, int soak, int init, int duration = -1)
        {
            Name = name; Species = species;
            MaxHealth = hp; Health = hp;
            Attack = atk; Damage = dmg; Defense = def; Soak = soak;
            Initiative = init; Duration = duration;
        }

        public void TakeDamage(int amount) => Health = Mathf.Max(0, Health - amount);

        public void TickDuration()
        {
            if (Duration > 0) Duration--;
        }
    }

    public static class CompanionSystem
    {
        public static List<Companion> ActiveCompanions { get; } = new();

        public static Companion SummonWolf(int casterLevel)
        {
            var wolf = new Companion(
                "Beschworener Wolf", "Wolf",
                15 + casterLevel * 3, 10 + casterLevel, 4 + casterLevel / 2,
                6, 0, 12, 5
            ) { SpecialAbility = "Biss" };
            ActiveCompanions.Add(wolf);
            return wolf;
        }

        public static Companion SummonSkeleton(int casterLevel)
        {
            var skel = new Companion(
                "Beschwoertes Skelett", "Skelett",
                20 + casterLevel * 4, 10 + casterLevel, 5 + casterLevel / 2,
                9, 1, 7, 10
            );
            ActiveCompanions.Add(skel);
            return skel;
        }

        public static Companion SummonFireElemental(int casterLevel)
        {
            var elemental = new Companion(
                "Feuerelementar", "Elementar",
                25 + casterLevel * 5, 14 + casterLevel, 8 + casterLevel / 2,
                8, 2, 14, 8
            ) { SpecialAbility = "Verbrennung" };
            ActiveCompanions.Add(elemental);
            return elemental;
        }

        public static Companion SummonBear(int casterLevel)
        {
            var bear = new Companion(
                "Beschwoerter Baer", "Baer",
                40 + casterLevel * 6, 16 + casterLevel, 10 + casterLevel / 2,
                8, 3, 8, -1
            ) { SpecialAbility = "Umklammerung" };
            ActiveCompanions.Add(bear);
            return bear;
        }

        public static void ClearExpired()
        {
            ActiveCompanions.RemoveAll(c => !c.IsActive);
        }

        public static void TickAll()
        {
            foreach (var c in ActiveCompanions)
                c.TickDuration();
            ClearExpired();
        }
    }

    /// <summary>
    /// Social interaction system: intimidate, persuade, bribe outside of dialogue.
    /// </summary>
    public static class SocialSystem
    {
        public enum SocialAction
        {
            Intimidate,
            Persuade,
            Bribe,
            Charm,
            Threaten
        }

        public static (bool success, string message) AttemptSocialAction(Character character, SocialAction action, int difficulty, int bribeAmount = 0)
        {
            string skillId = action switch
            {
                SocialAction.Intimidate => "bedrohen",
                SocialAction.Persuade => "ueberreden",
                SocialAction.Charm => "charisma",
                SocialAction.Threaten => "bedrohen",
                SocialAction.Bribe => "handel",
                _ => "charisma"
            };

            int modifier = action switch
            {
                SocialAction.Intimidate => character.Attributes[Attribute.Kraft] / 3,
                SocialAction.Persuade => character.Attributes[Attribute.Charisma] / 3,
                SocialAction.Charm => character.Attributes[Attribute.Charisma] / 2,
                SocialAction.Threaten => character.Attributes[Attribute.Kraft] / 4,
                SocialAction.Bribe => bribeAmount / 10,
                _ => 0
            };

            var result = character.PerformSkillCheck(skillId, difficulty - modifier);
            if (result.Success)
            {
                return (true, action switch
                {
                    SocialAction.Intimidate => $"{character.Name} schuechter den Gegner erfolgreich ein.",
                    SocialAction.Persuade => $"{character.Name} ueberzeugt den Gegner erfolgreich.",
                    SocialAction.Charm => $"{character.Name} bezaubert den Gegner.",
                    SocialAction.Threaten => $"{character.Name} droht dem Gegner erfolgreich.",
                    SocialAction.Bribe => $"{character.Name} besticht den Gegner erfolgreich. (-{bribeAmount} Gold)",
                    _ => "Erfolg."
                });
            }
            else
            {
                return (false, action switch
                {
                    SocialAction.Intimidate => $"{character.Name} kann den Gegner nicht einschuechtern.",
                    SocialAction.Persuade => $"{character.Name} kann den Gegner nicht ueberzeugen.",
                    SocialAction.Charm => $"{character.Name} verfehlt mit dem Charme.",
                    SocialAction.Threaten => $"{character.Name} Drohung bleibt wirkungslos.",
                    SocialAction.Bribe => $"{character.Name} Bestechung wird abgelehnt. (-{bribeAmount} Gold verloren)",
                    _ => "Fehlschlag."
                });
            }
        }
    }
}
