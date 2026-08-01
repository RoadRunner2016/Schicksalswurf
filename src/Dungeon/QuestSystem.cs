using System.Collections.Generic;
using Godot;

namespace Schicksalswurf.Dungeon
{
    using Characters;

    /// <summary>
    /// Quest system with objectives, tracking, and rewards.
    /// </summary>
    public class Quest
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public QuestStatus Status { get; set; } = QuestStatus.NotStarted;
        public int GoldReward { get; set; }
        public int ExpReward { get; set; }
        public List<string> ItemRewards { get; set; } = new();

        // Objective tracking
        public string TargetEnemy { get; set; } = "";
        public int TargetCount { get; set; } = 0;
        public int CurrentCount { get; set; } = 0;
        public int TargetLevel { get; set; } = 0; // reach dungeon level

        public bool IsComplete => Status == QuestStatus.Completed;
        public bool IsActive => Status == QuestStatus.Active;
    }

    public enum QuestStatus
    {
        NotStarted,
        Active,
        Completed,
        Failed
    }

    public static class QuestRegistry
    {
        private static List<Quest> _allQuests = new();
        public static IReadOnlyList<Quest> All => _allQuests;
        public static List<Quest> ActiveQuests => _allQuests.FindAll(q => q.IsActive);

        public static void Initialize()
        {
            _allQuests.Clear();

            _allQuests.Add(new Quest
            {
                Id = "clear_level_1",
                Title = "Erste Schritte",
                Description = "Erkunde den Dungeon und erreiche Ebene 2.",
                GoldReward = 50,
                ExpReward = 100,
                TargetLevel = 2
            });

            _allQuests.Add(new Quest
            {
                Id = "kill_goblins",
                Title = "Goblinplage",
                Description = "Besiege 5 Goblins im Dungeon.",
                GoldReward = 30,
                ExpReward = 80,
                TargetEnemy = "Goblin",
                TargetCount = 5
            });

            _allQuests.Add(new Quest
            {
                Id = "kill_skeletons",
                Title = "Wandelnde Toten",
                Description = "Besiege 3 Skelette.",
                GoldReward = 40,
                ExpReward = 100,
                TargetEnemy = "Skelett",
                TargetCount = 3
            });

            _allQuests.Add(new Quest
            {
                Id = "reach_level_3",
                Title = "In die Tiefe",
                Description = "Erreiche Ebene 3 des Dungeons.",
                GoldReward = 100,
                ExpReward = 200,
                TargetLevel = 3
            });

            _allQuests.Add(new Quest
            {
                Id = "kill_boss",
                Title = "Der Waechter",
                Description = "Besiege den Dungeonwaechter auf Ebene 3.",
                GoldReward = 200,
                ExpReward = 500,
                TargetEnemy = "Dungeonwaechter",
                TargetCount = 1
            });

            // Extended quests
            _allQuests.Add(new Quest
            {
                Id = "kill_spiders",
                Title = "Spinnentrieb",
                Description = "Besiege 4 Riesenspinnen.",
                GoldReward = 35,
                ExpReward = 90,
                TargetEnemy = "Riesenspinne",
                TargetCount = 4
            });

            _allQuests.Add(new Quest
            {
                Id = "kill_orcs",
                Title = "Orkbedrohung",
                Description = "Besiege 3 Orks.",
                GoldReward = 60,
                ExpReward = 150,
                TargetEnemy = "Ork",
                TargetCount = 3
            });

            _allQuests.Add(new Quest
            {
                Id = "kill_mages",
                Title = "Dunkle Magie",
                Description = "Besiege 2 Dunkle Magier.",
                GoldReward = 80,
                ExpReward = 200,
                TargetEnemy = "Dunkler Magier",
                TargetCount = 2
            });

            _allQuests.Add(new Quest
            {
                Id = "reach_level_5",
                Title = "Abenteurer",
                Description = "Erreiche Ebene 5 des Dungeons.",
                GoldReward = 250,
                ExpReward = 400,
                TargetLevel = 5
            });

            _allQuests.Add(new Quest
            {
                Id = "kill_dragon",
                Title = "Drachenjaeger",
                Description = "Besiege den Drachen.",
                GoldReward = 500,
                ExpReward = 1000,
                TargetEnemy = "Drache",
                TargetCount = 1
            });

            _allQuests.Add(new Quest
            {
                Id = "kill_undead",
                Title = "Untote plagen",
                Description = "Besiege 5 Skelette und Ghouls.",
                GoldReward = 70,
                ExpReward = 180,
                TargetEnemy = "Skelett",
                TargetCount = 5
            });

            _allQuests.Add(new Quest
            {
                Id = "kill_demons",
                Title = "Daemonenjaeger",
                Description = "Besiege 3 Daemonen.",
                GoldReward = 150,
                ExpReward = 350,
                TargetEnemy = "Dämon",
                TargetCount = 3
            });

            _allQuests.Add(new Quest
            {
                Id = "kill_lich",
                Title = "Koenig der Toten",
                Description = "Besiege den Lichkoenig.",
                GoldReward = 400,
                ExpReward = 800,
                TargetEnemy = "Lichkoenig",
                TargetCount = 1
            });
        }

        public static void StartQuest(string id)
        {
            var quest = _allQuests.Find(q => q.Id == id);
            if (quest != null && quest.Status == QuestStatus.NotStarted)
                quest.Status = QuestStatus.Active;
        }

        public static void OnEnemyKilled(string enemyName)
        {
            foreach (var quest in _allQuests)
            {
                if (quest.IsActive && quest.TargetEnemy == enemyName)
                {
                    quest.CurrentCount++;
                    if (quest.CurrentCount >= quest.TargetCount)
                        quest.Status = QuestStatus.Completed;
                }
            }
        }

        public static void OnLevelReached(int level)
        {
            foreach (var quest in _allQuests)
            {
                if (quest.IsActive && quest.TargetLevel > 0 && level >= quest.TargetLevel)
                    quest.Status = QuestStatus.Completed;
            }
        }

        public static List<Quest> GetCompletedQuests() =>
            _allQuests.FindAll(q => q.Status == QuestStatus.Completed);

        public static void ClaimReward(Quest quest, Party party)
        {
            if (quest.Status != QuestStatus.Completed) return;

            party.Gold += quest.GoldReward;
            foreach (var member in party.Members)
                member.GainExperience(quest.ExpReward);

            quest.Status = QuestStatus.Failed; // mark as claimed
        }
    }
}
