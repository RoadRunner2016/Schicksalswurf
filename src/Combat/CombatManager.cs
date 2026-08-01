using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Schicksalswurf.Combat
{
    using Core;
    using Characters;
    using Dungeon;

    public enum CombatPhase
    {
        Initiative,
        PlayerTurn,
        EnemyTurn,
        Victory,
        Defeat
    }

    public class InitiativeEntry
    {
        public Character Character { get; set; }
        public Enemy Enemy { get; set; }
        public bool IsPlayer => Character != null;
        public int Initiative { get; set; }
    }

    public class AttackResult
    {
        public string AttackerName { get; set; }
        public string DefenderName { get; set; }
        public bool Hit { get; set; }
        public int Damage { get; set; }
        public int Soak { get; set; }
        public int NetDamage { get; set; }
        public bool Critical { get; set; }
        public string Message { get; set; }

        public override string ToString() => Message;
    }

    /// <summary>
    /// Manages a turn-based combat encounter between the party and enemies.
    /// </summary>
    public class CombatManager
    {
        public Party Party { get; set; }
        public Encounter Encounter { get; set; }
        public List<InitiativeEntry> InitiativeOrder { get; } = new();
        public int CurrentIndex { get; set; } = 0;
        public CombatPhase Phase { get; set; } = CombatPhase.Initiative;
        public List<string> Log { get; } = new();
        public CombatGrid Grid { get; private set; }
        public Dictionary<Character, CombatUnit> PartyUnits { get; } = new();
        public Dictionary<Enemy, CombatUnit> EnemyUnits { get; } = new();
        public CombatUnit ActiveUnit { get; set; }

        public CombatManager(Party party, Encounter encounter)
        {
            Party = party;
            Encounter = encounter;
            Grid = new CombatGrid();
        }

        public void StartCombat()
        {
            Log.Clear();
            InitiativeOrder.Clear();
            CurrentIndex = 0;
            Grid.Clear();
            PartyUnits.Clear();
            EnemyUnits.Clear();

            // Place party on grid
            int slot = 0;
            foreach (var c in Party.AliveMembers)
            {
                int roll = DiceRoller.RollD20();
                var entry = new InitiativeEntry
                {
                    Character = c,
                    Initiative = roll + c.CombatStats.Initiative
                };
                InitiativeOrder.Add(entry);

                var gridPos = Grid.GetPartyPosition(slot);
                var unit = new CombatUnit
                {
                    Character = c,
                    GridPosition = gridPos,
                    MaxMovementPoints = 2 + c.CombatStats.Initiative / 4,
                    MovementPoints = 2 + c.CombatStats.Initiative / 4
                };
                Grid.Units[gridPos] = unit;
                PartyUnits[c] = unit;
                slot++;
            }

            // Place enemies on grid
            int enemyIdx = 0;
            int totalEnemies = Encounter.AliveEnemies.Count();
            foreach (var e in Encounter.AliveEnemies)
            {
                int roll = DiceRoller.RollD20();
                var entry = new InitiativeEntry
                {
                    Enemy = e,
                    Initiative = roll + e.Initiative
                };
                InitiativeOrder.Add(entry);

                var gridPos = Grid.GetEnemyPosition(enemyIdx, totalEnemies);
                var unit = new CombatUnit
                {
                    Enemy = e,
                    GridPosition = gridPos,
                    MaxMovementPoints = 2 + e.Initiative / 4,
                    MovementPoints = 2 + e.Initiative / 4
                };
                Grid.Units[gridPos] = unit;
                EnemyUnits[e] = unit;
                enemyIdx++;
            }

            InitiativeOrder.Sort((a, b) => b.Initiative.CompareTo(a.Initiative));

            Phase = CombatPhase.PlayerTurn;
            Log.Add("Der Kampf beginnt!");

            AdvanceToNextParticipant();
        }

        public InitiativeEntry CurrentParticipant =>
            CurrentIndex < InitiativeOrder.Count ? InitiativeOrder[CurrentIndex] : null;

        public void AdvanceToNextParticipant()
        {
            InitiativeOrder.RemoveAll(e =>
                (e.IsPlayer && !e.Character.IsAlive) ||
                (!e.IsPlayer && !e.Enemy.IsAlive));

            if (Encounter.IsCleared)
            {
                Phase = CombatPhase.Victory;
                Log.Add("Sieg! Alle Gegner wurden besiegt.");
                DistributeRewards();
                return;
            }

            if (Party.IsDefeated)
            {
                Phase = CombatPhase.Defeat;
                Log.Add("Niederlage! Die Gruppe wurde besiegt.");
                return;
            }

            if (CurrentIndex >= InitiativeOrder.Count)
                CurrentIndex = 0;

            var current = CurrentParticipant;
            if (current == null)
            {
                CurrentIndex = 0;
                return;
            }

            Phase = current.IsPlayer ? CombatPhase.PlayerTurn : CombatPhase.EnemyTurn;

            // Set active unit and reset its turn
            if (current.IsPlayer && PartyUnits.TryGetValue(current.Character, out var pUnit))
            {
                ActiveUnit = pUnit;
                pUnit.ResetTurn();
            }
            else if (!current.IsPlayer && EnemyUnits.TryGetValue(current.Enemy, out var eUnit))
            {
                ActiveUnit = eUnit;
                eUnit.ResetTurn();
            }

            if (!current.IsPlayer)
            {
                ExecuteEnemyTurn(current.Enemy);
            }
        }

        public AttackResult PlayerAttack(Character attacker, Enemy target)
        {
            var result = new AttackResult
            {
                AttackerName = attacker.Name,
                DefenderName = target.Name
            };

            // Check range on grid
            if (PartyUnits.TryGetValue(attacker, out var attackerUnit) &&
                EnemyUnits.TryGetValue(target, out var targetUnit))
            {
                int dist = Grid.Distance(attackerUnit.GridPosition, targetUnit.GridPosition);
                if (dist > 2)
                {
                    result.Hit = false;
                    result.Message = $"{attacker.Name} ist zu weit entfernt von {target.Name} (Distanz: {dist}).";
                    Log.Add(result.Message);
                    EndTurn();
                    return result;
                }
            }

            int attackValue = attacker.GetAttackValue();
            int defenseValue = target.Defense;
            int margin = CheckSystem.OpposedCheck(attackValue, defenseValue);

            if (margin > 0)
            {
                result.Hit = true;
                bool crit = DiceRoller.RollD20() == 1;
                result.Critical = crit;

                int damage = attacker.GetDamage();
                if (crit) damage *= 2;

                int netDamage = Mathf.Max(1, damage - target.Soak);
                result.Damage = damage;
                result.Soak = target.Soak;
                result.NetDamage = netDamage;

                target.TakeDamage(netDamage);
                result.Message = crit
                    ? $"{attacker.Name} trifft {target.Name} kritisch fuer {netDamage} Schaden!"
                    : $"{attacker.Name} trifft {target.Name} fuer {netDamage} Schaden.";
            }
            else
            {
                result.Hit = false;
                result.Message = $"{attacker.Name} verfehlt {target.Name}.";
            }

            Log.Add(result.Message);
            EndTurn();
            return result;
        }

        private void ExecuteEnemyTurn(Enemy enemy)
        {
            // Move enemy toward nearest party member if too far
            if (EnemyUnits.TryGetValue(enemy, out var enemyUnit))
            {
                var nearestParty = FindNearestPartyMember(enemyUnit.GridPosition);
                if (nearestParty != null)
                {
                    int dist = Grid.Distance(enemyUnit.GridPosition, nearestParty.GridPosition);
                    if (dist > 2 && enemyUnit.MovementPoints > 0)
                    {
                        MoveUnitToward(enemyUnit, nearestParty.GridPosition);
                    }
                }
            }

            var targets = Party.AliveMembers.ToList();
            if (targets.Count == 0) return;

            var messages = EnemyAI.ExecuteEnemyAction(enemy, targets, Party.Members.Count);
            foreach (var msg in messages)
                Log.Add(msg);

            EndTurn();
        }

        private CombatUnit FindNearestPartyMember(Vector2I fromPos)
        {
            CombatUnit nearest = null;
            int minDist = int.MaxValue;
            foreach (var kv in PartyUnits)
            {
                if (!kv.Value.IsAlive) continue;
                int d = Grid.Distance(fromPos, kv.Value.GridPosition);
                if (d < minDist) { minDist = d; nearest = kv.Value; }
            }
            return nearest;
        }

        private void MoveUnitToward(CombatUnit unit, Vector2I targetPos)
        {
            while (unit.MovementPoints > 0)
            {
                int dist = Grid.Distance(unit.GridPosition, targetPos);
                if (dist <= 2) break;

                var adjacent = Grid.GetAdjacentTiles(unit.GridPosition);
                if (adjacent.Count == 0) break;

                Vector2I best = adjacent[0];
                int bestDist = Grid.Distance(best, targetPos);
                foreach (var p in adjacent)
                {
                    int d = Grid.Distance(p, targetPos);
                    if (d < bestDist) { bestDist = d; best = p; }
                }

                Grid.MoveUnit(unit, best);
                unit.MovementPoints--;
            }
        }

        public bool MovePlayerUnit(Character character, Vector2I newPos)
        {
            if (!PartyUnits.TryGetValue(character, out var unit)) return false;
            if (unit.MovementPoints <= 0) return false;
            if (Grid.IsOccupied(newPos)) return false;
            if (!Grid.IsInBounds(newPos)) return false;

            int dist = Grid.Distance(unit.GridPosition, newPos);
            if (dist > unit.MovementPoints) return false;

            Grid.MoveUnit(unit, newPos);
            unit.MovementPoints -= dist;
            Log.Add($"{character.Name} bewegt sich {dist} Felder.");
            return true;
        }

        public List<Vector2I> GetMoveTilesForCurrentPlayer()
        {
            if (ActiveUnit == null || !ActiveUnit.IsPlayer) return new List<Vector2I>();
            return Grid.GetTilesInRange(ActiveUnit.GridPosition, ActiveUnit.MovementPoints);
        }

        public List<CombatUnit> GetAttackTargetsForCurrentPlayer()
        {
            if (ActiveUnit == null || !ActiveUnit.IsPlayer) return new List<CombatUnit>();
            return Grid.GetEnemiesInRange(ActiveUnit.GridPosition, 2);
        }

        public void PlayerDefend(Character character)
        {
            Log.Add($"{character.Name} geht in Verteidigungshaltung.");
            EndTurn();
        }

        public bool PlayerFlee()
        {
            int partyInit = 0, enemyInit = 0, count = 0;

            foreach (var m in Party.AliveMembers)
            {
                partyInit += DiceRoller.RollD20() + m.CombatStats.Initiative;
                count++;
            }
            partyInit /= Mathf.Max(1, count);

            var aliveEnemies = Encounter.AliveEnemies.ToList();
            foreach (var e in aliveEnemies)
                enemyInit += DiceRoller.RollD20() + e.Initiative;
            enemyInit /= Mathf.Max(1, aliveEnemies.Count);

            bool fled = partyInit > enemyInit;
            Log.Add(fled ? "Die Gruppe entkommt!" : "Flucht fehlgeschlagen!");
            if (fled)
                Phase = CombatPhase.Victory; // Not a real victory but ends combat
            else
                EndTurn();
            return fled;
        }

        private void EndTurn()
        {
            CurrentIndex++;
            AdvanceToNextParticipant();
        }

        private void DistributeRewards()
        {
            int totalExp = 0, totalGold = 0;
            foreach (var e in Encounter.Enemies)
            {
                totalExp += e.ExperienceReward;
                totalGold += e.GoldReward;

                // Notify quest system of enemy kill
                QuestRegistry.OnEnemyKilled(e.Name);
            }

            Party.Gold += totalGold;
            Log.Add($"+{totalGold} Gold erhalten.");

            int perMember = totalExp / Mathf.Max(1, Party.Members.Count);
            foreach (var m in Party.Members)
            {
                int oldLevel = m.Level;
                m.GainExperience(perMember);
                Log.Add($"{m.Name} erhaelt {perMember} EP.");
                if (m.Level > oldLevel)
                    Log.Add($"{m.Name} steigt auf Level {m.Level}! (+{m.UnspentAttributePoints} Attributspunkte, +{m.UnspentSkillPoints} Talentpunkte)");
            }
        }
    }
}
