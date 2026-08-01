using Godot;

namespace Schicksalswurf.Core
{
    using Characters;

    /// <summary>
    /// Hunger/thirst survival mechanics for dungeon exploration.
    /// </summary>
    public class SurvivalSystem
    {
        public class SurvivalState
        {
            public float Hunger { get; set; } = 100f; // 100 = full, 0 = starving
            public float Thirst { get; set; } = 100f;
            public float HungerRate { get; set; } = 0.5f; // per step
            public float ThirstRate { get; set; } = 0.7f; // per step

            public bool IsStarving => Hunger <= 0;
            public bool IsDehydrated => Thirst <= 0;
            public bool IsHungry => Hunger < 30;
            public bool IsThirsty => Thirst < 30;
        }

        public static void OnStep(SurvivalState state)
        {
            state.Hunger = Mathf.Max(0, state.Hunger - state.HungerRate);
            state.Thirst = Mathf.Max(0, state.Thirst - state.ThirstRate);
        }

        public static void OnRest(SurvivalState state)
        {
            // Resting consumes more food/water
            state.Hunger = Mathf.Max(0, state.Hunger - 10);
            state.Thirst = Mathf.Max(0, state.Thirst - 15);
        }

        public static string GetHungerWarning(SurvivalState state)
        {
            if (state.IsStarving) return "Eure Gruppe verhungert! Verliert HP pro Schritt.";
            if (state.IsHungry) return "Eure Gruppe ist hungrig. Sucht Nahrung.";
            return null;
        }

        public static string GetThirstWarning(SurvivalState state)
        {
            if (state.IsDehydrated) return "Eure Gruppe verdurstet! Verliert HP pro Schritt.";
            if (state.IsThirsty) return "Eure Gruppe ist durstig. Sucht Wasser.";
            return null;
        }

        public static void ApplyStarvationDamage(Party party, SurvivalState state)
        {
            if (state.IsStarving)
                foreach (var m in party.Members)
                    if (m.CombatStats.CurrentHealth > 0)
                        m.TakeDamage(1);
            if (state.IsDehydrated)
                foreach (var m in party.Members)
                    if (m.CombatStats.CurrentHealth > 0)
                        m.TakeDamage(2);
        }

        public static (bool success, string message) EatFood(Character character, Item food)
        {
            if (food.Id == "proviant" || food.Id == "fleisch" || food.Id == "brot")
            {
                character.Inventory.Remove(food);
                return (true, $"{character.Name} isst {food.Name}. (+30 Hunger)");
            }
            return (false, "Das ist keine Nahrung.");
        }

        public static (bool success, string message) DrinkWater(Character character, Item water)
        {
            if (water.Id == "wasserflasche" || water.Id == "trank")
            {
                character.Inventory.Remove(water);
                return (true, $"{character.Name} trinkt {water.Name}. (+30 Durst)");
            }
            return (false, "Das ist kein Getraenk.");
        }
    }
}
