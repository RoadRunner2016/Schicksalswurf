using Godot;

namespace Schicksalswurf.Dungeon
{
    /// <summary>
    /// Different dungeon types with varying appearance and generation parameters.
    /// </summary>
    public enum DungeonType
    {
        Verlies,    // Dungeon - stone walls
        Hoehle,     // Cave - natural walls
        Turm,       // Tower - vertical layout
        Katakomben  // Catacombs - narrow corridors
    }

    public class DungeonTheme
    {
        public DungeonType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MinRoomSize { get; set; } = 3;
        public int MaxRoomSize { get; set; } = 7;
        public int MaxRooms { get; set; } = 12;
        public float ChestChance { get; set; } = 0.12f;
        public float TrapChance { get; set; } = 0.06f;
        public float EncounterRate { get; set; } = 0.15f;

        // Visual theme
        public Color WallColor { get; set; } = new(0.4f, 0.35f, 0.3f);
        public Color FloorColor { get; set; } = new(0.2f, 0.18f, 0.15f);
        public Color CeilingColor { get; set; } = new(0.1f, 0.08f, 0.06f);
        public Color AmbientColor { get; set; } = new(0.3f, 0.25f, 0.2f);
        public float FogDensity { get; set; } = 0.8f;

        // Enemy pool for this dungeon type
        public string[] EnemyPool { get; set; } = { "Goblin", "Wolf", "Skeleton" };
    }

    public static class DungeonThemes
    {
        public static readonly DungeonTheme Verlies = new()
        {
            Type = DungeonType.Verlies,
            Name = "Verlies",
            Description = "Ein altes Verlies mit steinernen Waenden.",
            MinRoomSize = 3, MaxRoomSize = 7, MaxRooms = 12,
            ChestChance = 0.12f, TrapChance = 0.06f, EncounterRate = 0.15f,
            WallColor = new(0.4f, 0.35f, 0.3f),
            FloorColor = new(0.2f, 0.18f, 0.15f),
            CeilingColor = new(0.1f, 0.08f, 0.06f),
            AmbientColor = new(0.3f, 0.25f, 0.2f),
            FogDensity = 0.8f,
            EnemyPool = new[] { "Goblin", "Wolf", "Skeleton", "Bandit" }
        };

        public static readonly DungeonTheme Hoehle = new()
        {
            Type = DungeonType.Hoehle,
            Name = "Hoehle",
            Description = "Eine dunkle Hoehle mit unebenen Waenden.",
            MinRoomSize = 4, MaxRoomSize = 9, MaxRooms = 8,
            ChestChance = 0.08f, TrapChance = 0.04f, EncounterRate = 0.18f,
            WallColor = new(0.3f, 0.28f, 0.25f),
            FloorColor = new(0.15f, 0.13f, 0.1f),
            CeilingColor = new(0.08f, 0.07f, 0.05f),
            AmbientColor = new(0.25f, 0.22f, 0.18f),
            FogDensity = 0.9f,
            EnemyPool = new[] { "Wolf", "Riesenspinne", "Ork" }
        };

        public static readonly DungeonTheme Turm = new()
        {
            Type = DungeonType.Turm,
            Name = "Turm",
            Description = "Ein verwunschener Turm mit engen Gaengen.",
            MinRoomSize = 2, MaxRoomSize = 5, MaxRooms = 15,
            ChestChance = 0.15f, TrapChance = 0.10f, EncounterRate = 0.20f,
            WallColor = new(0.35f, 0.32f, 0.4f),
            FloorColor = new(0.18f, 0.16f, 0.22f),
            CeilingColor = new(0.1f, 0.08f, 0.12f),
            AmbientColor = new(0.28f, 0.25f, 0.35f),
            FogDensity = 0.7f,
            EnemyPool = new[] { "Skeleton", "Dunkler Magier", "Bandit" }
        };

        public static readonly DungeonTheme Katakomben = new()
        {
            Type = DungeonType.Katakomben,
            Name = "Katakomben",
            Description = "Enges Labyrinth unter der Erde.",
            MinRoomSize = 2, MaxRoomSize = 4, MaxRooms = 18,
            ChestChance = 0.10f, TrapChance = 0.12f, EncounterRate = 0.22f,
            WallColor = new(0.25f, 0.22f, 0.2f),
            FloorColor = new(0.12f, 0.1f, 0.08f),
            CeilingColor = new(0.06f, 0.05f, 0.04f),
            AmbientColor = new(0.2f, 0.18f, 0.15f),
            FogDensity = 1.0f,
            EnemyPool = new[] { "Skeleton", "Riesenspinne", "Dunkler Magier" }
        };

        public static DungeonTheme GetTheme(DungeonType type) => type switch
        {
            DungeonType.Verlies => Verlies,
            DungeonType.Hoehle => Hoehle,
            DungeonType.Turm => Turm,
            DungeonType.Katakomben => Katakomben,
            _ => Verlies
        };

        public static DungeonTheme GetThemeByLevel(int level)
        {
            // Cycle through themes every 3 levels
            int idx = (level - 1) / 3 % 4;
            return idx switch
            {
                0 => Verlies,
                1 => Katakomben,
                2 => Hoehle,
                3 => Turm,
                _ => Verlies
            };
        }
    }
}
