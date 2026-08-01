using Godot;

namespace Schicksalswurf.Dungeon
{
    using Characters;
    using Core;

    /// <summary>
    /// Trap mechanics: detection via perception check, disarming with lockpick/schurke.
    /// </summary>
    public static class TrapSystem
    {
        public enum TrapType
        {
            Spike,
            Poison,
            Fire,
            Pit,
            Arrow
        }

        public class TrapInfo
        {
            public TrapType Type;
            public int Damage;
            public int DetectDifficulty;
            public int DisarmDifficulty;
            public bool IsDetected;
            public bool IsDisarmed;
        }

        public static TrapInfo GenerateTrap(int dungeonLevel)
        {
            var rng = new RandomNumberGenerator();
            rng.Randomize();

            var type = (TrapType)rng.RandiRange(0, 4);
            int baseDmg = 3 + dungeonLevel * 2;

            return new TrapInfo
            {
                Type = type,
                Damage = type switch
                {
                    TrapType.Spike => baseDmg + 2,
                    TrapType.Poison => baseDmg,
                    TrapType.Fire => baseDmg + 3,
                    TrapType.Pit => baseDmg + 5,
                    TrapType.Arrow => baseDmg + 1,
                    _ => baseDmg
                },
                DetectDifficulty = 10 + dungeonLevel,
                DisarmDifficulty = 12 + dungeonLevel,
                IsDetected = false,
                IsDisarmed = false
            };
        }

        public static string GetTrapName(TrapType type) => type switch
        {
            TrapType.Spike => "Stachel-Falle",
            TrapType.Poison => "Gift-Falle",
            TrapType.Fire => "Feuer-Falle",
            TrapType.Pit => "Fallgrube",
            TrapType.Arrow => "Pfeil-Falle",
            _ => "Falle"
        };

        /// <summary>
        /// Attempts to detect a trap using perception check.
        /// </summary>
        public static bool TryDetect(Character detector, TrapInfo trap)
        {
            if (trap.IsDetected) return true;

            var result = detector.PerformSkillCheck("wahrnehmung", trap.DetectDifficulty);
            trap.IsDetected = result.Success;
            return trap.IsDetected;
        }

        /// <summary>
        /// Attempts to disarm a trap. Requires lockpick or schurke skill.
        /// </summary>
        public static (bool success, string message, bool lostLockpick) TryDisarm(Character disarmer, TrapInfo trap)
        {
            if (trap.IsDisarmed) return (true, "Falle ist bereits entschärft.", false);
            if (!trap.IsDetected) return (false, "Falle wurde nicht erkannt.", false);

            bool hasLockpick = disarmer.Inventory.Find(i => i.Id == "lockpick") != null;
            if (!hasLockpick)
                return (false, "Kein Dietrich im Inventar.", false);

            var result = disarmer.PerformSkillCheck("schleichen", trap.DisarmDifficulty);

            // Remove lockpick (consumed on attempt)
            var lockpick = disarmer.Inventory.Find(i => i.Id == "lockpick");
            disarmer.Inventory.Remove(lockpick);

            if (result.Success)
            {
                trap.IsDisarmed = true;
                return (true, $"{disarmer.Name} entschärft die Falle erfolgreich!", true);
            }
            else
            {
                return (false, $"{disarmer.Name} scheitert beim Entschärfen. Der Dietrich bricht.", true);
            }
        }

        /// <summary>
        /// Triggers the trap, dealing damage to the victim.
        /// </summary>
        public static string Trigger(Character victim, TrapInfo trap)
        {
            victim.TakeDamage(trap.Damage);

            // Poison traps apply poison status
            if (trap.Type == TrapType.Poison)
            {
                StatusSystem.ApplyStatus(victim, StatusEffectType.Poison, 3, 3, "Falle");
                return $"{victim.Name} wird von einer Gift-Falle getroffen (-{trap.Damage} HP, vergiftet!)";
            }

            if (trap.Type == TrapType.Fire)
            {
                StatusSystem.ApplyStatus(victim, StatusEffectType.Burn, 2, 2, "Falle");
                return $"{victim.Name} wird von einer Feuer-Falle getroffen (-{trap.Damage} HP, brennt!)";
            }

            return $"{victim.Name} wird von {GetTrapName(trap.Type)} getroffen (-{trap.Damage} HP).";
        }
    }
}
