using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Core
{
    /// <summary>
    /// Game statistics tracking: kills, chests, gold, play time, etc.
    /// </summary>
    public class GameStats
    {
        public int EnemiesKilled { get; set; } = 0;
        public int ChestsFound { get; set; } = 0;
        public int GoldSpent { get; set; } = 0;
        public int GoldEarned { get; set; } = 0;
        public int LevelsDescended { get; set; } = 0;
        public int TrapsTriggered { get; set; } = 0;
        public int TrapsDisarmed { get; set; } = 0;
        public int SpellsCast { get; set; } = 0;
        public int ItemsUsed { get; set; } = 0;
        public int RestsTaken { get; set; } = 0;
        public int FledBattles { get; set; } = 0;
        public int BossesKilled { get; set; } = 0;

        private float _playTime = 0;
        public float PlayTime => _playTime;
        public string PlayTimeStr
        {
            get
            {
                int h = (int)(_playTime / 3600);
                int m = (int)((_playTime % 3600) / 60);
                int s = (int)(_playTime % 60);
                return $"{h:D2}:{m:D2}:{s:D2}";
            }
        }

        public void AddTime(float delta) => _playTime += delta;

        // Achievement system
        public List<Achievement> Achievements { get; } = new();
        public List<string> UnlockedAchievementIds { get; } = new();

        public void InitializeAchievements()
        {
            Achievements.Clear();
            Achievements.Add(new Achievement("first_kill", "Erster Sieg", "Besiege deinen ersten Gegner.", 10));
            Achievements.Add(new Achievement("kill_10", "Krieger", "Besiege 10 Gegner.", 50));
            Achievements.Add(new Achievement("kill_50", "Schlachter", "Besiege 50 Gegner.", 200));
            Achievements.Add(new Achievement("kill_100", "Kriegsherr", "Besiege 100 Gegner.", 500));
            Achievements.Add(new Achievement("first_boss", "Boss-Schläger", "Besiege deinen ersten Boss.", 100));
            Achievements.Add(new Achievement("chests_10", "Schatzjäger", "Öffne 10 Truhen.", 50));
            Achievements.Add(new Achievement("chests_25", "Plünderer", "Öffne 25 Truhen.", 100));
            Achievements.Add(new Achievement("level_3", "Abenteurer", "Erreiche Ebene 3.", 100));
            Achievements.Add(new Achievement("level_5", "Tiefenforscher", "Erreiche Ebene 5.", 200));
            Achievements.Add(new Achievement("level_10", "Meister der Tiefe", "Erreiche Ebene 10.", 500));
            Achievements.Add(new Achievement("gold_500", "Reich", "Sammle 500 Gold.", 50));
            Achievements.Add(new Achievement("gold_2000", "Wohlhabend", "Sammle 2000 Gold.", 200));
            Achievements.Add(new Achievement("traps_5", "Vorsichtig", "Entschärfe 5 Fallen.", 50));
            Achievements.Add(new Achievement("spells_20", "Magier", "Wirke 20 Zauber.", 50));
            Achievements.Add(new Achievement("rests_10", "Rastender", "Raste 10 mal.", 10));
        }

        public void CheckAchievements()
        {
            TryUnlock("first_kill", EnemiesKilled >= 1);
            TryUnlock("kill_10", EnemiesKilled >= 10);
            TryUnlock("kill_50", EnemiesKilled >= 50);
            TryUnlock("kill_100", EnemiesKilled >= 100);
            TryUnlock("first_boss", BossesKilled >= 1);
            TryUnlock("chests_10", ChestsFound >= 10);
            TryUnlock("chests_25", ChestsFound >= 25);
            TryUnlock("level_3", LevelsDescended >= 2);
            TryUnlock("level_5", LevelsDescended >= 4);
            TryUnlock("level_10", LevelsDescended >= 9);
            TryUnlock("gold_500", GoldEarned >= 500);
            TryUnlock("gold_2000", GoldEarned >= 2000);
            TryUnlock("traps_5", TrapsDisarmed >= 5);
            TryUnlock("spells_20", SpellsCast >= 20);
            TryUnlock("rests_10", RestsTaken >= 10);
        }

        private void TryUnlock(string id, bool condition)
        {
            if (condition && !UnlockedAchievementIds.Contains(id))
            {
                UnlockedAchievementIds.Add(id);
                var ach = Achievements.Find(a => a.Id == id);
                if (ach != null)
                    ach.Unlocked = true;
            }
        }

        public List<Achievement> GetRecentlyUnlocked() =>
            Achievements.FindAll(a => a.Unlocked && a.IsNew);

        public void MarkAllSeen()
        {
            foreach (var a in Achievements)
                a.IsNew = false;
        }
    }

    public class Achievement
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ExpReward { get; set; }
        public bool Unlocked { get; set; } = false;
        public bool IsNew { get; set; } = true;

        public Achievement(string id, string title, string desc, int exp)
        {
            Id = id; Title = title; Description = desc; ExpReward = exp;
        }
    }
}
